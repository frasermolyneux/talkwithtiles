# User profile test plan

The authenticated profile at `/profile` displays the user's name, email and
user ID. Start each scenario with a signed-in test user; see
[`e2e/seed.spec.ts`](../e2e/seed.spec.ts) and the existing profile tests.

## Access and information

1. Open the profile from the user menu. Check the URL, heading and page title.
2. Verify the name, email and user ID match the signed-in user.
3. Verify the user's initial is visible and the fields are not editable.

## Navigation

1. Navigate from the profile to Home and Feedback; verify each destination.
2. Use browser back and verify the previous page.
3. Open `/profile` directly and verify the signed-in profile appears.

The [profile Playwright tests](../e2e/) are the source of truth for automated
coverage. Additional scenarios to consider include unauthenticated access,
responsive layout, unusual user names and long email addresses; these are not
assertions about existing coverage.
See [local test setup](../docs/container-runtime.md#disposable-localci-database).
