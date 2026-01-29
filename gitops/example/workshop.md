# GitOps Workshop: End-to-End Implementation

This workshop guides you through implementing a complete GitOps workflow for a .NET Products API, from source code to production deployment.

## 🎯 Workshop Objectives

By the end of this workshop, you will have:
- ✅ A complete .NET Products API with containerization
- ✅ Infrastructure provisioned with Terraform
- ✅ CI/CD pipeline with Jenkins or GitHub Actions
- ✅ GitOps deployment with ArgoCD
- ✅ Service mesh with Istio
- ✅ Monitoring and observability setup

## ⏱️ Estimated Time: 4-6 hours

## 📋 Prerequisites

### Required Tools
```bash
# Install required tools
brew install terraform
brew install kubectl
brew install helm
brew install docker
brew install git
brew install awscli

# Verify installations
terraform version
kubectl version --client
helm version
docker version
git --version
aws --version
```

### Required Accounts
- AWS Account with administrative access
- GitHub account
- Docker Hub or GitHub Container Registry access

### Environment Setup
```bash
# Set environment variables
export AWS_REGION=us-west-2
export CLUSTER_NAME=products-api-cluster
export GITHUB_USERNAME=your-username
export DOCKER_REGISTRY=ghcr.io
```

---

## 🚀 Phase 1: Source Code Setup (30 minutes)

### Step 1.1: Create .NET Products API

```bash
# Create new directory
mkdir products-api-gitops
cd products-api-gitops

# Create .NET API project
dotnet new webapi -n ProductsAPI
cd ProductsAPI
```

### Step 1.2: Add Required Packages

```bash
# Add Entity Framework and other packages
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Diagnostics.HealthChecks
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

### Step 1.3: Create Product Model

Create `Models/Product.cs`:
```csharp
namespace ProductsAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### Step 1.4: Create Database Context

Create `Data/ProductsDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsAPI.Models;

namespace ProductsAPI.Data
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            });
        }
    }
}
```

### Step 1.5: Create Product Service

Create `Services/IProductService.cs`:
```csharp
using ProductsAPI.Models;

namespace ProductsAPI.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(int id, Product product);
        Task<bool> DeleteProductAsync(int id);
    }
}
```

Create `Services/ProductService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data;
using ProductsAPI.Models;

namespace ProductsAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductsDbContext _context;

        public ProductService(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateProductAsync(int id, Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
```

### Step 1.6: Create Products Controller

Create `Controllers/ProductsController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Models;
using ProductsAPI.Services;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            if (updatedProduct == null) return NotFound();
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
```

### Step 1.7: Update Program.cs

Replace `Program.cs` content:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsAPI.Data;
using ProductsAPI.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<ProductsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("ProductsDb");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// Add application services
builder.Services.AddScoped<IProductService, ProductService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductsDbContext>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Add health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.MapControllers();

app.Run();
```

### Step 1.8: Create Dockerfile

Create `Dockerfile`:
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ProductsAPI.csproj", "."]
RUN dotnet restore "ProductsAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "ProductsAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductsAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 dotnet
RUN adduser --system --uid 1001 --ingroup dotnet dotnet

# Copy published app
COPY --from=publish /app/publish .

# Change ownership to dotnet user
RUN chown -R dotnet:dotnet /app
USER dotnet

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductsAPI.dll"]
```

### Step 1.9: Test the Application

```bash
# Build and run locally
dotnet build
dotnet run

# Test endpoints (in another terminal)
curl http://localhost:5000/health
curl http://localhost:5000/api/products

# Test with Docker
docker build -t products-api:local .
docker run -p 8080:8080 products-api:local
```

---

## 🏗️ Phase 2: Infrastructure Setup (45 minutes)

### Step 2.1: Create Terraform Configuration

Create directory structure:
```bash
mkdir -p infrastructure/{environments/dev,modules/{vpc,eks}}
```

### Step 2.2: VPC Module

Create `infrastructure/modules/vpc/main.tf`:
```hcl
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

# NAT Gateway and routing
resource "aws_nat_gateway" "main" {
  allocation_id = aws_eip.nat.id
  subnet_id     = aws_subnet.public[0].id

  tags = merge(var.tags, {
    Name = "${var.name}-nat-gateway"
  })

  depends_on = [aws_internet_gateway.main]
}

resource "aws_eip" "nat" {
  domain = "vpc"

  tags = merge(var.tags, {
    Name = "${var.name}-nat-eip"
  })

  depends_on = [aws_internet_gateway.main]
}

# Route tables
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
  vpc_id = aws_vpc.main.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main.id
  }

  tags = merge(var.tags, {
    Name = "${var.name}-private-rt"
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
  route_table_id = aws_route_table.private.id
}
```

### Step 2.3: Deploy Infrastructure

```bash
cd infrastructure/environments/dev

# Initialize Terraform
terraform init

# Plan deployment
terraform plan

# Apply infrastructure
terraform apply

# Update kubeconfig
aws eks update-kubeconfig --region us-west-2 --name products-api-dev-cluster

# Verify cluster access
kubectl get nodes
```

---

## 📦 Phase 3: Helm Charts (30 minutes)

### Step 3.1: Create Helm Chart

```bash
# Create Helm chart
mkdir -p helm-charts
cd helm-charts
helm create products-api
```

### Step 3.2: Update values.yaml

Edit `helm-charts/products-api/values.yaml`:
```yaml
replicaCount: 2

image:
  repository: ghcr.io/company/products-api
  pullPolicy: IfNotPresent
  tag: "latest"

service:
  type: ClusterIP
  port: 80
  targetPort: 8080

ingress:
  enabled: false

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 250m
    memory: 256Mi

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 80

# Health check configuration
healthCheck:
  enabled: true
  path: /health
  initialDelaySeconds: 30
  periodSeconds: 10

# PostgreSQL dependency
postgresql:
  enabled: true
  auth:
    postgresPassword: "products123"
    database: "productsdb"
```

### Step 3.3: Test Helm Chart

```bash
# Validate chart
helm lint helm-charts/products-api

# Test template rendering
helm template products-api helm-charts/products-api

# Install chart (dry run)
helm install products-api helm-charts/products-api --dry-run --debug
```

---

## 🔄 Phase 4: CI/CD Pipeline (45 minutes)

### Step 4.1: Create GitHub Repository

```bash
# Initialize git repository
git init
git add .
git commit -m "Initial commit: Products API with Helm chart"

# Create GitHub repository and push
gh repo create products-api-gitops --public
git remote add origin https://github.com/your-username/products-api-gitops.git
git push -u origin main
```

### Step 4.2: Create GitHub Actions Workflow

Create `.github/workflows/ci-cd.yml`:
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore ProductsAPI/ProductsAPI.csproj
    
    - name: Build
      run: dotnet build ProductsAPI/ProductsAPI.csproj --no-restore
    
    - name: Test
      run: dotnet test ProductsAPI/ProductsAPI.csproj --no-build --verbosity normal

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    permissions:
      contents: read
      packages: write
    
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
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: ./ProductsAPI
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
```

---

## 🚀 Phase 5: ArgoCD Setup (30 minutes)

### Step 5.1: Install ArgoCD

```bash
# Create namespace
kubectl create namespace argocd

# Install ArgoCD
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Wait for pods to be ready
kubectl wait --for=condition=available --timeout=300s deployment/argocd-server -n argocd

# Get initial admin password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d && echo

# Port forward to access UI
kubectl port-forward svc/argocd-server -n argocd 8080:443
```

### Step 5.2: Create ArgoCD Application

Create `argocd/applications/products-api-dev.yaml`:
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: products-api-dev
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/your-username/products-api-gitops.git
    targetRevision: main
    path: helm-charts/products-api
  destination:
    server: https://kubernetes.default.svc
    namespace: products-api-dev
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

### Step 5.3: Apply ArgoCD Application

```bash
# Apply the application
kubectl apply -f argocd/applications/products-api-dev.yaml

# Check application status
kubectl get applications -n argocd
```

---

## 🕸️ Phase 6: Istio Service Mesh (45 minutes)

### Step 6.1: Install Istio

```bash
# Download Istio
curl -L https://istio.io/downloadIstio | sh -
cd istio-*
export PATH=$PWD/bin:$PATH

# Install Istio
istioctl install --set values.pilot.traceSampling=100

# Enable automatic sidecar injection
kubectl label namespace products-api-dev istio-injection=enabled

# Install add-ons
kubectl apply -f samples/addons/
```

### Step 6.2: Create Istio Gateway

Create `istio/gateway.yaml`:
```yaml
apiVersion: networking.istio.io/v1beta1
kind: Gateway
metadata:
  name: products-api-gateway
  namespace: products-api-dev
spec:
  selector:
    istio: ingressgateway
  servers:
  - port:
      number: 80
      name: http
      protocol: HTTP
    hosts:
    - products-api.local
```

### Step 6.3: Create Virtual Service

Create `istio/virtual-service.yaml`:
```yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: products-api
  namespace: products-api-dev
spec:
  hosts:
  - products-api.local
  gateways:
  - products-api-gateway
  http:
  - match:
    - uri:
        prefix: /
    route:
    - destination:
        host: products-api
        port:
          number: 80
```

### Step 6.4: Apply Istio Configuration

```bash
# Apply Istio configurations
kubectl apply -f istio/

# Check Istio configuration
istioctl analyze -n products-api-dev

# Get ingress gateway external IP
kubectl get svc istio-ingressgateway -n istio-system
```

---

## 📊 Phase 7: Monitoring Setup (30 minutes)

### Step 7.1: Access Monitoring Dashboards

```bash
# Kiali dashboard
istioctl dashboard kiali

# Jaeger dashboard
istioctl dashboard jaeger

# Grafana dashboard
istioctl dashboard grafana

# Prometheus dashboard
istioctl dashboard prometheus
```

---

## 🧪 Phase 8: Testing the Complete Flow (30 minutes)

### Step 8.1: Test Application Deployment

```bash
# Check all pods are running
kubectl get pods -n products-api-dev

# Check ArgoCD application status
kubectl get applications -n argocd

# Check Istio configuration
istioctl proxy-status
```

### Step 8.2: Test API Endpoints

```bash
# Get ingress gateway IP
GATEWAY_IP=$(kubectl get svc istio-ingressgateway -n istio-system -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

# Test health endpoint
curl -H "Host: products-api.local" http://$GATEWAY_IP/health

# Test products API
curl -H "Host: products-api.local" http://$GATEWAY_IP/api/products

# Create a product
curl -X POST -H "Host: products-api.local" -H "Content-Type: application/json" \
  -d '{"name":"Test Product","description":"A test product","price":29.99,"category":"Test"}' \
  http://$GATEWAY_IP/api/products
```

### Step 8.3: Test GitOps Flow

```bash
# Make a code change
echo "// Updated at $(date)" >> ProductsAPI/Controllers/ProductsController.cs

# Commit and push
git add .
git commit -m "Test GitOps flow"
git push origin main

# Watch ArgoCD sync the changes
kubectl get applications -n argocd -w
```

### Step 8.4: Test Monitoring

```bash
# Generate some traffic
for i in {1..100}; do
  curl -H "Host: products-api.local" http://$GATEWAY_IP/api/products
  sleep 1
done

# Check metrics in Grafana
# Check traces in Jaeger
# Check service topology in Kiali
```

---

## 🎉 Phase 9: Verification and Cleanup (15 minutes)

### Step 9.1: Verify Complete Workflow

✅ **Checklist:**
- [ ] .NET API is containerized and running
- [ ] Infrastructure is provisioned with Terraform
- [ ] CI/CD pipeline builds and pushes images
- [ ] ArgoCD automatically deploys changes
- [ ] Istio manages traffic and provides observability
- [ ] Monitoring stack collects metrics and traces
- [ ] Health checks are working
- [ ] API endpoints are accessible

### Step 9.2: View Complete Architecture

```bash
# View all resources
kubectl get all -n products-api-dev
kubectl get all -n argocd
kubectl get all -n istio-system

# Check Istio configuration
istioctl proxy-config cluster products-api-dev
```

### Step 9.3: Cleanup (Optional)

```bash
# Delete ArgoCD application
kubectl delete application products-api-dev -n argocd

# Delete namespace
kubectl delete namespace products-api-dev

# Uninstall Istio
istioctl uninstall --purge

# Destroy infrastructure
cd infrastructure/environments/dev
terraform destroy
```

---

## 🎯 Workshop Summary

Congratulations! You have successfully implemented a complete GitOps workflow with:

### ✅ **What You Built:**
1. **Products API** - .NET 8 REST API with Entity Framework
2. **Containerization** - Docker multi-stage build
3. **Infrastructure** - EKS cluster with Terraform
4. **CI/CD Pipeline** - GitHub Actions
5. **Package Management** - Helm charts
6. **GitOps Deployment** - ArgoCD automation
7. **Service Mesh** - Istio traffic management
8. **Monitoring** - Prometheus, Grafana, Jaeger, Kiali

### 🔄 **GitOps Flow Achieved:**
```
Code Change → GitHub → CI/CD → Container Registry → GitOps Repo → ArgoCD → EKS → Istio → Production
```

### 📚 **Next Steps:**
1. **Security**: Implement RBAC, network policies, and secret management
2. **Scaling**: Add more environments (staging, production)
3. **Advanced Features**: Canary deployments, blue-green deployments
4. **Monitoring**: Custom dashboards and alerting rules
5. **Backup**: Database backups and disaster recovery

### 🔗 **Resources:**
- [Integration Guide](integration.md) - Component relationships
- [Main GitOps Documentation](../README.md) - Complete reference

---

**🎉 Congratulations on completing the GitOps workshop!** You now have hands-on experience with a production-ready GitOps implementation.