# **Bulkhead Pattern – Complete Guide**

Here’s a **complete, detailed guide on the Bulkhead Pattern**, covering **concepts, benefits, use cases, real-world examples, .NET implementation, AWS strategies, and best practices**.


## **1. What is the Bulkhead Pattern?**

The **Bulkhead Pattern** is a **resilience design pattern** used in microservices and distributed systems to **isolate resources into partitions** (like separate thread pools, connection pools, or processes).
The term comes from **ship design**, where bulkheads (compartments) prevent water from flooding the entire ship if one compartment is breached.

**Goal:**

* Prevent **cascading failures** caused by one failing or overloaded component.
* Ensure that other parts of the system continue to function, even if one service or resource is overloaded or failing.


## **2. Why Do We Need Bulkhead Pattern?**

### **The Problem Without Bulkhead**

* All requests share the same resources (threads, connections).
* If one external dependency becomes **slow or fails**, it can consume all resources.
* Other parts of the application are starved, resulting in **cascading failures**.

**Example:**
If a **Fraud Detection API** is slow, all threads in the Payment Service may get blocked waiting on HTTP calls to Fraud Detection, causing the **entire payment system** to stop functioning.


## **3. How Bulkhead Works**

* Resources are **divided into isolated pools**.
* Each pool serves only a **specific function** (e.g., fraud check, payment processing, notifications).
* When one pool is full or blocked, **other pools remain unaffected**.

**Analogy:**
A cruise ship has multiple water-tight compartments (bulkheads). If one floods, the rest of the ship remains operational.


## **4. Types of Bulkheads**

1. **Thread Pool Isolation**
   Each dependency (e.g., API call, DB operation) gets its own dedicated thread pool.

2. **Connection Pool Isolation**
   Separate database or HTTP connection pools for each microservice or workflow.

3. **Process/Container Isolation**
   Services run in **different containers (ECS/EKS)** with CPU/memory limits, preventing one from affecting another.

4. **Message Queue Isolation**
   Use **separate queues** for different workflows (e.g., PaymentQueue vs NotificationQueue).


## **5. Bulkhead Pattern in Action – Real-Life Use Cases**


### **Use Case 1: Payment Processing System**

**System:**

* **Payment Gateway Service** calls:

  * **Fraud Detection Service (HTTP API)**.
  * **External Payment Processor (Visa API)**.
  * **Database (RDS)**.
  * **Notification Service** (via Kafka).

**Risk:**
If **Fraud Detection API** becomes slow, all Payment Gateway threads may block, halting **Visa API calls** and **DB writes**.

**Solution:**

* Use **separate HttpClient pools** for Fraud API and Visa API.
* Use **separate DB connection pools** for Payment and Notification workflows.
* Use **Kafka consumer groups** with **separate thread pools** for each event type.


### **Use Case 2: E-Commerce Checkout**

* **Inventory Service** is isolated from **Order Service** so that inventory issues don’t impact order placement.
* Separate **DB connections and queues** for payment and order tracking.


### **Use Case 3: Streaming Services**

* **Video streaming service** isolates:

  * **Playback service** (critical).
  * **Recommendation service** (non-critical).
* If recommendations fail, playback still works.


## **6. Implementing Bulkhead in .NET**

### **6.1 Using Polly Bulkhead Policy**

Polly is a resilience library for .NET that supports Bulkhead.

```csharp
using Polly;
using Polly.Bulkhead;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Bulkhead: 3 concurrent tasks + 5 queued
        var bulkhead = Policy.BulkheadAsync(3, 5, onBulkheadRejectedAsync: context =>
        {
            Console.WriteLine("Bulkhead limit reached. Request rejected.");
            return Task.CompletedTask;
        });

        for (int i = 0; i < 10; i++)
        {
            var index = i;
            _ = bulkhead.ExecuteAsync(async () =>
            {
                Console.WriteLine($"Task {index} started.");
                await Task.Delay(2000); // simulate work
                Console.WriteLine($"Task {index} completed.");
            });
        }

        Console.ReadLine();
    }
}
```

* Only **3 tasks run concurrently**, and **5 wait in queue**.
* Remaining **2 tasks are rejected**.


### **6.2 HttpClient Isolation (using IHttpClientFactory)**

```csharp
services.AddHttpClient("FraudDetectionClient")
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, 20));

services.AddHttpClient("VisaAPIClient")
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50));
```

* Fraud Detection API has **10 max concurrent calls**, Visa API has **20**.


### **6.3 Database Connection Pool Isolation**

You can configure **separate DbContexts** or **connection strings** with different `Application Name` and `Max Pool Size`.

**Example:**

```csharp
services.AddDbContext<FraudDetectionDbContext>(options =>
    options.UseSqlServer("Server=...;Database=Payments;User Id=...;Password=...;Application Name=FraudDetection;Max Pool Size=20"));

services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer("Server=...;Database=Payments;User Id=...;Password=...;Application Name=PaymentProcessor;Max Pool Size=50"));
```

* Separate **pools (20 vs 50)** for Fraud and Payment workflows.


## **7. Bulkhead Pattern in AWS**

* **Lambda:** Use **Reserved Concurrency per function** to isolate workloads.
* **ECS/EKS:** Assign **CPU/memory limits** for containers running different microservices.
* **RDS Proxy:** Create **separate connection pools** for different services.
* **SQS:** Use **separate queues** for Payment vs Notification events.
* **API Gateway:** Configure **rate limiting per route** to avoid overload.



## **8. Bulkhead vs Other Patterns**

* **Bulkhead vs Circuit Breaker:**

  * Bulkhead isolates resource pools.
  * Circuit Breaker stops calling a failing service.
  * **Use together:** Bulkhead (limit concurrent calls) + Circuit Breaker (fail fast).

* **Bulkhead vs Retry:**

  * Retry handles **transient failures**.
  * Bulkhead handles **resource starvation**.
  * **Use together:** Retry inside Bulkhead partitions.


## **9. Best Practices**

1. **Define critical vs non-critical services.**
   Assign more resources (bigger pools) to critical paths like Payment Processing.

2. **Use `Application Name` in DB connection strings**
   for **logical pool separation**.

3. **Monitor Bulkhead queues**
   to ensure you're not rejecting critical requests due to wrong sizing.

4. **Combine with Timeouts & Circuit Breakers.**
   Example: `Bulkhead → Timeout → Retry → Circuit Breaker`.

5. **Test under load**
   to find optimal pool sizes (`Max Pool Size`, `maxParallelization`).


## **10. Example Architecture (Payment System)**

**Workflow:**

* **Payment API** →

  * Calls **Fraud Detection API (HttpClient Pool A: 10)**
  * Calls **Visa API (HttpClient Pool B: 20)**
  * Writes to **DB (Connection Pool A: 50)**
  * Publishes **PaymentCreated Event to Kafka**

* **Notification Service** →

  * Reads Kafka Event (Consumer Pool C).
  * Writes to **DB (Connection Pool B: 10)**.

**Effect of Bulkhead:**
If Fraud Detection API is slow and its 10-thread pool is blocked, **Visa API (20-thread pool)** and DB writes **continue unaffected**.


## **11. Key Benefits**

* **Fault isolation** – One service’s failure does not bring down the whole system.
* **Improved availability** – System stays partially operational.
* **Prevents cascading failures** – Protects critical paths.
* **Supports graceful degradation** – Non-critical services fail without affecting critical ones.

# With vs. Without the Bulkhead Pattern
Here’s a **side-by-side code comparison** of a Payment + Fraud + Notification microservice setup **with vs. without the Bulkhead Pattern**.


## **1. Without Bulkhead Pattern**

### **Single Shared Resources**

* **One DbContext** for all workflows.
* **One HttpClient** for all external calls.
* **Single Thread Pool** (default `ThreadPool`).

```csharp
// Startup.cs or Program.cs

// Single DbContext
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("MainDB")));

// Single HttpClient for all external services
services.AddHttpClient("MainClient", client =>
{
    client.BaseAddress = new Uri("https://external-api.com");
});

// Example usage
public class PaymentService
{
    private readonly HttpClient _client;
    private readonly AppDbContext _db;

    public PaymentService(IHttpClientFactory factory, AppDbContext db)
    {
        _client = factory.CreateClient("MainClient");
        _db = db;
    }

    public async Task ProcessPayment()
    {
        // Fraud check, Payment call, and Notifications use the same HttpClient + DB pool
        await _client.GetAsync("/fraud-check");
        await _client.PostAsync("/process-payment", null);
        await _db.Payments.AddAsync(new Payment { Amount = 100 });
        await _db.SaveChangesAsync();
    }
}
```

### **Problems:**

* If **Fraud Detection API** hangs, it blocks all requests (thread pool is consumed).
* **DB connection pool** is shared, so a spike in notifications can block payments.
* A **single failure cascades** through the system.


## **2. With Bulkhead Pattern**

### **Separate Resources per Workflow**

* **Different DbContexts** with `Application Name` and `Max Pool Size`.
* **Separate HttpClient pools** for each external API.
* **Bulkhead policies (via Polly)** to limit concurrent calls.


### **Startup.cs or Program.cs**

```csharp
// Separate DbContexts
services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("PaymentDB") + ";Application Name=Payment;Max Pool Size=50"));

services.AddDbContext<FraudDetectionDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("FraudDB") + ";Application Name=FraudDetection;Max Pool Size=20"));

services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("NotificationDB") + ";Application Name=Notification;Max Pool Size=10"));

// Separate HttpClients with Bulkhead
services.AddHttpClient("VisaAPI", client =>
{
    client.BaseAddress = new Uri("https://visa-api.com");
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50));

services.AddHttpClient("FraudDetectionAPI", client =>
{
    client.BaseAddress = new Uri("https://fraud-api.com");
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, 30));
```


### **PaymentService (Using Isolated Resources)**

```csharp
public class PaymentService
{
    private readonly HttpClient _visaClient;
    private readonly PaymentDbContext _db;

    public PaymentService(IHttpClientFactory factory, PaymentDbContext db)
    {
        _visaClient = factory.CreateClient("VisaAPI");
        _db = db;
    }

    public async Task ProcessPayment()
    {
        await _visaClient.PostAsync("/process-payment", null);
        await _db.Payments.AddAsync(new Payment { Amount = 100 });
        await _db.SaveChangesAsync();
    }
}
```


### **FraudDetectionService**

```csharp
public class FraudDetectionService
{
    private readonly HttpClient _fraudClient;
    private readonly FraudDetectionDbContext _db;

    public FraudDetectionService(IHttpClientFactory factory, FraudDetectionDbContext db)
    {
        _fraudClient = factory.CreateClient("FraudDetectionAPI");
        _db = db;
    }

    public async Task CheckFraud()
    {
        await _fraudClient.GetAsync("/fraud-check");
        await _db.Logs.AddAsync(new FraudLog { Status = "Checked" });
        await _db.SaveChangesAsync();
    }
}
```


### **NotificationService**

```csharp
public class NotificationService
{
    private readonly NotificationDbContext _db;

    public NotificationService(NotificationDbContext db)
    {
        _db = db;
    }

    public async Task SendNotification(string message)
    {
        await _db.Notifications.AddAsync(new Notification { Message = message });
        await _db.SaveChangesAsync();
    }
}
```


## **3. Key Differences**

| **Aspect**     | **Without Bulkhead**               | **With Bulkhead**                               |
| -------------- | ---------------------------------- | ----------------------------------------------- |
| **DbContext**  | Shared single pool                 | Separate pools (`PaymentDbContext`, etc.)       |
| **HttpClient** | Single HttpClient                  | Separate HttpClient pools (Visa, Fraud, etc.)   |
| **Isolation**  | No isolation (all share resources) | Fault isolation (one failure doesn’t block all) |
| **Resilience** | Low                                | High                                            |

---

## **4. What Happens Under Load?**

* **Without Bulkhead:**
  A spike in **Fraud API calls** can consume **all threads + DB connections**, blocking payments and notifications.

* **With Bulkhead:**
  **Fraud Detection** may hit its **10-connection pool limit**, but Payment (50 connections) and Notification (10 connections) **continue unaffected**.


## **Summary (One Line)**

The Bulkhead Pattern partitions resources (threads, connections, containers) so failures in one area do not affect others, ensuring resilience and high availability.

