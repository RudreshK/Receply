# Deploying Receply to Azure

This repo is set up for [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/) (`azd`) — the whole stack (two App Services, PostgreSQL, Container Registry, App Insights) is defined in `infra/*.bicep`, and `azure.yaml` tells `azd` how to build/deploy `Receply.Api` and `Receply.Workers`.

**Before you start**: this hasn't been run end-to-end from this environment (no Azure credentials or `azd` available in this dev sandbox to test against), so treat the first `azd up` as the real validation — if something in the Bicep needs adjusting, that's the moment you'll find out, not a sign anything obvious was skipped.

## 1. Prerequisites

- An Azure subscription ([portal.azure.com](https://portal.azure.com) to sign up — free tier covers this stack for a while).
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`).
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) (`azd`).
- Docker (azd builds the container images locally and pushes them to ACR).

## 2. First-time setup

```bash
az login
azd auth login

# From the repo root:
azd env new receply-prod
azd env set AZURE_LOCATION eastus   # or whichever region you want
```

Set the secrets `azd` needs to provision the app. Use the same values already stored in this session's `dotnet user-secrets` (access token, app secret, webhook verify token) — they never appear in this repo, so you'll need to have them on hand:

```bash
azd env set WHATSAPP_ACCESS_TOKEN "<your Meta permanent access token>"
azd env set WHATSAPP_APP_SECRET "<your Meta App Secret>"
azd env set WHATSAPP_WEBHOOK_VERIFY_TOKEN "<the webhook verify token generated this session>"
azd env set WHATSAPP_DEFAULT_PHONE_NUMBER_ID "<your WhatsApp phone_number_id>"
azd env set ANTHROPIC_API_KEY "<your Anthropic API key>"
azd env set JWT_SIGNING_KEY "$(openssl rand -base64 48)"
azd env set POSTGRES_ADMIN_PASSWORD "$(openssl rand -base64 24)"
```

`azd env set` stores these in `.azure/<env-name>/.env` on your machine only — that directory is not committed (add it to `.gitignore` if it isn't already by the time you get here).

## 3. Deploy

```bash
azd up
```

This provisions everything in `infra/` and builds + pushes + deploys both containers in one go. It'll print the API's URL when done (`https://app-api-<token>.azurewebsites.net`).

For later deploys after a code change, `azd deploy` alone is faster (skips re-provisioning infrastructure that hasn't changed).

## 4. Point Meta at the deployed webhook

1. Confirm the app is up: `https://<api-url>/api/webhooks/whatsapp` should respond (a bare GET with no query params returns 403 — that's expected, it means signature checking is live).
2. Meta for Developers → your app → **WhatsApp → Configuration**:
   - **Callback URL**: `https://<api-url>/api/webhooks/whatsapp`
   - **Verify Token**: the same value you set as `WHATSAPP_WEBHOOK_VERIFY_TOKEN` above.
3. Subscribe to the `messages` webhook field once verification succeeds.

## 5. Custom domain — `receply.in` (registered on GoDaddy)

1. In the Azure Portal, on the `api` App Service → **Custom domains** → add `api.receply.in`, and separately on wherever the Angular UI ends up hosted, add `www.receply.in` (or the apex). Azure gives you the exact CNAME/TXT records to create; add them in GoDaddy's DNS management for `receply.in`.
2. Once DNS verifies, App Service issues a free **managed certificate** automatically — no manual TLS cert handling needed.
3. Update Meta's Callback URL to `https://api.receply.in/api/webhooks/whatsapp` once it's live, and re-verify.

## 6. CI/CD via GitHub Actions (optional)

`.github/workflows/deploy.yml` is already in the repo, but it needs one-time setup before it'll run successfully:

1. Create an Azure AD app registration with a **federated credential** scoped to this repo (no stored client secret/password):
   ```bash
   az ad app create --display-name "receply-github-deploy"
   # then add a federated credential for repo:<owner>/<repo>:ref:refs/heads/main
   # see: https://learn.microsoft.com/azure/developer/github/connect-from-azure-openid-connect
   ```
2. Grant that app's service principal **Contributor** on the subscription or resource group.
3. In the GitHub repo → **Settings → Secrets and variables → Actions**:
   - **Variables**: `AZURE_ENV_NAME`, `AZURE_LOCATION`, `AZURE_SUBSCRIPTION_ID`
   - **Secrets**: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, plus the same six app secrets from step 2 above (`WHATSAPP_ACCESS_TOKEN`, `WHATSAPP_APP_SECRET`, `WHATSAPP_WEBHOOK_VERIFY_TOKEN`, `ANTHROPIC_API_KEY`, `JWT_SIGNING_KEY`, `POSTGRES_ADMIN_PASSWORD`)

Once that's in place, pushes to `main` deploy automatically.

## What's provisioned and roughly what it costs

| Resource | SKU | Purpose |
|---|---|---|
| App Service Plan (Linux) | B1 (Basic) | Hosts both `api` and `workers` App Services |
| App Service × 2 | — | `Receply.Api` (web-facing) and `Receply.Workers` (Quartz reminder job + a `/healthz` endpoint so App Service's container startup ping succeeds) |
| PostgreSQL Flexible Server | Standard_B1ms (Burstable) | Replaces the local dev Postgres |
| Container Registry | Basic | Holds the built container images |
| Log Analytics + Application Insights | pay-per-GB | Basic observability — App Service auto-instruments some request telemetry with zero code changes once `APPLICATIONINSIGHTS_CONNECTION_STRING` is set (already wired in the Bicep) |

Rough ballpark at this tier: **$25-40/month** total, dominated by the App Service Plan and Postgres. Not provisioned: Redis (nothing in the codebase uses it yet — add it when something does) and Key Vault (App Service Application Settings are used directly for secrets at this stage; Key Vault is a reasonable upgrade later, not required now).
