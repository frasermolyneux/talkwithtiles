"""Publish two immutable, repository-scoped images after exact-revision validation."""

import json
import os
from pathlib import Path
import re
import runpy
import subprocess
from urllib.error import HTTPError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


REGISTRY = "acr7ca0fd3326df.azurecr.io"


def run(*args, capture=False, **kwargs):
    result = subprocess.run(args, check=True, text=True,
                            stdout=subprocess.PIPE if capture else None, **kwargs)
    return result.stdout.strip() if capture else ""


def wait_quality(sha):
    gate = runpy.run_path(str(Path(__file__).with_name("require-quality.py")))
    gate["wait_for_quality"](sha, os.environ["GITHUB_REPOSITORY"])


def post(path, fields):
    request = Request(f"https://{REGISTRY}/{path}", data=urlencode(fields).encode(),
                      headers={"Content-Type": "application/x-www-form-urlencoded"})
    with urlopen(request, timeout=30) as response:
        return json.load(response)


def immutable(repository, reference):
    return json.loads(run("az", "acr", "repository", "show", "--name", REGISTRY.split(".")[0],
                          "--image", f"{repository}:{reference}", "--query", "changeableAttributes",
                          "--output", "json", capture=True))


def main():
    sha = os.environ["SOURCE_SHA"]
    if not re.fullmatch(r"[0-9a-f]{40}", sha):
        raise RuntimeError("Expected a complete source SHA.")
    run("git", "merge-base", "--is-ancestor", sha, "origin/main")
    wait_quality(sha)
    if os.environ["WORKLOAD_ACR_NAME"] != REGISTRY.split(".")[0]:
        raise RuntimeError("Registry does not match the reviewed workload binding.")
    images = {}
    for service, dockerfile in (("web", "Dockerfile"), ("postgres", "deployment/postgres.Dockerfile")):
        repository = f"workloads/talkwithtiles/{service}"
        tag = f"sha-{sha}"
        image = f"{REGISTRY}/{repository}:{tag}"
        run("docker", "build", "--file", dockerfile, "--tag", image, ".")
        arm_token = run("az", "account", "get-access-token", "--resource", "https://management.azure.com/",
                        "--query", "accessToken", "--output", "tsv", capture=True)
        refresh = post("oauth2/exchange", {
            "grant_type": "access_token", "service": REGISTRY,
            "tenant": os.environ["AZURE_TENANT_ID"], "access_token": arm_token,
        })["refresh_token"]
        token = post("oauth2/token", {
            "grant_type": "refresh_token", "service": REGISTRY, "refresh_token": refresh,
            "scope": f"repository:{repository}:pull,push",
        })["access_token"]
        request = Request(f"https://{REGISTRY}/v2/{repository}/manifests/{tag}", method="HEAD", headers={
            "Authorization": f"Bearer {token}",
            "Accept": "application/vnd.oci.image.manifest.v1+json, application/vnd.docker.distribution.manifest.v2+json",
        })
        exists = True
        try:
            with urlopen(request, timeout=30) as response:
                digest = response.headers["Docker-Content-Digest"]
        except HTTPError as error:
            if error.code != 404:
                raise RuntimeError(f"Registry lookup failed with HTTP {error.code}.") from None
            exists = False
        if exists:
            attributes = immutable(repository, tag)
            if attributes["writeEnabled"] or attributes["deleteEnabled"]:
                raise RuntimeError("Refusing to reuse a mutable source tag.")
        else:
            run("docker", "login", "--username", "00000000-0000-0000-0000-000000000000",
                "--password-stdin", REGISTRY, input=refresh)
            run("docker", "push", image)
            digest = run("az", "acr", "repository", "show", "--name", REGISTRY.split(".")[0],
                         "--image", f"{repository}:{tag}", "--query", "digest", "--output", "tsv", capture=True)
        if not re.fullmatch(r"sha256:[0-9a-f]{64}", digest):
            raise RuntimeError("Registry did not return a valid image digest.")
        run("az", "acr", "repository", "update", "--name", REGISTRY.split(".")[0],
            "--image", f"{repository}:{tag}", "--write-enabled", "false", "--delete-enabled", "false", "--output", "none")
        run("az", "acr", "repository", "update", "--name", REGISTRY.split(".")[0],
            "--image", f"{repository}@{digest}", "--delete-enabled", "false", "--output", "none")
        images[f"talkwithtiles-{service}"] = f"{REGISTRY}/{repository}@{digest}"
    Path("promotion").mkdir(exist_ok=True)
    Path("promotion/payload.json").write_text(json.dumps({"source_sha": sha, "images": images}) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
