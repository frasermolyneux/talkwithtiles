"""Disposable CI-only checks; never touches a host installation or real credentials."""

import base64
import json
import os
from pathlib import Path
import secrets
import shutil
import subprocess
import time


ROOT = Path(".ci-state/containers").resolve()
PREFIX = f"tiles-ci-{os.getpid()}"


def run(*args, capture=False, **kwargs):
    result = subprocess.run(args, check=True, text=True,
                            stdout=subprocess.PIPE if capture else None, **kwargs)
    return result.stdout.strip() if capture else ""


def wait_healthy(container):
    for _ in range(90):
        state = json.loads(run("docker", "inspect", container, capture=True))[0]["State"]
        if state.get("Health", {}).get("Status") == "healthy":
            return
        if not state["Running"]:
            break
        time.sleep(2)
    run("docker", "logs", container)
    raise RuntimeError(f"{container} did not become healthy")


def constrained(name, image, user, mounts):
    command = ["docker", "run", "--detach", "--name", name,
               "--network", PREFIX, "--user", user, "--read-only",
               "--cap-drop=ALL", "--security-opt=no-new-privileges:true",
               "--tmpfs", "/tmp:rw,noexec,nosuid,size=64m",
               "--pids-limit", "128", "--memory", "512m", "--cpus", "1"]
    for source, target, writable in mounts:
        command.extend(["--mount", f"type=bind,src={source},dst={target}" + ("" if writable else ",readonly")])
    command.append(image)
    run(*command)


def main():
    ROOT.mkdir(parents=True, exist_ok=True)
    for directory, uid in (("postgres", 999), ("keys", 1654)):
        target = ROOT / directory
        target.mkdir(exist_ok=True)
        if os.name != "nt":
            run("sudo", "chown", f"{uid}:1654", str(target))
            run("sudo", "chmod", "0770", str(target))
    password = ROOT / "database-password"
    password.write_text(secrets.token_urlsafe(32), encoding="utf-8")
    certificate = ROOT / "client-certificate"
    openssl = shutil.which("openssl")
    if not openssl and os.name == "nt":
        openssl = str(Path(os.environ["ProgramFiles"]) / "Git" / "usr" / "bin" / "openssl.exe")
    if not openssl:
        raise RuntimeError("OpenSSL is required to generate a disposable CI-only certificate.")
    run(openssl, "req", "-x509", "-newkey", "rsa:2048", "-nodes", "-days", "1",
        "-subj", "/CN=ci-only.invalid", "-keyout", str(ROOT / "test.key"), "-out", str(ROOT / "test.crt"))
    run(openssl, "pkcs12", "-export", "-passout", "pass:", "-inkey", str(ROOT / "test.key"),
        "-in", str(ROOT / "test.crt"), "-out", str(ROOT / "test.pfx"))
    certificate.write_text(base64.b64encode((ROOT / "test.pfx").read_bytes()).decode(), encoding="ascii")
    runtime = json.loads(Path("deployment/runtime.example.json").read_text(encoding="utf-8"))
    runtime["Database"]["Host"] = f"{PREFIX}-postgres"
    runtime["ForwardedHeaders"]["KnownProxies"] = ["127.0.0.1"]
    runtime["Testing"] = {"Enabled": True}  # Must not enable test identity in Production.
    (ROOT / "runtime-configuration").write_text(json.dumps(runtime), encoding="utf-8")
    for file in ("database-password", "client-certificate", "runtime-configuration"):
        if os.name != "nt":
            run("sudo", "chown", "0:1654", str(ROOT / file))
            run("sudo", "chmod", "0640", str(ROOT / file))
    for image, user in (("talkwithtiles-web:ci", "1654:1654"), ("talkwithtiles-postgres:ci", "999:1654")):
        config = json.loads(run("docker", "image", "inspect", image, capture=True))[0]["Config"]
        assert not config.get("Volumes"), f"{image} declares anonymous volumes"
        assert config["User"] == user
        assert config["Healthcheck"]["Test"][0] == "CMD"
    run("docker", "network", "create", "--internal", PREFIX)
    postgres = f"{PREFIX}-postgres"
    web = f"{PREFIX}-web"
    try:
        constrained(postgres, "talkwithtiles-postgres:ci", "999:1654", [
            (ROOT / "postgres", "/var/lib/postgresql/data", True),
            (password, "/run/secrets/database-password", False)])
        wait_healthy(postgres)
        mounts = [(ROOT / "keys", "/app/state/keys", True)] + [
            (ROOT / file, f"/run/secrets/{file}", False)
            for file in ("database-password", "client-certificate", "runtime-configuration")]
        constrained(web, "talkwithtiles-web:ci", "1654:1654", mounts)
        wait_healthy(web)
        # A committed row must survive both processes being removed and recreated.
        sql = "INSERT INTO contacts VALUES ('10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','CI restart marker',clock_timestamp())"
        run("docker", "exec", postgres, "psql", "-h", "/tmp", "-U", "talkwithtiles_bootstrap",
            "-d", "talkwithtiles", "-v", "ON_ERROR_STOP=1", "-c", sql)
        run("docker", "stop", web, postgres)
        run("docker", "rm", web, postgres)
        constrained(postgres, "talkwithtiles-postgres:ci", "999:1654", [
            (ROOT / "postgres", "/var/lib/postgresql/data", True),
            (password, "/run/secrets/database-password", False)])
        wait_healthy(postgres)
        constrained(web, "talkwithtiles-web:ci", "1654:1654", mounts)
        wait_healthy(web)
        count = run("docker", "exec", postgres, "psql", "-h", "/tmp", "-U", "talkwithtiles_bootstrap",
                    "-d", "talkwithtiles", "-Atc", "SELECT count(*) FROM contacts", capture=True)
        assert count == "1"
        run("docker", "stop", postgres)
        result = subprocess.run(["docker", "exec", web, "dotnet", "MX.TalkWithTiles.Web.dll", "--healthcheck"], check=False)
        assert result.returncode != 0, "Readiness incorrectly survived database loss"
        print("Constrained images, non-root initialization, persisted recreation and database readiness passed.")
    finally:
        for container in (web, postgres):
            subprocess.run(["docker", "rm", "--force", container], check=False, stdout=subprocess.DEVNULL)
        subprocess.run(["docker", "network", "rm", PREFIX], check=False, stdout=subprocess.DEVNULL)
        if os.name != "nt":
            run("sudo", "chown", "-R", f"{os.getuid()}:{os.getgid()}", str(ROOT))
        shutil.rmtree(ROOT)


if __name__ == "__main__":
    main()
