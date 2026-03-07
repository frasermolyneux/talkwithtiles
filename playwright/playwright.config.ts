import { defineConfig, devices } from '@playwright/test';

const baseURL = 'https://localhost:5001';

export default defineConfig({
  globalSetup: require.resolve('./global-setup'),
  testDir: './tests',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: process.env.CI
    ? [['html', { open: 'never' }], ['github'], ['junit', { outputFile: 'test-results/junit-report.xml' }]]
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

  webServer: {
    command: `dotnet run${process.env.CI ? ' --no-build' : ''} --project ../src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj --launch-profile MX.TalkWithTiles.Web`,
    url: `${baseURL}/api/health`,
    reuseExistingServer: !process.env.CI,
    ignoreHTTPSErrors: true,
    timeout: 30_000,
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
      'Testing__Enabled': 'true',
      'AppData__StorageConnectionString': 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;',
    },
  },
});
