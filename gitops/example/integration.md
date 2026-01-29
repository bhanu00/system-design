# GitOps Component Integration Guide

This document explains how each component in our GitOps workflow integrates with others to deliver the Products API from source code to production.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           GitOps Architecture Flow                             │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Developer Workflow                                                            │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                        │
│  │   Source    │───▶│   GitHub    │───▶│   Jenkins   │                        │
│  │    Code     │    │ Repository  │    │  CI Pipeline│                        │
│  └─────────────┘    └─────────────┘    └─────────────┘                        │
│                                                │                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Build & Package                              │                               │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                        │
│  │   Docker    │◀───│    Build    │───▶│    Helm     │                        │
│  │   Registry  │    │   Process   │    │   Charts    │                        │
│  └─────────────┘    └─────────────┘    └─────────────┘                        │
│                                                │                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Infrastructure & Deployment                  │                               │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                        │
│  │  Terraform  │───▶│     EKS     │◀───│   ArgoCD    │                        │
│  │Infrastructure│    │   Cluster   │    │   GitOps    │                        │
│  └─────────────┘    └─────────────┘    └─────────────┘                        │
│                                                │                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Service Mesh & Monitoring                    │                               │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                        │
│  │    Istio    │───▶│   Products  │◀───│ Monitoring  │                        │
│  │Service Mesh │    │     API     │    │   Stack     │                        │
│  └─────────────┘    └─────────────┘    └─────────────┘                        │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## 🔄 Component Integration Flow

### 1. **GitHub → Jenkins Integration**

**Purpose**: Trigger CI/CD pipeline on code changes

**How it works**:
- Developer pushes code to GitHub repository
- GitHub webhook triggers Jenkins pipeline
- Jenkins pulls source code and starts build process

**Configuration**:
```yaml
# .github/workflows/trigger-jenkins.yml
name: Trigger Jenkins
on:
  push:
    branches: [main, develop]
jobs:
  trigger:
    runs-on: ubuntu-latest
    steps:
    - name: Trigger Jenkins Build
      run: |
        curl -X POST "${{ secrets.JENKINS_URL }}/job/products-api/build" \
        --user "${{ secrets.JENKINS_USER }}:${{ secrets.JENKINS_TOKEN }}"
```

**Key Integration Points**:
- Webhook configuration in GitHub repository settings
- Jenkins GitHub plugin for repository access
- Shared secrets for authentication

---

### 2. **Jenkins → Docker Registry Integration**

**Purpose**: Build and store container images

**How it works**:
- Jenkins builds .NET application
- Creates Docker image using Dockerfile
- Pushes image to container registry (GitHub Container Registry)
- Tags image with build number and git commit hash

**Configuration**:
```groovy
// Jenkinsfile snippet
stage('Build & Push Docker Image') {
    steps {
        script {
            def image = docker.build("ghcr.io/company/products-api:${BUILD_NUMBER}")
            docker.withRegistry('https://ghcr.io', 'github-token') {
                image.push()
                image.push("latest")
            }
        }
    }
}
```

**Key Integration Points**:
- Docker daemon access in Jenkins
- Registry credentials management
- Image tagging strategy

---

### 3. **Jenkins → Helm Charts Integration**

**Purpose**: Package Kubernetes applications

**How it works**:
- Jenkins updates Helm chart values with new image tag
- Packages Helm chart with updated configurations
- Commits changes to GitOps repository

**Configuration**:
```groovy
// Jenkinsfile snippet
stage('Update Helm Chart') {
    steps {
        script {
            sh """
                cd helm-charts/products-api
                sed -i 's/tag: .*/tag: ${BUILD_NUMBER}/' values.yaml
                helm package .
                git add .
                git commit -m "Update image tag to ${BUILD_NUMBER}"
                git push origin main
            """
        }
    }
}
```

**Key Integration Points**:
- Helm CLI in Jenkins environment
- GitOps repository access
- Automated chart versioning

---

### 4. **Terraform → EKS Integration**

**Purpose**: Provision and manage Kubernetes infrastructure

**How it works**:
- Terraform creates EKS cluster with node groups
- Configures networking (VPC, subnets, security groups)
- Sets up IAM roles and policies
- Installs essential add-ons (AWS Load Balancer Controller, EBS CSI Driver)

**Configuration**:
```hcl
# terraform/modules/eks/main.tf
module "eks" {
  source = "terraform-aws-modules/eks/aws"
  
  cluster_name    = "products-api-cluster"
  cluster_version = "1.28"
  
  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnets
  
  node_groups = {
    main = {
      desired_capacity = 3
      max_capacity     = 6
      min_capacity     = 1
      instance_types   = ["m5.large"]
    }
  }
}
```

**Key Integration Points**:
- AWS provider configuration
- VPC and networking setup
- IAM service accounts for workloads
- Add-on installations

---

### 5. **ArgoCD → EKS Integration**

**Purpose**: Automated GitOps deployment

**How it works**:
- ArgoCD monitors GitOps repository for changes
- Detects Helm chart updates
- Automatically syncs changes to EKS cluster
- Manages application lifecycle and rollbacks

**Configuration**:
```yaml
# argocd/applications/products-api.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: products-api
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/company/gitops-configs
    targetRevision: main
    path: helm-charts/products-api
  destination:
    server: https://kubernetes.default.svc
    namespace: products-api
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

**Key Integration Points**:
- Repository access credentials
- Kubernetes RBAC permissions
- Sync policies and strategies
- Health checks and monitoring

---

### 6. **Istio → Products API Integration**

**Purpose**: Service mesh for traffic management and security

**How it works**:
- Istio injects Envoy sidecars into application pods
- Manages ingress traffic through Istio Gateway
- Implements security policies (mTLS, RBAC)
- Provides observability (metrics, tracing, logs)

**Configuration**:
```yaml
# istio/gateway.yaml
apiVersion: networking.istio.io/v1beta1
kind: Gateway
metadata:
  name: products-api-gateway
spec:
  selector:
    istio: ingressgateway
  servers:
  - port:
      number: 443
      name: https
      protocol: HTTPS
    tls:
      mode: SIMPLE
      credentialName: products-api-tls
    hosts:
    - api.company.com
```

**Key Integration Points**:
- Automatic sidecar injection
- Gateway and VirtualService configuration
- Certificate management
- Observability integration

---

### 7. **Monitoring Stack Integration**

**Purpose**: Observability and alerting

**How it works**:
- Prometheus scrapes metrics from application and Istio
- Grafana visualizes metrics with custom dashboards
- Jaeger collects distributed traces
- AlertManager sends notifications

**Configuration**:
```yaml
# monitoring/prometheus/servicemonitor.yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: products-api
spec:
  selector:
    matchLabels:
      app: products-api
  endpoints:
  - port: metrics
    interval: 30s
    path: /metrics
```

**Key Integration Points**:
- Service discovery configuration
- Metrics exposition from application
- Dashboard provisioning
- Alert rule configuration

---

## 🔗 Data Flow Between Components

### 1. **Source Code to Container Image**
```
Developer Code → GitHub → Jenkins → Docker Build → Container Registry
```

### 2. **Configuration Management**
```
Helm Charts → GitOps Repo → ArgoCD → Kubernetes Manifests → EKS Cluster
```

### 3. **Infrastructure Provisioning**
```
Terraform Code → AWS APIs → EKS Cluster → Node Groups → Add-ons
```

### 4. **Traffic Flow**
```
External Request → Istio Gateway → Envoy Proxy → Products API → Database
```

### 5. **Monitoring Data**
```
Application Metrics → Prometheus → Grafana Dashboards → Alerts
```

## 🔧 Key Integration Patterns

### 1. **GitOps Pattern**
- **Single Source of Truth**: All configurations in Git
- **Declarative**: Desired state defined in manifests
- **Automated**: ArgoCD ensures actual state matches desired state
- **Auditable**: All changes tracked in Git history

### 2. **Infrastructure as Code**
- **Versioned**: Infrastructure changes tracked in Git
- **Repeatable**: Same infrastructure across environments
- **Testable**: Infrastructure changes can be validated
- **Collaborative**: Team can review infrastructure changes

### 3. **Service Mesh Pattern**
- **Sidecar Proxy**: Envoy handles all network communication
- **Centralized Policy**: Security and traffic policies managed centrally
- **Observability**: Automatic metrics and tracing collection
- **Zero Trust**: mTLS between all services

### 4. **CI/CD Pipeline Pattern**
- **Automated Testing**: Every commit triggers tests
- **Immutable Artifacts**: Container images never change
- **Progressive Deployment**: Gradual rollout with monitoring
- **Rollback Capability**: Quick rollback on issues

## 🚨 Common Integration Challenges

### 1. **Secret Management**
**Challenge**: Securely managing secrets across components
**Solution**: 
- Use Kubernetes secrets with encryption at rest
- External secret management (AWS Secrets Manager)
- Sealed secrets for GitOps

### 2. **Network Connectivity**
**Challenge**: Components need to communicate securely
**Solution**:
- VPC configuration with proper subnets
- Security groups and network policies
- Service mesh for secure communication

### 3. **State Management**
**Challenge**: Managing Terraform state across team
**Solution**:
- Remote state backend (S3 + DynamoDB)
- State locking to prevent conflicts
- Workspace separation for environments

### 4. **Configuration Drift**
**Challenge**: Manual changes causing configuration drift
**Solution**:
- ArgoCD self-healing to revert manual changes
- RBAC to prevent unauthorized changes
- Regular drift detection and alerts

## 📊 Integration Monitoring

### Key Metrics to Monitor:
1. **Pipeline Success Rate**: Jenkins build success percentage
2. **Deployment Frequency**: How often deployments occur
3. **Lead Time**: Time from commit to production
4. **Mean Time to Recovery**: Time to fix production issues
5. **Application Performance**: Response times, error rates

### Monitoring Tools Integration:
- **Prometheus**: Metrics collection from all components
- **Grafana**: Unified dashboards for all metrics
- **Jaeger**: Distributed tracing across services
- **ELK Stack**: Centralized logging

## 🎯 Best Practices for Integration

1. **Loose Coupling**: Components should be independently deployable
2. **Event-Driven**: Use events for component communication
3. **Idempotent Operations**: Operations should be safe to retry
4. **Circuit Breakers**: Prevent cascade failures
5. **Health Checks**: Every component should expose health endpoints
6. **Graceful Degradation**: System should work even if some components fail

---

**Next Steps**: Follow the [workshop.md](workshop.md) guide to implement this integration step by step.