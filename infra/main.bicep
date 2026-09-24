// Subscription-scoped entrypoint for `azd provision` / `azd up`. Creates the resource group and
// hands off to resources.bicep for everything inside it. See DEPLOYMENT.md for how to run this.
targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the azd environment - used to derive resource names (e.g. "receply-prod").')
param environmentName string

@minLength(1)
@description('Azure region for all resources, e.g. eastus.')
param location string

@description('WhatsApp Cloud API permanent access token - set via azd env set WHATSAPP_ACCESS_TOKEN.')
@secure()
param whatsAppAccessToken string

@description('Meta App Secret, used to verify inbound webhook signatures - set via azd env set WHATSAPP_APP_SECRET.')
@secure()
param whatsAppAppSecret string

@description('Webhook verify token registered with Meta - set via azd env set WHATSAPP_WEBHOOK_VERIFY_TOKEN.')
@secure()
param whatsAppWebhookVerifyToken string

@description('phone_number_id the demo-tenant seeder bootstraps a ChannelAccount for. Optional.')
param whatsAppDefaultPhoneNumberId string = ''

@description('Anthropic API key for the AI reply pipeline - set via azd env set ANTHROPIC_API_KEY.')
@secure()
param anthropicApiKey string

@description('Signing key for staff-dashboard JWTs - set via azd env set JWT_SIGNING_KEY.')
@secure()
param jwtSigningKey string

@description('Administrator login for the PostgreSQL Flexible Server.')
param postgresAdminLogin string = 'receplyadmin'

@description('Administrator password for the PostgreSQL Flexible Server - set via azd env set POSTGRES_ADMIN_PASSWORD.')
@secure()
param postgresAdminPassword string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module resources 'resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    tags: tags
    whatsAppAccessToken: whatsAppAccessToken
    whatsAppAppSecret: whatsAppAppSecret
    whatsAppWebhookVerifyToken: whatsAppWebhookVerifyToken
    whatsAppDefaultPhoneNumberId: whatsAppDefaultPhoneNumberId
    anthropicApiKey: anthropicApiKey
    jwtSigningKey: jwtSigningKey
    postgresAdminLogin: postgresAdminLogin
    postgresAdminPassword: postgresAdminPassword
  }
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = resources.outputs.containerRegistryLoginServer
output API_BASE_URL string = resources.outputs.apiUrl
output WORKERS_BASE_URL string = resources.outputs.workersUrl
output WEB_BASE_URL string = resources.outputs.webUrl
