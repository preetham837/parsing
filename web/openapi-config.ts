import type { ConfigFile } from '@rtk-query/codegen-openapi'

const config: ConfigFile = {
  schemaFile: '../ApiService/Todos.Api.json',
  apiFile: './src/store/api/empty-api.ts',
  apiImport: 'emptySplitApi',
  outputFiles: {
    './src/store/api/generated/parsing.ts': {
      filterEndpoints: [/^Parse$/]
    },
  },
  exportName: 'api',
  hooks: true,
}

export default config