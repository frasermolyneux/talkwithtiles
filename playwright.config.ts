import { defineConfig, devices } from '@playwright/test';

const baseURL = 'https://localhost:5001';

export default defineConfig({
  globalSetup: require.resolve('./global-setup'),
  testDir: './e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: process.env.CI
    ? [['html', { open: 'never' }], ['github']]
    : [['html', { open: 'on-failure' }]],

  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 7'] },
    },
  ],

  webServer: [
    {
      command: `dotnet run${process.env.CI ? ' --no-build' : ''} --project src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj --launch-profile MX.TalkWithTiles.Web`,
      url: `${baseURL}/api/health/ready`,
      reuseExistingServer: !process.env.CI,
      ignoreHTTPSErrors: true,
      timeout: 30_000,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        'Testing__Enabled': 'true',
        'Database__Host': process.env.TEST_POSTGRES_HOST ?? '127.0.0.1',
        'Database__Port': process.env.TEST_POSTGRES_PORT ?? '5432',
        'Database__PasswordFile': process.env.TEST_POSTGRES_PASSWORD_FILE ?? `${process.cwd()}/.local/database-password`,
        'DataProtection__KeyPath': `${process.cwd()}/.local/keys`,
      },
    },
  ],
});
