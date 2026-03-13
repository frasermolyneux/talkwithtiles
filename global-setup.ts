import { TableClient, AzureNamedKeyCredential } from '@azure/data-tables';

const account = 'devstoreaccount1';
const accountKey =
  'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==';
const tableEndpoint = 'http://127.0.0.1:10002/devstoreaccount1';

const tableNames = ['GameState', 'GameStateIndex', 'GameStateBag', 'GameStateBoard'];

async function clearTable(tableName: string): Promise<void> {
  const credential = new AzureNamedKeyCredential(account, accountKey);
  const client = new TableClient(tableEndpoint, tableName, credential, {
    allowInsecureConnection: true,
  });

  try {
    const entities = client.listEntities();
    for await (const entity of entities) {
      await client.deleteEntity(entity.partitionKey!, entity.rowKey!);
    }
    console.log(`  Cleared table: ${tableName}`);
  } catch (error: unknown) {
    const err = error as { statusCode?: number };
    if (err.statusCode === 404) {
      console.log(`  Table ${tableName} does not exist (will be auto-created)`);
    } else {
      throw error;
    }
  }
}

async function globalSetup(): Promise<void> {
  console.log('Global setup: clearing Azurite tables...');
  for (const table of tableNames) {
    await clearTable(table);
  }
  console.log('Global setup complete.');
}

export default globalSetup;
