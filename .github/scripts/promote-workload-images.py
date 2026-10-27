"""Digest-only promotion PRs; exact-head quality and Copilot gate, never deploy/merge."""

import json
import os
from pathlib import Path
import re
import runpy
import subprocess
import time


wait_contract = runpy.run_path(str(Path(__file__).with_name("require-quality.py")))["wait_for_contract"]


def run(*args, capture=False):
    result = subprocess.run(args, check=True, text=True,
                            stdout=subprocess.PIPE if capture else None)
    return result.stdout.strip() if capture else ""


def gh(*args):
    return json.loads(run("gh", *args, capture=True))


def superseded(source):
    run("git", "fetch", "origin", "main:refs/remotes/origin/main")
    run("git", "merge-base", "--is-ancestor", source, "origin/main")
    changed = run("git", "log", "--first-parent", "--diff-merges=first-parent", "--format=", "--name-only",
                  f"{source}..origin/main", capture=True).splitlines()
    pattern = Path(".github/image-input-pattern.txt").read_text(encoding="utf-8").strip()
    if any(re.search(pattern, name) for name in changed):
        return True
    for commit in run("git", "rev-list", "--first-parent", f"{source}..origin/main", "--", "workload.json", capture=True).splitlines():
        old = json.loads(run("git", "show", f"{commit}^1:workload.json", capture=True))
        new = json.loads(run("git", "show", f"{commit}:workload.json", capture=True))
        if {s["name"]: s["image"] for s in old["services"]} != {s["name"]: s["image"] for s in new["services"]}:
            return True
    return False


def wait_workflow(workflow, branch, head, event):
    for _ in range(120):
        runs = gh("run", "list", "--workflow", workflow, "--branch", branch, "--event", event,
                  "--limit", "30", "--json", "databaseId,headSha,status,conclusion")
        matching = [r for r in runs if r["headSha"] == head]
        if matching:
            latest = max(matching, key=lambda r: r["databaseId"])
            if latest["status"] == "completed":
                if latest["conclusion"] != "success":
                    raise RuntimeError(f"{workflow} failed on exact promotion head.")
                return
        time.sleep(15)
    raise RuntimeError(f"Timed out waiting for {workflow}; promotion PR remains open.")


def wait_review(repository, number, head):
    owner, name = repository.split("/")
    query = """query($owner:String!,$name:String!,$number:Int!,$after:String){
      repository(owner:$owner,name:$name){pullRequest(number:$number){
        reviewThreads(first:100,after:$after){nodes{isResolved}pageInfo{hasNextPage endCursor}}
      }}}"""
    for _ in range(120):
        reviews = []
        page = 1
        while True:
            batch = gh("api", f"repos/{repository}/pulls/{number}/reviews?per_page=100&page={page}")
            reviews.extend(batch)
            if len(batch) < 100:
                break
            page += 1
        matching = [r for r in reviews if r["commit_id"] == head and r["user"]["login"] in
                    {"copilot-pull-request-reviewer", "copilot-pull-request-reviewer[bot]"}]
        if matching:
            if matching[-1]["state"] not in {"APPROVED", "COMMENTED"}:
                raise RuntimeError("Exact-head Copilot review is not mergeable.")
            after = None
            while True:
                args = ["api", "graphql", "-f", f"query={query}", "-f", f"owner={owner}", "-f", f"name={name}", "-F", f"number={number}"]
                if after:
                    args += ["-f", f"after={after}"]
                connection = gh(*args)["data"]["repository"]["pullRequest"]["reviewThreads"]
                if any(not thread["isResolved"] for thread in connection["nodes"]):
                    raise RuntimeError("Unresolved review threads; promotion PR remains open.")
                if not connection["pageInfo"]["hasNextPage"]:
                    return
                after = connection["pageInfo"]["endCursor"]
        time.sleep(10)
    raise RuntimeError("Timed out waiting for exact-head Copilot review; PR remains open.")


def main():
    payload = json.loads(Path("promotion/payload.json").read_text(encoding="utf-8"))
    source = payload["source_sha"]
    if not re.fullmatch(r"[0-9a-f]{40}", source):
        raise RuntimeError("Invalid source revision.")
    expected = {"talkwithtiles-web": "web", "talkwithtiles-postgres": "postgres"}
    if set(payload["images"]) != set(expected):
        raise RuntimeError("Promotion must contain exactly both workload services.")
    for service, suffix in expected.items():
        if not re.fullmatch(rf"acr7ca0fd3326df[.]azurecr[.]io/workloads/talkwithtiles/{suffix}@sha256:(?!0{{64}})[0-9a-f]{{64}}", payload["images"][service]):
            raise RuntimeError("Unexpected registry, repository or placeholder digest.")
    if superseded(source):
        print("A newer image-input or digest revision supersedes this publication.")
        return
    document = json.loads(run("git", "show", "origin/main:workload.json", capture=True))
    for service in document["services"]:
        service["image"] = payload["images"][service["name"]]
    repository = os.environ["GITHUB_REPOSITORY"]
    branch = f"automation/promote-{source[:12]}"
    existing = gh("pr", "list", "--head", branch, "--state", "open", "--json", "number,headRefOid,baseRefName")
    if existing:
        if existing[0]["baseRefName"] != "main":
            raise RuntimeError("Promotion base must be main.")
        number, head = existing[0]["number"], existing[0]["headRefOid"]
        run("git", "fetch", "origin", branch)
        files = run("git", "diff", "--name-only", "origin/main...FETCH_HEAD", capture=True).splitlines()
        if files != ["workload.json"] or json.loads(run("git", "show", "FETCH_HEAD:workload.json", capture=True)) != document:
            raise RuntimeError("Existing promotion contains unexpected changes.")
    else:
        run("git", "switch", "-c", branch, "origin/main")
        Path("workload.json").write_text(json.dumps(document, indent=2) + "\n", encoding="utf-8")
        run("git", "config", "user.name", "github-actions[bot]")
        run("git", "config", "user.email", "41898282+github-actions[bot]@users.noreply.github.com")
        run("git", "add", "workload.json")
        run("git", "commit", "-m", f"chore: promote immutable images for {source[:12]}")
        head = run("git", "rev-parse", "HEAD", capture=True)
        run("git", "push", "origin", branch)
        run("gh", "pr", "create", "--base", "main", "--head", branch,
            "--title", f"Promote Talk With Tiles images for {source[:12]}",
            "--body", f"Digest-only promotion for validated source `{source}`. Requires exact-head runtime/quality checks and Copilot review. No automatic merge or host activation.")
        number = gh("pr", "view", branch, "--json", "number")["number"]
    run("gh", "workflow", "run", "runtime-quality.yml", "--ref", branch)
    wait_workflow("runtime-quality.yml", branch, head, "workflow_dispatch")
    wait_workflow("pr-verify.yml", branch, head, "pull_request")
    wait_workflow("codequality.yml", branch, head, "pull_request")
    node_id = gh("pr", "view", str(number), "--json", "id")["id"]
    mutation = """mutation($id:ID!){requestReviewsByLogin(input:{pullRequestId:$id,botLogins:["copilot-pull-request-reviewer"],union:true}){pullRequest{id}}}"""
    gh("api", "graphql", "-f", f"query={mutation}", "-f", f"id={node_id}")
    wait_review(repository, number, head)
    wait_contract(head, repository)
    if superseded(source) or gh("pr", "view", str(number), "--json", "headRefOid,baseRefName") != {"baseRefName": "main", "headRefOid": head}:
        raise RuntimeError("Promotion was superseded or its exact head/base changed.")
    print(f"Promotion PR #{number} passed exact-head gates; remains open for reviewed merge.")


if __name__ == "__main__":
    main()
