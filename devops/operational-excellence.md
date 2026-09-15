# **Final Optimized Quick Refreshment Guide: Operational Excellence for .NET on AWS (Azure DevOps CI/CD)**

**Goal:** Run reliable, performant, and cost-efficient .NET apps on AWS.

---

### **I. Core Pillars of Operational Excellence**

1.  **Prepare & Operate:** Effectively run and monitor systems to deliver business value.
2.  **Anticipate Failure:** Design resilient systems with robust recovery plans.
3.  **Learn from Events:** Continuously improve operations based on insights from events and data.
4.  **Small, Frequent, Reversible Changes:** Minimize blast radius, enable quick recovery from issues.
5.  **Refine Procedures:** Ensure operational procedures remain effective as the system evolves.

---

### **II. Disaster Recovery (DR) in AWS**

* **Metrics:** **RPO** (max data loss) & **RTO** (max recovery time).
* **Strategies (Cost/Complexity ^, RPO/RTO v):**
    * **Backup & Restore:** S3, EBS/RDS Snapshots. (Lowest)
    * **Pilot Light:** Minimal live DR region (e.g., replicated DB).
    * **Warm Standby:** Scaled-down live replica.
    * **Multi-site Active/Active:** Full, simultaneous operation in multiple regions. (Highest)

---

### **III. Key AWS Services for Global Ops Excellence**

* **AWS Global Accelerator:** Performance & availability via AWS global network; directs traffic to nearest *healthy* endpoint.
* **Amazon Route 53:** Scalable DNS; supports **Failover, Latency, Geo, Weighted** routing & health checks for global traffic/DR.
* **Global Databases:**
    * **Aurora Global Database:** Cross-region storage replication ($<1$s RPO/RTO).
    * **DynamoDB Global Tables:** Multi-region, multi-active, auto-replicated for global scale.
* **Cross-Region Network Connectivity:**
    * **VPC Peering:** Direct private connection between 2 VPCs.
    * **Transit Gateway:** Centralized hub for multi-VPC/multi-region networking.
* **Azure DevOps Multi-Region Deployment:** Automates consistent deployments of .NET apps/IaC to primary & DR regions via **AWS Toolkit for Azure DevOps**.

---

### **IV. Advanced Deployment Strategies (via Azure DevOps & AWS)**

* **Blue/Green:**
    * Deploy new (.NET) version to isolated "Green" env on AWS.
    * Test Green thoroughly.
    * **Switch traffic** from "Blue" to "Green" via **Route 53 DNS update** or **ALB listener rule change**.
    * Instant rollback to Blue possible.
* **Canary:**
    * Deploy new (.NET) version to a small "canary" subset of production.
    * **Gradually shift small % of traffic** to canary via **ALB weighted target groups** or **AWS CodeDeploy**.
    * Continuous monitoring with automated rollback on issues.


### **V. Automated Operations (AWS Systems Manager Automation)**

* **Runbooks:** Define automated workflows (JSON/YAML) for operational tasks.
* **How it helps:** Standardizes procedures, automates repetitive tasks (e.g., restarts, patching), enables self-healing, speeds incident response, reduces human error.
* **Triggered by:** Typically **CloudWatch Alarms** (e.g., high error rate $\rightarrow$ runbook to restart service).


### **VI. General Operational Excellence Practices**

* **Observability:** Structured Logging (CloudWatch Logs), Metrics (CloudWatch Metrics), Tracing (X-Ray).
* **Configuration Management:** Externalize config/secrets (Parameter Store, Secrets Manager).
* **IaC:** Define all infrastructure as code (CloudFormation, Terraform).
* **Automated Testing:** Unit, integration, performance tests in CI/CD.
* **Cost Optimization:** Regular review (Cost Explorer), right-sizing.
* **Security & Compliance:** IAM Least Privilege, AWS Config, regular audits.
