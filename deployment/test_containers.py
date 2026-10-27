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


def constrained(name, image, user, mounts, extra_args=()):
    command = ["docker", "run", "--detach", "--name", name,
               "--network", PREFIX, "--user", user, "--read-only",
               "--cap-drop=ALL", "--security-opt=no-new-privileges:true",
               "--tmpfs", "/tmp:rw,noexec,nosuid,size=64m",
               "--pids-limit", "128", "--memory", "512m", "--cpus", "1"]
    command.extend(extra_args)
    for source, target, writable in mounts:
        mount_type = "bind" if isinstance(source, Path) else "volume"
        command.extend(["--mount", f"type={mount_type},src={source},dst={target}" + ("" if writable else ",readonly")])
    command.append(image)
    run(*command)


def main():
    ROOT.mkdir(parents=True, exist_ok=True)
    state_mounts = {}
    for directory, uid in (("postgres", 999), ("keys", 1654)):
        target = ROOT / directory
        target.mkdir(exist_ok=True)
        if os.name != "nt":
            run("sudo", "chown", f"{uid}:1654", str(target))
            run("sudo", "chmod", "0770", str(target))
            state_mounts[directory] = target
        else:
            # Desktop's NTFS bind adapter cannot implement PostgreSQL chmod(0700).
            volume = f"{PREFIX}-{directory}"
            run("docker", "volume", "create", volume)
            run("docker", "run", "--rm", "--user", "0:0", "--entrypoint", "/bin/bash",
                "--mount", f"type=volume,src={volume},dst=/state", "talkwithtiles-postgres:ci",
                "-c", f"chown {uid}:1654 /state && chmod 0770 /state")
            state_mounts[directory] = volume
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
    bootstrap_probe = ROOT / "psql"
    bootstrap_probe.write_text(
        "#!/bin/bash\nset -euo pipefail\n"
        "touch /tmp/ci-bootstrap-waiting\n"
        "while [[ ! -e /tmp/ci-bootstrap-release ]]; do sleep 0.1; done\n"
        'exec /usr/lib/postgresql/18/bin/psql "$@"\n', encoding="utf-8", newline="\n")
    if os.name != "nt":
        bootstrap_probe.chmod(0o555)
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
            (state_mounts["postgres"], "/var/lib/postgresql/data", True),
            (password, "/run/secrets/database-password", False),
            (bootstrap_probe, "/ci/psql", False)],
            extra_args=["--env", "PATH=/ci:/usr/lib/postgresql/18/bin:/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"])
        # Pause role/database creation while the temporary server is running.
        for _ in range(90):
            probe = subprocess.run(["docker", "exec", postgres, "test", "-f", "/tmp/ci-bootstrap-waiting"], check=False)
            if probe.returncode == 0:
                break
            time.sleep(1)
        else:
            raise RuntimeError("PostgreSQL did not reach the bootstrap probe")
        run("docker", "exec", postgres, "pg_isready", "--host=/tmp",
            "--username=talkwithtiles_bootstrap", "--dbname=postgres")
        probe = subprocess.run(["docker", "exec", postgres, "pg_isready", "--host=127.0.0.1",
                                "--username=talkwithtiles", "--dbname=talkwithtiles"], check=False)
        assert probe.returncode != 0, "Bootstrap TCP incorrectly passed the production healthcheck"
        run("docker", "exec", postgres, "touch", "/tmp/ci-bootstrap-release")
        wait_healthy(postgres)
        mounts = [(state_mounts["keys"], "/app/state/keys", True)] + [
            (ROOT / file, f"/run/secrets/{file}", False)
            for file in ("database-password", "client-certificate", "runtime-configuration")]
        constrained(web, "talkwithtiles-web:ci", "1654:1654", mounts)
        wait_healthy(web)
        response = run("docker", "exec", postgres, "bash", "-c",
                       'exec 3<>/dev/tcp/$1/8080; printf "%s" "$2" >&3; cat <&3', "probe", web,
                       "GET /game%20path?next=%2Fplay%3Fx%3D1 HTTP/1.1\r\n"
                       "Host: www.talkwithtiles.com\r\nConnection: close\r\n\r\n", capture=True)
        assert response.startswith("HTTP/1.1 308"), response
        assert "Location: https://talkwithtiles.com/game%20path?next=%2Fplay%3Fx%3D1" in response, response
        # A committed row must survive both processes being removed and recreated.
        sql = "INSERT INTO contacts VALUES ('10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','CI restart marker',clock_timestamp())"
        run("docker", "exec", postgres, "psql", "-h", "/tmp", "-U", "talkwithtiles_bootstrap",
            "-d", "talkwithtiles", "-v", "ON_ERROR_STOP=1", "-c", sql)
        run("docker", "stop", web, postgres)
        run("docker", "rm", web, postgres)
        constrained(postgres, "talkwithtiles-postgres:ci", "999:1654", [
            (state_mounts["postgres"], "/var/lib/postgresql/data", True),
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
        else:
            for volume in state_mounts.values():
                subprocess.run(["docker", "volume", "rm", volume], check=False, stdout=subprocess.DEVNULL)
        shutil.rmtree(ROOT)


if __name__ == "__main__":
    main()
