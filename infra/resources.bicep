// Resource-group-scoped: everything Receply actually runs on. Invoked by main.bicep.
@description('Azure region for all resources.')
param location string

@description('azd environment name, used in the resource-token hash for globally-unique names.')
param environmentName string

param tags object

@secure()
param whatsAppAccessToken string
@secure()
param whatsAppAppSecret string
@secure()
param whatsAppWebhookVerifyToken string
param whatsAppDefaultPhoneNumberId string
@secure()
param anthropicApiKey string
@secure()
param jwtSigningKey string
param postgresAdminLogin string
@secure()
param postgresAdminPassword string

// Short unique suffix so globally-unique names (ACR, Postgres server) don't collide across azd envs.
var resourceToken = toLower(uniqueString(resourceGroup().id, environmentName))
var abbrs = {
  containerRegistry: 'acr'
  appServicePlan: 'plan'
  appServiceApi: 'app-api'
  appServiceWorkers: 'app-workers'
  postgres: 'psql'
  logAnalytics: 'log'
  appInsights: 'appi'
}

// --- Observability -----------------------------------------------------------------------
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${abbrs.logAnalytics}-${resourceToken}'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${abbrs.appInsights}-${resourceToken}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// --- Container registry -------------------------------------------------------------------
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: '${abbrs.containerRegistry}${resourceToken}'
  location: location
  tags: tags
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: false // App Services pull via managed identity instead, see AcrPull role assignments below
  }
}

// --- PostgreSQL ----------------------------------------------------------------------------
resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: '${abbrs.postgres}-${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    storage: { storageSizeGB: 32 }
    backup: { backupRetentionDays: 7, geoRedundantBackup: 'Disabled' }
    highAvailability: { mode: 'Disabled' }
  }
}

resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: 'receply'
}

// Lets Azure-hosted resources (our App Services) reach the server without listing every outbound IP.
resource postgresFirewallAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: postgres
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

var postgresConnectionString = 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=receply;Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require'

// --- App Service plan + apps ----------------------------------------------------------------
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${abbrs.appServicePlan}-${resourceToken}'
  location: location
  tags: tags
  sku: { name: 'B1', tier: 'Basic' }
  kind: 'linux'
  properties: { reserved: true }
}

var commonAppSettings = [
  { name: 'WEBSITES_PORT', value: '8080' }
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'ConnectionStrings__Postgres', value: postgresConnectionString }
  { name: 'ConnectionStrings__Redis', value: '' } // unset until something actually uses Redis
  { name: 'WhatsApp__AccessToken', value: whatsAppAccessToken }
  { name: 'WhatsApp__AppSecret', value: whatsAppAppSecret }
  { name: 'WhatsApp__WebhookVerifyToken', value: whatsAppWebhookVerifyToken }
  { name: 'WhatsApp__DefaultPhoneNumberId', value: whatsAppDefaultPhoneNumberId }
  { name: 'Anthropic__ApiKey', value: anthropicApiKey }
  { name: 'Jwt__SigningKey', value: jwtSigningKey }
  { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
]

resource apiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${abbrs.appServiceApi}-${resourceToken}'
  location: location
  tags: union(tags, { 'azd-service-name': 'api' })
  kind: 'app,linux,container'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|mcr.microsoft.com/appsvc/staticsite:latest' // placeholder - `azd deploy` swaps in the real image
      acrUseManagedIdentityCreds: true
      appSettings: commonAppSettings
    }
  }
}

resource workersApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${abbrs.appServiceWorkers}-${resourceToken}'
  location: location
  tags: union(tags, { 'azd-service-name': 'workers' })
  kind: 'app,linux,container'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|mcr.microsoft.com/appsvc/staticsite:latest' // placeholder - `azd deploy` swaps in the real image
      acrUseManagedIdentityCreds: true
      appSettings: commonAppSettings
    }
  }
}

// Lets each App Service pull its own image from ACR using its managed identity - no stored registry credentials.
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource apiAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, apiApp.id, acrPullRoleId)
  scope: containerRegistry
  properties: {
    roleDefinitionId: acrPullRoleId
    principalId: apiApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource workersAcrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, workersApp.id, acrPullRoleId)
  scope: containerRegistry
  properties: {
    roleDefinitionId: acrPullRoleId
    principalId: workersApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output containerRegistryLoginServer string = containerRegistry.properties.loginServer
output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output workersUrl string = 'https://${workersApp.properties.defaultHostName}'
