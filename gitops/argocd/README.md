# ArgoCD - GitOps Continuous Delivery

ArgoCD is a declarative, GitOps continuous delivery tool for Kubernetes that automatically synchronizes applications with their desired state defined in Git repositories.

## 🎯 Overview

ArgoCD in our GitOps workflow provides:
- **Declarative GitOps** - Application definitions stored in Git
- **Automated Deployment** - Continuous synchronization with Git repositories
- **Multi-Environment Management** - Deploy to multiple clusters and environments
- **Rollback Capabilities** - Easy rollback to previous application states
- **Security & Compliance** - RBAC, audit trails, and policy enforcement

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ArgoCD Architecture                      │
├─────────────────────────────────────────────────────────────┤
│  Git Repositories                                          │
│  ├── Application Manifests (YAML)                          │
│  ├── Helm Charts                                           │
│  ├── Kustomize Overlays                                    │
│  └── Jsonnet Templates                                     │
├─────────────────────────────────────────────────────────────┤
│  ArgoCD Components                                          │
│  ├── API Server (Web UI, CLI, API)                         │
│  ├── Repository Server (Git operations)                    │
│  ├── Application Controller (Sync logic)                   │
│  └── Redis (Caching)                                       │
├─────────────────────────────────────────────────────────────┤
│  Target Clusters                                           │
│  ├── Production EKS                                        │
│  ├── Staging EKS                                           │
│  ├── Development EKS                                       │
│  └── External Clusters                                     │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
argocd/
├── README.md
├── installation/
│   ├── namespace.yaml
│   ├── argocd-install.yaml
│   ├── argocd-config.yaml
│   └── ingress.yaml
├── applications/
│   ├── app-of-apps.yaml
│   ├── frontend-app.yaml
│   ├── backend-api.yaml
│   ├── database.yaml
│   └── monitoring.yaml
├── projects/
│   ├── default-project.yaml
│   ├── production-project.yaml
│   └── development-project.yaml
├── repositories/
│   ├── private-repo.yaml
│   ├── helm-repo.yaml
│   └── git-credentials.yaml
├── clusters/
│   ├── production-cluster.yaml
│   ├── staging-cluster.yaml
│   └── development-cluster.yaml
├── rbac/
│   ├── policy.csv
│   └── rbac-config.yaml
├── notifications/
│   ├── notifications-config.yaml
│   └── triggers.yaml
└── examples/
    ├── simple-app/
    ├── helm-app/
    ├── kustomize-app/
    └── multi-source-app/
```

## 🚀 Quick Setup

### 1. Install ArgoCD

```bash
# Create namespace
kubectl create namespace argocd

# Install ArgoCD
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Wait for pods to be ready
kubectl wait --for=condition=available --timeout=300s deployment/argocd-server -n argocd
```

### 2. Access ArgoCD UI

```bash
# Get initial admin password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d && echo

# Port forward to access UI
kubectl port-forward svc/argocd-server -n argocd 8080:443

# Access UI at https://localhost:8080
# Username: admin
# Password: (from above command)
```

### 3. Install ArgoCD CLI

```bash
# Download and install ArgoCD CLI
curl -sSL -o argocd-linux-amd64 https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64
sudo install -m 555 argocd-linux-amd64 /usr/local/bin/argocd
rm argocd-linux-amd64

# Login via CLI
argocd login localhost:8080 --username admin --password <password> --insecure
```

## 🔧 Configuration

### ArgoCD Configuration

```yaml
# installation/argocd-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-cm
  namespace: argocd
  labels:
    app.kubernetes.io/name: argocd-cm
    app.kubernetes.io/part-of: argocd
data:
  # Git repositories
  repositories: |
    - type: git
      url: https://github.com/company/app-configs
      name: app-configs
    - type: helm
      url: https://charts.bitnami.com/bitnami
      name: bitnami
      
  # OIDC configuration
  oidc.config: |
    name: OIDC
    issuer: https://your-oidc-provider.com
    clientId: argocd
    clientSecret: $oidc.clientSecret
    requestedScopes: ["openid", "profile", "email", "groups"]
    requestedIDTokenClaims: {"groups": {"essential": true}}
    
  # URL configuration
  url: https://argocd.company.com
  
  # Application instance label key
  application.instanceLabelKey: argocd.argoproj.io/instance
  
  # Server configuration
  server.insecure: "false"
  server.grpc.web: "true"
  
  # Repository server configuration
  reposerver.parallelism.limit: "10"
  
  # Timeout configurations
  timeout.hard.reconciliation: "0"
  timeout.reconciliation: "180s"
  
  # Resource exclusions
  resource.exclusions: |
    - apiGroups:
      - cilium.io
      kinds:
      - CiliumIdentity
      clusters:
      - "*"
```

### RBAC Configuration

```yaml
# rbac/rbac-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-rbac-cm
  namespace: argocd
  labels:
    app.kubernetes.io/name: argocd-rbac-cm
    app.kubernetes.io/part-of: argocd
data:
  policy.default: role:readonly
  policy.csv: |
    # Admin role
    p, role:admin, applications, *, */*, allow
    p, role:admin, clusters, *, *, allow
    p, role:admin, repositories, *, *, allow
    p, role:admin, projects, *, *, allow
    
    # Developer role
    p, role:developer, applications, get, */*, allow
    p, role:developer, applications, sync, */*, allow
    p, role:developer, applications, action/*, */*, allow
    p, role:developer, repositories, get, *, allow
    p, role:developer, projects, get, *, allow
    
    # Read-only role
    p, role:readonly, applications, get, */*, allow
    p, role:readonly, repositories, get, *, allow
    p, role:readonly, projects, get, *, allow
    
    # Group mappings
    g, argocd-admins, role:admin
    g, developers, role:developer
    g, viewers, role:readonly
    
    # User mappings
    g, admin@company.com, role:admin
    g, developer@company.com, role:developer
```

## 📝 Application Definitions

### Simple Application

```yaml
# applications/frontend-app.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: frontend-app
  namespace: argocd
  labels:
    app: frontend
    environment: production
  annotations:
    argocd.argoproj.io/sync-wave: "1"
  finalizers:
    - resources-finalizer.argocd.argoproj.io
spec:
  project: default
  source:
    repoURL: https://github.com/company/frontend-app
    targetRevision: main
    path: k8s/overlays/production
  destination:
    server: https://kubernetes.default.svc
    namespace: frontend
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
      allowEmpty: false
    syncOptions:
      - CreateNamespace=true
      - PrunePropagationPolicy=foreground
      - PruneLast=true
    retry:
      limit: 5
      backoff:
        duration: 5s
        factor: 2
        maxDuration: 3m
  revisionHistoryLimit: 10
```

### Helm Application

```yaml
# applications/backend-api.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: backend-api
  namespace: argocd
  labels:
    app: backend-api
    environment: production
spec:
  project: production
  source:
    repoURL: https://github.com/company/helm-charts
    targetRevision: main
    path: charts/backend-api
    helm:
      valueFiles:
        - values-production.yaml
      parameters:
        - name: image.tag
          value: v1.2.3
        - name: replicaCount
          value: "3"
      values: |
        ingress:
          enabled: true
          hosts:
            - host: api.company.com
              paths:
                - path: /
                  pathType: Prefix
  destination:
    server: https://kubernetes.default.svc
    namespace: backend
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
      - ServerSideApply=true
```

### Multi-Source Application

```yaml
# applications/multi-source-app.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: multi-source-app
  namespace: argocd
spec:
  project: default
  sources:
    - repoURL: https://github.com/company/app-manifests
      targetRevision: main
      path: base
    - repoURL: https://github.com/company/app-configs
      targetRevision: main
      path: overlays/production
      kustomize:
        images:
          - myapp:v1.2.3
  destination:
    server: https://kubernetes.default.svc
    namespace: myapp
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

### App of Apps Pattern

```yaml
# applications/app-of-apps.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: app-of-apps
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/company/argocd-apps
    targetRevision: main
    path: applications
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

## 🏗️ Project Management

### Production Project

```yaml
# projects/production-project.yaml
apiVersion: argoproj.io/v1alpha1
kind: AppProject
metadata:
  name: production
  namespace: argocd
spec:
  description: Production applications
  
  # Source repositories
  sourceRepos:
    - 'https://github.com/company/*'
    - 'https://charts.bitnami.com/bitnami'
    - 'https://kubernetes-charts.storage.googleapis.com'
  
  # Destination clusters and namespaces
  destinations:
    - namespace: 'frontend'
      server: https://kubernetes.default.svc
    - namespace: 'backend'
      server: https://kubernetes.default.svc
    - namespace: 'database'
      server: https://kubernetes.default.svc
  
  # Cluster resource whitelist
  clusterResourceWhitelist:
    - group: ''
      kind: Namespace
    - group: 'rbac.authorization.k8s.io'
      kind: ClusterRole
    - group: 'rbac.authorization.k8s.io'
      kind: ClusterRoleBinding
  
  # Namespace resource whitelist
  namespaceResourceWhitelist:
    - group: ''
      kind: ConfigMap
    - group: ''
      kind: Secret
    - group: ''
      kind: Service
    - group: 'apps'
      kind: Deployment
    - group: 'apps'
      kind: StatefulSet
    - group: 'networking.k8s.io'
      kind: Ingress
  
  # Roles
  roles:
    - name: production-admin
      description: Production admin access
      policies:
        - p, proj:production:production-admin, applications, *, production/*, allow
        - p, proj:production:production-admin, repositories, *, *, allow
      groups:
        - production-admins
    
    - name: production-developer
      description: Production developer access
      policies:
        - p, proj:production:production-developer, applications, get, production/*, allow
        - p, proj:production:production-developer, applications, sync, production/*, allow
      groups:
        - production-developers
  
  # Sync windows
  syncWindows:
    - kind: allow
      schedule: '0 9-17 * * MON-FRI'
      duration: 8h
      applications:
        - '*'
      manualSync: true
    - kind: deny
      schedule: '0 0-8,18-23 * * *'
      duration: 10h
      applications:
        - '*'
      manualSync: false
```

## 🔄 GitOps Workflow Integration

### Jenkins Integration

```groovy
// Jenkinsfile with ArgoCD integration
pipeline {
    agent any
    
    environment {
        ARGOCD_SERVER = 'argocd.company.com'
        ARGOCD_AUTH_TOKEN = credentials('argocd-auth-token')
    }
    
    stages {
        stage('Build and Push') {
            steps {
                script {
                    // Build and push Docker image
                    def imageTag = "${BUILD_NUMBER}"
                    sh """
                        docker build -t myregistry/myapp:${imageTag} .
                        docker push myregistry/myapp:${imageTag}
                    """
                }
            }
        }
        
        stage('Update Manifest') {
            steps {
                script {
                    // Update Kubernetes manifest or Helm values
                    sh """
                        git clone https://github.com/company/app-configs.git
                        cd app-configs
                        sed -i 's|image: myregistry/myapp:.*|image: myregistry/myapp:${BUILD_NUMBER}|' k8s/deployment.yaml
                        git add .
                        git commit -m "Update image tag to ${BUILD_NUMBER}"
                        git push origin main
                    """
                }
            }
        }
        
        stage('Trigger ArgoCD Sync') {
            steps {
                script {
                    sh """
                        argocd app sync myapp --server ${ARGOCD_SERVER} --auth-token ${ARGOCD_AUTH_TOKEN}
                        argocd app wait myapp --timeout 300 --server ${ARGOCD_SERVER} --auth-token ${ARGOCD_AUTH_TOKEN}
                    """
                }
            }
        }
    }
}
```

### GitHub Actions Integration

```yaml
# .github/workflows/deploy.yml
name: Deploy Application
on:
  push:
    branches: [main]
    
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build and Push Image
        run: |
          docker build -t myregistry/myapp:${{ github.sha }} .
          docker push myregistry/myapp:${{ github.sha }}
          
      - name: Update Manifest
        run: |
          git clone https://github.com/company/app-configs.git
          cd app-configs
          sed -i 's|image: myregistry/myapp:.*|image: myregistry/myapp:${{ github.sha }}|' k8s/deployment.yaml
          git add .
          git commit -m "Update image tag to ${{ github.sha }}"
          git push origin main
          
      - name: Sync ArgoCD Application
        uses: clowdhaus/argo-cd-action/@main
        with:
          command: app sync myapp
          options: --server argocd.company.com --auth-token ${{ secrets.ARGOCD_TOKEN }}
```

## 📊 Monitoring and Observability

### Application Health Checks

```yaml
# Custom health check for application
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-cm
  namespace: argocd
data:
  resource.customizations.health.argoproj.io_Rollout: |
    hs = {}
    if obj.status ~= nil then
      if obj.status.replicas ~= nil and obj.status.updatedReplicas ~= nil and obj.status.availableReplicas ~= nil then
        if obj.status.replicas == obj.status.updatedReplicas and obj.status.replicas == obj.status.availableReplicas then
          hs.status = "Healthy"
          hs.message = "Rollout is healthy"
          return hs
        end
      end
    end
    hs.status = "Progressing"
    hs.message = "Rollout is progressing"
    return hs
```

### Notifications Configuration

```yaml
# notifications/notifications-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: argocd-notifications-cm
  namespace: argocd
data:
  service.slack: |
    token: $slack-token
    
  service.email: |
    host: smtp.company.com
    port: 587
    from: argocd@company.com
    
  template.app-deployed: |
    email:
      subject: Application {{.app.metadata.name}} is now running new version.
    slack:
      attachments: |
        [{
          "title": "{{ .app.metadata.name}}",
          "title_link":"{{.context.argocdUrl}}/applications/{{.app.metadata.name}}",
          "color": "#18be52",
          "fields": [
          {
            "title": "Sync Status",
            "value": "{{.app.status.sync.status}}",
            "short": true
          },
          {
            "title": "Repository",
            "value": "{{.app.spec.source.repoURL}}",
            "short": true
          },
          {
            "title": "Revision",
            "value": "{{.app.status.sync.revision}}",
            "short": true
          }
          {{range $index, $c := .app.status.conditions}}
          {{if not $index}},{{end}}
          {{if $index}},{{end}}
          {
            "title": "{{$c.type}}",
            "value": "{{$c.message}}",
            "short": true
          }
          {{end}}
          ]
        }]
        
  template.app-health-degraded: |
    email:
      subject: Application {{.app.metadata.name}} has degraded.
    slack:
      attachments: |
        [{
          "title": "{{ .app.metadata.name}}",
          "title_link": "{{.context.argocdUrl}}/applications/{{.app.metadata.name}}",
          "color": "#f4c430",
          "fields": [
          {
            "title": "Health Status",
            "value": "{{.app.status.health.status}}",
            "short": true
          },
          {
            "title": "Repository",
            "value": "{{.app.spec.source.repoURL}}",
            "short": true
          }
          {{range $index, $c := .app.status.conditions}}
          {{if not $index}},{{end}}
          {{if $index}},{{end}}
          {
            "title": "{{$c.type}}",
            "value": "{{$c.message}}",
            "short": true
          }
          {{end}}
          ]
        }]
        
  trigger.on-deployed: |
    - description: Application is synced and healthy
      send:
      - app-deployed
      when: app.status.operationState.phase in ['Succeeded'] and app.status.health.status == 'Healthy'
      
  trigger.on-health-degraded: |
    - description: Application has degraded
      send:
      - app-health-degraded
      when: app.status.health.status == 'Degraded'
```

### Prometheus Metrics

```yaml
# ServiceMonitor for ArgoCD metrics
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: argocd-metrics
  namespace: argocd
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: argocd-metrics
  endpoints:
  - port: metrics
    interval: 30s
    path: /metrics
---
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: argocd-server-metrics
  namespace: argocd
spec:
  selector:
    matchLabels:
      app.kubernetes.io/name: argocd-server-metrics
  endpoints:
  - port: metrics
    interval: 30s
    path: /metrics
```

## 🔒 Security Best Practices

### 1. Repository Access Control

```yaml
# repositories/private-repo.yaml
apiVersion: v1
kind: Secret
metadata:
  name: private-repo
  namespace: argocd
  labels:
    argocd.argoproj.io/secret-type: repository
type: Opaque
stringData:
  type: git
  url: https://github.com/company/private-repo
  password: ghp_xxxxxxxxxxxxxxxxxxxx
  username: not-used
```

### 2. Cluster Credentials

```yaml
# clusters/production-cluster.yaml
apiVersion: v1
kind: Secret
metadata:
  name: production-cluster
  namespace: argocd
  labels:
    argocd.argoproj.io/secret-type: cluster
type: Opaque
stringData:
  name: production
  server: https://prod-k8s-api.company.com
  config: |
    {
      "bearerToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IiJ9...",
      "tlsClientConfig": {
        "insecure": false,
        "caData": "LS0tLS1CRUdJTi..."
      }
    }
```

### 3. OIDC Integration

```yaml
# OIDC configuration in argocd-cm
data:
  oidc.config: |
    name: OIDC
    issuer: https://auth.company.com
    clientId: argocd
    clientSecret: $oidc.clientSecret
    requestedScopes: ["openid", "profile", "email", "groups"]
    requestedIDTokenClaims: {"groups": {"essential": true}}
  
  # OIDC secret
---
apiVersion: v1
kind: Secret
metadata:
  name: argocd-secret
  namespace: argocd
type: Opaque
stringData:
  oidc.clientSecret: your-oidc-client-secret
```

## 🛠️ Advanced Features

### 1. ApplicationSets

```yaml
# ApplicationSet for multiple environments
apiVersion: argoproj.io/v1alpha1
kind: ApplicationSet
metadata:
  name: myapp-environments
  namespace: argocd
spec:
  generators:
  - list:
      elements:
      - cluster: dev
        url: https://dev-k8s.company.com
        namespace: myapp-dev
        values: values-dev.yaml
      - cluster: staging
        url: https://staging-k8s.company.com
        namespace: myapp-staging
        values: values-staging.yaml
      - cluster: prod
        url: https://prod-k8s.company.com
        namespace: myapp-prod
        values: values-prod.yaml
  template:
    metadata:
      name: 'myapp-{{cluster}}'
    spec:
      project: default
      source:
        repoURL: https://github.com/company/myapp-helm
        targetRevision: main
        path: chart
        helm:
          valueFiles:
          - '{{values}}'
      destination:
        server: '{{url}}'
        namespace: '{{namespace}}'
      syncPolicy:
        automated:
          prune: true
          selfHeal: true
        syncOptions:
        - CreateNamespace=true
```

### 2. Sync Waves and Hooks

```yaml
# Application with sync waves
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: database-app
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "0"  # Deploy first
spec:
  # ... application spec
---
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: backend-app
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "1"  # Deploy after database
spec:
  # ... application spec
---
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: frontend-app
  namespace: argocd
  annotations:
    argocd.argoproj.io/sync-wave: "2"  # Deploy last
spec:
  # ... application spec
```

### 3. Resource Hooks

```yaml
# Pre-sync hook for database migration
apiVersion: batch/v1
kind: Job
metadata:
  name: database-migration
  annotations:
    argocd.argoproj.io/hook: PreSync
    argocd.argoproj.io/hook-delete-policy: BeforeHookCreation
spec:
  template:
    spec:
      containers:
      - name: migrate
        image: myapp:latest
        command: ["./migrate.sh"]
      restartPolicy: Never
```

## 🚨 Troubleshooting

### Common Issues

#### 1. Sync Failures
```bash
# Check application status
argocd app get myapp

# View sync operation details
argocd app get myapp --show-operation

# Check application events
kubectl get events -n myapp --sort-by='.lastTimestamp'
```

#### 2. Repository Access Issues
```bash
# Test repository connection
argocd repo get https://github.com/company/myapp

# Check repository credentials
kubectl get secret -n argocd -l argocd.argoproj.io/secret-type=repository
```

#### 3. RBAC Issues
```bash
# Check user permissions
argocd account can-i sync applications '*'

# View RBAC policy
argocd proj role get production production-developer
```

### Debug Commands

```bash
# List all applications
argocd app list

# Get application details
argocd app get myapp --output yaml

# View application logs
argocd app logs myapp

# Force refresh application
argocd app get myapp --refresh

# Hard refresh (ignore cache)
argocd app get myapp --hard-refresh
```

## 📚 Best Practices

### 1. Application Management
- **Use App of Apps Pattern** - Manage multiple applications declaratively
- **Implement Sync Waves** - Control deployment order
- **Set Resource Limits** - Prevent resource exhaustion
- **Use Health Checks** - Monitor application health
- **Implement Rollback Strategy** - Quick recovery from failures

### 2. Security
- **Use RBAC** - Implement least privilege access
- **Secure Repository Access** - Use proper authentication
- **Enable Audit Logging** - Track all changes
- **Use OIDC/SSO** - Centralized authentication
- **Regular Security Updates** - Keep ArgoCD updated

### 3. Operations
- **Monitor Sync Status** - Set up alerts for sync failures
- **Backup Configuration** - Regular backups of ArgoCD config
- **Use Notifications** - Alert on deployment events
- **Implement GitOps Workflow** - All changes through Git
- **Document Procedures** - Clear operational procedures

---

**Next Steps:**
1. Install ArgoCD in your EKS cluster
2. Configure RBAC and authentication
3. Create your first application
4. Set up notifications and monitoring
5. Implement the App of Apps pattern

For advanced ArgoCD features and troubleshooting, refer to the [ArgoCD Documentation](https://argo-cd.readthedocs.io/) and [ArgoCD Best Practices](https://argoproj.github.io/argo-cd/operator-manual/).