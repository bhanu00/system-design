# Terraform Infrastructure as Code

Terraform manages our cloud infrastructure as code, providing declarative infrastructure provisioning and management for our GitOps workflow.

## 🎯 Overview

Terraform in our GitOps architecture provides:
- **Infrastructure as Code** - Declarative infrastructure definitions
- **State Management** - Centralized state tracking and locking
- **Multi-Cloud Support** - Consistent provisioning across cloud providers
- **Resource Lifecycle** - Create, update, and destroy infrastructure resources
- **Dependency Management** - Automatic resource dependency resolution

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                 Terraform Architecture                      │
├─────────────────────────────────────────────────────────────┤
│  Version Control (Git)                                     │
│  ├── Terraform Configurations (.tf files)                  │
│  ├── Variable Definitions (.tfvars)                        │
│  ├── Module Definitions                                    │
│  └── Environment Configurations                            │
├─────────────────────────────────────────────────────────────┤
│  Terraform State                                           │
│  ├── Remote Backend (S3 + DynamoDB)                        │
│  ├── State Locking                                         │
│  ├── State Versioning                                      │
│  └── Workspace Management                                  │
├─────────────────────────────────────────────────────────────┤
│  Cloud Providers                                           │
│  ├── AWS (EKS, VPC, IAM, etc.)                             │
│  ├── Azure (AKS, Resource Groups)                          │
│  ├── GCP (GKE, Projects)                                   │
│  └── Kubernetes Resources                                  │
├─────────────────────────────────────────────────────────────┤
│  CI/CD Integration                                          │
│  ├── Jenkins Pipeline                                      │
│  ├── GitHub Actions                                        │
│  ├── GitLab CI                                             │
│  └── ArgoCD Workflows                                      │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
terraform/
├── README.md
├── environments/
│   ├── dev/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   ├── terraform.tfvars
│   │   └── backend.tf
│   ├── staging/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   ├── terraform.tfvars
│   │   └── backend.tf
│   └── production/
│       ├── main.tf
│       ├── variables.tf
│       ├── outputs.tf
│       ├── terraform.tfvars
│       └── backend.tf
├── modules/
│   ├── vpc/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   └── README.md
│   ├── eks/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   └── README.md
│   ├── rds/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── outputs.tf
│   │   └── README.md
│   └── security-groups/
│       ├── main.tf
│       ├── variables.tf
│       ├── outputs.tf
│       └── README.md
├── global/
│   ├── iam/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   ├── s3/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   └── route53/
│       ├── main.tf
│       ├── variables.tf
│       └── outputs.tf
├── scripts/
│   ├── init.sh
│   ├── plan.sh
│   ├── apply.sh
│   ├── destroy.sh
│   └── validate.sh
└── policies/
    ├── security-policy.json
    ├── backup-policy.json
    └── compliance-policy.json
```

## 🚀 Quick Setup

### 1. Install Terraform

```bash
# Install Terraform (Linux/macOS)
curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
sudo apt-add-repository "deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
sudo apt-get update && sudo apt-get install terraform

# Verify installation
terraform version

# Install additional tools
terraform -install-autocomplete
```

### 2. Configure Backend

```hcl
# environments/production/backend.tf
terraform {
  required_version = ">= 1.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11"
    }
  }
  
  backend "s3" {
    bucket         = "company-terraform-state"
    key            = "production/terraform.tfstate"
    region         = "us-west-2"
    encrypt        = true
    dynamodb_table = "terraform-state-lock"
    
    # Optional: Use assume role for cross-account access
    role_arn = "arn:aws:iam::ACCOUNT-ID:role/TerraformRole"
  }
}
```

### 3. Initialize Terraform

```bash
# Navigate to environment directory
cd environments/production

# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Format code
terraform fmt -recursive
```

## 🏗️ Infrastructure Modules

### VPC Module

```hcl
# modules/vpc/main.tf
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true
  
  tags = merge(var.tags, {
    Name = "${var.name}-vpc"
  })
}

resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id
  
  tags = merge(var.tags, {
    Name = "${var.name}-igw"
  })
}

resource "aws_subnet" "public" {
  count = length(var.public_subnet_cidrs)
  
  vpc_id                  = aws_vpc.main.id
  cidr_block              = var.public_subnet_cidrs[count.index]
  availability_zone       = var.availability_zones[count.index]
  map_public_ip_on_launch = true
  
  tags = merge(var.tags, {
    Name = "${var.name}-public-subnet-${count.index + 1}"
    Type = "public"
    "kubernetes.io/role/elb" = "1"
  })
}

resource "aws_subnet" "private" {
  count = length(var.private_subnet_cidrs)
  
  vpc_id            = aws_vpc.main.id
  cidr_block        = var.private_subnet_cidrs[count.index]
  availability_zone = var.availability_zones[count.index]
  
  tags = merge(var.tags, {
    Name = "${var.name}-private-subnet-${count.index + 1}"
    Type = "private"
    "kubernetes.io/role/internal-elb" = "1"
  })
}

resource "aws_nat_gateway" "main" {
  count = var.enable_nat_gateway ? length(aws_subnet.public) : 0
  
  allocation_id = aws_eip.nat[count.index].id
  subnet_id     = aws_subnet.public[count.index].id
  
  tags = merge(var.tags, {
    Name = "${var.name}-nat-gateway-${count.index + 1}"
  })
  
  depends_on = [aws_internet_gateway.main]
}

resource "aws_eip" "nat" {
  count = var.enable_nat_gateway ? length(aws_subnet.public) : 0
  
  domain = "vpc"
  
  tags = merge(var.tags, {
    Name = "${var.name}-nat-eip-${count.index + 1}"
  })
  
  depends_on = [aws_internet_gateway.main]
}

resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id
  
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main.id
  }
  
  tags = merge(var.tags, {
    Name = "${var.name}-public-rt"
  })
}

resource "aws_route_table" "private" {
  count = var.enable_nat_gateway ? length(aws_nat_gateway.main) : 1
  
  vpc_id = aws_vpc.main.id
  
  dynamic "route" {
    for_each = var.enable_nat_gateway ? [1] : []
    content {
      cidr_block     = "0.0.0.0/0"
      nat_gateway_id = aws_nat_gateway.main[count.index].id
    }
  }
  
  tags = merge(var.tags, {
    Name = "${var.name}-private-rt-${count.index + 1}"
  })
}

resource "aws_route_table_association" "public" {
  count = length(aws_subnet.public)
  
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_route_table_association" "private" {
  count = length(aws_subnet.private)
  
  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = var.enable_nat_gateway ? aws_route_table.private[count.index].id : aws_route_table.private[0].id
}
```

```hcl
# modules/vpc/variables.tf
variable "name" {
  description = "Name prefix for VPC resources"
  type        = string
}

variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "List of availability zones"
  type        = list(string)
}

variable "public_subnet_cidrs" {
  description = "List of public subnet CIDR blocks"
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "List of private subnet CIDR blocks"
  type        = list(string)
}

variable "enable_nat_gateway" {
  description = "Enable NAT Gateway for private subnets"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
```

```hcl
# modules/vpc/outputs.tf
output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.main.id
}

output "vpc_cidr_block" {
  description = "CIDR block of the VPC"
  value       = aws_vpc.main.cidr_block
}

output "public_subnet_ids" {
  description = "IDs of the public subnets"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "IDs of the private subnets"
  value       = aws_subnet.private[*].id
}

output "internet_gateway_id" {
  description = "ID of the Internet Gateway"
  value       = aws_internet_gateway.main.id
}

output "nat_gateway_ids" {
  description = "IDs of the NAT Gateways"
  value       = aws_nat_gateway.main[*].id
}
```

### EKS Module

```hcl
# modules/eks/main.tf
data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

resource "aws_eks_cluster" "main" {
  name     = var.cluster_name
  role_arn = aws_iam_role.cluster.arn
  version  = var.kubernetes_version
  
  vpc_config {
    subnet_ids              = concat(var.public_subnet_ids, var.private_subnet_ids)
    endpoint_private_access = var.endpoint_private_access
    endpoint_public_access  = var.endpoint_public_access
    public_access_cidrs     = var.public_access_cidrs
    security_group_ids      = [aws_security_group.cluster.id]
  }
  
  encryption_config {
    provider {
      key_arn = aws_kms_key.eks.arn
    }
    resources = ["secrets"]
  }
  
  enabled_cluster_log_types = var.cluster_log_types
  
  tags = var.tags
  
  depends_on = [
    aws_iam_role_policy_attachment.cluster_AmazonEKSClusterPolicy,
    aws_iam_role_policy_attachment.cluster_AmazonEKSVPCResourceController,
    aws_cloudwatch_log_group.cluster,
  ]
}

resource "aws_eks_node_group" "main" {
  for_each = var.node_groups
  
  cluster_name    = aws_eks_cluster.main.name
  node_group_name = each.key
  node_role_arn   = aws_iam_role.node_group.arn
  subnet_ids      = var.private_subnet_ids
  
  capacity_type  = each.value.capacity_type
  instance_types = each.value.instance_types
  ami_type       = each.value.ami_type
  disk_size      = each.value.disk_size
  
  scaling_config {
    desired_size = each.value.desired_size
    max_size     = each.value.max_size
    min_size     = each.value.min_size
  }
  
  update_config {
    max_unavailable_percentage = each.value.max_unavailable_percentage
  }
  
  remote_access {
    ec2_ssh_key               = each.value.key_name
    source_security_group_ids = [aws_security_group.node_group.id]
  }
  
  labels = each.value.labels
  taints = each.value.taints
  
  tags = merge(var.tags, each.value.tags)
  
  depends_on = [
    aws_iam_role_policy_attachment.node_group_AmazonEKSWorkerNodePolicy,
    aws_iam_role_policy_attachment.node_group_AmazonEKS_CNI_Policy,
    aws_iam_role_policy_attachment.node_group_AmazonEC2ContainerRegistryReadOnly,
  ]
}

resource "aws_eks_addon" "main" {
  for_each = var.cluster_addons
  
  cluster_name             = aws_eks_cluster.main.name
  addon_name               = each.key
  addon_version            = each.value.version
  resolve_conflicts        = each.value.resolve_conflicts
  service_account_role_arn = each.value.service_account_role_arn
  
  tags = var.tags
}

# IAM Roles and Policies
resource "aws_iam_role" "cluster" {
  name = "${var.cluster_name}-cluster-role"
  
  assume_role_policy = jsonencode({
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "eks.amazonaws.com"
      }
    }]
    Version = "2012-10-17"
  })
  
  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "cluster_AmazonEKSClusterPolicy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSClusterPolicy"
  role       = aws_iam_role.cluster.name
}

resource "aws_iam_role_policy_attachment" "cluster_AmazonEKSVPCResourceController" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSVPCResourceController"
  role       = aws_iam_role.cluster.name
}

resource "aws_iam_role" "node_group" {
  name = "${var.cluster_name}-node-group-role"
  
  assume_role_policy = jsonencode({
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ec2.amazonaws.com"
      }
    }]
    Version = "2012-10-17"
  })
  
  tags = var.tags
}

resource "aws_iam_role_policy_attachment" "node_group_AmazonEKSWorkerNodePolicy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
  role       = aws_iam_role.node_group.name
}

resource "aws_iam_role_policy_attachment" "node_group_AmazonEKS_CNI_Policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
  role       = aws_iam_role.node_group.name
}

resource "aws_iam_role_policy_attachment" "node_group_AmazonEC2ContainerRegistryReadOnly" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
  role       = aws_iam_role.node_group.name
}

# Security Groups
resource "aws_security_group" "cluster" {
  name_prefix = "${var.cluster_name}-cluster-"
  vpc_id      = var.vpc_id
  
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = var.public_access_cidrs
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = merge(var.tags, {
    Name = "${var.cluster_name}-cluster-sg"
  })
}

resource "aws_security_group" "node_group" {
  name_prefix = "${var.cluster_name}-node-group-"
  vpc_id      = var.vpc_id
  
  ingress {
    description     = "Cluster API"
    from_port       = 443
    to_port         = 443
    protocol        = "tcp"
    security_groups = [aws_security_group.cluster.id]
  }
  
  ingress {
    description = "Node to node"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    self        = true
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = merge(var.tags, {
    Name = "${var.cluster_name}-node-group-sg"
  })
}

# KMS Key for EKS encryption
resource "aws_kms_key" "eks" {
  description             = "EKS Secret Encryption Key"
  deletion_window_in_days = 7
  enable_key_rotation     = true
  
  tags = var.tags
}

resource "aws_kms_alias" "eks" {
  name          = "alias/${var.cluster_name}-eks"
  target_key_id = aws_kms_key.eks.key_id
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "cluster" {
  name              = "/aws/eks/${var.cluster_name}/cluster"
  retention_in_days = var.log_retention_days
  kms_key_id        = aws_kms_key.eks.arn
  
  tags = var.tags
}
```

## 🔄 Environment Configuration

### Production Environment

```hcl
# environments/production/main.tf
terraform {
  required_version = ">= 1.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
  
  default_tags {
    tags = {
      Environment   = "production"
      Project       = "gitops-platform"
      ManagedBy     = "terraform"
      Owner         = "devops-team"
      CostCenter    = "engineering"
    }
  }
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

# Local values
locals {
  cluster_name = "gitops-prod-cluster"
  
  common_tags = {
    Environment = "production"
    Project     = "gitops-platform"
    ManagedBy   = "terraform"
  }
}

# VPC
module "vpc" {
  source = "../../modules/vpc"
  
  name               = "gitops-prod"
  vpc_cidr           = "10.0.0.0/16"
  availability_zones = slice(data.aws_availability_zones.available.names, 0, 3)
  
  public_subnet_cidrs  = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  private_subnet_cidrs = ["10.0.11.0/24", "10.0.12.0/24", "10.0.13.0/24"]
  
  enable_nat_gateway = true
  
  tags = local.common_tags
}

# EKS Cluster
module "eks" {
  source = "../../modules/eks"
  
  cluster_name       = local.cluster_name
  kubernetes_version = "1.28"
  
  vpc_id             = module.vpc.vpc_id
  public_subnet_ids  = module.vpc.public_subnet_ids
  private_subnet_ids = module.vpc.private_subnet_ids
  
  endpoint_private_access = true
  endpoint_public_access  = true
  public_access_cidrs     = ["0.0.0.0/0"]
  
  cluster_log_types = ["api", "audit", "authenticator", "controllerManager", "scheduler"]
  
  node_groups = {
    main = {
      capacity_type  = "ON_DEMAND"
      instance_types = ["m5.large"]
      ami_type       = "AL2_x86_64"
      disk_size      = 50
      
      desired_size = 3
      max_size     = 6
      min_size     = 1
      
      max_unavailable_percentage = 25
      
      key_name = var.key_pair_name
      
      labels = {
        Environment = "production"
        NodeGroup   = "main"
      }
      
      taints = []
      
      tags = {
        NodeGroup = "main"
      }
    }
    
    spot = {
      capacity_type  = "SPOT"
      instance_types = ["m5.large", "m5a.large", "m4.large"]
      ami_type       = "AL2_x86_64"
      disk_size      = 50
      
      desired_size = 2
      max_size     = 10
      min_size     = 0
      
      max_unavailable_percentage = 50
      
      key_name = var.key_pair_name
      
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
      
      tags = {
        NodeGroup = "spot"
      }
    }
  }
  
  cluster_addons = {
    coredns = {
      version                  = "v1.10.1-eksbuild.4"
      resolve_conflicts        = "OVERWRITE"
      service_account_role_arn = null
    }
    kube-proxy = {
      version                  = "v1.28.2-eksbuild.2"
      resolve_conflicts        = "OVERWRITE"
      service_account_role_arn = null
    }
    vpc-cni = {
      version                  = "v1.15.1-eksbuild.1"
      resolve_conflicts        = "OVERWRITE"
      service_account_role_arn = null
    }
    aws-ebs-csi-driver = {
      version                  = "v1.24.0-eksbuild.1"
      resolve_conflicts        = "OVERWRITE"
      service_account_role_arn = module.ebs_csi_irsa.iam_role_arn
    }
  }
  
  tags = local.common_tags
}

# IRSA for EBS CSI Driver
module "ebs_csi_irsa" {
  source = "terraform-aws-modules/iam/aws//modules/iam-role-for-service-accounts-eks"
  
  role_name = "${local.cluster_name}-ebs-csi-driver"
  
  attach_ebs_csi_policy = true
  
  oidc_providers = {
    main = {
      provider_arn               = module.eks.oidc_provider_arn
      namespace_service_accounts = ["kube-system:ebs-csi-controller-sa"]
    }
  }
  
  tags = local.common_tags
}

# RDS Database
module "rds" {
  source = "../../modules/rds"
  
  identifier = "gitops-prod-db"
  
  engine         = "postgres"
  engine_version = "15.4"
  instance_class = "db.r6g.large"
  
  allocated_storage     = 100
  max_allocated_storage = 1000
  storage_encrypted     = true
  
  db_name  = "gitops"
  username = "postgres"
  password = var.db_password
  
  vpc_id             = module.vpc.vpc_id
  subnet_ids         = module.vpc.private_subnet_ids
  security_group_ids = [aws_security_group.rds.id]
  
  backup_retention_period = 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "sun:04:00-sun:05:00"
  
  monitoring_interval = 60
  monitoring_role_arn = aws_iam_role.rds_monitoring.arn
  
  deletion_protection = true
  skip_final_snapshot = false
  
  tags = local.common_tags
}

# Security Group for RDS
resource "aws_security_group" "rds" {
  name_prefix = "gitops-prod-rds-"
  vpc_id      = module.vpc.vpc_id
  
  ingress {
    description     = "PostgreSQL from EKS"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [module.eks.node_security_group_id]
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = merge(local.common_tags, {
    Name = "gitops-prod-rds-sg"
  })
}

# IAM Role for RDS Monitoring
resource "aws_iam_role" "rds_monitoring" {
  name = "gitops-prod-rds-monitoring-role"
  
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "monitoring.rds.amazonaws.com"
        }
      }
    ]
  })
  
  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "rds_monitoring" {
  role       = aws_iam_role.rds_monitoring.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}
```

```hcl
# environments/production/variables.tf
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-west-2"
}

variable "key_pair_name" {
  description = "EC2 Key Pair name for EKS nodes"
  type        = string
}

variable "db_password" {
  description = "Password for RDS database"
  type        = string
  sensitive   = true
}

variable "domain_name" {
  description = "Domain name for the application"
  type        = string
  default     = "company.com"
}

variable "certificate_arn" {
  description = "ACM certificate ARN for HTTPS"
  type        = string
}
```

```hcl
# environments/production/outputs.tf
output "vpc_id" {
  description = "ID of the VPC"
  value       = module.vpc.vpc_id
}

output "cluster_endpoint" {
  description = "Endpoint for EKS control plane"
  value       = module.eks.cluster_endpoint
}

output "cluster_name" {
  description = "Name of the EKS cluster"
  value       = module.eks.cluster_name
}

output "cluster_arn" {
  description = "ARN of the EKS cluster"
  value       = module.eks.cluster_arn
}

output "cluster_certificate_authority_data" {
  description = "Base64 encoded certificate data required to communicate with the cluster"
  value       = module.eks.cluster_certificate_authority_data
}

output "cluster_oidc_issuer_url" {
  description = "The URL on the EKS cluster OIDC Issuer"
  value       = module.eks.cluster_oidc_issuer_url
}

output "rds_endpoint" {
  description = "RDS instance endpoint"
  value       = module.rds.db_instance_endpoint
  sensitive   = true
}

output "rds_port" {
  description = "RDS instance port"
  value       = module.rds.db_instance_port
}
```

```hcl
# environments/production/terraform.tfvars
aws_region      = "us-west-2"
key_pair_name   = "gitops-prod-key"
domain_name     = "company.com"
certificate_arn = "arn:aws:acm:us-west-2:123456789012:certificate/12345678-1234-1234-1234-123456789012"

# Database password should be set via environment variable:
# export TF_VAR_db_password="your-secure-password"
```

## 🔄 GitOps Integration

### Jenkins Pipeline

```groovy
// Jenkinsfile for Terraform
pipeline {
    agent any
    
    environment {
        AWS_DEFAULT_REGION = 'us-west-2'
        TF_VAR_db_password = credentials('db-password')
        TF_IN_AUTOMATION = 'true'
    }
    
    parameters {
        choice(
            name: 'ENVIRONMENT',
            choices: ['dev', 'staging', 'production'],
            description: 'Target environment'
        )
        choice(
            name: 'ACTION',
            choices: ['plan', 'apply', 'destroy'],
            description: 'Terraform action'
        )
        booleanParam(
            name: 'AUTO_APPROVE',
            defaultValue: false,
            description: 'Auto approve terraform apply'
        )
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Terraform Init') {
            steps {
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    sh 'terraform init -upgrade'
                }
            }
        }
        
        stage('Terraform Validate') {
            steps {
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    sh 'terraform validate'
                    sh 'terraform fmt -check=true -diff=true'
                }
            }
        }
        
        stage('Terraform Plan') {
            steps {
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    sh 'terraform plan -out=tfplan'
                    sh 'terraform show -no-color tfplan > tfplan.txt'
                }
                
                publishHTML([
                    allowMissing: false,
                    alwaysLinkToLastBuild: true,
                    keepAll: true,
                    reportDir: "terraform/environments/${params.ENVIRONMENT}",
                    reportFiles: 'tfplan.txt',
                    reportName: 'Terraform Plan'
                ])
            }
        }
        
        stage('Terraform Apply') {
            when {
                expression { params.ACTION == 'apply' }
            }
            steps {
                script {
                    if (!params.AUTO_APPROVE && params.ENVIRONMENT == 'production') {
                        input message: 'Apply Terraform changes to production?', ok: 'Apply'
                    }
                }
                
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    sh 'terraform apply -auto-approve tfplan'
                }
            }
        }
        
        stage('Terraform Destroy') {
            when {
                expression { params.ACTION == 'destroy' }
            }
            steps {
                input message: 'Destroy infrastructure?', ok: 'Destroy'
                
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    sh 'terraform destroy -auto-approve'
                }
            }
        }
        
        stage('Update Kubeconfig') {
            when {
                expression { params.ACTION == 'apply' }
            }
            steps {
                dir("terraform/environments/${params.ENVIRONMENT}") {
                    script {
                        def clusterName = sh(
                            script: 'terraform output -raw cluster_name',
                            returnStdout: true
                        ).trim()
                        
                        sh "aws eks update-kubeconfig --region ${AWS_DEFAULT_REGION} --name ${clusterName}"
                    }
                }
            }
        }
    }
    
    post {
        always {
            dir("terraform/environments/${params.ENVIRONMENT}") {
                archiveArtifacts artifacts: 'tfplan.txt', allowEmptyArchive: true
                cleanWs()
            }
        }
        success {
            echo 'Terraform pipeline completed successfully!'
        }
        failure {
            echo 'Terraform pipeline failed!'
        }
    }
}
```

### GitHub Actions

```yaml
# .github/workflows/terraform.yml
name: Terraform Infrastructure

on:
  push:
    branches: [main]
    paths: ['terraform/**']
  pull_request:
    branches: [main]
    paths: ['terraform/**']
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment to deploy'
        required: true
        default: 'dev'
        type: choice
        options:
        - dev
        - staging
        - production
      action:
        description: 'Terraform action'
        required: true
        default: 'plan'
        type: choice
        options:
        - plan
        - apply
        - destroy

env:
  TF_VERSION: '1.6.0'
  AWS_REGION: 'us-west-2'

jobs:
  terraform:
    name: Terraform
    runs-on: ubuntu-latest
    
    strategy:
      matrix:
        environment: [dev, staging, production]
    
    defaults:
      run:
        working-directory: terraform/environments/${{ matrix.environment }}
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Setup Terraform
      uses: hashicorp/setup-terraform@v3
      with:
        terraform_version: ${{ env.TF_VERSION }}
    
    - name: Configure AWS Credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ env.AWS_REGION }}
    
    - name: Terraform Init
      run: terraform init
    
    - name: Terraform Validate
      run: terraform validate
    
    - name: Terraform Format Check
      run: terraform fmt -check=true -diff=true
    
    - name: Terraform Plan
      run: |
        terraform plan -out=tfplan
        terraform show -no-color tfplan > tfplan.txt
      env:
        TF_VAR_db_password: ${{ secrets.DB_PASSWORD }}
    
    - name: Upload Plan
      uses: actions/upload-artifact@v3
      with:
        name: terraform-plan-${{ matrix.environment }}
        path: terraform/environments/${{ matrix.environment }}/tfplan.txt
    
    - name: Terraform Apply
      if: github.ref == 'refs/heads/main' && github.event_name == 'push'
      run: terraform apply -auto-approve tfplan
      env:
        TF_VAR_db_password: ${{ secrets.DB_PASSWORD }}
    
    - name: Update Kubeconfig
      if: github.ref == 'refs/heads/main' && github.event_name == 'push'
      run: |
        CLUSTER_NAME=$(terraform output -raw cluster_name)
        aws eks update-kubeconfig --region ${{ env.AWS_REGION }} --name $CLUSTER_NAME
```

## 🔒 Security and Best Practices

### State Management

```hcl
# S3 Backend with encryption and locking
terraform {
  backend "s3" {
    bucket         = "company-terraform-state"
    key            = "production/terraform.tfstate"
    region         = "us-west-2"
    encrypt        = true
    kms_key_id     = "arn:aws:kms:us-west-2:123456789012:key/12345678-1234-1234-1234-123456789012"
    dynamodb_table = "terraform-state-lock"
    
    # Enable versioning
    versioning = true
    
    # Server-side encryption
    server_side_encryption_configuration {
      rule {
        apply_server_side_encryption_by_default {
          kms_master_key_id = "arn:aws:kms:us-west-2:123456789012:key/12345678-1234-1234-1234-123456789012"
          sse_algorithm     = "aws:kms"
        }
      }
    }
  }
}
```

### Security Policies

```json
// policies/security-policy.json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "DenyUnencryptedStorage",
      "Effect": "Deny",
      "Action": [
        "s3:PutObject"
      ],
      "Resource": "*",
      "Condition": {
        "StringNotEquals": {
          "s3:x-amz-server-side-encryption": "AES256"
        }
      }
    },
    {
      "Sid": "RequireMFAForDestroy",
      "Effect": "Deny",
      "Action": [
        "ec2:TerminateInstances",
        "rds:DeleteDBInstance",
        "eks:DeleteCluster"
      ],
      "Resource": "*",
      "Condition": {
        "BoolIfExists": {
          "aws:MultiFactorAuthPresent": "false"
        }
      }
    }
  ]
}
```

### Validation Scripts

```bash
#!/bin/bash
# scripts/validate.sh

set -e

ENVIRONMENT=${1:-dev}
TERRAFORM_DIR="environments/${ENVIRONMENT}"

echo "Validating Terraform configuration for ${ENVIRONMENT}..."

cd "${TERRAFORM_DIR}"

# Initialize Terraform
terraform init -backend=false

# Validate configuration
terraform validate

# Check formatting
terraform fmt -check=true -diff=true

# Security scan with tfsec
if command -v tfsec &> /dev/null; then
    echo "Running security scan..."
    tfsec .
fi

# Cost estimation with infracost
if command -v infracost &> /dev/null; then
    echo "Estimating costs..."
    infracost breakdown --path .
fi

# Compliance check with checkov
if command -v checkov &> /dev/null; then
    echo "Running compliance checks..."
    checkov -d .
fi

echo "Validation completed successfully!"
```

## 📚 Best Practices

### 1. Code Organization
- **Module Structure** - Create reusable modules for common resources
- **Environment Separation** - Separate configurations per environment
- **Version Pinning** - Pin provider and module versions
- **Documentation** - Document all modules and variables
- **Naming Conventions** - Use consistent naming patterns

### 2. State Management
- **Remote Backend** - Always use remote state storage
- **State Locking** - Enable state locking with DynamoDB
- **Encryption** - Encrypt state files at rest and in transit
- **Backup Strategy** - Regular backups of state files
- **Access Control** - Restrict access to state files

### 3. Security
- **Least Privilege** - Grant minimal required permissions
- **Secrets Management** - Never store secrets in code
- **Encryption** - Encrypt all sensitive data
- **Network Security** - Use private subnets and security groups
- **Compliance** - Regular compliance and security scans

### 4. Operations
- **CI/CD Integration** - Automate terraform workflows
- **Plan Review** - Always review plans before applying
- **Gradual Rollouts** - Test changes in lower environments first
- **Monitoring** - Monitor infrastructure changes and costs
- **Disaster Recovery** - Plan for infrastructure recovery

## 🚨 Troubleshooting

### Common Issues

#### 1. State Lock Issues
```bash
# Force unlock state (use with caution)
terraform force-unlock <LOCK_ID>

# Check state lock status
aws dynamodb get-item --table-name terraform-state-lock --key '{"LockID":{"S":"path/to/state"}}'
```

#### 2. Provider Version Conflicts
```bash
# Upgrade providers
terraform init -upgrade

# Check provider versions
terraform version
terraform providers
```

#### 3. Resource Import Issues
```bash
# Import existing resource
terraform import aws_instance.example i-1234567890abcdef0

# Generate configuration from existing resources
terraform show -json | jq '.values.root_module.resources[]'
```

### Debug Commands

```bash
# Enable debug logging
export TF_LOG=DEBUG
export TF_LOG_PATH=terraform.log

# Validate configuration
terraform validate

# Check state
terraform state list
terraform state show <resource>

# Refresh state
terraform refresh

# Show current state
terraform show
```

---

**Next Steps:**
1. Set up your Terraform backend and state management
2. Create your first infrastructure modules
3. Implement environment-specific configurations
4. Set up CI/CD pipelines for infrastructure
5. Establish monitoring and cost management

For advanced Terraform features and best practices, refer to the [Terraform Documentation](https://www.terraform.io/docs/) and [Terraform Best Practices Guide](https://www.terraform.io/docs/cloud/guides/recommended-practices/index.html).