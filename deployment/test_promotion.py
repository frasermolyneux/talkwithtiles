import importlib.util
import json
import os
from pathlib import Path
import unittest
from unittest.mock import patch


def load(name, file):
    spec = importlib.util.spec_from_file_location(name, Path(".github/scripts") / file)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


promotion = load("tiles_promotion", "promote-workload-images.py")
publication = load("tiles_publication", "publish-images.py")


class PromotionTests(unittest.TestCase):
    def test_promotion_cannot_report_success_without_trusted_exact_head_contract(self):
        head = "b" * 40
        payload = {
            "source_sha": "a" * 40,
            "images": {
                name: f"acr7ca0fd3326df.azurecr.io/workloads/talkwithtiles/{image}@sha256:{'1' * 64}"
                for name, image in (("talkwithtiles-web", "web"), ("talkwithtiles-postgres", "postgres"))
            },
        }
        manifest = {"services": [{"name": name, "image": image}
                                 for name, image in payload["images"].items()]}

        def commands(*args, **_):
            if args[:2] == ("git", "show"):
                return json.dumps(manifest)
            if args[:2] == ("git", "diff"):
                return "workload.json"
            return ""

        def github(*args):
            if args[:2] == ("pr", "list"):
                return [{"number": 1, "headRefOid": head, "baseRefName": "main"}]
            return {"id": "test-node"}

        with patch.dict(os.environ, {"GITHUB_REPOSITORY": "example/repository"}), \
                patch.object(Path, "read_text", return_value=json.dumps(payload)), \
                patch.object(promotion, "superseded", return_value=False), \
                patch.object(promotion, "run", side_effect=commands), \
                patch.object(promotion, "gh", side_effect=github), \
                patch.object(promotion, "wait_workflow"), patch.object(promotion, "wait_review"), \
                patch.object(promotion, "wait_contract", side_effect=RuntimeError("contract failure")) as contract, \
                patch("builtins.print") as output:
            with self.assertRaisesRegex(RuntimeError, "contract failure"):
                promotion.main()
            contract.assert_called_once_with(head, "example/repository")
            output.assert_not_called()

    def test_placeholder_digest_is_rejected_before_repository_mutation(self):
        payload = {
            "source_sha": "a" * 40,
            "images": {
                name: f"acr7ca0fd3326df.azurecr.io/workloads/talkwithtiles/{image}@sha256:{'0' * 64}"
                for name, image in (("talkwithtiles-web", "web"), ("talkwithtiles-postgres", "postgres"))
            },
        }
        with patch.object(Path, "read_text", return_value=json.dumps(payload)), patch.object(promotion, "run") as run:
            with self.assertRaisesRegex(RuntimeError, "placeholder digest"):
                promotion.main()
            run.assert_not_called()

    def test_exact_head_review_with_unresolved_thread_cannot_promote(self):
        head = "a" * 40

        def gh(*args):
            if "graphql" in args:
                return {"data": {"repository": {"pullRequest": {"reviewThreads": {
                    "nodes": [{"isResolved": False}], "pageInfo": {"hasNextPage": False}
                }}}}}
            return [{"commit_id": head, "user": {"login": "copilot-pull-request-reviewer[bot]"}, "state": "COMMENTED"}]

        with patch.object(promotion, "gh", side_effect=gh):
            with self.assertRaisesRegex(RuntimeError, "Unresolved review threads"):
                promotion.wait_review("example/repository", 1, head)

    def test_publication_gate_failure_prevents_image_build_and_registry_access(self):
        head = "a" * 40
        with patch.dict(os.environ, {"SOURCE_SHA": head, "GITHUB_REPOSITORY": "example/repository"}), \
                patch.object(publication, "run") as commands, \
                patch.object(publication, "wait_quality", side_effect=RuntimeError("Exact-head Code Quality did not succeed.")):
            with self.assertRaisesRegex(RuntimeError, "Exact-head Code Quality did not succeed"):
                publication.main()
            commands.assert_called_once_with("git", "merge-base", "--is-ancestor", head, "origin/main")

    def test_new_source_inputs_supersede_old_image_publication(self):
        def run(*args, **kwargs):
            return "src/MX.TalkWithTiles.Web/Program.cs" if args[:2] == ("git", "log") else ""

        with patch.object(promotion, "run", side_effect=run):
            self.assertTrue(promotion.superseded("a" * 40))


if __name__ == "__main__":
    unittest.main()
