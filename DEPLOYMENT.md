# Deploying Receply to Azure

This repo is set up for [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/) (`azd`) — the whole stack (two App Services, a Static Web App, PostgreSQL, Container Registry, App Insights) is defined in `infra/*.bicep`, and `azure.yaml` tells `azd` how to build/deploy `Receply.Api`, `Receply.Workers`, and the Angular `web` app in one `azd deploy`.

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

This provisions everything in `infra/` and builds + pushes + deploys the API container, the workers container, and the Angular app in one go. It'll print each service's URL when done - the API at `https://app-api-<token>.azurewebsites.net`, the Angular app at `https://<swa-token>.azurestaticapps.net`.

For later deploys after a code change, `azd deploy` alone is faster (skips re-provisioning infrastructure that hasn't changed).

## 4. Point Meta at the deployed webhook

1. Confirm the app is up: `https://<api-url>/api/webhooks/whatsapp` should respond (a bare GET with no query params returns 403 — that's expected, it means signature checking is live).
2. Meta for Developers → your app → **WhatsApp → Configuration**:
   - **Callback URL**: `https://<api-url>/api/webhooks/whatsapp`
   - **Verify Token**: the same value you set as `WHATSAPP_WEBHOOK_VERIFY_TOKEN` above.
3. Subscribe to the `messages` webhook field once verification succeeds.

## 5. Custom domain — `receply.in` (registered on GoDaddy)

1. In the Azure Portal, on the `api` App Service → **Custom domains** → add `api.receply.in`. Azure gives you the exact CNAME/TXT records to create; add them in GoDaddy's DNS management for `receply.in`.
2. On the Static Web App (the `web` service) → **Custom domains** → add `www.receply.in` (or the apex, via ALIAS/ANAME if GoDaddy supports it, otherwise a CNAME on `www`). Same idea — Azure gives you the record to add in GoDaddy.
3. Once DNS verifies on each resource, they issue a free **managed certificate** automatically — no manual TLS cert handling needed.
4. Update Meta's Callback URL to `https://api.receply.in/api/webhooks/whatsapp` once it's live, and re-verify.
5. Once `www.receply.in` is live, add it to `Cors:AllowedOrigins` if it isn't already (`src/Receply.Api/appsettings.json` already has it pre-populated) and drop the `*.azurestaticapps.net` origin if you no longer need it.

## 6. CI/CD via GitHub Actions

`main` is the integration branch - it is never deployed directly. Production deploys go through two dedicated release branches, each with its own workflow so the two halves of the app ship independently:

| Branch | Workflow | Deploys |
|---|---|---|
| `release/services` | `.github/workflows/deploy-services.yml` | `api` + `workers` (backend) |
| `release/ui` | `.github/workflows/deploy-ui.yml` | `web` (Angular app) |

Day to day: land your changes on `main` as usual, then when you're ready to ship, merge `main` into whichever release branch(es) you want to deploy and push - the matching workflow deploys automatically. Merging into both deploys the whole stack; merging into just one leaves the other side untouched.

`.github/workflows/deploy.yml` still exists as a manual (`workflow_dispatch`-only) fallback that deploys everything in one run - use it for the very first `azd provision`/`azd up`, or if you ever need a full redeploy without touching either release branch.

All three workflows need the same one-time setup before any of them will run successfully:

1. Create an Azure AD app registration with a **federated credential** scoped to this repo (no stored client secret/password) - add one federated credential per branch that triggers a deploy:
   ```bash
   az ad app create --display-name "receply-github-deploy"
   # then add a federated credential for each of:
   #   repo:<owner>/<repo>:ref:refs/heads/release/services
   #   repo:<owner>/<repo>:ref:refs/heads/release/ui
   # (and optionally repo:<owner>/<repo>:ref:refs/heads/main if you still want manual dispatch from there)
   # see: https://learn.microsoft.com/azure/developer/github/connect-from-azure-openid-connect
   ```
2. Grant that app's service principal **Contributor** on the subscription or resource group.
3. In the GitHub repo → **Settings → Secrets and variables → Actions**:
   - **Variables**: `AZURE_ENV_NAME`, `AZURE_LOCATION`, `AZURE_SUBSCRIPTION_ID`
   - **Secrets**: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, plus the same six app secrets from step 2 above (`WHATSAPP_ACCESS_TOKEN`, `WHATSAPP_APP_SECRET`, `WHATSAPP_WEBHOOK_VERIFY_TOKEN`, `ANTHROPIC_API_KEY`, `JWT_SIGNING_KEY`, `POSTGRES_ADMIN_PASSWORD`) - all three workflows run `azd provision` against the same shared bicep stack, so each needs the full set even if it only deploys one side afterward.
4. Consider branch protection on `release/services` and `release/ui` (e.g. require a PR instead of direct pushes) since a push to either deploys straight to production.

Once that's in place, merging into `release/services` or `release/ui` deploys automatically.

## What's provisioned and roughly what it costs

| Resource | SKU | Purpose |
|---|---|---|
| App Service Plan (Linux) | B1 (Basic) | Hosts both `api` and `workers` App Services |
| App Service × 2 | — | `Receply.Api` (web-facing) and `Receply.Workers` (Quartz reminder job + a `/healthz` endpoint so App Service's container startup ping succeeds) |
| Static Web App | Free | Hosts the built Angular `web` app - global CDN + free managed TLS, no App Service cost |
| PostgreSQL Flexible Server | Standard_B1ms (Burstable) | Replaces the local dev Postgres |
| Container Registry | Basic | Holds the built container images |
| Log Analytics + Application Insights | pay-per-GB | Basic observability — App Service auto-instruments some request telemetry with zero code changes once `APPLICATIONINSIGHTS_CONNECTION_STRING` is set (already wired in the Bicep) |

Rough ballpark at this tier: **$25-40/month** total, dominated by the App Service Plan and Postgres - the Static Web App's Free tier adds nothing. Not provisioned: Redis (nothing in the codebase uses it yet — add it when something does) and Key Vault (App Service Application Settings are used directly for secrets at this stage; Key Vault is a reasonable upgrade later, not required now).
