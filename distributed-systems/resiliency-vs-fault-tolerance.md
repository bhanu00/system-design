# **Resiliency vs Fault Tolerance – Complete Guide**


## **1. Introduction**

Both **resiliency** and **fault tolerance** aim to ensure systems remain operational under adverse conditions, but their **approaches and outcomes differ**.

* **Fault Tolerance**: Prevents failures from impacting users by **masking faults** through redundancy.
* **Resiliency**: Accepts that failures will occur but ensures **fast recovery and graceful degradation** through patterns like retries and circuit breakers.


## **2. Key Differences**

| **Aspect**           | **Fault Tolerance**                                  | **Resiliency**                                           |
| -------------------- | ---------------------------------------------------- | -------------------------------------------------------- |
| **Goal**             | Prevent user-visible failures entirely.              | Ensure fast recovery and minimize failure impact.        |
| **Service Behavior** | Continues operating **without interruption**.        | May degrade temporarily but recovers automatically.      |
| **Approach**         | Uses **replication, redundancy, and failover**.      | Uses **retries, circuit breakers, and fallback**.        |
| **Design Level**     | Mostly **infrastructure-level** (hardware, cloud).   | Mostly **application-level** (code and logic).           |
| **Examples**         | AWS RDS Multi-AZ, load balancers, redundant servers. | Polly retry, bulkhead, circuit breaker in microservices. |
| **Cost**             | Usually **higher** (requires extra hardware).        | **Lower cost**, but requires smart coding.               |
| **Failures Handled** | Masks **permanent and transient failures**.          | Handles **transient and cascading failures**.            |
| **User Experience**  | No noticeable impact.                                | May see fallback data or a temporary delay.              |


## **3. Fault Tolerance Approaches (Infrastructure Level)**

### **3.1. Multi-AZ and Replication**

* **AWS RDS Multi-AZ:** Automatically fails over to a standby replica in a different availability zone.
* **S3 and DynamoDB:** Data is replicated across multiple availability zones.
* **EC2 Auto Scaling Groups:** Replace failed instances automatically.
* **Load Balancers (ALB/NLB):** Direct traffic only to healthy ECS/EC2 tasks.


### **3.2. Example Fault-Tolerant Architecture**

```
User -> AWS API Gateway -> ALB -> ECS Payment Service (Tasks in 3 AZs)
                        |
                        +--> RDS Multi-AZ
                        |
                        +--> SQS (for asynchronous tasks)
```


## **4. Resiliency Patterns (Application Level)**

Resiliency ensures the **system can recover quickly** even if failures occur. In .NET, the **Polly** library is widely used.


### **4.1. Retry Pattern**

Automatically retries a failing operation, often with **exponential backoff**.

**Use Case:** Temporary network issues.

**Example with Polly:**

```csharp
using Polly;
using System.Net.Http;

var retryPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s due to {outcome.Result.StatusCode}");
        });
```

---

### **4.2. Circuit Breaker Pattern**

Stops calling a failing service for a period of time to avoid cascading failures.

**Example with Polly:**

```csharp
var circuitBreaker = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
        onBreak: (result, ts) => Console.WriteLine("Circuit Opened!"),
        onReset: () => Console.WriteLine("Circuit Closed!"));
```


### **4.3. Bulkhead Pattern**

Limits the number of concurrent requests, isolating resources to prevent total failure.

**Example with Polly:**

```csharp
using Polly.Bulkhead;

var bulkhead = Policy.BulkheadAsync(5, 10); 
// Allows 5 concurrent calls, 10 queued
```


### **4.4. Combining Policies**

You can wrap multiple policies together:

```csharp
var resilientPolicy = Policy.WrapAsync(
    bulkhead,
    retryPolicy,
    circuitBreaker
);
```


## **5. Practical Comparison**

### **Fault Tolerance Scenario**

* **Payment Service:** Hosted across **3 AZs** behind a load balancer.
* If one AZ goes down, the load balancer seamlessly redirects traffic to healthy tasks.
* The database uses **RDS Multi-AZ**, so failover happens automatically.

### **Resiliency Scenario**

* If the **payment gateway is slow**:

  * The service retries the request 3 times using **retry policy**.
  * If failures persist, **circuit breaker** opens for 30 seconds.
  * **Bulkhead** ensures no more than 5 concurrent calls, preventing resource starvation.


## **6. Real-World Example: Payment System**

* **Fault-Tolerant Layer:**

  * **ALB** directs traffic to ECS tasks across multiple AZs.
  * **RDS Multi-AZ** ensures DB availability.
  * **SQS** buffers payment processing messages.

* **Resiliency Layer:**

  * **Polly Retry & Circuit Breaker** handle third-party payment gateway failures.
  * **Redis Cache Fallback** provides last-known payment status if DB queries fail.


## **7. When to Use Which?**

* **Use Fault Tolerance** for **critical, stateful components** like databases or core payment services.
* **Use Resiliency** for **stateless microservice calls**, external APIs, or distributed system interactions.

**Modern architectures use both together.**


## **8. Summary**

* **Fault Tolerance** = Masking failures using redundancy (e.g., AWS Multi-AZ, load balancers).
* **Resiliency** = Handling and recovering from failures using software strategies (e.g., Polly in .NET).
* Combining **infrastructure-level fault tolerance** with **application-level resiliency** ensures **high availability, scalability, and user satisfaction**.
