# Amazon EKS (Elastic Kubernetes Service)

Amazon EKS is our managed Kubernetes service that provides the foundation for our containerized applications and GitOps workflow.

## 🎯 Overview

EKS in our GitOps architecture serves as:
- **Container Orchestration** - Manages application containers and workloads
- **Service Discovery** - Enables service-to-service communication
- **Load Balancing** - Distributes traffic across application instances
- **Auto Scaling** - Automatically scales applications based on demand
- **Security** - Provides RBAC, network policies, and pod security

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        EKS Cluster                         │
├─────────────────────────────────────────────────────────────┤
│  Control Plane (Managed by AWS)                            │
│  ├── API Server                                            │
│  ├── etcd                                                  │
│  ├── Controller Manager                                    │
│  └── Scheduler                                             │
├─────────────────────────────────────────────────────────────┤
│  Data Plane (Managed by You)                               │
│  ├── Node Group 1 (On-Demand)                              │
│  │   ├── Application Pods                                  │
│  │   ├── System Pods (kube-proxy, aws-node)               │
│  │   └── Add-on Pods (ArgoCD, Istio, etc.)                │
│  ├── Node Group 2 (Spot Instances)                         │
│  │   └── Batch/Non-Critical Workloads                     │
│  └── Fargate Profile (Serverless)                          │
│      └── Specific Workloads                                │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
eks/
├── README.md
├── cluster-config/
│   ├── cluster.yaml
│   ├── nodegroups.yaml
│   └── fargate-profiles.yaml
├── addons/
│   ├── aws-load-balancer-controller/
│   ├── cluster-autoscaler/
│   ├── ebs-csi-driver/
│   └── vpc-cni/
├── security/
│   ├── rbac.yaml
│   ├── network-policies.yaml
│   ├── pod-security-policies.yaml
│   └── service-accounts.yaml
├── monitoring/
│   ├── cloudwatch-insights/
│   ├── prometheus/
│   └── grafana/
├── terraform/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── modules/
└── scripts/
    ├── setup-cluster.sh
    ├── configure-kubectl.sh
    └── install-addons.sh
```

## 🚀 Quick Setup

### 1. Prerequisites

```bash
# Install required tools
aws --version          # AWS CLI v2
eksctl version         # eksctl
kubectl version        # kubectl
helm version           # Helm v3
```

### 2. Create EKS Cluster

#### Using eksctl (Quick Start)

```bash
# Create cluster with eksctl
eksctl create cluster \
  --name gitops-cluster \
  --version 1.28 \
  --region us-west-2 \
  --nodegroup-name standard-workers \
  --node-type m5.large \
  --nodes 3 \
  --nodes-min 1 \
  --nodes-max 4 \
  --managed
```

#### Using Terraform (Production)

```hcl
# terraform/main.tf
module "eks" {
  source = "terraform-aws-modules/eks/aws"
  version = "~> 19.0"

  cluster_name    = "gitops-cluster"
  cluster_version = "1.28"

  vpc_id                         = module.vpc.vpc_id
  subnet_ids                     = module.vpc.private_subnets
  cluster_endpoint_public_access = true

  eks_managed_node_groups = {
    main = {
      name = "main-node-group"
      
      instance_types = ["m5.large"]
      
      min_size     = 1
      max_size     = 4
      desired_size = 3
      
      disk_size = 50
      
      labels = {
        Environment = "production"
        NodeGroup   = "main"
      }
      
      taints = []
    }
    
    spot = {
      name = "spot-node-group"
      
      instance_types = ["m5.large", "m5a.large", "m4.large"]
      capacity_type  = "SPOT"
      
      min_size     = 0
      max_size     = 10
      desired_size = 2
      
      labels = {
        Environment = "production"
        NodeGroup   = "spot"
      }
      
      taints = [
        {
          key    = "spot-instance"
          value  = "true"
          effect = "NO_SCHEDULE"
        }
      ]
    }
  }

  fargate_profiles = {
    default = {
      name = "default"
      selectors = [
        {
          namespace = "kube-system"
          labels = {
            k8s-app = "kube-dns"
          }
        },
        {
          namespace = "default"
        }
      ]
    }
  }

  tags = {
    Environment = "production"
    Terraform   = "true"
  }
}
```

### 3. Configure kubectl

```bash
# Update kubeconfig
aws eks update-kubeconfig --region us-west-2 --name gitops-cluster

# Verify connection
kubectl get nodes
kubectl get pods --all-namespaces
```

## 🔧 Essential Add-ons

### 1. AWS Load Balancer Controller

```bash
# Install AWS Load Balancer Controller
helm repo add eks https://aws.github.io/eks-charts
helm repo update

helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=gitops-cluster \
  --set serviceAccount.create=false \
  --set serviceAccount.name=aws-load-balancer-controller
```

### 2. Cluster Autoscaler

```yaml
# addons/cluster-autoscaler/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: cluster-autoscaler
  namespace: kube-system
spec:
  replicas: 1
  selector:
    matchLabels:
      app: cluster-autoscaler
  template:
    metadata:
      labels:
        app: cluster-autoscaler
    spec:
      serviceAccountName: cluster-autoscaler
      containers:
      - image: k8s.gcr.io/autoscaling/cluster-autoscaler:v1.28.0
        name: cluster-autoscaler
        resources:
          limits:
            cpu: 100m
            memory: 300Mi
          requests:
            cpu: 100m
            memory: 300Mi
        command:
        - ./cluster-autoscaler
        - --v=4
        - --stderrthreshold=info
        - --cloud-provider=aws
        - --skip-nodes-with-local-storage=false
        - --expander=least-waste
        - --node-group-auto-discovery=asg:tag=k8s.io/cluster-autoscaler/enabled,k8s.io/cluster-autoscaler/gitops-cluster
        env:
        - name: AWS_REGION
          value: us-west-2
```

### 3. EBS CSI Driver

```bash
# Install EBS CSI Driver
helm repo add aws-ebs-csi-driver https://kubernetes-sigs.github.io/aws-ebs-csi-driver
helm repo update

helm install aws-ebs-csi-driver aws-ebs-csi-driver/aws-ebs-csi-driver \
  --namespace kube-system \
  --set enableVolumeScheduling=true \
  --set enableVolumeResizing=true \
  --set enableVolumeSnapshot=true
```

## 🔒 Security Configuration

### 1. RBAC (Role-Based Access Control)

```yaml
# security/rbac.yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: developer-role
rules:
- apiGroups: [""]
  resources: ["pods", "services", "configmaps", "secrets"]
  verbs: ["get", "list", "create", "update", "patch", "delete"]
- apiGroups: ["apps"]
  resources: ["deployments", "replicasets"]
  verbs: ["get", "list", "create", "update", "patch", "delete"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: developer-binding
subjects:
- kind: User
  name: developer@company.com
  apiGroup: rbac.authorization.k8s.io
roleRef:
  kind: ClusterRole
  name: developer-role
  apiGroup: rbac.authorization.k8s.io
```

### 2. Network Policies

```yaml
# security/network-policies.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: deny-all-ingress
  namespace: production
spec:
  podSelector: {}
  policyTypes:
  - Ingress
---
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-same-namespace
  namespace: production
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: production
```

### 3. Pod Security Standards

```yaml
# security/pod-security-policies.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: production
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

## 📊 Monitoring and Observability

### 1. CloudWatch Container Insights

```bash
# Install CloudWatch agent
curl https://raw.githubusercontent.com/aws-samples/amazon-cloudwatch-container-insights/latest/k8s-deployment-manifest-templates/deployment-mode/daemonset/container-insights-monitoring/quickstart/cwagent-fluentd-quickstart.yaml | sed "s/{{cluster_name}}/gitops-cluster/;s/{{region_name}}/us-west-2/" | kubectl apply -f -
```

### 2. Prometheus and Grafana

```bash
# Install kube-prometheus-stack
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

helm install prometheus prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace \
  --set prometheus.prometheusSpec.serviceMonitorSelectorNilUsesHelmValues=false \
  --set prometheus.prometheusSpec.podMonitorSelectorNilUsesHelmValues=false
```

### 3. Logging with Fluent Bit

```yaml
# monitoring/fluent-bit.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: fluent-bit-config
  namespace: amazon-cloudwatch
data:
  fluent-bit.conf: |
    [SERVICE]
        Flush                     5
        Grace                     30
        Log_Level                 info
        Daemon                    off
        Parsers_File              parsers.conf
        HTTP_Server               On
        HTTP_Listen               0.0.0.0
        HTTP_Port                 2020
        storage.path              /var/fluent-bit/state/flb-storage/
        storage.sync              normal
        storage.checksum          off
        storage.backlog.mem_limit 5M
        
    [INPUT]
        Name                tail
        Tag                 application.*
        Exclude_Path        /var/log/containers/cloudwatch-agent*, /var/log/containers/fluent-bit*, /var/log/containers/aws-node*, /var/log/containers/kube-proxy*
        Path                /var/log/containers/*.log
        Docker_Mode         On
        Docker_Mode_Flush   5
        Docker_Mode_Parser  container_firstline
        Parser              docker
        DB                  /var/fluent-bit/state/flb_container.db
        Mem_Buf_Limit       50MB
        Skip_Long_Lines     On
        Refresh_Interval    10
        Rotate_Wait         30
        storage.type        filesystem
        Read_from_Head      Off
        
    [OUTPUT]
        Name                cloudwatch_logs
        Match               application.*
        region              us-west-2
        log_group_name      /aws/containerinsights/gitops-cluster/application
        log_stream_prefix   ${hostname}-
        auto_create_group   true
        extra_user_agent    container-insights
```

## 🔄 GitOps Integration

### 1. ArgoCD Installation

```bash
# Create ArgoCD namespace
kubectl create namespace argocd

# Install ArgoCD
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Expose ArgoCD server
kubectl patch svc argocd-server -n argocd -p '{"spec": {"type": "LoadBalancer"}}'
```

### 2. Istio Service Mesh

```bash
# Install Istio
curl -L https://istio.io/downloadIstio | sh -
cd istio-*
export PATH=$PWD/bin:$PATH

# Install Istio on EKS
istioctl install --set values.pilot.traceSampling=100

# Enable Istio injection for default namespace
kubectl label namespace default istio-injection=enabled
```

## 🛠️ Operational Tasks

### 1. Cluster Upgrades

```bash
# Check current version
kubectl version --short

# Upgrade control plane
aws eks update-cluster-version --region us-west-2 --name gitops-cluster --kubernetes-version 1.28

# Upgrade node groups
aws eks update-nodegroup-version --cluster-name gitops-cluster --nodegroup-name main-node-group --kubernetes-version 1.28
```

### 2. Node Management

```bash
# Scale node group
aws eks update-nodegroup-config \
  --cluster-name gitops-cluster \
  --nodegroup-name main-node-group \
  --scaling-config minSize=2,maxSize=6,desiredSize=4

# Drain node for maintenance
kubectl drain <node-name> --ignore-daemonsets --delete-emptydir-data

# Uncordon node after maintenance
kubectl uncordon <node-name>
```

### 3. Backup and Disaster Recovery

```bash
# Backup etcd (managed by AWS, but backup application data)
kubectl get all --all-namespaces -o yaml > cluster-backup.yaml

# Backup persistent volumes
kubectl get pv -o yaml > pv-backup.yaml
kubectl get pvc --all-namespaces -o yaml > pvc-backup.yaml
```

## 🚨 Troubleshooting

### Common Issues

#### 1. Node Not Ready
```bash
# Check node status
kubectl get nodes
kubectl describe node <node-name>

# Check node logs
kubectl logs -n kube-system -l k8s-app=aws-node
```

#### 2. Pod Scheduling Issues
```bash
# Check pod events
kubectl describe pod <pod-name>

# Check resource availability
kubectl top nodes
kubectl top pods --all-namespaces
```

#### 3. Network Connectivity Issues
```bash
# Test DNS resolution
kubectl run -it --rm debug --image=busybox --restart=Never -- nslookup kubernetes.default

# Check CNI plugin
kubectl logs -n kube-system -l k8s-app=aws-node
```

### Debug Commands

```bash
# Cluster information
kubectl cluster-info
kubectl get componentstatuses

# Check all resources
kubectl get all --all-namespaces

# Check events
kubectl get events --all-namespaces --sort-by='.lastTimestamp'

# Check logs
kubectl logs -n kube-system deployment/coredns
kubectl logs -n kube-system daemonset/aws-node
```

## 📚 Best Practices

### 1. Resource Management
- **Set Resource Requests and Limits** - Ensure proper resource allocation
- **Use Horizontal Pod Autoscaler** - Automatically scale based on metrics
- **Implement Vertical Pod Autoscaler** - Right-size container resources
- **Use Node Affinity** - Control pod placement on nodes

### 2. Security
- **Enable Pod Security Standards** - Enforce security policies
- **Use Network Policies** - Control network traffic between pods
- **Implement RBAC** - Least privilege access control
- **Regular Security Scans** - Scan images and configurations

### 3. High Availability
- **Multi-AZ Deployment** - Distribute workloads across availability zones
- **Pod Disruption Budgets** - Ensure minimum replicas during updates
- **Health Checks** - Implement liveness and readiness probes
- **Backup Strategy** - Regular backups of critical data

### 4. Cost Optimization
- **Use Spot Instances** - For non-critical workloads
- **Right-size Resources** - Monitor and adjust resource allocation
- **Implement Cluster Autoscaler** - Scale nodes based on demand
- **Use Fargate for Specific Workloads** - Serverless containers

## 🔗 Integration with GitOps Components

### Jenkins Integration
- **Dynamic Agents** - Use Kubernetes plugin for Jenkins agents
- **Deployment Jobs** - Deploy applications to EKS via Jenkins pipelines

### ArgoCD Integration
- **Application Deployment** - ArgoCD deploys applications to EKS
- **Configuration Management** - Manage Kubernetes manifests via Git

### Helm Integration
- **Package Management** - Deploy applications using Helm charts
- **Release Management** - Manage application releases and rollbacks

### Istio Integration
- **Service Mesh** - Manage service-to-service communication
- **Traffic Management** - Implement canary deployments and traffic splitting

---

**Next Steps:**
1. Set up your EKS cluster using the provided configurations
2. Install essential add-ons for your use case
3. Configure security policies and RBAC
4. Set up monitoring and logging
5. Integrate with other GitOps components

For advanced configurations and AWS-specific features, refer to the [Amazon EKS Documentation](https://docs.aws.amazon.com/eks/) and [Kubernetes Documentation](https://kubernetes.io/docs/).