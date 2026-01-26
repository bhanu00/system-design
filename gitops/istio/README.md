# Istio Service Mesh

Istio is a service mesh that provides traffic management, security, and observability for microservices in our GitOps architecture.

## 🎯 Overview

Istio in our GitOps workflow provides:
- **Traffic Management** - Load balancing, routing, and traffic splitting
- **Security** - mTLS, authentication, and authorization policies
- **Observability** - Metrics, logging, and distributed tracing
- **Policy Enforcement** - Rate limiting, access control, and compliance
- **Resilience** - Circuit breakers, retries, and fault injection

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Istio Service Mesh                      │
├─────────────────────────────────────────────────────────────┤
│  Control Plane (istiod)                                    │
│  ├── Pilot (Traffic Management)                            │
│  ├── Citadel (Security)                                    │
│  ├── Galley (Configuration)                                │
│  └── Mixer (Telemetry & Policy) [Deprecated]               │
├─────────────────────────────────────────────────────────────┤
│  Data Plane (Envoy Sidecars)                               │
│  ├── Service A ←→ Envoy Proxy                               │
│  ├── Service B ←→ Envoy Proxy                               │
│  ├── Service C ←→ Envoy Proxy                               │
│  └── Ingress Gateway ←→ Envoy Proxy                         │
├─────────────────────────────────────────────────────────────┤
│  Add-ons                                                   │
│  ├── Kiali (Service Mesh Visualization)                    │
│  ├── Jaeger (Distributed Tracing)                          │
│  ├── Prometheus (Metrics Collection)                       │
│  └── Grafana (Metrics Visualization)                       │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
istio/
├── README.md
├── installation/
│   ├── istio-operator.yaml
│   ├── istio-control-plane.yaml
│   └── addons/
│       ├── kiali.yaml
│       ├── jaeger.yaml
│       ├── prometheus.yaml
│       └── grafana.yaml
├── gateways/
│   ├── istio-gateway.yaml
│   ├── external-gateway.yaml
│   └── internal-gateway.yaml
├── virtual-services/
│   ├── frontend-vs.yaml
│   ├── backend-vs.yaml
│   └── api-vs.yaml
├── destination-rules/
│   ├── frontend-dr.yaml
│   ├── backend-dr.yaml
│   └── circuit-breaker-dr.yaml
├── security/
│   ├── peer-authentication.yaml
│   ├── authorization-policy.yaml
│   ├── request-authentication.yaml
│   └── security-policy.yaml
├── traffic-management/
│   ├── canary-deployment.yaml
│   ├── blue-green-deployment.yaml
│   ├── traffic-splitting.yaml
│   └── fault-injection.yaml
├── observability/
│   ├── telemetry.yaml
│   ├── access-logging.yaml
│   └── distributed-tracing.yaml
└── examples/
    ├── bookinfo/
    ├── microservices/
    └── ingress-examples/
```

## 🚀 Quick Setup

### 1. Install Istio

```bash
# Download Istio
curl -L https://istio.io/downloadIstio | sh -
cd istio-*
export PATH=$PWD/bin:$PATH

# Install Istio with demo profile
istioctl install --set values.pilot.traceSampling=100

# Verify installation
kubectl get pods -n istio-system

# Enable automatic sidecar injection
kubectl label namespace default istio-injection=enabled
```

### 2. Install Add-ons

```bash
# Install Kiali, Jaeger, Prometheus, and Grafana
kubectl apply -f samples/addons/

# Wait for deployments
kubectl rollout status deployment/kiali -n istio-system
kubectl rollout status deployment/jaeger -n istio-system
kubectl rollout status deployment/prometheus -n istio-system
kubectl rollout status deployment/grafana -n istio-system
```

### 3. Access Dashboards

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

## 🔧 Configuration

### Istio Control Plane Configuration

```yaml
# installation/istio-control-plane.yaml
apiVersion: install.istio.io/v1alpha1
kind: IstioOperator
metadata:
  name: control-plane
  namespace: istio-system
spec:
  values:
    global:
      meshID: mesh1
      multiCluster:
        clusterName: cluster1
      network: network1
    pilot:
      traceSampling: 1.0
      env:
        EXTERNAL_ISTIOD: false
  components:
    pilot:
      k8s:
        resources:
          requests:
            cpu: 200m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        hpaSpec:
          minReplicas: 2
          maxReplicas: 5
          metrics:
          - type: Resource
            resource:
              name: cpu
              target:
                type: Utilization
                averageUtilization: 80
    ingressGateways:
    - name: istio-ingressgateway
      enabled: true
      k8s:
        service:
          type: LoadBalancer
          ports:
          - port: 15021
            targetPort: 15021
            name: status-port
          - port: 80
            targetPort: 8080
            name: http2
          - port: 443
            targetPort: 8443
            name: https
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 2000m
            memory: 1024Mi
        hpaSpec:
          minReplicas: 2
          maxReplicas: 5
    egressGateways:
    - name: istio-egressgateway
      enabled: true
      k8s:
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 2000m
            memory: 1024Mi
```

## 🌐 Traffic Management

### Gateway Configuration

```yaml
# gateways/istio-gateway.yaml
apiVersion: networking.istio.io/v1beta1
kind: Gateway
metadata:
  name: frontend-gateway
  namespace: default
spec:
  selector:
    istio: ingressgateway
  servers:
  - port:
      number: 80
      name: http
      protocol: HTTP
    hosts:
    - app.company.com
    tls:
      httpsRedirect: true
  - port:
      number: 443
      name: https
      protocol: HTTPS
    tls:
      mode: SIMPLE
      credentialName: app-tls-secret
    hosts:
    - app.company.com
---
apiVersion: networking.istio.io/v1beta1
kind: Gateway
metadata:
  name: api-gateway
  namespace: default
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
      credentialName: api-tls-secret
    hosts:
    - api.company.com
```

### Virtual Service Configuration

```yaml
# virtual-services/frontend-vs.yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: frontend
  namespace: default
spec:
  hosts:
  - app.company.com
  gateways:
  - frontend-gateway
  http:
  - match:
    - uri:
        prefix: /api/
    route:
    - destination:
        host: backend-service
        port:
          number: 8080
  - match:
    - uri:
        prefix: /
    route:
    - destination:
        host: frontend-service
        port:
          number: 80
---
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: backend-api
  namespace: default
spec:
  hosts:
  - api.company.com
  gateways:
  - api-gateway
  http:
  - match:
    - uri:
        prefix: /v1/
    route:
    - destination:
        host: backend-v1
        port:
          number: 8080
      weight: 90
    - destination:
        host: backend-v2
        port:
          number: 8080
      weight: 10
    timeout: 30s
    retries:
      attempts: 3
      perTryTimeout: 10s
      retryOn: 5xx,reset,connect-failure,refused-stream
```

### Destination Rules

```yaml
# destination-rules/backend-dr.yaml
apiVersion: networking.istio.io/v1beta1
kind: DestinationRule
metadata:
  name: backend-service
  namespace: default
spec:
  host: backend-service
  trafficPolicy:
    loadBalancer:
      simple: LEAST_CONN
    connectionPool:
      tcp:
        maxConnections: 100
      http:
        http1MaxPendingRequests: 50
        http2MaxRequests: 100
        maxRequestsPerConnection: 10
        maxRetries: 3
        consecutiveGatewayErrors: 5
        interval: 30s
        baseEjectionTime: 30s
        maxEjectionPercent: 50
    outlierDetection:
      consecutiveGatewayErrors: 5
      interval: 30s
      baseEjectionTime: 30s
      maxEjectionPercent: 50
      minHealthPercent: 50
  subsets:
  - name: v1
    labels:
      version: v1
    trafficPolicy:
      circuitBreaker:
        consecutiveGatewayErrors: 3
        interval: 10s
        baseEjectionTime: 10s
  - name: v2
    labels:
      version: v2
    trafficPolicy:
      circuitBreaker:
        consecutiveGatewayErrors: 3
        interval: 10s
        baseEjectionTime: 10s
```

## 🔒 Security Configuration

### Peer Authentication

```yaml
# security/peer-authentication.yaml
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: istio-system
spec:
  mtls:
    mode: STRICT
---
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: frontend-peer-auth
  namespace: default
spec:
  selector:
    matchLabels:
      app: frontend
  mtls:
    mode: STRICT
  portLevelMtls:
    80:
      mode: DISABLE  # Allow plain HTTP for health checks
```

### Authorization Policies

```yaml
# security/authorization-policy.yaml
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: frontend-authz
  namespace: default
spec:
  selector:
    matchLabels:
      app: frontend
  rules:
  - from:
    - source:
        principals: ["cluster.local/ns/istio-system/sa/istio-ingressgateway-service-account"]
  - to:
    - operation:
        methods: ["GET", "POST"]
---
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: backend-authz
  namespace: default
spec:
  selector:
    matchLabels:
      app: backend
  rules:
  - from:
    - source:
        principals: ["cluster.local/ns/default/sa/frontend"]
  - to:
    - operation:
        methods: ["GET", "POST", "PUT", "DELETE"]
        paths: ["/api/*"]
  - when:
    - key: request.headers[authorization]
      values: ["Bearer *"]
---
apiVersion: security.istio.io/v1beta1
kind: AuthorizationPolicy
metadata:
  name: deny-all
  namespace: default
spec:
  selector:
    matchLabels:
      app: sensitive-service
  # No rules means deny all
```

### Request Authentication

```yaml
# security/request-authentication.yaml
apiVersion: security.istio.io/v1beta1
kind: RequestAuthentication
metadata:
  name: jwt-auth
  namespace: default
spec:
  selector:
    matchLabels:
      app: backend
  jwtRules:
  - issuer: "https://auth.company.com"
    jwksUri: "https://auth.company.com/.well-known/jwks.json"
    audiences:
    - "api.company.com"
    forwardOriginalToken: true
  - issuer: "https://accounts.google.com"
    jwksUri: "https://www.googleapis.com/oauth2/v3/certs"
    audiences:
    - "your-google-client-id.apps.googleusercontent.com"
```

## 🔄 Advanced Traffic Management

### Canary Deployment

```yaml
# traffic-management/canary-deployment.yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: backend-canary
  namespace: default
spec:
  hosts:
  - backend-service
  http:
  - match:
    - headers:
        canary:
          exact: "true"
    route:
    - destination:
        host: backend-service
        subset: v2
  - route:
    - destination:
        host: backend-service
        subset: v1
      weight: 95
    - destination:
        host: backend-service
        subset: v2
      weight: 5
---
apiVersion: networking.istio.io/v1beta1
kind: DestinationRule
metadata:
  name: backend-canary-dr
  namespace: default
spec:
  host: backend-service
  subsets:
  - name: v1
    labels:
      version: v1
  - name: v2
    labels:
      version: v2
```

### Blue-Green Deployment

```yaml
# traffic-management/blue-green-deployment.yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: backend-blue-green
  namespace: default
spec:
  hosts:
  - backend-service
  http:
  - route:
    - destination:
        host: backend-service
        subset: blue  # Switch to 'green' for deployment
      weight: 100
---
apiVersion: networking.istio.io/v1beta1
kind: DestinationRule
metadata:
  name: backend-blue-green-dr
  namespace: default
spec:
  host: backend-service
  subsets:
  - name: blue
    labels:
      version: blue
  - name: green
    labels:
      version: green
```

### Fault Injection

```yaml
# traffic-management/fault-injection.yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: backend-fault-injection
  namespace: default
spec:
  hosts:
  - backend-service
  http:
  - match:
    - headers:
        test-fault:
          exact: "delay"
    fault:
      delay:
        percentage:
          value: 50
        fixedDelay: 5s
    route:
    - destination:
        host: backend-service
  - match:
    - headers:
        test-fault:
          exact: "abort"
    fault:
      abort:
        percentage:
          value: 50
        httpStatus: 500
    route:
    - destination:
        host: backend-service
  - route:
    - destination:
        host: backend-service
```

## 📊 Observability Configuration

### Telemetry Configuration

```yaml
# observability/telemetry.yaml
apiVersion: telemetry.istio.io/v1alpha1
kind: Telemetry
metadata:
  name: default
  namespace: istio-system
spec:
  metrics:
  - providers:
    - name: prometheus
  - overrides:
    - match:
        metric: ALL_METRICS
      tagOverrides:
        request_id:
          operation: UPSERT
          value: "%{REQUEST_ID}"
        custom_header:
          operation: UPSERT
          value: "%{REQUEST_HEADERS['x-custom-header']}"
  accessLogging:
  - providers:
    - name: otel
  tracing:
  - providers:
    - name: jaeger
  - customTags:
      user_id:
        header:
          name: x-user-id
      request_id:
        header:
          name: x-request-id
```

### Access Logging

```yaml
# observability/access-logging.yaml
apiVersion: telemetry.istio.io/v1alpha1
kind: Telemetry
metadata:
  name: access-logging
  namespace: default
spec:
  accessLogging:
  - providers:
    - name: otel
  - format: |
      {
        "timestamp": "%START_TIME%",
        "method": "%REQ(:METHOD)%",
        "path": "%REQ(X-ENVOY-ORIGINAL-PATH?:PATH)%",
        "protocol": "%PROTOCOL%",
        "response_code": "%RESPONSE_CODE%",
        "response_flags": "%RESPONSE_FLAGS%",
        "bytes_received": "%BYTES_RECEIVED%",
        "bytes_sent": "%BYTES_SENT%",
        "duration": "%DURATION%",
        "upstream_service_time": "%RESP(X-ENVOY-UPSTREAM-SERVICE-TIME)%",
        "x_forwarded_for": "%REQ(X-FORWARDED-FOR)%",
        "user_agent": "%REQ(USER-AGENT)%",
        "request_id": "%REQ(X-REQUEST-ID)%",
        "authority": "%REQ(:AUTHORITY)%",
        "upstream_host": "%UPSTREAM_HOST%",
        "upstream_cluster": "%UPSTREAM_CLUSTER%",
        "upstream_local_address": "%UPSTREAM_LOCAL_ADDRESS%",
        "downstream_local_address": "%DOWNSTREAM_LOCAL_ADDRESS%",
        "downstream_remote_address": "%DOWNSTREAM_REMOTE_ADDRESS%",
        "requested_server_name": "%REQUESTED_SERVER_NAME%",
        "route_name": "%ROUTE_NAME%"
      }
```

## 🔄 GitOps Integration

### ArgoCD Application for Istio

```yaml
# argocd/istio-application.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: istio-system
  namespace: argocd
spec:
  project: infrastructure
  source:
    repoURL: https://github.com/company/istio-configs
    targetRevision: main
    path: istio
  destination:
    server: https://kubernetes.default.svc
    namespace: istio-system
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
      - ServerSideApply=true
  syncWaves:
    - order: 0
      resources:
        - group: install.istio.io
          kind: IstioOperator
    - order: 1
      resources:
        - group: networking.istio.io
          kind: Gateway
    - order: 2
      resources:
        - group: networking.istio.io
          kind: VirtualService
        - group: networking.istio.io
          kind: DestinationRule
```

### Helm Chart for Istio Configuration

```yaml
# helm/istio-config/Chart.yaml
apiVersion: v2
name: istio-config
description: Istio configuration for microservices
type: application
version: 0.1.0
appVersion: "1.0"

dependencies:
  - name: base
    repository: https://istio-release.storage.googleapis.com/charts
    version: 1.19.0
  - name: istiod
    repository: https://istio-release.storage.googleapis.com/charts
    version: 1.19.0
  - name: gateway
    repository: https://istio-release.storage.googleapis.com/charts
    version: 1.19.0
```

```yaml
# helm/istio-config/values.yaml
global:
  meshID: mesh1
  network: network1

gateways:
  - name: frontend-gateway
    hosts:
      - app.company.com
    tls:
      enabled: true
      secretName: app-tls-secret
  - name: api-gateway
    hosts:
      - api.company.com
    tls:
      enabled: true
      secretName: api-tls-secret

virtualServices:
  - name: frontend
    gateway: frontend-gateway
    host: app.company.com
    routes:
      - match:
          prefix: /api/
        destination:
          host: backend-service
          port: 8080
      - match:
          prefix: /
        destination:
          host: frontend-service
          port: 80

security:
  peerAuthentication:
    mode: STRICT
  authorizationPolicies:
    - name: frontend-authz
      selector:
        app: frontend
      rules:
        - from:
            principals: ["cluster.local/ns/istio-system/sa/istio-ingressgateway-service-account"]
```

## 🛠️ Monitoring and Troubleshooting

### Istio Proxy Configuration

```bash
# Check Envoy configuration
istioctl proxy-config cluster <pod-name> -n <namespace>
istioctl proxy-config listener <pod-name> -n <namespace>
istioctl proxy-config route <pod-name> -n <namespace>
istioctl proxy-config endpoint <pod-name> -n <namespace>

# Check Envoy access logs
kubectl logs <pod-name> -c istio-proxy -n <namespace>

# Analyze configuration
istioctl analyze -n <namespace>
```

### Traffic Analysis

```bash
# Check traffic routing
istioctl proxy-config route <pod-name> -n <namespace> --name <route-name>

# Verify mTLS status
istioctl authn tls-check <pod-name>.<namespace>.svc.cluster.local

# Check security policies
istioctl x authz check <pod-name> -n <namespace>
```

### Performance Monitoring

```bash
# Check proxy stats
istioctl proxy-status

# Get proxy metrics
kubectl exec <pod-name> -c istio-proxy -n <namespace> -- curl localhost:15000/stats/prometheus

# Check control plane metrics
kubectl port-forward -n istio-system svc/istiod 15014:15014
curl http://localhost:15014/metrics
```

## 📚 Best Practices

### 1. Traffic Management
- **Use Gradual Rollouts** - Implement canary deployments for safer releases
- **Set Proper Timeouts** - Configure appropriate timeout values
- **Implement Circuit Breakers** - Prevent cascade failures
- **Use Retry Policies** - Handle transient failures gracefully
- **Monitor Traffic Patterns** - Use observability tools to understand traffic flow

### 2. Security
- **Enable mTLS** - Use strict mTLS for service-to-service communication
- **Implement Zero Trust** - Use authorization policies for all services
- **Validate JWT Tokens** - Implement proper authentication
- **Use Least Privilege** - Grant minimal required permissions
- **Regular Security Audits** - Review and update security policies

### 3. Observability
- **Enable Distributed Tracing** - Track requests across services
- **Collect Metrics** - Monitor service performance and health
- **Implement Logging** - Capture detailed access logs
- **Set Up Alerts** - Monitor critical metrics and errors
- **Use Service Topology** - Visualize service dependencies

### 4. Operations
- **Version Control Configuration** - Store all Istio configs in Git
- **Test Configuration Changes** - Validate configs before deployment
- **Monitor Control Plane** - Ensure Istio components are healthy
- **Plan for Upgrades** - Regular Istio version updates
- **Backup Configuration** - Regular backups of Istio settings

## 🚨 Common Issues and Solutions

### 1. Sidecar Injection Issues
```bash
# Check if namespace has injection enabled
kubectl get namespace <namespace> --show-labels

# Manually inject sidecar
istioctl kube-inject -f deployment.yaml | kubectl apply -f -

# Check injection status
kubectl get pods -o jsonpath='{.items[*].spec.containers[*].name}'
```

### 2. mTLS Issues
```bash
# Check mTLS status
istioctl authn tls-check <service>.<namespace>.svc.cluster.local

# Verify certificates
istioctl proxy-config secret <pod-name> -n <namespace>
```

### 3. Traffic Routing Issues
```bash
# Validate virtual service
istioctl analyze

# Check route configuration
istioctl proxy-config route <pod-name> -n <namespace>

# Test connectivity
kubectl exec -it <pod-name> -c <container> -- curl -v <service-url>
```

---

**Next Steps:**
1. Install Istio in your EKS cluster
2. Configure gateways and virtual services
3. Implement security policies
4. Set up observability tools
5. Integrate with your GitOps workflow

For advanced Istio features and troubleshooting, refer to the [Istio Documentation](https://istio.io/latest/docs/) and [Istio Best Practices](https://istio.io/latest/docs/ops/best-practices/).