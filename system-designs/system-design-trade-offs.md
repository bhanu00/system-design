# System Design Trade-Offs
Designing **highly scalable, reliable, and fault-tolerant systems** always involves **trade-offs**. You can’t optimize everything at once—improving one quality often comes at the expense of another. Here's a structured guide covering the **key trade-offs** you need to consider:


## 1. **Consistency vs Availability vs Partition Tolerance (CAP Theorem)**

* **Consistency**: Every read receives the most recent write.
* **Availability**: Every request receives a (non-error) response.
* **Partition Tolerance**: System continues to function even if network partitions occur.

 **Trade-off**:
In distributed systems, you **must sacrifice either Consistency or Availability during a Partition** (you can’t have all three).

* Examples:

  * **CP**: MongoDB (Strong consistency, availability may suffer)
  * **AP**: Cassandra, DynamoDB (Availability prioritized, eventual consistency)


## 2. **Latency vs Consistency**

* **Strong consistency** often increases latency because updates must propagate and confirm.
* **Eventual consistency** reduces latency but risks stale reads.

 Choose **strong consistency** for:

* Financial transactions
* Authentication & authorization

 Choose **eventual consistency** for:

* Social media feeds
* Product recommendations


## 3. **Scalability vs Cost**

* **Horizontal scaling** (adding more servers) improves scalability but increases cost.
* **Vertical scaling** (bigger servers) is cheaper initially but has physical limits.

 Cloud-native systems often favor **horizontal scaling (microservices, serverless)**, but it needs careful **cost control**.


## 4. **Reliability vs Complexity**

* Adding redundancy (multiple instances, data replication) improves **reliability** but increases **system complexity** (more moving parts, more things to fail).

 Examples:

* Active-active failover vs active-passive failover.
* Multi-region deployments for disaster recovery.


## 5. **Performance vs Fault Tolerance**

* Adding retries, circuit breakers, timeouts increases **fault tolerance** but can introduce **latency** and reduce **throughput**.

 Example:

* In high-traffic APIs, aggressive retries can cause **thundering herd** problems.


## 6. **Security vs Usability / Performance**

* Adding security (encryption, multi-factor authentication, rate limiting) often degrades **performance** or **user experience**.
* Data at rest encryption may slow down disk IO.

 Example:

* JWT token validation adds slight overhead vs no token.


## 7. **Monolith vs Microservices (Modularity vs Simplicity)**

* **Microservices** scale better and isolate failures but add **operational complexity** (networking, observability, transactions across services).
* **Monoliths** are simpler to develop/deploy but don’t scale or isolate failures as well.


## 8. **Durability vs Performance**

* Writing to multiple storage nodes ensures **durability** but increases **write latency**.
* Writing to fast but volatile storage (in-memory) improves performance but risks data loss.

 Example:

* Redis (in-memory) vs RDBMS with ACID guarantees.


## 9. **Global Distribution vs Consistency/Latency**

* Multi-region deployments reduce **latency** but introduce **consistency challenges** (data replication delay, split-brain scenarios).

Example:

* AWS Global DynamoDB tables (eventual consistency)
* Azure Cosmos DB offers consistency models to balance this trade-off.


## Summary Table of Trade-Offs:

| Aspect 1     | Aspect 2        | Typical Trade-off Scenario                   |
| ------------ | --------------- | -------------------------------------------- |
| Consistency  | Availability    | Choose based on business criticality         |
| Latency      | Consistency     | Real-time speed vs data accuracy             |
| Scalability  | Cost            | Elasticity vs cloud bills                    |
| Reliability  | Complexity      | Redundancy vs operational burden             |
| Performance  | Fault Tolerance | Resilience vs speed                          |
| Security     | Usability       | Strong controls vs ease of use               |
| Modularity   | Simplicity      | Microservices vs Monolith                    |
| Durability   | Performance     | Persistence vs speed                         |
| Global Speed | Consistency     | Low latency vs cross-region data consistency |


**Key Takeaways:**

* There's **no single best architecture**—it depends on your system's **business goals**, **user expectations**, and **failure tolerance**.
* Every design choice introduces **risk** somewhere else—balance is key.


Thank you—that’s a crucial point. Let me **refocus the design** exactly around the real-world flow you described, with clear emphasis on the **scanning of physical items at the billing desk by the Account Manager**.

---

# Example - A Physical Store under a Franchisee
Here we will talk about what can run in cloud and what can run locally:

### Scenario:

* Customer freely picks physical grocery items in the store.
* At checkout, **Account Manager scans each physical item** at the billing counter.
* System shows product details, price, stock (for validation), applies offers (optional).
* Account Manager completes the order and proceeds to payment.
* Payment triggers notifications (receipt, alerts, loyalty points, etc.).


## Core System Characteristics for Your Case:

| Key Design Factor                                          | Implication                                                            |
| ---------------------------------------------------------- | ---------------------------------------------------------------------- |
| Physical product is **already in hand**                    | No need for real-time inventory validation before scanning.            |
| **Scanning is 100% mandatory**                             | System **cannot function without scanning**—must be fast and reliable. |
| **Order creation and payment** still need central services | To enable multi-store visibility, payments, loyalty.                   |


## What Must Run Locally (In-Store):

| Must Be Local                                 | Reason                                               |
| --------------------------------------------- | ---------------------------------------------------- |
| **Product Scanning** (Barcode → Product Info) | Scanning must be **instant** even if internet fails. |
| **Basic Product Catalog & Prices** (cached)   | Quick response; avoid network dependency.            |
| **Cart Management (Order Building)**          | Real-time as customer items are scanned.             |
| **Optional**: Offline order queuing           | So orders can be synced when connectivity returns.   |

Local device (POS terminal, tablet, PC) runs a **lightweight app with local product database/cache** (daily or hourly synced from cloud).


## What Can Run in Cloud (Centralized):

| Centralized (Cloud)               | Reason                                           |
| --------------------------------- | ------------------------------------------------ |
| **Order Submission & Processing** | Ensures order IDs are unique, stored, processed. |
| **Payment Handling**              | Compliance, reliability, security.               |
| **Notifications** (receipt, SMS)  | Cross-store uniformity.                          |
| **Inventory Sync & Analytics**    | Central view of stock, trends, sales, reports.   |


## Proposed Solution: Hybrid Model in Detail

### 1. In-Store (Edge/Device):

* Mobile/Web App (React Native, Flutter, or Electron App).
* **Local product database** (SQLite, IndexedDB, RealmDB).
* **Local cart management** while scanning.
* Background process to **sync prices & products** (Amazon AppSync, Amplify, or manual sync via REST).

### 2. Cloud:

* Order API (AWS API Gateway + Lambda/ECS microservices).
* Order storage (DynamoDB / Aurora Serverless).
* Payment integration (Stripe/Razorpay).
* Notifications (SNS, Pinpoint, SES).


## Real-World Walkthrough (Your Case Applied):

| Step                                             | Action                               | Runs Where?  |
| ------------------------------------------------ | ------------------------------------ | ------------ |
| 1️⃣ Customer picks items → brings to counter     | Physical                             | Store        |
| 2️⃣ Account Manager scans physical items         | **Local App + Local Product Cache**  | Store Device |
| 3️⃣ Product name, price, tax displayed instantly | Local + fallback to cloud if missing | Store Device |
| 4️⃣ Cart finalized → **Order placed**            | **Order Service (Cloud)**            | AWS          |
| 5️⃣ Payment processed                            | **Payment Gateway via Cloud**        | AWS          |
| 6️⃣ Notification sent                            | Cloud SNS/Email/SMS                  | AWS          |
| 7️⃣ Inventory updated                            | Async push to **Cloud Inventory**    | AWS          |


**Key Benefits:**

* **Scanning is lightning fast**—no internet lag.
* **Failsafe operations**—store works even without internet.
* **Centralized orders/payments**—clean financial reporting.
* **Inventory updates are eventually consistent** but that’s fine since customer has items physically.


## Trade-Offs Applied to Your Case:

| Trade-Off Area             | Decision                                                                                               |
| -------------------------- | ------------------------------------------------------------------------------------------------------ |
| **Consistency vs Latency** | **Latency wins locally** (scanning), **strong consistency in order/payment cloud**                     |
| **Cost vs Reliability**    | Slightly higher development cost for **hybrid model**, but avoids revenue loss from downtime.          |
| **Scalability**            | Cloud handles store growth effortlessly; local devices need sync capability only.                      |
| **Complexity**             | Slightly higher (edge + cloud) but manageable with modern tools like AWS Amplify, AppSync, Greengrass. |

