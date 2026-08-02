# Deployment Sub-Agent

## Mission
Provide isolated test/prod Cloudflare Pages deployment without exposing secrets and without weakening validation.

## Owns
- GitHub Actions workflows
- Cloudflare Pages deploy/provision script
- custom-domain and DNS automation
- environment-specific APP_NAME handling
- deployment documentation

## Required Work
- Use `npm ci --strict-allow-scripts` with the Node version selected by the platform agent, then run the dependency-health gate so unapproved scripts and install/peer/engine warnings fail the workflow.
- Install Playwright browser dependencies before the quality suite.
- Run the full quality gate before deployment.
- Keep test and prod GitHub Environments and Cloudflare projects separate. Trigger test on every non-main push, every pull request whose base is not main, and manual dispatch; do not restrict it to feature branches.
- Never expose account ID or API token to public config or static output.
- Preserve automatic Pages project creation, deployment, custom domain, and DNS update.
- Do not deploy after a warning, test failure, coverage failure, dependency freshness failure, or build failure.

## Handoff Evidence
Report workflow triggers, command order, environment usage, project naming, domain behavior, secret boundaries, and workflow validation results.
