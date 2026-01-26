# Envoy Proxy

Envoy is a high-performance proxy that serves as the data plane for Istio service mesh, handling all network communication between services in our GitOps architecture.

## 🎯 Overview

Envoy in our GitOps workflow provides:
- **Load Balancing** - Advanced load balancing algorithms and health checking
- **Service Discovery** - Dynamic service discovery and endpoint management
- **Traffic Management** - Request routing, retries, and circuit breaking
- **Security** - TLS termination, authentication, and authorization
- **Observability** - Detailed metrics, logging, and distributed tracing

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Envoy Proxy Architecture                 │
├─────────────────────────────────────────────────────────────┤
│  Ingress Gateway (Envoy)                                   │
│  ├── TLS Termination                                       │
│  ├── Request Routing                                       │
│  ├── Rate Limiting                                         │
│  └── Authentication                                        │
├─────────────────────────────────────────────────────────────┤
│  Sidecar Proxies (Envoy)                                   │
│  ├── Service A ←→ Envoy Sidecar                             │
│  ├── Service B ←→ Envoy Sidecar                             │
│  ├── Service C ←→ Envoy Sidecar                             │
│  └── Database ←→ Envoy Sidecar                              │
├─────────────────────────────────────────────────────────────┤
│  Egress Gateway (Envoy)                                    │
│  ├── External API Calls                                    │
│  ├── Security Policies                                     │
│  ├── Traffic Monitoring                                    │
│  └── Protocol Translation                                  │
├─────────────────────────────────────────────────────────────┤
│  Control Plane Integration                                  │
│  ├── xDS APIs (Configuration)                              │
│  ├── Pilot (Service Discovery)                             │
│  ├── Citadel (Certificate Management)                      │
│  └── Telemetry Collection                                  │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Directory Structure

```
envoy/
├── README.md
├── configurations/
│   ├── envoy-gateway.yaml
│   ├── envoy-sidecar.yaml
│   ├── envoy-egress.yaml
│   └── bootstrap-config.yaml
├── filters/
│   ├── http-filters/
│   │   ├── rate-limit.yaml
│   │   ├── jwt-auth.yaml
│   │   ├── cors.yaml
│   │   └── fault-injection.yaml
│   ├── network-filters/
│   │   ├── tcp-proxy.yaml
│   │   ├── http-connection-manager.yaml
│   │   └── tls-inspector.yaml
│   └── access-log-filters/
│       ├── json-format.yaml
│       └── custom-format.yaml
├── load-balancing/
│   ├── round-robin.yaml
│   ├── least-request.yaml
│   ├── ring-hash.yaml
│   └── maglev.yaml
├── security/
│   ├── tls-config.yaml
│   ├── mtls-config.yaml
│   ├── rbac-config.yaml
│   └── ext-authz.yaml
├── observability/
│   ├── metrics-config.yaml
│   ├── tracing-config.yaml
│   ├── access-logs.yaml
│   └── health-checks.yaml
├── examples/
│   ├── standalone-envoy/
│   ├── istio-integration/
│   └── custom-filters/
└── scripts/
    ├── generate-certs.sh
    ├── validate-config.sh
    └── performance-test.sh
```

## 🚀 Quick Setup

### 1. Standalone Envoy Deployment

```yaml
# configurations/envoy-gateway.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: envoy-gateway
  namespace: envoy-system
spec:
  replicas: 2
  selector:
    matchLabels:
      app: envoy-gateway
  template:
    metadata:
      labels:
        app: envoy-gateway
    spec:
      containers:
      - name: envoy
        image: envoyproxy/envoy:v1.28-latest
        ports:
        - containerPort: 8080
          name: http
        - containerPort: 8443
          name: https
        - containerPort: 9901
          name: admin
        command:
        - /usr/local/bin/envoy
        args:
        - -c
        - /etc/envoy/envoy.yaml
        - --service-cluster
        - envoy-gateway
        - --service-node
        - envoy-gateway
        - --log-level
        - info
        volumeMounts:
        - name: envoy-config
          mountPath: /etc/envoy
        - name: certs
          mountPath: /etc/ssl/certs
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /ready
            port: 9901
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 9901
          initialDelaySeconds: 5
          periodSeconds: 5
      volumes:
      - name: envoy-config
        configMap:
          name: envoy-config
      - name: certs
        secret:
          secretName: envoy-certs
---
apiVersion: v1
kind: Service
metadata:
  name: envoy-gateway
  namespace: envoy-system
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
    name: http
  - port: 443
    targetPort: 8443
    name: https
  selector:
    app: envoy-gateway
```

### 2. Envoy Configuration

```yaml
# configurations/bootstrap-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: envoy-config
  namespace: envoy-system
data:
  envoy.yaml: |
    admin:
      address:
        socket_address:
          address: 0.0.0.0
          port_value: 9901
    
    static_resources:
      listeners:
      - name: http_listener
        address:
          socket_address:
            address: 0.0.0.0
            port_value: 8080
        filter_chains:
        - filters:
          - name: envoy.filters.network.http_connection_manager
            typed_config:
              "@type": type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager
              stat_prefix: ingress_http
              access_log:
              - name: envoy.access_loggers.stdout
                typed_config:
                  "@type": type.googleapis.com/envoy.extensions.access_loggers.stream.v3.StdoutAccessLog
                  log_format:
                    json_format:
                      timestamp: "%START_TIME%"
                      method: "%REQ(:METHOD)%"
                      path: "%REQ(X-ENVOY-ORIGINAL-PATH?:PATH)%"
                      protocol: "%PROTOCOL%"
                      response_code: "%RESPONSE_CODE%"
                      response_flags: "%RESPONSE_FLAGS%"
                      bytes_received: "%BYTES_RECEIVED%"
                      bytes_sent: "%BYTES_SENT%"
                      duration: "%DURATION%"
                      upstream_service_time: "%RESP(X-ENVOY-UPSTREAM-SERVICE-TIME)%"
                      x_forwarded_for: "%REQ(X-FORWARDED-FOR)%"
                      user_agent: "%REQ(USER-AGENT)%"
                      request_id: "%REQ(X-REQUEST-ID)%"
                      authority: "%REQ(:AUTHORITY)%"
                      upstream_host: "%UPSTREAM_HOST%"
              route_config:
                name: local_route
                virtual_hosts:
                - name: backend
                  domains: ["*"]
                  routes:
                  - match:
                      prefix: "/api/v1/"
                    route:
                      cluster: backend_service
                      timeout: 30s
                      retry_policy:
                        retry_on: "5xx,reset,connect-failure,refused-stream"
                        num_retries: 3
                        per_try_timeout: 10s
                  - match:
                      prefix: "/health"
                    route:
                      cluster: backend_service
                  - match:
                      prefix: "/"
                    route:
                      cluster: frontend_service
              http_filters:
              - name: envoy.filters.http.cors
                typed_config:
                  "@type": type.googleapis.com/envoy.extensions.filters.http.cors.v3.Cors
              - name: envoy.filters.http.jwt_authn
                typed_config:
                  "@type": type.googleapis.com/envoy.extensions.filters.http.jwt_authn.v3.JwtAuthentication
                  providers:
                    auth0:
                      issuer: "https://your-domain.auth0.com/"
                      audiences:
                      - "your-api-identifier"
                      remote_jwks:
                        http_uri:
                          uri: "https://your-domain.auth0.com/.well-known/jwks.json"
                          cluster: auth0_jwks
                          timeout: 5s
                        cache_duration: 300s
                  rules:
                  - match:
                      prefix: "/api/"
                    requires:
                      provider_name: "auth0"
              - name: envoy.filters.http.local_ratelimit
                typed_config:
                  "@type": type.googleapis.com/udpa.type.v1.TypedStruct
                  type_url: type.googleapis.com/envoy.extensions.filters.http.local_ratelimit.v3.LocalRateLimit
                  value:
                    stat_prefix: http_local_rate_limiter
                    token_bucket:
                      max_tokens: 1000
                      tokens_per_fill: 100
                      fill_interval: 1s
                    filter_enabled:
                      runtime_key: test_enabled
                      default_value:
                        numerator: 100
                        denominator: HUNDRED
                    filter_enforced:
                      runtime_key: test_enforced
                      default_value:
                        numerator: 100
                        denominator: HUNDRED
              - name: envoy.filters.http.router
                typed_config:
                  "@type": type.googleapis.com/envoy.extensions.filters.http.router.v3.Router
      
      - name: https_listener
        address:
          socket_address:
            address: 0.0.0.0
            port_value: 8443
        filter_chains:
        - filters:
          - name: envoy.filters.network.http_connection_manager
            typed_config:
              "@type": type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager
              stat_prefix: ingress_https
              route_config:
                name: local_route
                virtual_hosts:
                - name: backend
                  domains: ["*"]
                  routes:
                  - match:
                      prefix: "/"
                    route:
                      cluster: backend_service
              http_filters:
              - name: envoy.filters.http.router
                typed_config:
                  "@type": type.googleapis.com/envoy.extensions.filters.http.router.v3.Router
          transport_socket:
            name: envoy.transport_sockets.tls
            typed_config:
              "@type": type.googleapis.com/envoy.extensions.transport_sockets.tls.v3.DownstreamTlsContext
              common_tls_context:
                tls_certificates:
                - certificate_chain:
                    filename: "/etc/ssl/certs/tls.crt"
                  private_key:
                    filename: "/etc/ssl/certs/tls.key"
      
      clusters:
      - name: backend_service
        connect_timeout: 5s
        type: STRICT_DNS
        lb_policy: ROUND_ROBIN
        load_assignment:
          cluster_name: backend_service
          endpoints:
          - lb_endpoints:
            - endpoint:
                address:
                  socket_address:
                    address: backend-service
                    port_value: 8080
        health_checks:
        - timeout: 5s
          interval: 10s
          unhealthy_threshold: 3
          healthy_threshold: 2
          http_health_check:
            path: "/health"
        circuit_breakers:
          thresholds:
          - priority: DEFAULT
            max_connections: 100
            max_pending_requests: 50
            max_requests: 100
            max_retries: 3
        outlier_detection:
          consecutive_5xx: 3
          interval: 30s
          base_ejection_time: 30s
          max_ejection_percent: 50
          min_health_percent: 50
      
      - name: frontend_service
        connect_timeout: 5s
        type: STRICT_DNS
        lb_policy: LEAST_REQUEST
        load_assignment:
          cluster_name: frontend_service
          endpoints:
          - lb_endpoints:
            - endpoint:
                address:
                  socket_address:
                    address: frontend-service
                    port_value: 80
      
      - name: auth0_jwks
        connect_timeout: 5s
        type: LOGICAL_DNS
        lb_policy: ROUND_ROBIN
        load_assignment:
          cluster_name: auth0_jwks
          endpoints:
          - lb_endpoints:
            - endpoint:
                address:
                  socket_address:
                    address: your-domain.auth0.com
                    port_value: 443
        transport_socket:
          name: envoy.transport_sockets.tls
          typed_config:
            "@type": type.googleapis.com/envoy.extensions.transport_sockets.tls.v3.UpstreamTlsContext
            sni: your-domain.auth0.com
```

## 🔧 Advanced Configurations

### Load Balancing Strategies

```yaml
# load-balancing/advanced-lb.yaml
clusters:
- name: backend_round_robin
  connect_timeout: 5s
  type: STRICT_DNS
  lb_policy: ROUND_ROBIN
  load_assignment:
    cluster_name: backend_round_robin
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-1
              port_value: 8080
      - endpoint:
          address:
            socket_address:
              address: backend-2
              port_value: 8080

- name: backend_least_request
  connect_timeout: 5s
  type: STRICT_DNS
  lb_policy: LEAST_REQUEST
  least_request_lb_config:
    choice_count: 3
  load_assignment:
    cluster_name: backend_least_request
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-1
              port_value: 8080
        load_balancing_weight: 100
      - endpoint:
          address:
            socket_address:
              address: backend-2
              port_value: 8080
        load_balancing_weight: 200

- name: backend_ring_hash
  connect_timeout: 5s
  type: STRICT_DNS
  lb_policy: RING_HASH
  ring_hash_lb_config:
    minimum_ring_size: 1024
    maximum_ring_size: 8192
    hash_function: XX_HASH
  load_assignment:
    cluster_name: backend_ring_hash
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-1
              port_value: 8080
      - endpoint:
          address:
            socket_address:
              address: backend-2
              port_value: 8080

- name: backend_maglev
  connect_timeout: 5s
  type: STRICT_DNS
  lb_policy: MAGLEV
  maglev_lb_config:
    table_size: 65537
  load_assignment:
    cluster_name: backend_maglev
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-1
              port_value: 8080
      - endpoint:
          address:
            socket_address:
              address: backend-2
              port_value: 8080
```

### Circuit Breaker Configuration

```yaml
# security/circuit-breaker.yaml
clusters:
- name: backend_with_circuit_breaker
  connect_timeout: 5s
  type: STRICT_DNS
  lb_policy: ROUND_ROBIN
  load_assignment:
    cluster_name: backend_with_circuit_breaker
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-service
              port_value: 8080
  circuit_breakers:
    thresholds:
    - priority: DEFAULT
      max_connections: 100
      max_pending_requests: 50
      max_requests: 100
      max_retries: 3
      track_remaining: true
    - priority: HIGH
      max_connections: 200
      max_pending_requests: 100
      max_requests: 200
      max_retries: 5
  outlier_detection:
    consecutive_5xx: 3
    consecutive_gateway_failure: 3
    interval: 30s
    base_ejection_time: 30s
    max_ejection_percent: 50
    min_health_percent: 50
    split_external_local_origin_errors: true
```

### Rate Limiting Configuration

```yaml
# filters/http-filters/rate-limit.yaml
http_filters:
- name: envoy.filters.http.local_ratelimit
  typed_config:
    "@type": type.googleapis.com/udpa.type.v1.TypedStruct
    type_url: type.googleapis.com/envoy.extensions.filters.http.local_ratelimit.v3.LocalRateLimit
    value:
      stat_prefix: http_local_rate_limiter
      token_bucket:
        max_tokens: 1000
        tokens_per_fill: 100
        fill_interval: 1s
      filter_enabled:
        runtime_key: local_rate_limit_enabled
        default_value:
          numerator: 100
          denominator: HUNDRED
      filter_enforced:
        runtime_key: local_rate_limit_enforced
        default_value:
          numerator: 100
          denominator: HUNDRED
      response_headers_to_add:
      - append: false
        header:
          key: x-local-rate-limit
          value: 'true'
      local_rate_limit_per_downstream_connection: false

- name: envoy.filters.http.ratelimit
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.filters.http.ratelimit.v3.RateLimit
    domain: production
    stage: 0
    request_type: external
    timeout: 0.25s
    failure_mode_deny: false
    rate_limit_service:
      grpc_service:
        envoy_grpc:
          cluster_name: rate_limit_service
        timeout: 0.25s
      transport_api_version: V3
```

## 🔒 Security Features

### mTLS Configuration

```yaml
# security/mtls-config.yaml
transport_socket:
  name: envoy.transport_sockets.tls
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.transport_sockets.tls.v3.DownstreamTlsContext
    require_client_certificate: true
    common_tls_context:
      tls_certificates:
      - certificate_chain:
          filename: "/etc/ssl/certs/server.crt"
        private_key:
          filename: "/etc/ssl/certs/server.key"
      validation_context:
        trusted_ca:
          filename: "/etc/ssl/certs/ca.crt"
        verify_certificate_spki:
        - "NdwrBQdxetywlKDJ2+Wusr/rLAHzw3M5XdnT6sHG2Uw="
        verify_certificate_hash:
        - "E9C42B4E90075B1C1DEAE68E1E52E09D6B4B5F4A"
      tls_params:
        tls_minimum_protocol_version: TLSv1_2
        tls_maximum_protocol_version: TLSv1_3
        cipher_suites:
        - "ECDHE-ECDSA-AES256-GCM-SHA384"
        - "ECDHE-RSA-AES256-GCM-SHA384"
        - "ECDHE-ECDSA-CHACHA20-POLY1305"
        - "ECDHE-RSA-CHACHA20-POLY1305"
        ecdh_curves:
        - "X25519"
        - "P-256"
```

### External Authorization

```yaml
# security/ext-authz.yaml
http_filters:
- name: envoy.filters.http.ext_authz
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.filters.http.ext_authz.v3.ExtAuthz
    transport_api_version: V3
    grpc_service:
      envoy_grpc:
        cluster_name: ext_authz_service
      timeout: 1s
    failure_mode_allow: false
    include_peer_certificate: true
    status_on_error:
      code: Forbidden
    metadata_context_namespaces:
    - envoy.common
    with_request_body:
      max_request_bytes: 8192
      allow_partial_message: true
      pack_as_bytes: true
```

## 📊 Observability Configuration

### Metrics and Tracing

```yaml
# observability/metrics-config.yaml
stats_config:
  stats_tags:
  - tag_name: cluster_name
    regex: "^cluster\\.((.+?)\\.).*"
  - tag_name: virtual_host_name
    regex: "^vhost\\.((.+?)\\.).*"
  - tag_name: listener_address
    regex: "^listener\\.((.+?)\\.).*"
  histogram_bucket_settings:
  - match:
      prefix: "http"
    buckets: [0.5, 1, 5, 10, 25, 50, 100, 250, 500, 1000, 2500, 5000, 10000]

tracing:
  http:
    name: envoy.tracers.jaeger
    typed_config:
      "@type": type.googleapis.com/envoy.config.trace.v3.JaegerConfig
      collector_cluster: jaeger
      collector_endpoint: "/api/traces"
      collector_endpoint_version: HTTP_JSON
      trace_id_128bit: true
      shared_span_context: false
```

### Custom Access Logs

```yaml
# observability/access-logs.yaml
access_log:
- name: envoy.access_loggers.file
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.access_loggers.file.v3.FileAccessLog
    path: "/var/log/envoy/access.log"
    log_format:
      json_format:
        timestamp: "%START_TIME%"
        method: "%REQ(:METHOD)%"
        path: "%REQ(X-ENVOY-ORIGINAL-PATH?:PATH)%"
        protocol: "%PROTOCOL%"
        response_code: "%RESPONSE_CODE%"
        response_flags: "%RESPONSE_FLAGS%"
        bytes_received: "%BYTES_RECEIVED%"
        bytes_sent: "%BYTES_SENT%"
        duration: "%DURATION%"
        upstream_service_time: "%RESP(X-ENVOY-UPSTREAM-SERVICE-TIME)%"
        x_forwarded_for: "%REQ(X-FORWARDED-FOR)%"
        user_agent: "%REQ(USER-AGENT)%"
        request_id: "%REQ(X-REQUEST-ID)%"
        authority: "%REQ(:AUTHORITY)%"
        upstream_host: "%UPSTREAM_HOST%"
        upstream_cluster: "%UPSTREAM_CLUSTER%"
        upstream_local_address: "%UPSTREAM_LOCAL_ADDRESS%"
        downstream_local_address: "%DOWNSTREAM_LOCAL_ADDRESS%"
        downstream_remote_address: "%DOWNSTREAM_REMOTE_ADDRESS%"
        requested_server_name: "%REQUESTED_SERVER_NAME%"
        route_name: "%ROUTE_NAME%"

- name: envoy.access_loggers.http_grpc
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.access_loggers.grpc.v3.HttpGrpcAccessLogConfig
    common_config:
      log_name: "http_access_log"
      grpc_service:
        envoy_grpc:
          cluster_name: access_log_service
        timeout: 1s
      transport_api_version: V3
    additional_request_headers_to_log:
    - "x-request-id"
    - "x-user-id"
    - "authorization"
    additional_response_headers_to_log:
    - "x-response-time"
    - "x-cache-status"
```

## 🔄 GitOps Integration

### Envoy with ArgoCD

```yaml
# argocd/envoy-application.yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: envoy-gateway
  namespace: argocd
spec:
  project: infrastructure
  source:
    repoURL: https://github.com/company/envoy-configs
    targetRevision: main
    path: envoy/gateway
  destination:
    server: https://kubernetes.default.svc
    namespace: envoy-system
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
        - group: ""
          kind: ConfigMap
        - group: ""
          kind: Secret
    - order: 1
      resources:
        - group: apps
          kind: Deployment
        - group: ""
          kind: Service
```

### Helm Chart for Envoy

```yaml
# helm/envoy/Chart.yaml
apiVersion: v2
name: envoy-gateway
description: Envoy proxy gateway
type: application
version: 0.1.0
appVersion: "1.28.0"

dependencies:
  - name: common
    repository: https://charts.bitnami.com/bitnami
    version: 2.x.x
```

```yaml
# helm/envoy/values.yaml
image:
  repository: envoyproxy/envoy
  tag: v1.28-latest
  pullPolicy: IfNotPresent

replicaCount: 2

service:
  type: LoadBalancer
  ports:
    http: 80
    https: 443
    admin: 9901

resources:
  requests:
    cpu: 100m
    memory: 128Mi
  limits:
    cpu: 500m
    memory: 512Mi

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 80

config:
  listeners:
    - name: http_listener
      port: 8080
      protocol: HTTP
    - name: https_listener
      port: 8443
      protocol: HTTPS
  
  clusters:
    - name: backend_service
      endpoints:
        - address: backend-service
          port: 8080
      health_check:
        path: /health
        interval: 10s
        timeout: 5s
      circuit_breaker:
        max_connections: 100
        max_pending_requests: 50

tls:
  enabled: true
  secretName: envoy-tls-secret

monitoring:
  enabled: true
  serviceMonitor:
    enabled: true
    interval: 30s
    path: /stats/prometheus
```

## 🛠️ Performance Tuning

### Connection Pool Settings

```yaml
# Performance optimized cluster configuration
clusters:
- name: high_performance_backend
  connect_timeout: 1s
  type: STRICT_DNS
  lb_policy: LEAST_REQUEST
  http2_protocol_options:
    max_concurrent_streams: 100
    initial_stream_window_size: 65536
    initial_connection_window_size: 1048576
  upstream_connection_options:
    tcp_keepalive:
      keepalive_probes: 3
      keepalive_time: 30
      keepalive_interval: 5
    socket_options:
    - level: 1
      name: 7
      int_value: 1
    - level: 6
      name: 1
      int_value: 1
  load_assignment:
    cluster_name: high_performance_backend
    endpoints:
    - lb_endpoints:
      - endpoint:
          address:
            socket_address:
              address: backend-service
              port_value: 8080
  circuit_breakers:
    thresholds:
    - priority: DEFAULT
      max_connections: 1000
      max_pending_requests: 500
      max_requests: 1000
      max_retries: 10
      retry_budget:
        budget_percent:
          value: 25.0
        min_retry_concurrency: 10
```

### Buffer Limits

```yaml
# Buffer configuration for high throughput
listener_filters:
- name: envoy.filters.listener.original_dst
  typed_config:
    "@type": type.googleapis.com/envoy.extensions.filters.listener.original_dst.v3.OriginalDst

per_connection_buffer_limit_bytes: 32768

http_connection_manager:
  request_timeout: 300s
  request_headers_timeout: 60s
  stream_idle_timeout: 300s
  drain_timeout: 60s
  delayed_close_timeout: 1s
  
  # Buffer settings
  buffer_flush_interval: 1s
  max_request_headers_kb: 60
  max_request_headers_count: 100
  
  # Connection settings
  connection_idle_interval: 60s
  max_connection_duration: 0s
  max_stream_duration: 0s
```

## 🚨 Troubleshooting

### Debug Configuration

```bash
# Validate Envoy configuration
envoy --mode validate --config-path /etc/envoy/envoy.yaml

# Check admin interface
curl http://localhost:9901/help
curl http://localhost:9901/stats
curl http://localhost:9901/clusters
curl http://localhost:9901/listeners
curl http://localhost:9901/config_dump

# Check health status
curl http://localhost:9901/ready
curl http://localhost:9901/server_info
```

### Common Issues

#### 1. Configuration Validation
```bash
# Test configuration syntax
envoy --mode validate --config-path envoy.yaml

# Check for deprecated fields
envoy --mode validate --config-path envoy.yaml --log-level warn
```

#### 2. Connection Issues
```bash
# Check cluster status
curl http://localhost:9901/clusters

# Check listener status
curl http://localhost:9901/listeners

# View active connections
curl http://localhost:9901/stats | grep connection
```

#### 3. Performance Issues
```bash
# Check memory usage
curl http://localhost:9901/memory

# View request statistics
curl http://localhost:9901/stats | grep http

# Check circuit breaker status
curl http://localhost:9901/stats | grep circuit_breaker
```

## 📚 Best Practices

### 1. Configuration Management
- **Version Control** - Store all Envoy configs in Git
- **Validation** - Always validate configs before deployment
- **Gradual Rollouts** - Use canary deployments for config changes
- **Monitoring** - Monitor config reload success/failure
- **Documentation** - Document all custom configurations

### 2. Performance Optimization
- **Connection Pooling** - Configure appropriate connection limits
- **Buffer Tuning** - Optimize buffer sizes for your workload
- **Load Balancing** - Choose appropriate LB algorithms
- **Health Checks** - Configure proper health checking
- **Circuit Breakers** - Implement circuit breaker patterns

### 3. Security
- **TLS Configuration** - Use strong TLS settings
- **Certificate Management** - Automate certificate rotation
- **Access Control** - Implement proper RBAC
- **Rate Limiting** - Protect against abuse
- **Audit Logging** - Log security-relevant events

### 4. Observability
- **Metrics Collection** - Export relevant metrics
- **Distributed Tracing** - Enable tracing for requests
- **Access Logging** - Log all requests appropriately
- **Health Monitoring** - Monitor Envoy health
- **Alerting** - Set up alerts for critical metrics

---

**Next Steps:**
1. Deploy Envoy in your environment
2. Configure basic routing and load balancing
3. Implement security policies
4. Set up monitoring and observability
5. Integrate with your GitOps workflow

For advanced Envoy features and troubleshooting, refer to the [Envoy Documentation](https://www.envoyproxy.io/docs/) and [Envoy Best Practices](https://www.envoyproxy.io/docs/envoy/latest/configuration/best_practices/edge).