"""Fail closed unless reviewed workflow producers validated this exact source."""

import json
import os
import re
import subprocess
import time


ACTIONS_APP_ID = 15368
CONTRACT_WORKFLOW = ".github/workflows/validate-workload-contract.yml"


def gh(path):
    return json.loads(subprocess.check_output(["gh", "api", path], text=True))


def trusted_contract_producer(check, sha, repository, default_branch):
    prefix = f"https://github.com/{repository}/actions/runs/"
    match = re.fullmatch(re.escape(prefix) + r"([1-9][0-9]*)", check.get("details_url", ""))
    if not match:
        raise RuntimeError("Untrusted workload contract producer URL.")
    run = gh(f"repos/{repository}/actions/runs/{match[1]}")
    if (run.get("event") != "workflow_run" or run.get("path") != CONTRACT_WORKFLOW
            or run.get("head_branch") != default_branch
            or run.get("repository", {}).get("full_name") != repository):
        raise RuntimeError("Untrusted workload contract workflow.")
    if run["status"] != "completed":
        return False
    if run["conclusion"] != "success":
        raise RuntimeError("Trusted workload contract producer did not succeed.")
    jobs = gh(f"repos/{repository}/actions/runs/{match[1]}/jobs?filter=latest&per_page=100")["jobs"]
    # The reviewed producer binds its job name to workflow_run.head_sha.
    # A same-App forged check cannot borrow a producer run for another SHA.
    exact = [job for job in jobs if job["name"] == f"workload-contract ({sha})"]
    if not exact or max(exact, key=lambda job: job["id"])["conclusion"] != "success":
        raise RuntimeError("Trusted workload contract job does not validate this SHA.")
    return True


def latest_quality(sha, repository, default_branch):
    runs = gh(f"repos/{repository}/actions/workflows/codequality.yml/runs?event=push&head_sha={sha}&per_page=100")["workflow_runs"]
    exact = [run for run in runs if run["head_sha"] == sha
             and run.get("event") == "push" and run.get("head_branch") == default_branch
             and run.get("path") == ".github/workflows/codequality.yml"
             and run.get("repository", {}).get("full_name") == repository]
    return max(exact, key=lambda run: run["id"]) if exact else None


def latest_contract(sha, repository):
    checks = []
    for page in range(1, 21):
        batch = gh(f"repos/{repository}/commits/{sha}/check-runs?check_name=workload-contract&filter=all&per_page=100&page={page}")["check_runs"]
        checks.extend(batch)
        if len(batch) < 100:
            break
    else:
        raise RuntimeError("Too many workload contract results.")
    exact = [check for check in checks if check.get("name") == "workload-contract"
             and check.get("head_sha") == sha and check.get("app", {}).get("id") == ACTIONS_APP_ID
             and check.get("app", {}).get("slug") == "github-actions"]
    return max(exact, key=lambda check: check["id"]) if exact else None


def wait_for_success(fetch, label, attest=None):
    for _ in range(120):
        latest = fetch()
        if latest and latest["status"] == "completed":
            if latest["conclusion"] != "success":
                raise RuntimeError(f"{label} did not succeed.")
            if attest is None or attest(latest):
                return
        time.sleep(5)
    raise RuntimeError(f"Timed out waiting for {label}.")


def wait_for_quality(sha, repository):
    if not re.fullmatch(r"[0-9a-f]{40}", sha):
        raise RuntimeError("Expected a complete source SHA.")
    default_branch = gh(f"repos/{repository}")["default_branch"]
    wait_for_success(lambda: latest_quality(sha, repository, default_branch), "Exact-head Code Quality")
    wait_for_success(lambda: latest_contract(sha, repository), "Exact-head trusted workload contract",
                     lambda check: trusted_contract_producer(check, sha, repository, default_branch))


if __name__ == "__main__":
    wait_for_quality(os.environ["SOURCE_SHA"], os.environ["GITHUB_REPOSITORY"])
