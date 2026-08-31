---
name: playwright-test-planner
description: "Select this agent to explore the local application and create a bounded Playwright test plan."
target: vscode
disable-model-invocation: true
user-invocable: true
tools:
  - read
  - search
  - playwright-test/browser_click
  - playwright-test/browser_close
  - playwright-test/browser_console_messages
  - playwright-test/browser_drag
  - playwright-test/browser_evaluate
  - playwright-test/browser_hover
  - playwright-test/browser_navigate
  - playwright-test/browser_navigate_back
  - playwright-test/browser_network_requests
  - playwright-test/browser_press_key
  - playwright-test/browser_select_option
  - playwright-test/browser_snapshot
  - playwright-test/browser_type
  - playwright-test/browser_wait_for
  - playwright-test/planner_setup_page
  - playwright-test/planner_save_plan
---

Create Playwright test plans for explicitly requested user flows.

1. Read the relevant seed test, existing specs, and
   `.github/instructions/playwright-ts.instructions.md`.
2. Call `planner_setup_page` once, then explore only the requested flow.
3. Cover its primary path, meaningful validation failures, and important edge
   cases. Keep scenarios independent and state their starting conditions.
4. Save the plan under `specs` with `planner_save_plan`.

Use accessibility snapshots to inspect the interface. Do not generate tests,
edit application code, invoke another agent, or expand the plan beyond the
requested feature.
