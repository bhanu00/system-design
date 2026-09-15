### **What is Rate Limiting?**

**Rate Limiting** is a technique used to control the number of requests or actions a user, API client, or system can perform within a specific time frame.
It is essential for:

* **Preventing abuse** (e.g., brute-force attacks, API abuse).
* **Ensuring fair usage** of system resources.
* **Maintaining system stability** under heavy load.

For example:

> An API may allow only 100 requests per user per minute.

---

## **Types of Rate Limiting**


## **1. Fixed Window Counter**

### **How It Works**

* A **fixed time window** (e.g., 1 minute) is set.
* Each request increments a counter for that window.
* If the counter exceeds the limit, further requests are rejected until the window resets.

---

### **Pros**

* Simple and easy to implement.
* Low memory overhead.

### **Cons**

* **Burst problem:** A client can send maximum requests at the boundary of two windows (e.g., 100 at 11:59:59 and 100 at 12:00:01).

### **Example**

* **Rule:** 100 requests per minute.
* If a user sends **100 requests between 11:59:00 and 11:59:59**, they reach the limit.
* When the clock hits **12:00:00**, the counter resets.
* So if the user sends **another 100 requests between 12:00:00 and 12:00:59**, all are allowed.
  **Problem:** The user effectively made 200 requests in 2 seconds (at 11:59:59 and 12:00:01).


### **.NET Example: Fixed Window Rate Limiter**

```csharp
public class FixedWindowRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly Dictionary<string, (int Count, DateTime WindowStart)> _requests = new();
    private readonly object _lock = new();

    public FixedWindowRateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public bool IsAllowed(string clientId)
    {
        lock (_lock)
        {
            if (!_requests.ContainsKey(clientId))
            {
                _requests[clientId] = (1, DateTime.UtcNow);
                return true;
            }

            var (count, windowStart) = _requests[clientId];

            if (DateTime.UtcNow - windowStart > _window)
            {
                _requests[clientId] = (1, DateTime.UtcNow);
                return true;
            }

            if (count < _limit)
            {
                _requests[clientId] = (count + 1, windowStart);
                return true;
            }

            return false;
        }
    }
}
```

**Usage Example:**

```csharp
var limiter = new FixedWindowRateLimiter(5, TimeSpan.FromSeconds(10));
if (limiter.IsAllowed("client1"))
    Console.WriteLine("Request Allowed");
else
    Console.WriteLine("Too Many Requests");
```

---

## **2. Sliding Window**

Sliding window addresses the burst issue by considering a **moving time frame** rather than a fixed interval.

---

### **2a. Sliding Window Log**

#### **How It Works**

* Store a **timestamp** for each request.
* Remove expired timestamps outside the current window.
* Allow a request only if the count of timestamps in the window is below the limit.

---

### **Pros**

* Accurate and prevents bursts.

### **Cons**

* High memory usage (stores all timestamps).

### **Example**

* **Rule:** 100 requests per minute.
* User sends **100 requests between 11:59:10 and 12:00:10**.
* At **12:00:11**, the window shifts (older requests expire), and user can send more requests.
* Bursts are less likely because the window **"slides" with time** rather than resetting.


### **.NET Example: Sliding Window Log**

```csharp
public class SlidingWindowLogRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly Dictionary<string, Queue<DateTime>> _requestLogs = new();
    private readonly object _lock = new();

    public SlidingWindowLogRateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public bool IsAllowed(string clientId)
    {
        lock (_lock)
        {
            if (!_requestLogs.ContainsKey(clientId))
                _requestLogs[clientId] = new Queue<DateTime>();

            var now = DateTime.UtcNow;
            var logs = _requestLogs[clientId];

            while (logs.Count > 0 && now - logs.Peek() > _window)
                logs.Dequeue();

            if (logs.Count < _limit)
            {
                logs.Enqueue(now);
                return true;
            }

            return false;
        }
    }
}
```

---

### **2b. Sliding Window Counter**

#### **How It Works**

* Count requests in **current window** and **previous window**.
* Apply a **weighted average** based on how far the current time is into the current window.

### **Example**

* **Rule:** 100 requests/minute.
* If user sends **80 requests in the last 30 seconds of the previous minute** and **30 requests in the first 30 seconds of the current minute**, the sliding counter calculates a **weighted average**, allowing only 20 additional requests.

### **Pros**

* Less memory usage than Sliding Window Log.
* Avoids burst issue.

### **Cons**

* Approximation (less accurate than log-based).

---

### **.NET Example: Sliding Window Counter**

```csharp
public class SlidingWindowCounterRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly Dictionary<string, (int CurrentCount, DateTime WindowStart)> _requestCounts = new();
    private readonly object _lock = new();

    public SlidingWindowCounterRateLimiter(int limit, TimeSpan window)
    {
        _limit = limit;
        _window = window;
    }

    public bool IsAllowed(string clientId)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            if (!_requestCounts.ContainsKey(clientId))
            {
                _requestCounts[clientId] = (1, now);
                return true;
            }

            var (count, start) = _requestCounts[clientId];

            if (now - start > _window)
            {
                _requestCounts[clientId] = (1, now);
                return true;
            }

            if (count < _limit)
            {
                _requestCounts[clientId] = (count + 1, start);
                return true;
            }

            return false;
        }
    }
}
```

---

## **3. Token Bucket**

### **How It Works**

* A "bucket" starts with a fixed number of **tokens**.
* Tokens are added to the bucket at a fixed rate.
* Each request consumes 1 token.
* If no tokens are left, requests are rejected.

### **Example**

* **Rule:** Bucket capacity = 100 tokens, refill = 10 tokens/sec.
* A user can send **100 requests instantly** (if bucket is full).
* After that, they can send 10 requests per second.

### **Pros**

* Allows bursts up to bucket size.
* Smooths traffic.

### **Cons**

* Slightly more complex than window counters.

---

### **.NET Example: Token Bucket**

```csharp
public class TokenBucketRateLimiter
{
    private readonly int _capacity;
    private readonly double _refillRatePerSecond;
    private double _tokens;
    private DateTime _lastRefill;

    public TokenBucketRateLimiter(int capacity, double refillRatePerSecond)
    {
        _capacity = capacity;
        _refillRatePerSecond = refillRatePerSecond;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var tokensToAdd = (now - _lastRefill).TotalSeconds * _refillRatePerSecond;
        _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
        _lastRefill = now;
    }

    public bool IsAllowed()
    {
        Refill();
        if (_tokens >= 1)
        {
            _tokens -= 1;
            return true;
        }
        return false;
    }
}
```

---

## **4. Leaky Bucket**

### **How It Works**

* Requests are added to a queue ("bucket").
* Requests are processed at a **fixed rate** (like water dripping).
* If the bucket is full, new requests are dropped.


### **Example**

* **Rule:** Process 5 requests/sec.
* Even if 100 requests arrive instantly, only 5 requests per second will be processed.
* The rest are queued (or dropped if queue is full).

### **Pros**

* Smooths out spikes (traffic shaping).

### **Cons**

* Bursts are not allowed beyond queue size.

---

### **.NET Example: Leaky Bucket**

```csharp
public class LeakyBucketRateLimiter
{
    private readonly int _capacity;
    private readonly double _drainRatePerSecond;
    private double _water;
    private DateTime _lastDrain;

    public LeakyBucketRateLimiter(int capacity, double drainRatePerSecond)
    {
        _capacity = capacity;
        _drainRatePerSecond = drainRatePerSecond;
        _water = 0;
        _lastDrain = DateTime.UtcNow;
    }

    private void Drain()
    {
        var now = DateTime.UtcNow;
        var drained = (now - _lastDrain).TotalSeconds * _drainRatePerSecond;
        _water = Math.Max(0, _water - drained);
        _lastDrain = now;
    }

    public bool IsAllowed()
    {
        Drain();
        if (_water < _capacity)
        {
            _water++;
            return true;
        }
        return false;
    }
}
```

---

## **5. Concurrency Limit**

### **How It Works**

* Limits the **number of concurrent requests** (instead of time-based rate).
* When the limit is reached, new requests are rejected or queued until one completes.

### **Example**

* **Rule:** Only 3 concurrent requests allowed per user.
* If 3 requests are being processed, the **4th request is blocked** or rejected until one of the first 3 completes.


### **.NET Example: Concurrency Rate Limiter**

```csharp
public class ConcurrencyRateLimiter
{
    private readonly SemaphoreSlim _semaphore;

    public ConcurrencyRateLimiter(int maxConcurrency)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public async Task<bool> TryEnterAsync(int timeoutMs = 0)
    {
        return await _semaphore.WaitAsync(timeoutMs);
    }

    public void Exit()
    {
        _semaphore.Release();
    }
}
```

**Usage:**

```csharp
var limiter = new ConcurrencyRateLimiter(3);
if (await limiter.TryEnterAsync(100))
{
    try
    {
        // Process request
    }
    finally
    {
        limiter.Exit();
    }
}
else
{
    Console.WriteLine("Too many concurrent requests");
}
```


## **6. Adaptive Rate Limiting**

* **Adjusts the rate dynamically** based on system metrics like CPU, memory, or request latency.
* For example, reduce rate if latency increases or CPU > 80%.
* Implementation often involves **monitoring + token bucket** logic.


# **Which Algorithm Should You Use?**

* **Fixed Window**: Simple APIs, predictable workloads.
* **Sliding Window**: When accuracy matters (e.g., financial APIs).
* **Token Bucket**: Allows controlled bursts (most common in APIs).
* **Leaky Bucket**: Smooths traffic at constant rate (telecom, streaming).
* **Concurrency**: For resource-bound operations (e.g., DB connections).
* **Adaptive**: Dynamic scaling systems (e.g., cloud microservices).
