# GitHub Source Code Management

GitHub serves as our central source code repository and version control system, providing the foundation for our GitOps workflow and collaboration.

## 🎯 Overview

GitHub in our GitOps architecture provides:
- **Source Code Management** - Centralized code repository with version control
- **Collaboration** - Pull requests, code reviews, and team collaboration
- **CI/CD Integration** - GitHub Actions for automated workflows
- **Security** - Branch protection, security scanning, and access control
- **Documentation** - Wiki, README files, and project documentation

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Architecture                      │
├─────────────────────────────────────────────────────────────┤
│  Repositories                                               │
│  ├── Application Code                                       │
│  ├── Infrastructure Code (Terraform)                       │
│  ├── Configuration Manifests (K8s, Helm)                   │
│  ├── CI/CD Pipelines (GitHub Actions)                      │
│  └── Documentation                                         │
├─────────────────────────────────────────────────────────────┤
│  GitHub Actions                                             │
│  ├── Build Workflows                                       │
│  ├── Test Workflows                                        │
│  ├── Security Scanning                                     │
│  ├── Deployment Workflows                                  │
│  └── Infrastructure Provisioning                           │
├─────────────────────────────────────────────────────────────┤
│  Integrations                                               │
│  ├── Jenkins (Webhooks)                                    │
│  ├── ArgoCD (Repository Access)                            │
│  ├── Docker Registry                                       │
│  ├── AWS/Cloud Providers                                   │
│  └── Monitoring Tools                                      │
├─────────────────────────────────────────────────────────────┤
│  Security & Compliance                                      │
│  ├── Branch Protection Rules                               │
│  ├── Required Status Checks                                │
│  ├── Code Scanning (CodeQL)                                │
│  ├── Dependency Scanning                                   │
│  └── Secret Scanning                                       │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Repository Structure

```
github/
├── README.md
├── workflows/
│   ├── ci-cd.yml
│   ├── security-scan.yml
│   ├── infrastructure.yml
│   ├── release.yml
│   └── dependency-update.yml
├── templates/
│   ├── pull-request-template.md
│   ├── issue-templates/
│   │   ├── bug-report.yml
│   │   ├── feature-request.yml
│   │   └── security-issue.yml
│   └── repository-template/
├── branch-protection/
│   ├── main-branch-rules.json
│   ├── develop-branch-rules.json
│   └── release-branch-rules.json
├── security/
│   ├── codeql-config.yml
│   ├── dependabot.yml
│   ├── security-policy.md
│   └── code-scanning-alerts.yml
├── organization/
│   ├── org-settings.yml
│   ├── team-permissions.yml
│   ├── repository-settings.yml
│   └── webhook-config.yml
└── examples/
    ├── microservice-repo/
    ├── infrastructure-repo/
    └── documentation-repo/
```

## 🚀 Quick Setup

### 1. Repository Creation

```bash
# Create new repository using GitHub CLI
gh repo create company/myapp --public --description "My Application"

# Clone repository
git clone https://github.com/company/myapp.git
cd myapp

# Initialize with basic structure
mkdir -p .github/workflows
mkdir -p .github/ISSUE_TEMPLATE
mkdir -p src tests docs

# Create initial files
touch README.md
touch .gitignore
touch LICENSE
```

### 2. Basic Repository Configuration

```yaml
# .github/workflows/ci.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
    
    - name: Install dependencies
      run: npm ci
    
    - name: Run tests
      run: npm test
    
    - name: Run linting
      run: npm run lint
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/lcov.info
        flags: unittests
        name: codecov-umbrella

  build:
    needs: test
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      packages: write
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}

  deploy:
    needs: build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Checkout config repo
      uses: actions/checkout@v4
      with:
        repository: company/k8s-configs
        token: ${{ secrets.CONFIG_REPO_TOKEN }}
        path: k8s-configs
    
    - name: Update image tag
      run: |
        cd k8s-configs
        sed -i "s|image: .*|image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}|" apps/myapp/deployment.yaml
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
        git add .
        git commit -m "Update myapp image to ${{ github.sha }}"
        git push
```

### 3. Branch Protection Rules

```json
{
  "required_status_checks": {
    "strict": true,
    "contexts": [
      "test",
      "build",
      "security-scan"
    ]
  },
  "enforce_admins": true,
  "required_pull_request_reviews": {
    "required_approving_review_count": 2,
    "dismiss_stale_reviews": true,
    "require_code_owner_reviews": true,
    "require_last_push_approval": true
  },
  "restrictions": {
    "users": [],
    "teams": ["senior-developers", "devops-team"],
    "apps": []
  },
  "allow_force_pushes": false,
  "allow_deletions": false,
  "block_creations": false,
  "required_conversation_resolution": true,
  "lock_branch": false,
  "allow_fork_syncing": true
}
```

## 🔧 GitHub Actions Workflows

### Comprehensive CI/CD Pipeline

```yaml
# .github/workflows/comprehensive-ci-cd.yml
name: Comprehensive CI/CD

on:
  push:
    branches: [main, develop, 'release/*']
  pull_request:
    branches: [main, develop]
  release:
    types: [published]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}
  NODE_VERSION: '18'
  PYTHON_VERSION: '3.11'

jobs:
  changes:
    runs-on: ubuntu-latest
    outputs:
      app: ${{ steps.changes.outputs.app }}
      infrastructure: ${{ steps.changes.outputs.infrastructure }}
      docs: ${{ steps.changes.outputs.docs }}
    steps:
    - uses: actions/checkout@v4
    - uses: dorny/paths-filter@v2
      id: changes
      with:
        filters: |
          app:
            - 'src/**'
            - 'package*.json'
            - 'Dockerfile'
          infrastructure:
            - 'terraform/**'
            - 'k8s/**'
            - 'helm/**'
          docs:
            - 'docs/**'
            - '*.md'

  lint-and-format:
    runs-on: ubuntu-latest
    needs: changes
    if: needs.changes.outputs.app == 'true'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'
    
    - name: Install dependencies
      run: npm ci
    
    - name: Run ESLint
      run: npm run lint
    
    - name: Run Prettier
      run: npm run format:check
    
    - name: Run type checking
      run: npm run type-check

  test:
    runs-on: ubuntu-latest
    needs: changes
    if: needs.changes.outputs.app == 'true'
    
    strategy:
      matrix:
        node-version: [16, 18, 20]
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup Node.js ${{ matrix.node-version }}
      uses: actions/setup-node@v4
      with:
        node-version: ${{ matrix.node-version }}
        cache: 'npm'
    
    - name: Install dependencies
      run: npm ci
    
    - name: Run unit tests
      run: npm run test:unit
    
    - name: Run integration tests
      run: npm run test:integration
      env:
        DATABASE_URL: postgresql://postgres:postgres@localhost:5432/test
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      if: matrix.node-version == 18
      with:
        file: ./coverage/lcov.info
        flags: unittests
        name: codecov-umbrella

  security-scan:
    runs-on: ubuntu-latest
    needs: changes
    if: needs.changes.outputs.app == 'true'
    
    permissions:
      security-events: write
      actions: read
      contents: read
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
    
    - name: Run npm audit
      run: npm audit --audit-level high
    
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/node@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high

  build:
    runs-on: ubuntu-latest
    needs: [lint-and-format, test, security-scan]
    if: always() && (needs.lint-and-format.result == 'success' || needs.lint-and-format.result == 'skipped') && (needs.test.result == 'success' || needs.test.result == 'skipped') && (needs.security-scan.result == 'success' || needs.security-scan.result == 'skipped')
    
    permissions:
      contents: read
      packages: write
    
    outputs:
      image-digest: ${{ steps.build.outputs.digest }}
      image-tags: ${{ steps.meta.outputs.tags }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=semver,pattern={{version}}
          type=semver,pattern={{major}}.{{minor}}
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push Docker image
      id: build
      uses: docker/build-push-action@v5
      with:
        context: .
        platforms: linux/amd64,linux/arm64
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
    
    - name: Generate SBOM
      uses: anchore/sbom-action@v0
      with:
        image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}@${{ steps.build.outputs.digest }}
        format: spdx-json
        output-file: sbom.spdx.json
    
    - name: Upload SBOM
      uses: actions/upload-artifact@v3
      with:
        name: sbom
        path: sbom.spdx.json

  infrastructure-validate:
    runs-on: ubuntu-latest
    needs: changes
    if: needs.changes.outputs.infrastructure == 'true'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup Terraform
      uses: hashicorp/setup-terraform@v3
      with:
        terraform_version: 1.6.0
    
    - name: Terraform Format Check
      run: terraform fmt -check -recursive terraform/
    
    - name: Terraform Init
      run: terraform init -backend=false
      working-directory: terraform/environments/dev
    
    - name: Terraform Validate
      run: terraform validate
      working-directory: terraform/environments/dev
    
    - name: Run tfsec
      uses: aquasecurity/tfsec-action@v1.0.0
      with:
        soft_fail: true

  deploy-dev:
    runs-on: ubuntu-latest
    needs: [build, changes]
    if: github.ref == 'refs/heads/develop' && needs.changes.outputs.app == 'true'
    environment: development
    
    steps:
    - name: Checkout config repo
      uses: actions/checkout@v4
      with:
        repository: company/k8s-configs
        token: ${{ secrets.CONFIG_REPO_TOKEN }}
        path: k8s-configs
    
    - name: Update development deployment
      run: |
        cd k8s-configs
        IMAGE_TAG=$(echo "${{ needs.build.outputs.image-tags }}" | grep "develop-" | head -1)
        sed -i "s|image: .*|image: ${IMAGE_TAG}|" environments/dev/myapp/deployment.yaml
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
        git add .
        git commit -m "Deploy myapp to dev: ${IMAGE_TAG}"
        git push

  deploy-staging:
    runs-on: ubuntu-latest
    needs: [build, changes]
    if: startsWith(github.ref, 'refs/heads/release/') && needs.changes.outputs.app == 'true'
    environment: staging
    
    steps:
    - name: Checkout config repo
      uses: actions/checkout@v4
      with:
        repository: company/k8s-configs
        token: ${{ secrets.CONFIG_REPO_TOKEN }}
        path: k8s-configs
    
    - name: Update staging deployment
      run: |
        cd k8s-configs
        IMAGE_TAG=$(echo "${{ needs.build.outputs.image-tags }}" | grep "release-" | head -1)
        sed -i "s|image: .*|image: ${IMAGE_TAG}|" environments/staging/myapp/deployment.yaml
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
        git add .
        git commit -m "Deploy myapp to staging: ${IMAGE_TAG}"
        git push

  deploy-production:
    runs-on: ubuntu-latest
    needs: [build, changes]
    if: github.event_name == 'release' && github.event.action == 'published'
    environment: production
    
    steps:
    - name: Checkout config repo
      uses: actions/checkout@v4
      with:
        repository: company/k8s-configs
        token: ${{ secrets.CONFIG_REPO_TOKEN }}
        path: k8s-configs
    
    - name: Update production deployment
      run: |
        cd k8s-configs
        IMAGE_TAG=$(echo "${{ needs.build.outputs.image-tags }}" | grep -E "^[0-9]+\.[0-9]+\.[0-9]+$" | head -1)
        sed -i "s|image: .*|image: ${IMAGE_TAG}|" environments/prod/myapp/deployment.yaml
        git config user.name "GitHub Actions"
        git config user.email "actions@github.com"
        git add .
        git commit -m "Deploy myapp to production: ${IMAGE_TAG}"
        git push
    
    - name: Create deployment record
      uses: actions/github-script@v7
      with:
        script: |
          github.rest.repos.createDeployment({
            owner: context.repo.owner,
            repo: context.repo.repo,
            ref: context.sha,
            environment: 'production',
            description: 'Production deployment via release',
            auto_merge: false
          });

  notify:
    runs-on: ubuntu-latest
    needs: [deploy-dev, deploy-staging, deploy-production]
    if: always()
    
    steps:
    - name: Notify Slack
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
        fields: repo,message,commit,author,action,eventName,ref,workflow
      env:
        SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK }}
```

### Security Scanning Workflow

```yaml
# .github/workflows/security-scan.yml
name: Security Scanning

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  schedule:
    - cron: '0 2 * * 1'  # Weekly on Monday at 2 AM

permissions:
  security-events: write
  actions: read
  contents: read

jobs:
  codeql:
    name: CodeQL Analysis
    runs-on: ubuntu-latest
    
    strategy:
      fail-fast: false
      matrix:
        language: ['javascript', 'python']
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
    
    - name: Initialize CodeQL
      uses: github/codeql-action/init@v2
      with:
        languages: ${{ matrix.language }}
        config-file: ./.github/codeql/codeql-config.yml
    
    - name: Autobuild
      uses: github/codeql-action/autobuild@v2
    
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v2
      with:
        category: "/language:${{matrix.language}}"

  dependency-scan:
    name: Dependency Vulnerability Scan
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
    
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/node@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=medium --file=package.json
    
    - name: Upload result to GitHub Code Scanning
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: snyk.sarif

  container-scan:
    name: Container Security Scan
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
    
    - name: Build Docker image
      run: docker build -t myapp:${{ github.sha }} .
    
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: 'myapp:${{ github.sha }}'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'

  secret-scan:
    name: Secret Scanning
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
    
    - name: Run TruffleHog OSS
      uses: trufflesecurity/trufflehog@main
      with:
        path: ./
        base: main
        head: HEAD
        extra_args: --debug --only-verified
```

## 🔒 Security Configuration

### Dependabot Configuration

```yaml
# .github/dependabot.yml
version: 2
updates:
  # Enable version updates for npm
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
      day: "monday"
      time: "09:00"
    open-pull-requests-limit: 10
    reviewers:
      - "devops-team"
    assignees:
      - "lead-developer"
    commit-message:
      prefix: "npm"
      include: "scope"
    labels:
      - "dependencies"
      - "npm"
    ignore:
      - dependency-name: "lodash"
        versions: ["4.17.19"]

  # Enable version updates for Docker
  - package-ecosystem: "docker"
    directory: "/"
    schedule:
      interval: "weekly"
    reviewers:
      - "devops-team"
    labels:
      - "dependencies"
      - "docker"

  # Enable version updates for GitHub Actions
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
    reviewers:
      - "devops-team"
    labels:
      - "dependencies"
      - "github-actions"

  # Enable version updates for Terraform
  - package-ecosystem: "terraform"
    directory: "/terraform"
    schedule:
      interval: "weekly"
    reviewers:
      - "devops-team"
    labels:
      - "dependencies"
      - "terraform"
```

### CodeQL Configuration

```yaml
# .github/codeql/codeql-config.yml
name: "CodeQL Config"

disable-default-queries: false

queries:
  - name: security-extended
    uses: security-extended
  - name: security-and-quality
    uses: security-and-quality

paths-ignore:
  - node_modules
  - '**/*.test.js'
  - '**/*.spec.js'
  - dist
  - build

paths:
  - src
  - lib

query-filters:
  - exclude:
      id: js/unused-local-variable
```

### Security Policy

```markdown
# .github/SECURITY.md
# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Which versions are eligible for receiving such patches depends on the CVSS v3.0 Rating:

| Version | Supported          |
| ------- | ------------------ |
| 2.x.x   | :white_check_mark: |
| 1.x.x   | :x:                |

## Reporting a Vulnerability

Please report (suspected) security vulnerabilities to **security@company.com**. You will receive a response from us within 48 hours. If the issue is confirmed, we will release a patch as soon as possible depending on complexity but historically within a few days.

## Security Measures

- All dependencies are regularly updated using Dependabot
- Code is scanned using CodeQL for security vulnerabilities
- Container images are scanned using Trivy
- Secrets are scanned using TruffleHog
- All commits are signed and verified
- Branch protection rules enforce code review requirements
```

## 🔄 GitOps Integration

### Repository Templates

```yaml
# templates/repository-template/.github/workflows/template-sync.yml
name: Template Sync

on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly on Sunday
  workflow_dispatch:

jobs:
  template-sync:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      with:
        token: ${{ secrets.TEMPLATE_SYNC_TOKEN }}
    
    - name: Template Sync
      uses: AndreasAugustin/actions-template-sync@v1
      with:
        github_token: ${{ secrets.TEMPLATE_SYNC_TOKEN }}
        source_repo_path: company/repository-template
        upstream_branch: main
        pr_title: "chore: sync with template"
        pr_body: |
          This PR updates the repository with the latest changes from the template.
          
          Please review the changes and merge if appropriate.
        pr_labels: "template-sync,automated"
```

### Issue Templates

```yaml
# .github/ISSUE_TEMPLATE/bug-report.yml
name: Bug Report
description: File a bug report to help us improve
title: "[BUG] "
labels: ["bug", "triage"]
assignees:
  - lead-developer

body:
  - type: markdown
    attributes:
      value: |
        Thanks for taking the time to fill out this bug report!

  - type: input
    id: contact
    attributes:
      label: Contact Details
      description: How can we get in touch with you if we need more info?
      placeholder: ex. email@example.com
    validations:
      required: false

  - type: textarea
    id: what-happened
    attributes:
      label: What happened?
      description: Also tell us, what did you expect to happen?
      placeholder: Tell us what you see!
      value: "A bug happened!"
    validations:
      required: true

  - type: dropdown
    id: version
    attributes:
      label: Version
      description: What version of our software are you running?
      options:
        - 2.1.0 (Default)
        - 2.0.0
        - 1.9.0
        - 1.8.0
    validations:
      required: true

  - type: dropdown
    id: browsers
    attributes:
      label: What browsers are you seeing the problem on?
      multiple: true
      options:
        - Firefox
        - Chrome
        - Safari
        - Microsoft Edge

  - type: textarea
    id: logs
    attributes:
      label: Relevant log output
      description: Please copy and paste any relevant log output. This will be automatically formatted into code, so no need for backticks.
      render: shell

  - type: checkboxes
    id: terms
    attributes:
      label: Code of Conduct
      description: By submitting this issue, you agree to follow our [Code of Conduct](https://example.com)
      options:
        - label: I agree to follow this project's Code of Conduct
          required: true
```

### Pull Request Template

```markdown
# .github/pull_request_template.md
## Description

Brief description of the changes in this PR.

## Type of Change

- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Infrastructure change
- [ ] Performance improvement
- [ ] Code refactoring

## Testing

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed
- [ ] Security scan passed

## Checklist

- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] Any dependent changes have been merged and published in downstream modules

## Screenshots (if applicable)

## Additional Notes

Any additional information that reviewers should know.

## Related Issues

Closes #(issue number)
```

## 📊 Monitoring and Analytics

### Repository Insights

```yaml
# .github/workflows/repository-metrics.yml
name: Repository Metrics

on:
  schedule:
    - cron: '0 0 * * 1'  # Weekly on Monday
  workflow_dispatch:

jobs:
  metrics:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Generate Repository Metrics
      uses: lowlighter/metrics@latest
      with:
        token: ${{ secrets.METRICS_TOKEN }}
        user: company
        repo: myapp
        template: repository
        base: header, repositories
        plugin_lines: yes
        plugin_languages: yes
        plugin_languages_details: bytes-size, percentage
        plugin_followup: yes
        plugin_projects: yes
        plugin_projects_repositories: company/myapp
    
    - name: Upload metrics
      uses: actions/upload-artifact@v3
      with:
        name: repository-metrics
        path: github-metrics.svg
```

### Code Quality Metrics

```yaml
# .github/workflows/code-quality.yml
name: Code Quality Metrics

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  sonarcloud:
    name: SonarCloud Analysis
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
    
    - name: Install dependencies
      run: npm ci
    
    - name: Run tests with coverage
      run: npm run test:coverage
    
    - name: SonarCloud Scan
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
      with:
        args: >
          -Dsonar.projectKey=company_myapp
          -Dsonar.organization=company
          -Dsonar.javascript.lcov.reportPaths=coverage/lcov.info
          -Dsonar.coverage.exclusions=**/*.test.js,**/*.spec.js
```

## 🛠️ Advanced Features

### Automated Release Management

```yaml
# .github/workflows/release.yml
name: Release Management

on:
  push:
    branches: [main]

jobs:
  release:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
        token: ${{ secrets.RELEASE_TOKEN }}
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'
    
    - name: Install dependencies
      run: npm ci
    
    - name: Build
      run: npm run build
    
    - name: Semantic Release
      uses: cycjimmy/semantic-release-action@v4
      with:
        semantic_version: 19
        extra_plugins: |
          @semantic-release/changelog
          @semantic-release/git
          @semantic-release/github
      env:
        GITHUB_TOKEN: ${{ secrets.RELEASE_TOKEN }}
        NPM_TOKEN: ${{ secrets.NPM_TOKEN }}
```

### Multi-Environment Deployment

```yaml
# .github/workflows/multi-env-deploy.yml
name: Multi-Environment Deployment

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy to'
        required: true
        default: 'dev'
        type: choice
        options:
        - dev
        - staging
        - production
      version:
        description: 'Version to deploy'
        required: true
        type: string

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment }}
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: us-west-2
    
    - name: Update EKS kubeconfig
      run: |
        aws eks update-kubeconfig --region us-west-2 --name gitops-${{ github.event.inputs.environment }}-cluster
    
    - name: Deploy to ${{ github.event.inputs.environment }}
      run: |
        kubectl set image deployment/myapp myapp=ghcr.io/company/myapp:${{ github.event.inputs.version }} -n ${{ github.event.inputs.environment }}
        kubectl rollout status deployment/myapp -n ${{ github.event.inputs.environment }} --timeout=300s
    
    - name: Run smoke tests
      run: |
        kubectl run smoke-test --image=curlimages/curl --rm -i --restart=Never -- \
          curl -f http://myapp.${{ github.event.inputs.environment }}.svc.cluster.local/health
```

## 📚 Best Practices

### 1. Repository Management
- **Clear Structure** - Organize code with consistent directory structure
- **Meaningful Commits** - Use conventional commit messages
- **Branch Strategy** - Implement GitFlow or GitHub Flow
- **Code Reviews** - Require pull request reviews
- **Documentation** - Maintain up-to-date README and documentation

### 2. Security
- **Branch Protection** - Enable branch protection rules
- **Secret Management** - Use GitHub Secrets for sensitive data
- **Dependency Updates** - Enable Dependabot for automatic updates
- **Security Scanning** - Implement CodeQL and vulnerability scanning
- **Access Control** - Use teams and CODEOWNERS for access management

### 3. CI/CD
- **Automated Testing** - Run tests on every commit
- **Build Optimization** - Use caching and parallel jobs
- **Environment Parity** - Consistent environments across stages
- **Deployment Automation** - Automate deployments with proper approvals
- **Monitoring** - Monitor pipeline performance and success rates

### 4. Collaboration
- **Issue Templates** - Standardize bug reports and feature requests
- **PR Templates** - Consistent pull request information
- **Code Owners** - Define code ownership for reviews
- **Team Communication** - Integrate with Slack or Teams
- **Documentation** - Keep documentation in sync with code

## 🚨 Troubleshooting

### Common Issues

#### 1. Action Failures
```bash
# Check action logs
gh run list --repo company/myapp
gh run view <run-id> --repo company/myapp

# Re-run failed jobs
gh run rerun <run-id> --repo company/myapp
```

#### 2. Permission Issues
```bash
# Check repository permissions
gh api repos/company/myapp/collaborators

# Check team permissions
gh api orgs/company/teams/devops-team/repos
```

#### 3. Secret Management
```bash
# List repository secrets
gh secret list --repo company/myapp

# Set repository secret
gh secret set SECRET_NAME --body "secret-value" --repo company/myapp
```

### Debug Commands

```bash
# GitHub CLI commands for troubleshooting
gh repo view company/myapp
gh pr list --repo company/myapp
gh issue list --repo company/myapp
gh workflow list --repo company/myapp
gh run list --repo company/myapp --limit 10
```

---

**Next Steps:**
1. Set up your GitHub repository with proper structure
2. Configure branch protection rules and security settings
3. Implement CI/CD workflows using GitHub Actions
4. Set up integrations with other GitOps components
5. Establish monitoring and metrics collection

For advanced GitHub features and best practices, refer to the [GitHub Documentation](https://docs.github.com/) and [GitHub Actions Documentation](https://docs.github.com/en/actions).