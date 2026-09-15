## ✅ Complete Guide: Event Sourcing + Materialized View + Kafka + .NET

---

### 🧩 1. Event Sourcing - Overview

**Definition:**
Event Sourcing is a pattern where state changes are stored as a sequence of **immutable events**, rather than directly persisting the current state in a database.

> Instead of updating state directly, we record **what happened** (an event), and derive state by replaying those events.

### 🔁 Example:

```json
{ "eventType": "OrderCreated", "aggregate_id": "ORDER-123", "data": {"amount": 100}, "timestamp": "..." }
{ "eventType": "OrderPaid", "aggregate_id": "ORDER-123", "data": {"paymentId": "PAY-456"} }
{ "eventType": "OrderShipped", "aggregate_id": "ORDER-123", "data": {"courier": "DHL"} }
```

### 🔄 State Reconstruction (Rehydration):

1. Fetch all events for a given `aggregate_id`
2. Replay them in order
3. Rebuild the current state object

---

### 🧱 2. Role of `aggregate_id`

| Feature             | Purpose                                                       |
| ------------------- | ------------------------------------------------------------- |
| Unique ID           | Identifies a specific instance of an aggregate                |
| Grouping            | All events related to this entity use the same `aggregate_id` |
| Rehydration         | Used to fetch event history for one object                    |
| Kafka Partitioning  | Used as partition key for ordered delivery                    |
| Concurrency Control | Helps ensure optimistic locking                               |

---

### 💾 3. Event Store

**Definition:**
An Event Store is a permanent, append-only log of all events.

**Implementation Options:**

* SQL table
* NoSQL DB (e.g., DynamoDB)
* Kafka (used for streaming + decoupling)
* EventStoreDB (purpose-built)

**Structure:**

| id | aggregate\_id | type         | data          | timestamp |
| -- | ------------- | ------------ | ------------- | --------- |
| 1  | ORDER-123     | OrderCreated | {amount: 100} | ...       |

**YES**, the event store **stores events permanently** (unless you configure TTL).

---

### 🧰 4. Kafka in Event Sourcing

Kafka helps in distributing events and decoupling producers and consumers.

**Pattern:**

* Events are published to Kafka
* Consumers (e.g., projection updaters) listen to Kafka
* Database views are updated

Kafka gives:

* High throughput
* Partitioning for parallelism
* Durability and ordering (per partition)

**Partitioning:**

* Use `aggregate_id` as partition key to maintain order
* Kafka **does not auto-decide** number of partitions — **you must define** it when creating the topic

---

### 📊 5. Materialized View Pattern

**Definition:**
A materialized view is a denormalized, read-optimized version of current state derived from the event stream.

**Why?**

* Replaying all events for reads is slow
* Queries require fast lookup (SQL/Redis/etc.)

**How It Works:**

1. New event (e.g. OrderShipped) is published
2. Projection handler consumes the event
3. Updates materialized view (e.g., SQL Orders table)

**Example SQL Read Table:**

| OrderId   | Status  | Courier |
| --------- | ------- | ------- |
| ORDER-123 | Shipped | DHL     |

---

### 🧪 .NET Example

**Event Model:**

```csharp
public class DomainEvent {
    public Guid Id { get; set; }
    public string AggregateId { get; set; }
    public string EventType { get; set; }
    public string Data { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**Aggregate Rehydration:**

```csharp
public Order Rehydrate(IEnumerable<DomainEvent> events) {
    var order = new Order();
    foreach (var e in events) order.Apply(e);
    return order;
}
```

**Materialized View Updater:**

```csharp
public class OrderProjectionHandler {
    private readonly SqlDbContext _context;

    public async Task Handle(DomainEvent e) {
        if (e.EventType == "OrderShipped") {
            var data = JsonSerializer.Deserialize<OrderShippedData>(e.Data);
            var order = await _context.Orders.FindAsync(e.AggregateId);
            order.Status = "Shipped";
            order.Courier = data.Courier;
            await _context.SaveChangesAsync();
        }
    }
}
```

---

### ⚙️ End-to-End Architecture

```
   +-----------+                 +---------------+
   |   API     |   POST Event   |    Kafka       |
   |  (Write)  +--------------->+  Topic (Events)|
   +-----------+                 +---------------+
         |                             |
         v                             v
  +----------------+        +-----------------------+
  | Event Store DB |        | Kafka Consumer Service|
  +----------------+        | (Projection Updater)  |
                            +-----------+-----------+
                                        |
                                        v
                          +--------------------------+
                          |  Materialized View (SQL) |
                          +--------------------------+
```

---

### ✅ Benefits

| Feature       | Value                                         |
| ------------- | --------------------------------------------- |
| Audit Trail   | Full history of changes                       |
| Replayability | Rebuild projections anytime                   |
| CQRS Support  | Separation of Write (events) and Read (views) |
| Scalability   | Kafka + Materialized View enables parallelism |

---

### 🔐 Other Concepts

* **Snapshots**: Used to avoid replaying long event chains
* **Idempotent Consumers**: Ensure same event processed once
* **Event Versioning**: For evolving event schemas
* **Consistency**: Use eventual consistency between Event Store and Read DB

---

Let me know if you want:

* Kafka topic creation & partitioning guide
* Docker-based local setup for Kafka + SQL + .NET
* Integration tests for event sourcing with projections
