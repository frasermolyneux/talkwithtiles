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
    "run_started_at": "2026-10-27T18:00:00Z", "run_attempt": 1,
    "id": 10, "head_sha": SHA, "event": "push", "head_branch": "main",
    "path": ".github/workflows/codequality.yml",
    "repository": {"full_name": REPOSITORY}, "status": "completed", "conclusion": "success",
}
CHECK = {
    "id": 20, "head_sha": SHA, "name": "workload-contract",
    "app": {"id": 15368, "slug": "github-actions"},
    "status": "completed", "conclusion": "success",
    "details_url": f"https://github.com/{REPOSITORY}/actions/runs/99",
    "external_id": "trusted-workload-contract:99",
}
PRODUCER = {
    "run_started_at": "2026-10-27T18:00:00Z", "run_attempt": 1,
    "id": 99,
    "display_title": f"Validate workload contract {SHA}",
    "event": "workflow_run", "path": ".github/workflows/validate-workload-contract.yml",
    "head_branch": "main", "repository": {"full_name": REPOSITORY},
    "status": "completed", "conclusion": "success",
}
JOB = {"id": 30, "name": f"workload-contract ({SHA})", "status": "completed", "conclusion": "success"}


class PublicationGateTests(unittest.TestCase):
    def run_gate(self, qualities=None, checks=None, producer=None, jobs=None, pending_producer=False,
                 newer_producer=None, producer_pages=None, quality_pages=None,
                 quality_after_contract=None, partitioned_producers=None):
        attempts = 0
        quality_reads = 0
        partition_reads = 0

        def output(command, **_):
            nonlocal attempts, quality_reads, partition_reads
            path = command[-1]
            if path == f"repos/{REPOSITORY}":
                result = {"default_branch": "main", "created_at": "2026-10-27T00:00:00Z"}
            elif "/actions/workflows/codequality.yml/runs?" in path:
                quality_reads += 1
                result = {"workflow_runs": [QUALITY] if qualities is None else qualities}
                if quality_after_contract is not None and quality_reads > 1:
                    result = {"workflow_runs": quality_after_contract}
                if quality_pages is not None:
                    result = {"workflow_runs": quality_pages[int(path.rsplit("page=", 1)[1]) - 1]}
            elif "/check-runs?" in path:
                result = {"check_runs": [CHECK] if checks is None else checks}
            elif "/actions/workflows/validate-workload-contract.yml/runs?" in path:
                attempts += 1
                result = PRODUCER if producer is None else producer
                if pending_producer and attempts == 1:
                    result = {**result, "status": "in_progress", "conclusion": None}
                result = {"workflow_runs": [result] + ([newer_producer] if newer_producer else [])}
                if producer_pages is not None:
                    result = {"workflow_runs": producer_pages[int(path.rsplit("page=", 1)[1]) - 1]}
                if partitioned_producers is not None:
                    result = {"workflow_runs": [], "total_count": 1000}
                    if "&created=" in path:
                        result = {"workflow_runs": partitioned_producers[partition_reads]}
                        partition_reads += 1
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

    def test_quality_failure_during_contract_wait_is_rechecked_before_publication(self):
        with self.assertRaisesRegex(RuntimeError, "Code Quality did not succeed"):
            self.run_gate(quality_after_contract=[{**QUALITY, "conclusion": "failure", "run_attempt": 2}])

    def test_capped_history_is_partitioned_without_losing_older_failed_reruns(self):
        failed = {**PRODUCER, "run_started_at": "2026-10-27T19:00:00Z",
                  "run_attempt": 2, "conclusion": "failure"}
        with self.assertRaisesRegex(RuntimeError, "producer did not succeed"):
            self.run_gate(partitioned_producers=[[failed], [{**PRODUCER, "id": 100}]])
        self.assertEqual(0, self.run_gate(partitioned_producers=[
            [{**PRODUCER, "display_title": "Another source"}], [PRODUCER]]))

    def test_normalized_display_url_cannot_break_or_replace_explicit_producer_binding(self):
        normalized = {**CHECK, "details_url": f"https://github.com/{REPOSITORY}/runs/20"}
        self.assertEqual(0, self.run_gate(checks=[normalized]))
        for binding in ("", "trusted-workload-contract:100", "untrusted:99"):
            with self.subTest(binding=binding), self.assertRaisesRegex(RuntimeError, "Untrusted"):
                self.run_gate(checks=[{**normalized, "external_id": binding}])

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
            {"checks": [{**CHECK, "external_id": "untrusted:99"}]},
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

    def test_lower_id_rerun_failure_cannot_be_hidden_by_newer_id_success(self):
        for field in ("contract", "quality"):
            failed = {**(PRODUCER if field == "contract" else QUALITY),
                      "run_started_at": "2026-10-27T19:00:00Z", "run_attempt": 2, "conclusion": "failure"}
            with self.subTest(field=field), self.assertRaisesRegex(RuntimeError, "did not succeed"):
                if field == "contract":
                    self.run_gate(producer=failed, newer_producer={**PRODUCER, "id": 100},
                                  checks=[{**CHECK, "details_url": CHECK["details_url"].replace("/99", "/100")}])
                else:
                    self.run_gate(qualities=[failed, {**QUALITY, "id": 11}])

    def test_latest_failed_rerun_on_later_page_rejects_replayed_success(self):
        first = [{**PRODUCER, "id": 100}] + [
            {**PRODUCER, "id": number, "display_title": "Another source"} for number in range(101, 200)
        ]
        failed = {**PRODUCER, "run_started_at": "2026-10-27T19:00:00Z",
                  "run_attempt": 2, "conclusion": "failure"}
        with self.assertRaisesRegex(RuntimeError, "producer did not succeed"):
            self.run_gate(producer_pages=[first, [failed]],
                          checks=[{**CHECK, "details_url": CHECK["details_url"].replace("/99", "/100")}])

    def test_completed_check_waits_for_actual_trusted_producer_completion(self):
        self.assertEqual(1, self.run_gate(pending_producer=True))

    def test_filtered_api_limit_is_not_mistaken_for_complete_history(self):
        for field in ("quality", "contract"):
            full = [QUALITY if field == "quality" else PRODUCER] * 100
            with self.subTest(field=field), self.assertRaisesRegex(RuntimeError, "Too many"):
                self.run_gate(**{
                    "quality_pages" if field == "quality" else "producer_pages": [full] * 10 + [[]]
                })


if __name__ == "__main__":
    unittest.main()
