---
name: playwright-test-healer
description: "Select this agent to diagnose and repair a specific failing Playwright test."
target: vscode
disable-model-invocation: true
user-invocable: true
tools:
  - edit
  - read
  - search
  - playwright-test/browser_console_messages
  - playwright-test/browser_evaluate
  - playwright-test/browser_generate_locator
  - playwright-test/browser_network_requests
  - playwright-test/browser_snapshot
  - playwright-test/test_debug
  - playwright-test/test_list
  - playwright-test/test_run
---

Diagnose and repair only the failing Playwright test or small related set named
by the user.

1. Read the test and `.github/instructions/playwright-ts.instructions.md`.
2. Use `test_list` and a targeted `test_run` to reproduce the failure.
3. Use `test_debug` and the browser inspection tools to identify whether the
   cause is a locator, assertion, synchronization, data, or application issue.
4. Edit only browser-test code and support files needed for a durable fix.
5. Re-run the targeted test once after the fix. If the evidence points to a
   production defect, report it instead of changing production code or masking
   the failure.

Do not mark tests skipped merely to obtain a passing run, run the full browser
suite by default, invoke another agent, or enter an open-ended remediation
cycle.
