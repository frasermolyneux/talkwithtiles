import copy
import json
from pathlib import Path
import runpy
import unittest
from unittest.mock import patch


GATE = runpy.run_path(str(Path(__file__).with_name("require-quality.py")))["wait_for_quality"]
SHA = "a" * 40
REPOSITORY = "owner/repository"
QUALITY = {
    "id": 10, "head_sha": SHA, "event": "push", "head_branch": "main",
    "path": ".github/workflows/codequality.yml",
    "repository": {"full_name": REPOSITORY}, "status": "completed", "conclusion": "success",
}
CHECK = {
    "id": 20, "head_sha": SHA, "name": "workload-contract",
    "app": {"id": 15368, "slug": "github-actions"},
    "status": "completed", "conclusion": "success",
    "details_url": f"https://github.com/{REPOSITORY}/actions/runs/99",
}
PRODUCER = {
    "id": 99,
    "event": "workflow_run", "path": ".github/workflows/validate-workload-contract.yml",
    "head_branch": "main", "repository": {"full_name": REPOSITORY},
    "status": "completed", "conclusion": "success",
}
JOB = {"id": 30, "name": f"workload-contract ({SHA})", "status": "completed", "conclusion": "success"}


class PublicationGateTests(unittest.TestCase):
    def run_gate(self, qualities=None, checks=None, producer=None, jobs=None, pending_producer=False,
                 newer_producer=None):
        attempts = 0

        def output(command, **_):
            nonlocal attempts
            path = command[-1]
            if path == f"repos/{REPOSITORY}":
                result = {"default_branch": "main"}
            elif "/actions/workflows/codequality.yml/runs?" in path:
                result = {"workflow_runs": [QUALITY] if qualities is None else qualities}
            elif "/check-runs?" in path:
                result = {"check_runs": [CHECK] if checks is None else checks}
            elif "/actions/workflows/validate-workload-contract.yml/runs?" in path:
                attempts += 1
                result = PRODUCER if producer is None else producer
                if pending_producer and attempts == 1:
                    result = {**result, "status": "in_progress", "conclusion": None}
                result = {"workflow_runs": [result] + ([newer_producer] if newer_producer else [])}
            elif "/actions/runs/99/jobs?" in path or "/actions/runs/100/jobs?" in path:
                result = {"jobs": [JOB] if jobs is None else jobs}
            else:
                self.fail(f"Unexpected API request: {path}")
            return json.dumps(result)

        with patch("subprocess.check_output", side_effect=output), patch("time.sleep") as sleep:
            GATE(SHA, REPOSITORY)
        return sleep.call_count

    def test_exact_quality_and_sha_bound_trusted_contract_allow_publication(self):
        self.assertEqual(0, self.run_gate())

    def test_latest_failed_quality_or_contract_cannot_reuse_old_success(self):
        for field in ("quality", "contract"):
            with self.subTest(field=field), self.assertRaisesRegex(RuntimeError, "did not succeed"):
                if field == "quality":
                    self.run_gate(qualities=[QUALITY, {**QUALITY, "id": 11, "conclusion": "failure"}])
                else:
                    self.run_gate(checks=[CHECK, {**CHECK, "id": 21, "conclusion": "failure"}])

    def test_missing_wrong_sha_or_spoofed_app_cannot_authorize_publication(self):
        variants = [
            [],
            [{**CHECK, "head_sha": "b" * 40}],
            [{**CHECK, "name": "other-check"}],
            [{**CHECK, "app": {"id": 999, "slug": "github-actions"}}],
            [{**CHECK, "app": {"id": 15368, "slug": "other-app"}}],
        ]
        for checks in variants:
            with self.subTest(checks=checks), self.assertRaisesRegex(RuntimeError, "Timed out"):
                self.run_gate(checks=checks)
        with self.assertRaisesRegex(RuntimeError, "Code Quality"):
            self.run_gate(qualities=[{**QUALITY, "head_sha": "b" * 40}])

    def test_same_app_check_cannot_borrow_wrong_workflow_or_other_sha_producer(self):
        variants = [
            {"checks": [{**CHECK, "details_url": "https://github.com/other/repo/actions/runs/99"}]},
            {"producer": {**PRODUCER, "event": "pull_request"}},
            {"producer": {**PRODUCER, "path": ".github/workflows/untrusted.yml"}},
            {"producer": {**PRODUCER, "head_branch": "feature/untrusted"}},
            {"producer": {**PRODUCER, "conclusion": "failure"}},
            {"jobs": [{**JOB, "name": f"workload-contract ({'b' * 40})"}]},
        ]
        for variant in variants:
            with self.subTest(variant=variant), self.assertRaisesRegex(RuntimeError, "[Uu]ntrusted|Trusted workload"):
                self.run_gate(**copy.deepcopy(variant))

    def test_new_same_app_check_cannot_replay_success_before_newer_producer_failure(self):
        with self.assertRaisesRegex(RuntimeError, "producer did not succeed"):
            self.run_gate(
                checks=[{**CHECK, "id": 200}],
                newer_producer={**PRODUCER, "id": 100, "conclusion": "failure"},
            )

    def test_new_same_app_check_must_reference_the_latest_successful_producer(self):
        with self.assertRaisesRegex(RuntimeError, "replays an older producer"):
            self.run_gate(newer_producer={**PRODUCER, "id": 100})

    def test_completed_check_waits_for_actual_trusted_producer_completion(self):
        self.assertEqual(1, self.run_gate(pending_producer=True))


if __name__ == "__main__":
    unittest.main()
