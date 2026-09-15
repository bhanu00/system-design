# Comprehensive guide on Caching
This guide is covering everything from what caching is to its types, eviction policies, invalidation strategies, placement options, and practical use cases.

## ✅ What is Caching?

**Caching** is a technique used to **store a copy of data temporarily** in a high-speed storage layer (called a cache) so that future requests for that data can be served faster.

* **Purpose**: Improve performance, reduce latency, offload backend systems.
* **Commonly Cached**: Database query results, API responses, files, computations.


## 📚 Types of Caches

Caching can be categorized based on **what** is being cached and **where** the cache is placed.

### 1. **Based on Cache Scope**

| Type                  | Description                                                                 |
| --------------------- | --------------------------------------------------------------------------- |
| **In-Memory Cache**   | Stored in application memory (RAM). Very fast. Example: `.NET MemoryCache`. |
| **Distributed Cache** | Shared across multiple servers. Example: Redis, Memcached.                  |
| **Persistent Cache**  | Stored on disk, survives restarts. Slower than memory. Example: Ehcache.    |
| **Browser Cache**     | Stores static assets (images, scripts) in the user's browser.               |
| **CDN Cache**         | Stores assets close to users geographically using edge locations.           |
| **OS/Page Cache**     | Managed by the operating system to speed up disk I/O.                       |


### 2. **Based on Cache Location (Placement)**

| Placement          | Description                                                                         |
| ------------------ | ----------------------------------------------------------------------------------- |
| **Client-Side**    | Caching in the browser or mobile app.                                               |
| **Server-Side**    | Caching inside the backend application.                                             |
| **Proxy-Level**    | Between client and server, like Varnish or NGINX cache.                             |
| **CDN-Level**      | Content Delivery Networks like Cloudflare or Akamai cache assets at edge locations. |
| **Database-Level** | Query result or row-level cache within or outside the DB (e.g., Redis, pgBouncer).  |


### 1. **In-Memory Cache**

* **Explanation**: Stored inside the same process as the application. Fastest because it's on RAM.
* **Example**: You cache configuration settings or user roles in `MemoryCache` in .NET.

```csharp
MemoryCache cache = MemoryCache.Default;
cache.Add("AppTheme", "Dark", DateTimeOffset.Now.AddHours(1));
```

### 2. **Distributed Cache**

* **Explanation**: Shared among multiple servers. Ideal in load-balanced environments.
* **Example**: Use Redis or Memcached to store session info like `user:123:cart`.

```csharp
// Store
await distributedCache.SetStringAsync("user:123:cart", cartJson);

// Retrieve
var cart = await distributedCache.GetStringAsync("user:123:cart");
```

### 3. **Persistent Cache**

* **Explanation**: Stored on disk and survives app restarts.
* **Use Case**: Analytics systems or systems with large datasets that are rarely updated.


### 4. **Browser Cache**

* **Explanation**: Static assets like CSS, JS, images are stored on the client side.
* **Example**: Using cache headers in HTTP response:

```
Cache-Control: public, max-age=86400
```


### 5. **CDN Cache**

* **Explanation**: Cache assets in geographically distributed edge servers.
* **Example**: Cloudflare caches your images. A user from Japan gets the image from a Tokyo server, not your origin server.

## 🧠 Cache Design Considerations

When implementing cache, consider:

1. **Data Volatility**: How often does data change?
2. **Staleness Tolerance**: Can you afford slightly outdated data?
3. **Cost vs Speed**: In-memory caches are fast but costly.
4. **Consistency**: Do cached data and source always need to be in sync?


## 🔁 Cache Eviction Policies

Eviction is necessary when cache reaches its memory limit. Different policies determine **which item to remove**:

### 🧠 LRU (Least Recently Used)

* **When to use**: You have limited memory and want to keep most actively used data.
* **Example**: Caching search results — older ones will be removed if not used recently.

### 🔢 LFU (Least Frequently Used)

* **When to use**: Some data is rarely used, remove the least accessed item.
* **Example**: Product catalog — keep frequently viewed items in cache.

### 🧓 FIFO

* **When to use**: Simplicity matters more than efficiency.
* **Analogy**: A queue at a canteen.

### ⌛ TTL

* **Explanation**: Auto-expire after a time.
* **Example**: Weather API data refreshed every 30 minutes.

```csharp
new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
};
```

## 🚫 Cache Invalidation Strategies

Invalidation is how we **remove or update** stale data in the cache.

### 1. **Manual Invalidation**

* The application explicitly removes/updates cache when data changes.
* Example: `cache.remove("user_123")` after updating the user.

### 2. **Time-based Invalidation (TTL)**

* Automatically evict cache after a set time (e.g., 10 minutes).

### 3. **Write-through Cache**

* Every write to the database also updates the cache.
* Ensures consistency but adds latency to writes.

### 4. **Write-behind Cache (Write-back)**

* Writes are first made to the cache and persisted asynchronously to DB.
* Better performance, but risk of data loss on crash.

### 5. **Cache-aside Pattern (Lazy Loading)**

* Application checks the cache first.
* If data is not present (cache miss), load from DB and insert into cache.
* On update/delete, app also removes cache manually.


## 🧱 Cache Strategies & Patterns

| Pattern           | Description                                                     |
| ----------------- | --------------------------------------------------------------- |
| **Read-through**  | Cache sits between app and DB. Auto loads data if not in cache. |
| **Write-through** | Writes go through cache to DB (both updated simultaneously).    |
| **Write-behind**  | Writes go to cache first, then asynchronously to DB.            |
| **Cache-aside**   | App controls cache access. Load on miss, invalidate manually.   |
| **Refresh-ahead** | Proactively refresh data before it expires.                     |


## ⚙️ Popular Caching Tools & Technologies

| Tool                   | Description                                                     |
| ---------------------- | --------------------------------------------------------------- |
| **MemoryCache (.NET)** | In-memory cache for .NET apps.                                  |
| **Redis**              | In-memory key-value store, supports eviction, persistence, TTL. |
| **Memcached**          | Lightweight distributed cache, fast but limited features.       |
| **Ehcache**            | Java-based caching with disk persistence support.               |
| **Varnish**            | Reverse proxy HTTP cache.                                       |
| **Cloudflare CDN**     | Caches static assets at edge globally.                          |


## 📈 Cache Performance Benefits

* ✅ Reduce latency by serving from memory.
* ✅ Reduce load on databases or external APIs.
* ✅ Improve throughput and user experience.
* ✅ Decrease infrastructure costs (fewer DB reads).


## ⚠️ Cache Pitfalls to Avoid

* ❌ Serving stale data if not invalidated properly.
* ❌ Cache stampede (many clients request at once on cache miss).
* ❌ Memory overflow if eviction not configured.
* ❌ Inconsistent cache state in distributed systems (fix using locks or versioning).


## 🧪 Real-World Use Cases

| Use Case                     | Description                                                                  |
| ---------------------------- | ---------------------------------------------------------------------------- |
| **API Response Caching**     | Avoid repeated computation or DB access. Cache JSON for specific parameters. |
| **Database Query Caching**   | Frequently used queries like `SELECT * FROM Products` can be cached.         |
| **Authentication Tokens**    | Cache user session tokens or JWT validation data.                            |
| **Configuration Settings**   | Load once and cache for all instances (until app restarts or TTL expires).   |
| **E-commerce Product Pages** | Cache static product info to improve page load times.                        |
| **Leaderboard or Rankings**  | Expensive computations can be cached and refreshed periodically.             |
| **Rate Limiting**            | Use Redis to track request counts and enforce limits.                        |
| **CDN for Static Assets**    | Cache images, CSS, JS on edge servers close to the user.                     |

### ✅ **E-commerce**

* Product details → Cache static info
* User cart → Redis for session-aware carts
* Price → TTL or manual invalidation during sale periods

### ✅ **News Website**

* Homepage → cache full HTML for 10 mins
* Breaking news → skip cache or clear cache on publish

### ✅ **Banking**

* Cache rarely changing data like IFSC codes or branch locations
* Do NOT cache transaction data due to consistency requirements

### ✅ **Social Media**

* User timeline → cache recent posts
* Likes/Comments count → Redis with TTL + background refresh

## 🧩 Example: Cache-aside Pattern in C# with Redis

```csharp
public async Task<Product> GetProductAsync(int productId)
{
    string key = $"product:{productId}";
    var cachedData = await redisCache.GetStringAsync(key);
    if (cachedData != null)
        return JsonConvert.DeserializeObject<Product>(cachedData);

    // Cache miss: load from DB
    var product = await dbContext.Products.FindAsync(productId);

    if (product != null)
    {
        await redisCache.SetStringAsync(
            key,
            JsonConvert.SerializeObject(product),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            }
        );
    }

    return product;
}
```

## 🧠 When **NOT** to Use Cache

* Data changes frequently and must always be current (e.g., financial transactions).
* Cache management overhead exceeds performance benefits.
* Memory constraints are too tight.
* Strict consistency requirements.


## Summary Table

| Topic                 | Summary                                                      |
| --------------------- | ------------------------------------------------------------ |
| **What is Cache**     | Temporary storage for fast access.                           |
| **Types**             | In-memory, distributed, browser, CDN, persistent.            |
| **Eviction Policies** | LRU, LFU, FIFO, TTL, Random.                                 |
| **Invalidation**      | TTL, manual, write-through, write-behind, cache-aside.       |
| **Placement**         | Client, server, proxy, CDN, database.                        |
| **Use Cases**         | API responses, static files, sessions, queries, rate limits. |
| **Tools**             | Redis, Memcached, .NET MemoryCache, Varnish, CDN.            |

## Good Article to read
[Caching Strategies](https://codeahoy.com/2017/08/11/caching-strategies-and-how-to-choose-the-right-one/)
