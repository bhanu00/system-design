# Helm Package Manager

Helm is the package manager for Kubernetes that simplifies the deployment and management of applications in our GitOps workflow.

## 🎯 Overview

Helm in our GitOps architecture provides:
- **Package Management** - Bundles Kubernetes manifests into reusable charts
- **Templating** - Dynamic configuration through values and templates
- **Release Management** - Versioned deployments with rollback capabilities
- **Dependency Management** - Manages chart dependencies and sub-charts
- **Configuration Management** - Environment-specific configurations

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Helm Ecosystem                          │
├─────────────────────────────────────────────────────────────┤
│  Chart Repository                                          │
│  ├── Public Charts (Bitnami, Stable)                       │
│  ├── Private Charts (Internal Applications)                │
│  └── OCI Registry (Harbor, ECR)                            │
├─────────────────────────────────────────────────────────────┤
│  Helm Client                                               │
│  ├── Chart Development                                     │
│  ├── Release Management                                    │
│  └── Template Rendering                                    │
├─────────────────────────────────────────────────────────────┤
│  Kubernetes Cluster                                        │
│  ├── Deployed Applications                                 │
│  ├── Release History                                       │
│  └── Configuration Secrets                                 │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
helm/
├── README.md
├── charts/
│   ├── microservice-template/
│   │   ├── Chart.yaml
│   │   ├── values.yaml
│   │   ├── values-dev.yaml
│   │   ├── values-staging.yaml
│   │   ├── values-prod.yaml
│   │   └── templates/
│   │       ├── deployment.yaml
│   │       ├── service.yaml
│   │       ├── ingress.yaml
│   │       ├── configmap.yaml
│   │       ├── secret.yaml
│   │       ├── hpa.yaml
│   │       └── _helpers.tpl
│   ├── frontend-app/
│   ├── backend-api/
│   └── database/
├── repositories/
│   ├── bitnami.yaml
│   ├── stable.yaml
│   └── internal.yaml
├── scripts/
│   ├── install-helm.sh
│   ├── package-charts.sh
│   ├── deploy.sh
│   └── rollback.sh
├── environments/
│   ├── dev/
│   ├── staging/
│   └── prod/
└── examples/
    ├── simple-app/
    ├── microservice/
    └── database-app/
```

## 🚀 Quick Setup

### 1. Install Helm

```bash
# Install Helm (Linux/macOS)
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Verify installation
helm version

# Add common repositories
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo add stable https://charts.helm.sh/stable
helm repo update
```

### 2. Create Your First Chart

```bash
# Create a new chart
helm create myapp

# Chart structure will be created
myapp/
├── Chart.yaml
├── values.yaml
└── templates/
    ├── deployment.yaml
    ├── service.yaml
    ├── ingress.yaml
    └── _helpers.tpl
```

### 3. Deploy Application

```bash
# Install chart
helm install myapp ./myapp

# Install with custom values
helm install myapp ./myapp -f values-prod.yaml

# Upgrade release
helm upgrade myapp ./myapp

# Check status
helm status myapp
```

## 📝 Chart Development

### Chart.yaml Structure

```yaml
# Chart.yaml
apiVersion: v2
name: microservice-template
description: A Helm chart for microservice deployment
type: application
version: 0.1.0
appVersion: "1.0.0"

keywords:
  - microservice
  - api
  - web

home: https://github.com/company/microservice-template
sources:
  - https://github.com/company/microservice-template

maintainers:
  - name: DevOps Team
    email: devops@company.com

dependencies:
  - name: postgresql
    version: 12.1.2
    repository: https://charts.bitnami.com/bitnami
    condition: postgresql.enabled
  - name: redis
    version: 17.3.7
    repository: https://charts.bitnami.com/bitnami
    condition: redis.enabled

annotations:
  category: Application
```

### Values.yaml Configuration

```yaml
# values.yaml
# Default values for microservice-template

replicaCount: 3

image:
  repository: myregistry/myapp
  pullPolicy: IfNotPresent
  tag: "latest"

imagePullSecrets: []
nameOverride: ""
fullnameOverride: ""

serviceAccount:
  create: true
  annotations: {}
  name: ""

podAnnotations: {}

podSecurityContext:
  fsGroup: 2000

securityContext:
  capabilities:
    drop:
    - ALL
  readOnlyRootFilesystem: true
  runAsNonRoot: true
  runAsUser: 1000

service:
  type: ClusterIP
  port: 80
  targetPort: 8080

ingress:
  enabled: false
  className: ""
  annotations: {}
  hosts:
    - host: chart-example.local
      paths:
        - path: /
          pathType: Prefix
  tls: []

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 250m
    memory: 256Mi

autoscaling:
  enabled: false
  minReplicas: 1
  maxReplicas: 100
  targetCPUUtilizationPercentage: 80

nodeSelector: {}

tolerations: []

affinity: {}

# Application specific configuration
config:
  database:
    host: ""
    port: 5432
    name: myapp
  redis:
    host: ""
    port: 6379
  logging:
    level: INFO
  features:
    enableMetrics: true
    enableTracing: true

# External dependencies
postgresql:
  enabled: true
  auth:
    postgresPassword: "changeme"
    database: "myapp"

redis:
  enabled: true
  auth:
    enabled: false
```

### Environment-Specific Values

```yaml
# values-prod.yaml
replicaCount: 5

image:
  tag: "v1.2.3"

resources:
  limits:
    cpu: 1000m
    memory: 1Gi
  requests:
    cpu: 500m
    memory: 512Mi

autoscaling:
  enabled: true
  minReplicas: 3
  maxReplicas: 20
  targetCPUUtilizationPercentage: 70

ingress:
  enabled: true
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
  hosts:
    - host: api.company.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: api-tls
      hosts:
        - api.company.com

config:
  database:
    host: "prod-postgres.company.com"
  redis:
    host: "prod-redis.company.com"
  logging:
    level: WARN

postgresql:
  enabled: false  # Use external database in production

redis:
  enabled: false  # Use external Redis in production
```

## 🔧 Template Development

### Deployment Template

```yaml
# templates/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "microservice-template.fullname" . }}
  labels:
    {{- include "microservice-template.labels" . | nindent 4 }}
spec:
  {{- if not .Values.autoscaling.enabled }}
  replicas: {{ .Values.replicaCount }}
  {{- end }}
  selector:
    matchLabels:
      {{- include "microservice-template.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      annotations:
        checksum/config: {{ include (print $.Template.BasePath "/configmap.yaml") . | sha256sum }}
        {{- with .Values.podAnnotations }}
        {{- toYaml . | nindent 8 }}
        {{- end }}
      labels:
        {{- include "microservice-template.selectorLabels" . | nindent 8 }}
    spec:
      {{- with .Values.imagePullSecrets }}
      imagePullSecrets:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      serviceAccountName: {{ include "microservice-template.serviceAccountName" . }}
      securityContext:
        {{- toYaml .Values.podSecurityContext | nindent 8 }}
      containers:
        - name: {{ .Chart.Name }}
          securityContext:
            {{- toYaml .Values.securityContext | nindent 12 }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - name: http
              containerPort: {{ .Values.service.targetPort }}
              protocol: TCP
          livenessProbe:
            httpGet:
              path: /health
              port: http
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /ready
              port: http
            initialDelaySeconds: 5
            periodSeconds: 5
          env:
            - name: DATABASE_HOST
              value: {{ .Values.config.database.host | quote }}
            - name: DATABASE_PORT
              value: {{ .Values.config.database.port | quote }}
            - name: DATABASE_NAME
              value: {{ .Values.config.database.name | quote }}
            - name: REDIS_HOST
              value: {{ .Values.config.redis.host | quote }}
            - name: REDIS_PORT
              value: {{ .Values.config.redis.port | quote }}
            - name: LOG_LEVEL
              value: {{ .Values.config.logging.level | quote }}
          envFrom:
            - configMapRef:
                name: {{ include "microservice-template.fullname" . }}-config
            - secretRef:
                name: {{ include "microservice-template.fullname" . }}-secret
          resources:
            {{- toYaml .Values.resources | nindent 12 }}
          volumeMounts:
            - name: tmp
              mountPath: /tmp
            - name: cache
              mountPath: /app/cache
      volumes:
        - name: tmp
          emptyDir: {}
        - name: cache
          emptyDir: {}
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.affinity }}
      affinity:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.tolerations }}
      tolerations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
```

### Service Template

```yaml
# templates/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ include "microservice-template.fullname" . }}
  labels:
    {{- include "microservice-template.labels" . | nindent 4 }}
spec:
  type: {{ .Values.service.type }}
  ports:
    - port: {{ .Values.service.port }}
      targetPort: http
      protocol: TCP
      name: http
  selector:
    {{- include "microservice-template.selectorLabels" . | nindent 4 }}
```

### ConfigMap Template

```yaml
# templates/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: {{ include "microservice-template.fullname" . }}-config
  labels:
    {{- include "microservice-template.labels" . | nindent 4 }}
data:
  app.properties: |
    # Database Configuration
    database.host={{ .Values.config.database.host }}
    database.port={{ .Values.config.database.port }}
    database.name={{ .Values.config.database.name }}
    
    # Redis Configuration
    redis.host={{ .Values.config.redis.host }}
    redis.port={{ .Values.config.redis.port }}
    
    # Logging Configuration
    logging.level={{ .Values.config.logging.level }}
    
    # Feature Flags
    features.metrics.enabled={{ .Values.config.features.enableMetrics }}
    features.tracing.enabled={{ .Values.config.features.enableTracing }}
  
  {{- if .Values.config.customConfig }}
  custom.properties: |
    {{- .Values.config.customConfig | nindent 4 }}
  {{- end }}
```

### Helper Templates

```yaml
# templates/_helpers.tpl
{{/*
Expand the name of the chart.
*/}}
{{- define "microservice-template.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "microservice-template.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "microservice-template.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "microservice-template.labels" -}}
helm.sh/chart: {{ include "microservice-template.chart" . }}
{{ include "microservice-template.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "microservice-template.selectorLabels" -}}
app.kubernetes.io/name: {{ include "microservice-template.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "microservice-template.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "microservice-template.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
```

## 🔄 GitOps Integration

### Jenkins Pipeline Integration

```groovy
// Jenkinsfile with Helm deployment
pipeline {
    agent any
    
    environment {
        HELM_CHART_PATH = './helm/charts/myapp'
        DOCKER_REGISTRY = 'myregistry.com'
        IMAGE_NAME = 'myapp'
    }
    
    stages {
        stage('Build and Push Image') {
            steps {
                script {
                    def imageTag = "${BUILD_NUMBER}"
                    sh """
                        docker build -t ${DOCKER_REGISTRY}/${IMAGE_NAME}:${imageTag} .
                        docker push ${DOCKER_REGISTRY}/${IMAGE_NAME}:${imageTag}
                    """
                }
            }
        }
        
        stage('Deploy to Dev') {
            steps {
                script {
                    sh """
                        helm upgrade --install myapp-dev ${HELM_CHART_PATH} \\
                            --namespace dev \\
                            --create-namespace \\
                            --set image.tag=${BUILD_NUMBER} \\
                            --values ${HELM_CHART_PATH}/values-dev.yaml
                    """
                }
            }
        }
        
        stage('Deploy to Staging') {
            when {
                branch 'main'
            }
            steps {
                script {
                    sh """
                        helm upgrade --install myapp-staging ${HELM_CHART_PATH} \\
                            --namespace staging \\
                            --create-namespace \\
                            --set image.tag=${BUILD_NUMBER} \\
                            --values ${HELM_CHART_PATH}/values-staging.yaml
                    """
                }
            }
        }
        
        stage('Deploy to Production') {
            when {
                tag pattern: "v\\d+\\.\\d+\\.\\d+", comparator: "REGEXP"
            }
            steps {
                input message: 'Deploy to production?', ok: 'Deploy'
                script {
                    sh """
                        helm upgrade --install myapp-prod ${HELM_CHART_PATH} \\
                            --namespace production \\
                            --create-namespace \\
                            --set image.tag=${TAG_NAME} \\
                            --values ${HELM_CHART_PATH}/values-prod.yaml
                    """
                }
            }
        }
    }
}
```

### ArgoCD Application

```yaml
# argocd/applications/myapp.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: myapp
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/company/myapp-charts
    targetRevision: HEAD
    path: charts/myapp
    helm:
      valueFiles:
        - values-prod.yaml
      parameters:
        - name: image.tag
          value: v1.2.3
  destination:
    server: https://kubernetes.default.svc
    namespace: production
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

## 📊 Chart Testing and Validation

### Unit Testing with helm-unittest

```bash
# Install helm-unittest plugin
helm plugin install https://github.com/quintush/helm-unittest

# Create test file
mkdir charts/myapp/tests
```

```yaml
# charts/myapp/tests/deployment_test.yaml
suite: test deployment
templates:
  - deployment.yaml
tests:
  - it: should create deployment with correct name
    asserts:
      - isKind:
          of: Deployment
      - equal:
          path: metadata.name
          value: RELEASE-NAME-myapp
          
  - it: should set correct image
    set:
      image.repository: myregistry/myapp
      image.tag: v1.0.0
    asserts:
      - equal:
          path: spec.template.spec.containers[0].image
          value: myregistry/myapp:v1.0.0
          
  - it: should set resource limits
    asserts:
      - equal:
          path: spec.template.spec.containers[0].resources.limits.cpu
          value: 500m
      - equal:
          path: spec.template.spec.containers[0].resources.limits.memory
          value: 512Mi
```

```bash
# Run tests
helm unittest charts/myapp
```

### Chart Linting

```bash
# Lint chart
helm lint charts/myapp

# Template validation
helm template myapp charts/myapp --values charts/myapp/values-prod.yaml

# Dry run installation
helm install myapp charts/myapp --dry-run --debug
```

## 🔒 Security Best Practices

### 1. Secure Values Management

```bash
# Use Helm secrets plugin
helm plugin install https://github.com/jkroepke/helm-secrets

# Encrypt sensitive values
helm secrets enc values-prod.yaml

# Deploy with encrypted values
helm secrets upgrade --install myapp ./charts/myapp -f values-prod.yaml
```

### 2. RBAC for Helm

```yaml
# rbac.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: helm-deployer
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: helm-deployer
rules:
- apiGroups: ["*"]
  resources: ["*"]
  verbs: ["*"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: helm-deployer
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: helm-deployer
subjects:
- kind: ServiceAccount
  name: helm-deployer
  namespace: kube-system
```

### 3. Chart Signing

```bash
# Generate GPG key for signing
gpg --gen-key

# Sign chart package
helm package --sign --key 'Your Name' --keyring ~/.gnupg/secring.gpg charts/myapp

# Verify signed chart
helm verify myapp-0.1.0.tgz
```

## 🛠️ Advanced Features

### 1. Chart Dependencies

```yaml
# Chart.yaml
dependencies:
  - name: postgresql
    version: "12.1.2"
    repository: "https://charts.bitnami.com/bitnami"
    condition: postgresql.enabled
  - name: redis
    version: "17.3.7"
    repository: "https://charts.bitnami.com/bitnami"
    condition: redis.enabled
```

```bash
# Update dependencies
helm dependency update charts/myapp

# Build dependencies
helm dependency build charts/myapp
```

### 2. Hooks and Tests

```yaml
# templates/tests/test-connection.yaml
apiVersion: v1
kind: Pod
metadata:
  name: "{{ include "myapp.fullname" . }}-test"
  labels:
    {{- include "myapp.labels" . | nindent 4 }}
  annotations:
    "helm.sh/hook": test
spec:
  restartPolicy: Never
  containers:
    - name: wget
      image: busybox
      command: ['wget']
      args: ['{{ include "myapp.fullname" . }}:{{ .Values.service.port }}']
```

```bash
# Run tests
helm test myapp
```

### 3. Post-Install Hooks

```yaml
# templates/post-install-job.yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: "{{ include "myapp.fullname" . }}-post-install"
  labels:
    {{- include "myapp.labels" . | nindent 4 }}
  annotations:
    "helm.sh/hook": post-install
    "helm.sh/hook-weight": "-5"
    "helm.sh/hook-delete-policy": hook-succeeded
spec:
  template:
    metadata:
      name: "{{ include "myapp.fullname" . }}-post-install"
    spec:
      restartPolicy: Never
      containers:
      - name: post-install-job
        image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
        command: ["/bin/sh"]
        args: ["-c", "echo 'Post-install setup complete'"]
```

## 📚 Best Practices

### 1. Chart Development
- **Use Semantic Versioning** - Version charts consistently
- **Template Everything** - Make charts configurable through values
- **Include Documentation** - Document all values and templates
- **Test Thoroughly** - Use helm-unittest and integration tests
- **Follow Naming Conventions** - Use consistent naming patterns

### 2. Values Management
- **Environment-Specific Values** - Separate values files per environment
- **Secure Secrets** - Use external secret management
- **Default Values** - Provide sensible defaults
- **Validation** - Validate input values
- **Documentation** - Comment all values

### 3. Release Management
- **Atomic Deployments** - Use --atomic flag for rollback on failure
- **Release History** - Keep reasonable history limit
- **Rollback Strategy** - Test rollback procedures
- **Monitoring** - Monitor release health
- **Cleanup** - Clean up failed releases

## 🚨 Troubleshooting

### Common Issues

#### 1. Template Rendering Errors
```bash
# Debug template rendering
helm template myapp ./charts/myapp --debug

# Check specific template
helm get manifest myapp
```

#### 2. Release Issues
```bash
# Check release status
helm status myapp

# View release history
helm history myapp

# Rollback release
helm rollback myapp 1
```

#### 3. Dependency Problems
```bash
# Update dependencies
helm dependency update ./charts/myapp

# Check dependency status
helm dependency list ./charts/myapp
```

### Debug Commands

```bash
# List all releases
helm list --all-namespaces

# Get release values
helm get values myapp

# Get release notes
helm get notes myapp

# Uninstall release
helm uninstall myapp --keep-history
```

---

**Next Steps:**
1. Create your first Helm chart using the provided templates
2. Set up environment-specific values files
3. Integrate with your CI/CD pipeline
4. Implement chart testing and validation
5. Set up chart repository for sharing

For advanced Helm features and best practices, refer to the [Helm Documentation](https://helm.sh/docs/) and [Helm Best Practices Guide](https://helm.sh/docs/chart_best_practices/).