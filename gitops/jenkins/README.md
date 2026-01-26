# Jenkins CI/CD Pipeline

Jenkins serves as our Continuous Integration (CI) server, automating the build, test, and deployment pipeline for our applications.

## 🎯 Overview

Jenkins in our GitOps workflow handles:
- **Source Code Integration** - Pulls code from GitHub repositories
- **Build Automation** - Compiles applications and creates artifacts
- **Testing** - Runs unit tests, integration tests, and security scans
- **Docker Image Building** - Creates and pushes container images
- **Deployment Triggering** - Updates Helm charts and triggers ArgoCD

## 🏗️ Architecture

```
GitHub → Jenkins → Docker Registry → Helm Charts → ArgoCD → EKS
   ↓         ↓           ↓              ↓           ↓        ↓
 Webhook   Build      Push Image    Update Chart  Deploy   Running App
```

## 📁 Directory Structure

```
jenkins/
├── README.md
├── pipelines/
│   ├── Jenkinsfile.build
│   ├── Jenkinsfile.deploy
│   └── Jenkinsfile.rollback
├── configurations/
│   ├── jenkins.yaml
│   ├── plugins.txt
│   └── security-config.groovy
├── scripts/
│   ├── build.sh
│   ├── test.sh
│   └── docker-build.sh
├── helm-charts/
│   └── jenkins/
│       ├── Chart.yaml
│       ├── values.yaml
│       └── templates/
└── examples/
    ├── nodejs-pipeline/
    ├── dotnet-pipeline/
    └── python-pipeline/
```

## 🚀 Quick Setup

### 1. Install Jenkins on EKS

```bash
# Add Jenkins Helm repository
helm repo add jenkins https://charts.jenkins.io
helm repo update

# Install Jenkins
helm install jenkins jenkins/jenkins \
  --namespace jenkins \
  --create-namespace \
  --values helm-charts/jenkins/values.yaml
```

### 2. Get Jenkins Admin Password

```bash
kubectl exec --namespace jenkins -it svc/jenkins -c jenkins -- /bin/cat /run/secrets/additional/chart-admin-password && echo
```

### 3. Access Jenkins UI

```bash
kubectl --namespace jenkins port-forward svc/jenkins 8080:8080
```

Navigate to `http://localhost:8080`

## 🔧 Configuration

### Essential Plugins

```txt
# Core plugins (plugins.txt)
kubernetes:latest
workflow-aggregator:latest
git:latest
github:latest
docker-workflow:latest
pipeline-stage-view:latest
blueocean:latest
helm:latest
```

### Jenkins Configuration as Code (JCasC)

```yaml
# jenkins.yaml
jenkins:
  systemMessage: "GitOps Jenkins Controller"
  numExecutors: 0
  mode: NORMAL
  
  clouds:
    - kubernetes:
        name: "kubernetes"
        serverUrl: "https://kubernetes.default"
        namespace: "jenkins"
        
  securityRealm:
    local:
      allowsSignup: false
      users:
        - id: "admin"
          password: "${JENKINS_ADMIN_PASSWORD}"
          
  authorizationStrategy:
    roleBased:
      roles:
        global:
          - name: "admin"
            description: "Jenkins administrators"
            permissions:
              - "Overall/Administer"
            assignments:
              - "admin"
```

## 📝 Pipeline Examples

### Basic Build Pipeline

```groovy
// Jenkinsfile.build
pipeline {
    agent {
        kubernetes {
            yaml """
                apiVersion: v1
                kind: Pod
                spec:
                  containers:
                  - name: docker
                    image: docker:latest
                    command:
                    - cat
                    tty: true
                    volumeMounts:
                    - mountPath: /var/run/docker.sock
                      name: docker-sock
                  volumes:
                  - name: docker-sock
                    hostPath:
                      path: /var/run/docker.sock
            """
        }
    }
    
    environment {
        DOCKER_REGISTRY = 'your-registry.com'
        IMAGE_NAME = 'myapp'
        IMAGE_TAG = "${BUILD_NUMBER}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                script {
                    sh './scripts/build.sh'
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    sh './scripts/test.sh'
                }
            }
        }
        
        stage('Docker Build & Push') {
            steps {
                container('docker') {
                    script {
                        sh """
                            docker build -t ${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} .
                            docker push ${DOCKER_REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}
                        """
                    }
                }
            }
        }
        
        stage('Update Helm Chart') {
            steps {
                script {
                    sh """
                        sed -i 's/tag: .*/tag: ${IMAGE_TAG}/' helm-charts/myapp/values.yaml
                        git add helm-charts/myapp/values.yaml
                        git commit -m "Update image tag to ${IMAGE_TAG}"
                        git push origin main
                    """
                }
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
```

### Multi-Environment Deployment Pipeline

```groovy
// Jenkinsfile.deploy
pipeline {
    agent any
    
    parameters {
        choice(
            name: 'ENVIRONMENT',
            choices: ['dev', 'staging', 'prod'],
            description: 'Target environment'
        )
        string(
            name: 'IMAGE_TAG',
            defaultValue: 'latest',
            description: 'Docker image tag to deploy'
        )
    }
    
    stages {
        stage('Deploy to Dev') {
            when {
                expression { params.ENVIRONMENT == 'dev' }
            }
            steps {
                script {
                    deployToEnvironment('dev', params.IMAGE_TAG)
                }
            }
        }
        
        stage('Deploy to Staging') {
            when {
                expression { params.ENVIRONMENT == 'staging' }
            }
            steps {
                script {
                    deployToEnvironment('staging', params.IMAGE_TAG)
                }
            }
        }
        
        stage('Deploy to Production') {
            when {
                expression { params.ENVIRONMENT == 'prod' }
            }
            steps {
                input message: 'Deploy to production?', ok: 'Deploy'
                script {
                    deployToEnvironment('prod', params.IMAGE_TAG)
                }
            }
        }
    }
}

def deployToEnvironment(environment, imageTag) {
    sh """
        # Update Helm values for specific environment
        helm upgrade --install myapp-${environment} ./helm-charts/myapp \\
            --namespace ${environment} \\
            --create-namespace \\
            --set image.tag=${imageTag} \\
            --set environment=${environment}
    """
}
```

## 🔒 Security Configuration

### Secure Jenkins Setup

```groovy
// security-config.groovy
import jenkins.model.*
import hudson.security.*

def instance = Jenkins.getInstance()

// Disable CLI over remoting
instance.getDescriptor("jenkins.CLI").get().setEnabled(false)

// Enable CSRF protection
instance.setCrumbIssuer(new DefaultCrumbIssuer(true))

// Disable usage statistics
instance.setNoUsageStatistics(true)

// Save configuration
instance.save()
```

### GitHub Integration

```yaml
# GitHub webhook configuration
github:
  servers:
    - name: "GitHub"
      apiUrl: "https://api.github.com"
      credentialsId: "github-token"
      manageHooks: true
```

## 🔄 GitOps Integration

### ArgoCD Integration

```groovy
stage('Trigger ArgoCD Sync') {
    steps {
        script {
            sh """
                # Update application manifest
                argocd app sync myapp-${ENVIRONMENT}
                argocd app wait myapp-${ENVIRONMENT} --timeout 300
            """
        }
    }
}
```

### Helm Chart Updates

```bash
#!/bin/bash
# scripts/update-helm-chart.sh

CHART_PATH="helm-charts/myapp"
NEW_TAG=$1
ENVIRONMENT=$2

# Update values.yaml
yq eval ".image.tag = \"${NEW_TAG}\"" -i ${CHART_PATH}/values-${ENVIRONMENT}.yaml

# Commit changes
git add ${CHART_PATH}/values-${ENVIRONMENT}.yaml
git commit -m "chore: update ${ENVIRONMENT} image tag to ${NEW_TAG}"
git push origin main

echo "Helm chart updated for ${ENVIRONMENT} with tag ${NEW_TAG}"
```

## 📊 Monitoring and Observability

### Pipeline Metrics

```groovy
// Add to pipeline for metrics collection
pipeline {
    agent any
    
    options {
        timestamps()
        timeout(time: 30, unit: 'MINUTES')
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }
    
    stages {
        stage('Metrics Collection') {
            steps {
                script {
                    // Collect build metrics
                    def buildDuration = currentBuild.duration
                    def buildResult = currentBuild.result
                    
                    // Send to monitoring system
                    sh """
                        curl -X POST http://prometheus-pushgateway:9091/metrics/job/jenkins/instance/${BUILD_NUMBER} \\
                        --data-binary @<(echo "jenkins_build_duration_seconds ${buildDuration}")
                    """
                }
            }
        }
    }
}
```

### Health Checks

```bash
#!/bin/bash
# scripts/health-check.sh

# Check Jenkins health
JENKINS_URL="http://jenkins:8080"
HEALTH_CHECK=$(curl -s ${JENKINS_URL}/login)

if [[ $? -eq 0 ]]; then
    echo "Jenkins is healthy"
    exit 0
else
    echo "Jenkins health check failed"
    exit 1
fi
```

## 🛠️ Troubleshooting

### Common Issues

#### 1. Build Failures
```bash
# Check build logs
kubectl logs -f deployment/jenkins -n jenkins

# Check pod status
kubectl get pods -n jenkins
```

#### 2. Plugin Issues
```bash
# Restart Jenkins
kubectl rollout restart deployment/jenkins -n jenkins

# Check plugin status
kubectl exec -it deployment/jenkins -n jenkins -- jenkins-plugin-cli --list
```

#### 3. Kubernetes Agent Issues
```bash
# Check agent pods
kubectl get pods -l jenkins=agent

# Check agent logs
kubectl logs -l jenkins=agent
```

### Debug Commands

```bash
# Access Jenkins container
kubectl exec -it deployment/jenkins -n jenkins -- /bin/bash

# Check Jenkins logs
kubectl logs deployment/jenkins -n jenkins --tail=100

# Verify Jenkins configuration
kubectl get configmap jenkins-config -n jenkins -o yaml
```

## 📚 Best Practices

### Pipeline Design
1. **Keep Pipelines Simple** - Break complex workflows into smaller, manageable stages
2. **Use Shared Libraries** - Create reusable pipeline components
3. **Implement Proper Error Handling** - Always include post-build actions
4. **Secure Credentials** - Use Jenkins credential store, never hardcode secrets
5. **Version Control Everything** - Store Jenkinsfiles in source control

### Performance Optimization
1. **Use Kubernetes Agents** - Dynamic scaling based on workload
2. **Parallel Execution** - Run independent stages in parallel
3. **Caching** - Cache dependencies and build artifacts
4. **Resource Limits** - Set appropriate CPU and memory limits
5. **Clean Workspace** - Always clean up after builds

### Security Guidelines
1. **Least Privilege** - Grant minimal required permissions
2. **Regular Updates** - Keep Jenkins and plugins updated
3. **Audit Logs** - Enable and monitor audit logging
4. **Network Security** - Use proper network policies
5. **Backup Strategy** - Regular backups of Jenkins configuration

## 🔗 Integration Points

### With Other GitOps Components
- **GitHub** - Source code triggers and webhook integration
- **ArgoCD** - Deployment automation and sync triggers
- **Helm** - Chart packaging and deployment
- **EKS** - Kubernetes cluster deployment target
- **Istio** - Service mesh configuration updates
- **Terraform** - Infrastructure provisioning triggers

### External Tools
- **SonarQube** - Code quality analysis
- **Nexus/Artifactory** - Artifact repository
- **Slack/Teams** - Notification integration
- **JIRA** - Issue tracking integration

---

**Next Steps:**
1. Set up Jenkins using the provided Helm chart
2. Configure your first pipeline using the examples
3. Integrate with GitHub webhooks
4. Set up ArgoCD synchronization
5. Implement monitoring and alerting

For advanced configurations and troubleshooting, refer to the [Jenkins Documentation](https://www.jenkins.io/doc/) and our internal DevOps guidelines.