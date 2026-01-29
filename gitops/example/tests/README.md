# Testing Guide for Products API

This directory contains various testing scripts and configurations for the Products API GitOps example.

## Test Files

### 1. API Integration Tests (`api-test.sh`)

A comprehensive bash script that tests all API endpoints.

**Usage:**
```bash
# Make script executable (Linux/Mac)
chmod +x api-test.sh

# Run tests against local development server
./api-test.sh

# Run tests against specific URL
BASE_URL=http://your-api-url ./api-test.sh

# Run with verbose output
VERBOSE=true ./api-test.sh
```

**What it tests:**
- Health check endpoints (`/health`, `/ready`, `/live`)
- CRUD operations on products
- Error handling (404, 400 responses)
- Data validation

### 2. Load Testing (`load-test.js`)

K6 performance testing script for load testing the API.

**Prerequisites:**
```bash
# Install k6
# On macOS
brew install k6

# On Windows
choco install k6

# On Linux
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

**Usage:**
```bash
# Run load test against local server
k6 run load-test.js

# Run against specific URL
BASE_URL=http://your-api-url k6 run load-test.js

# Run with custom options
k6 run --vus 50 --duration 5m load-test.js
```

**Test Scenarios:**
- Gradual ramp-up from 0 to 20 virtual users
- Tests all CRUD operations
- Measures response times and error rates
- Validates performance thresholds

## Running Tests in Different Environments

### Local Development
```bash
# Start the API locally
cd ../source-code/ProductsAPI
dotnet run

# In another terminal, run tests
cd ../tests
BASE_URL=http://localhost:5000 ./api-test.sh
```

### Docker Container
```bash
# Build and run container
cd ../source-code/ProductsAPI
docker build -t products-api .
docker run -p 8080:8080 products-api

# Run tests
cd ../tests
BASE_URL=http://localhost:8080 ./api-test.sh
```

### Kubernetes Cluster
```bash
# Port forward to the service
kubectl port-forward svc/products-api 8080:80 -n products-api-dev

# Run tests
BASE_URL=http://localhost:8080 ./api-test.sh
```

### Through Istio Gateway
```bash
# Get the gateway IP
GATEWAY_IP=$(kubectl get svc istio-ingressgateway -n istio-system -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

# Run tests with Host header
curl -H "Host: products-api.local" http://$GATEWAY_IP/api/products
```

## Continuous Integration

These tests can be integrated into CI/CD pipelines:

### GitHub Actions Example
```yaml
- name: Run API Tests
  run: |
    cd gitops/example/tests
    chmod +x api-test.sh
    BASE_URL=http://localhost:8080 ./api-test.sh

- name: Run Load Tests
  run: |
    cd gitops/example/tests
    k6 run --quiet load-test.js
```

### Jenkins Pipeline Example
```groovy
stage('API Tests') {
    steps {
        script {
            sh '''
                cd gitops/example/tests
                chmod +x api-test.sh
                BASE_URL=http://products-api:80 ./api-test.sh
            '''
        }
    }
}
```

## Test Results and Reporting

### API Test Output
The `api-test.sh` script provides colored output:
- ✅ Green: Successful tests
- ❌ Red: Failed tests
- ⚠️ Yellow: Warnings and debug info

### Load Test Metrics
K6 provides detailed metrics:
- **http_req_duration**: Response time percentiles
- **http_req_failed**: Error rate
- **iterations**: Number of test iterations
- **vus**: Virtual users

### Expected Performance Baselines
- **Response Time**: 95th percentile < 500ms
- **Error Rate**: < 1%
- **Throughput**: > 100 requests/second
- **Availability**: > 99.9%

## Troubleshooting

### Common Issues

1. **Connection Refused**
   ```bash
   # Check if API is running
   curl http://localhost:8080/health
   
   # Check port forwarding
   kubectl get svc -n products-api-dev
   ```

2. **404 Errors**
   ```bash
   # Verify API routes
   curl http://localhost:8080/swagger
   
   # Check base URL
   echo $BASE_URL
   ```

3. **Database Connection Issues**
   ```bash
   # Check readiness endpoint
   curl http://localhost:8080/ready
   
   # Check database connectivity
   kubectl logs deployment/products-api -n products-api-dev
   ```

### Debug Mode
Enable verbose output for detailed debugging:
```bash
VERBOSE=true ./api-test.sh
```

## Integration with Monitoring

Test results can be integrated with monitoring systems:

### Prometheus Metrics
The API exposes metrics that can be scraped:
```
http_requests_total
http_request_duration_seconds
```

### Grafana Dashboards
Use the provided dashboard configuration in `../monitoring/grafana-dashboard.json` to visualize test results and API performance.

### Alerting
Set up alerts based on test results:
- API response time > 1s
- Error rate > 5%
- Test failures in CI/CD pipeline