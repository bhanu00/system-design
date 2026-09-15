# Events Non-Delivery
When an event is not delivered (or acknowledged) in an **event-driven architecture**, what happens depends on the **messaging system** (e.g., Kafka, RabbitMQ, AWS SQS, EventBridge) and the **delivery guarantees** configured.

## **1. Possible Reasons for Event Non-Delivery**

* **Consumer is down or unreachable.**
* **Consumer fails to process the event** (e.g., exception or timeout).
* **Network issues or partition failures.**
* **Broker issues** (e.g., Kafka topic retention, RabbitMQ queue overflow).

---

# **2. How Event Systems Handle Undelivered Events**

### **2.1. Message Retention (Broker Stores Events)**

* Most brokers (Kafka, RabbitMQ, SQS) store events until:

  * A consumer **acknowledges** the message.
  * Retention time or size limit is reached.
* If the event is not consumed within retention time, **the event may expire and be deleted**.

---

### **2.2. Retry Mechanisms**

* **Automatic retries:** The broker redelivers the event if the consumer does not acknowledge it (e.g., RabbitMQ requeues messages).
* **Exponential backoff retries:** Some systems (like AWS EventBridge or SNS) retry at intervals (e.g., 1s, 2s, 4s, etc.) until the message is delivered or the retry policy is exhausted.

---

### **2.3. Dead Letter Queue (DLQ)**

* If an event cannot be delivered (or fails after max retries), it is sent to a **Dead Letter Queue** (DLQ).
* DLQ is used for debugging and replaying failed events later.

**Example in AWS SQS:**

* If a consumer fails 5 times, the event is moved to DLQ for later analysis.

---

### **2.4. Event Replay (Idempotency Needed)**

* Systems like **Kafka** allow consumers to re-read messages from the log (offset-based).
* If an event wasn’t processed due to consumer failure, once the consumer comes back online, it can **resume from the last committed offset**.

---

# **3. Event Delivery Guarantees**

### **3.1. At-Most-Once**

* Event is delivered **zero or one time**.
* If delivery fails, the event is **lost**. (Fast, but risky.)

### **3.2. At-Least-Once**

* Event is retried until it is acknowledged.
* May result in **duplicate events**, so **idempotency is required** in consumers.

### **3.3. Exactly-Once**

* Event is delivered **exactly once** (complex to implement; Kafka + idempotent producers + transactional consumers achieve this).

---

# **4. What Should You Do with Undelivered Events?**

1. **Use DLQ for inspection:**
   Failed messages are stored for future replay and debugging.

2. **Implement Idempotent Consumers:**
   Ensure processing the same event multiple times doesn't cause data inconsistency.

3. **Configure Retry Policies:**
   Use exponential backoff and limited retries to avoid hammering a failing consumer.

4. **Use Monitoring & Alerts:**
   Track undelivered messages via metrics (e.g., Kafka consumer lag, SQS DLQ count).

---

# **5. Real-World Example (AWS Kafka + SQS)**

* **Scenario:** Payment event not delivered due to consumer crash.
* **Kafka:** Stores event in topic for retention (e.g., 7 days). Consumer resumes from last offset after restart.
* **SQS:** Retries N times → moves event to DLQ if still undelivered.
* **Replay:** Ops team inspects DLQ → fixes consumer bug → replays events to main queue.

---

## **6. Next Step**

Would you like me to create a **guide on "Event Failure Handling in Event-Driven Architectures"**, including:

* **DLQ patterns** (AWS SQS, SNS, Kafka).
* **Event replay and poison message handling.**
* **Idempotency in event processing (with .NET example)?**
