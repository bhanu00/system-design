# Amazon DynamoDB
Amazon DynamoDB is a fully managed, serverless NoSQL database service provided by AWS that offers blazing-fast performance at any scale. It's designed for internet-scale applications that require consistent, single-digit millisecond latency, and provides built-in high availability, durability, and fault tolerance.

## 1\. Core Concepts and Data Model

DynamoDB is a **key-value** and **document** database.

  * **Tables:** A collection of items.
  * **Items:** A group of attributes, similar to a row in a relational database.
  * **Attributes:** A fundamental data element, similar to a column. DynamoDB has a flexible schema, meaning items in the same table can have different attributes (unlike relational databases that enforce a strict schema for all rows).
  * **Primary Key:** Uniquely identifies each item in a table. It can be:
      * **Partition Key (Hash Key):** A single attribute. DynamoDB uses its value to distribute data across partitions. Each item must have a unique partition key value.
      * **Composite Primary Key (Partition Key + Sort Key / Range Key):** Consists of two attributes. Items with the same partition key are grouped together and ordered by the sort key. The combination of partition key and sort key must be unique. This allows for efficient range queries within a partition.

## 2\. Ensuring Scalability, Availability, Resilience, Performance, and Fault Tolerance

DynamoDB achieves its robust characteristics through a well-engineered architecture:

### Scalability

  * **Managed Service & Serverless:** DynamoDB is fully managed by AWS, eliminating the need to provision, patch, or manage servers. It's inherently serverless, meaning it automatically scales its underlying infrastructure to handle your workload.
  * **Capacity Modes:**
      * **On-Demand Capacity:** DynamoDB automatically adjusts throughput capacity as your workload increases or decreases. You pay for read and write requests as they occur, making it ideal for unpredictable traffic patterns.
      * **Provisioned Capacity:** You specify the desired Read Capacity Units (RCUs) and Write Capacity Units (WCUs). This is suitable for predictable workloads.
          * **Auto Scaling:** You can configure Auto Scaling policies to automatically adjust provisioned capacity based on actual usage, preventing throttling during spikes and optimizing costs during lulls.
  * **Horizontal Scaling with Partitions:** DynamoDB automatically partitions your data across multiple storage nodes. This allows it to distribute the workload and handle virtually limitless amounts of data and throughput. As data grows or throughput demand increases, DynamoDB transparently splits and rebalances partitions.
  * **Flexible Schema:** The NoSQL flexible schema allows for easier adaptation to changing data requirements without complex schema migrations, which can be bottlenecks in highly scalable systems.

### Availability

  * **Multi-Availability Zone (AZ) Replication:** DynamoDB automatically replicates your data synchronously across at least three different Availability Zones within an AWS Region. If one AZ experiences an outage, your data remains accessible from the replicas in other AZs.
  * **Automatic Failover:** In case of an AZ or hardware failure, DynamoDB automatically detects the issue and redirects requests to healthy replicas, ensuring continuous operation with no downtime for your application.
  * **Leader-Follower Model:** Each partition has a leader replica that handles write requests. This leader then synchronously replicates the writes to follower replicas in other AZs, ensuring data consistency and durability.
  * **Global Tables (Active-Active Replication):** For global applications, Global Tables provide active-active replication of your data across multiple AWS Regions. This allows users to read and write from the nearest region, reducing latency and providing multi-region disaster recovery.

### Resilience

  * **Redundant Storage and Replication:** Data is inherently redundant due to multi-AZ replication.
  * **Write-Ahead Logs (WALs):** All writes are first recorded in WALs, which are replicated across replicas and periodically archived to S3. This ensures durability and enables crash recovery.
  * **Continuous Verification:** DynamoDB continuously verifies data at rest to detect and correct silent data errors ("bit rot") and ensures consistency across all replicas using checksums.
  * **Point-in-Time Recovery (PITR):** Enables recovery of your table to any point in time within the last 35 days, protecting against accidental writes or deletes.
  * **On-Demand Backup and Restore:** Supports full backups for long-term retention without impacting live table performance.
  * **Transactions (ACID):** DynamoDB supports native server-side ACID (Atomicity, Consistency, Isolation, Durability) transactions, allowing all-or-nothing changes to multiple items within and across tables.

### Performance

  * **SSD-Backed Storage:** Utilizes Solid State Drives for low-latency data access.
  * **Predictable Performance:** Designed to provide consistent, single-digit millisecond latency for most operations, even at high throughput levels.
  * **Partition Key Design:** A well-designed partition key that distributes reads and writes evenly across partitions is critical for optimal performance, preventing "hot partitions."
  * **DynamoDB Accelerator (DAX):** An in-memory cache that provides microsecond-level response times for read-heavy workloads, offloading the primary DynamoDB table.
  * **Batch Operations:** `BatchGetItem` and `BatchWriteItem` APIs allow reading or writing up to 100 items (up to 16MB) in a single request, reducing network round trips.
  * **Projection Expressions:** When reading data, specifying only the necessary attributes using projection expressions reduces data transfer size and improves performance and cost.

### Fault Tolerance

  * **Built-in Redundancy:** The multi-AZ replication inherently provides fault tolerance.
  * **Automatic Node Replacement:** Failed storage nodes or servers are automatically detected and replaced without manual intervention.
  * **Consistent Hashing:** Used for data distribution, minimizing disruption during partition additions or removals.
  * **Quorum-Based Writes:** For strongly consistent writes, a write is only considered successful after a quorum of replicas acknowledge it, guaranteeing durability even if some replicas are temporarily unavailable.
  * **Monitoring and Alarms:** Integration with AWS CloudWatch provides detailed metrics and allows setting up alarms for proactive issue detection.

## 3\. How Partitioning Works in DynamoDB

Partitions are the fundamental units of storage and throughput in DynamoDB.

1.  **Partition Key (Hash Key):** When an item is added, DynamoDB applies an internal hash function to the item's partition key value. This hash value determines which physical partition the item will be stored in.
2.  **Sort Key (Optional):** If a composite primary key (partition key + sort key) is used, items with the same partition key are stored together on the same partition and are sorted by their sort key.
3.  **Data Distribution:** DynamoDB aims to distribute data evenly across partitions to ensure balanced workload and prevent "hot partitions."
4.  **Dynamic Scaling:**
      * DynamoDB automatically allocates an initial number of partitions based on your capacity settings.
      * As data volume or throughput requirements increase, DynamoDB automatically splits existing partitions and creates new ones, rebalancing data and capacity across the new partitions. This process is fully managed and transparent.
      * Each partition has limits (currently 10 GB of storage, 3,000 RCUs, 1,000 WCUs). Partitions are split to stay within these limits.

## 4\. Global Secondary Indexes (GSIs) and Local Secondary Indexes (LSIs)

Secondary indexes provide alternative query capabilities beyond the primary key.

### Local Secondary Indexes (LSIs)

  * **Same Partition Key, Different Sort Key:** An LSI *must* have the same partition key as the base table but a *different* sort key.
  * **Co-located Data:** LSI data is physically co-located on the same partitions as its corresponding base table data.
  * **Strongly Consistent Reads:** You can perform strongly consistent reads against an LSI.
  * **Item Collection Size Limit:** Subject to a 10 GB item collection size limit (all items with the same partition key, including their LSI entries).
  * **Shared Throughput:** LSIs share the provisioned throughput of the base table.
  * **Creation Time:** Must be defined when the table is created; cannot be added/removed later.

**Use Case:** Efficiently query items that share the same partition key but need to be sorted or filtered by a different attribute.
*Example: `Customers` table (`CustomerId` PK, `OrderDate` SK). LSI: `CustomerId` PK, `LastLoginDate` SK. Find all login events for a specific customer.*

### Global Secondary Indexes (GSIs)

  * **Different Primary Key:** A GSI can have a *completely different partition key* and an *optional sort key* from the base table.
  * **Separate Partitions and Throughput:** GSIs are stored on their own separate partitions and have their *own* independent provisioned throughput settings (RCUs/WCUs).
  * **Eventually Consistent Reads:** Data updates from the base table are **asynchronously** propagated to GSIs. Reads from a GSI are *always eventually consistent*.
  * **No Item Collection Size Limit:** Not subject to the 10 GB item collection size limit.
  * **Flexibility:** Can be added or deleted on an existing table without affecting base table availability.

**Use Case:** Querying data using attributes that are not part of the base table's primary key, enabling more flexible access patterns.
*Example: `Customers` table (`CustomerId` PK, `OrderDate` SK). GSI: `City` PK, `EmailAddress` SK. Find all customers in a specific city or by email address.*

## 5\. Idempotency in DynamoDB

Idempotency ensures that an operation produces the same result regardless of how many times it is executed. This is critical in distributed systems to handle network retries or duplicate requests gracefully.

**DynamoDB's Built-in Aids:**

  * **Conditional Writes:** `PutItem`, `UpdateItem`, `DeleteItem` support `ConditionExpression`. You can use this to ensure an operation only proceeds if a specific condition is met.
      * Example: `attribute_not_exists(primary_key)` when `PutItem` to prevent creating duplicate items.
      * Example: Use a version number attribute (`Expected: version_number`) with `UpdateItem` to prevent stale updates (optimistic locking).

**Application-Level Idempotency (Recommended Best Practice):**

The most robust way to achieve idempotency for complex operations is at the application layer using an idempotency key.

1.  **Generate Unique Idempotency Key:** The client generates a unique key for each request (e.g., UUID, hash of request payload).
2.  **Idempotency Table:** A dedicated DynamoDB table stores `idempotencyKey`, `status` (e.g., `IN_PROGRESS`, `COMPLETED`), `result` (of the operation), and `expiry` (using TTL).
3.  **Process Flow:**
      * Attempt to `PutItem` with `idempotencyKey` and `IN_PROGRESS` status, using `ConditionExpression: attribute_not_exists(idempotencyKey)`.
      * **If successful:** Process the main logic. Upon completion, `UpdateItem` status to `COMPLETED` and store the `result`.
      * **If `ConditionalCheckFailedException`:** The request is a duplicate. `GetItem` from the idempotency table.
          * If `status` is `IN_PROGRESS`, wait and retry or inform the client.
          * If `status` is `COMPLETED`, return the stored `result` directly.

This pattern prevents duplicate side effects (e.g., double-charging) even with multiple retries.

## 6\. Replication in DynamoDB (Synchronous vs. Asynchronous)

  * **Within a Region (Multi-AZ): Synchronous Replication**

      * Writes to a DynamoDB table are **synchronously replicated** across at least three Availability Zones (AZs) within the same AWS Region.
      * A write operation is considered successful and acknowledged to the client only after a **quorum** of replicas (typically the leader and at least one follower) has durably stored the data. This guarantees high durability and allows for strongly consistent reads.

  * **Across Regions (Global Tables): Asynchronous Replication**

      * Replication between different AWS Regions using Global Tables is **asynchronous**.
      * Writes are first synchronously replicated within the source region, then changes are captured by DynamoDB Streams and **asynchronously propagated** to replica tables in other configured regions.
      * This asynchronous nature leads to **eventual consistency** between regions. There's a short replication lag (typically milliseconds).

## 7\. Leader Failure in DynamoDB

DynamoDB uses a leader-follower model per partition within a region.

  * **Leader Role:** All write requests for a specific partition are directed to its elected leader replica.
  * **Failure Detection & Failover:** DynamoDB continuously monitors the health of its replicas. If a leader replica fails or becomes unreachable:
    1.  **Rapid Detection:** The system quickly detects the failure.
    2.  **New Leader Election:** A new leader is automatically and rapidly elected from the remaining healthy follower replicas for that partition. This process is highly optimized and typically takes only a few seconds.
    3.  **No Data Loss:** Since committed writes require a quorum acknowledgment (meaning the data is already durable on multiple replicas) *before* a success response is sent, there is no data loss for successfully acknowledged writes during a leader failure.
    4.  **Temporary Write Unavailability:** There might be a very brief period of unavailability or increased latency for writes to that specific partition during the leader election. Applications should implement retry logic with exponential backoff to handle this gracefully.

## 8\. Global Tables Latency and Consistency

Global Tables provide a multi-region, active-active setup for DynamoDB.

  * **Latency Advantage:** The primary benefit is **low-latency access** for globally distributed users/applications. Users can read from and write to the nearest AWS Region's replica table, minimizing network latency.
  * **Asynchronous Replication & Eventual Consistency:** Replication *between* regions is asynchronous.
      * **Replication Lag:** There's always a short replication lag. A write in one region might not be immediately visible in another region. Therefore, Global Tables provide **eventual consistency** across regions.
      * **Conflict Resolution ("Last Writer Wins"):** If the same item is updated concurrently in multiple regions, DynamoDB resolves conflicts using a "last writer wins" approach, where the item with the latest server-side timestamp prevails. This implies that if your application logic requires strict global consistency, you need to implement additional mechanisms or re-evaluate Global Tables for that specific use case.
  * **Multi-Region Strong Consistency (New Feature):** Recently, AWS introduced the option for **multi-Region strong consistency** for Global Tables. This mode ensures that a successfully acknowledged write to any replica in any Region is immediately available for reads from any other replica. If concurrent writes modify the same item in different regions, one of the writes will fail with a retryable exception, allowing the application to resolve the conflict. This offers the highest level of consistency but has different implications for application design and retries compared to eventual consistency.

## 9\. Scan vs. Query Operations

These are the primary ways to retrieve data from DynamoDB.

### Scan Operation

  * **Reads All Items:** A `Scan` operation reads *every item* in a table or a secondary index.
  * **Filters After Read:** It then applies an optional `FilterExpression` to discard items that don't meet the criteria *after* they have been read from the underlying storage.
  * **Inefficient for Large Tables:** Consumes Read Capacity Units (RCUs) for *all* items read, even those filtered out. This makes it expensive and slow for large tables, especially if the filter is highly selective.
  * **No Index Required:** Can be performed on any attribute.
  * **Use Cases:**
      * Small tables.
      * One-time administrative tasks (e.g., data export, batch processing of most of the table).
      * When you need to retrieve items based on an attribute that is *not* part of any index.

### Query Operation

  * **Targets a Single Partition:** A `Query` operation retrieves items based on a specific **partition key value**. DynamoDB directly accesses the relevant partition(s).
  * **Efficient Search within Partition:** Once the partition is identified, it efficiently retrieves items matching the partition key.
  * **Optional Sort Key Conditions:** If a sort key is defined, you can apply conditions (e.g., `equals`, `begins_with`, `between`, `greater_than`) to further refine results *within that partition*.
  * **Optional `FilterExpression`:** Like `Scan`, `Query` supports `FilterExpression`, but it's applied *after* items are read by the key conditions. It reduces data returned but *not* RCUs consumed.
  * **Efficient for Large Tables:** Much more efficient and cost-effective than `Scan` for targeted data retrieval, as it only reads data from relevant partitions.
  * **Requires Primary Key (or Index Key):** You must specify the partition key value to perform a `Query`.
  * **Consistency:** Can be *eventually consistent* (default, lower latency, fewer RCUs) or *strongly consistent* (most up-to-date data, slightly higher latency, more RCUs).
  * **Use Cases:**
      * Retrieving specific items when the partition key is known.
      * Getting a range of items using the sort key.
      * Real-time lookups where performance is critical.

**Recommendation:** **Always prefer `Query` over `Scan`** for performance and cost efficiency. Design your table and index primary keys to support your most common `Query` access patterns.

## Key Principles of DynamoDB Data Modeling
Data modeling in DynamoDB is fundamentally different from traditional relational database (RDBMS) modeling. In RDBMS, you design your schema first, often aiming for normalization to reduce data redundancy, and then define queries (with joins) to retrieve the data. In DynamoDB, the process is inverted: **you design your data model around your application's access patterns.**

Here's a breakdown of what data modeling in DynamoDB entails and its core principles:

1.  **Access Patterns First (Query-Centric Design):**
    * This is the most crucial principle. Before you even think about tables or attributes, you must list *all* the ways your application will read and write data. What queries will you perform? What data do you need to retrieve together?
    * DynamoDB excels at fast lookups by primary key and efficient queries on secondary indexes. It does **not** support joins across tables. This means you must design your data so that related items can be retrieved efficiently with a single `Query` or `GetItem` operation.

2.  **Single-Table Design (Denormalization):**
    * Unlike RDBMS where you often have many normalized tables, DynamoDB generally advocates for storing *all* or most of your application's data in a single table.
    * This might seem counterintuitive if you're coming from a relational background, but it's essential because DynamoDB doesn't have joins. By putting related data into the same table (and often the same partition), you can retrieve it in a single, highly efficient `Query` operation.
    * **Denormalization is embraced:** You'll intentionally duplicate data if it helps satisfy an access pattern efficiently without requiring multiple requests. Data consistency for denormalized data becomes an application-level concern (e.g., updating multiple items in a transaction).

3.  **Optimize for Queries using Primary Keys and Indexes:**
    * The primary key (Partition Key and optional Sort Key) is the most critical element of your table design. It determines how your data is physically distributed and how you can efficiently query it.
    * **Partition Key (Hash Key):**
        * Determines the physical partition where data resides.
        * **Cardinality:** Choose a partition key with high cardinality (many unique values) to ensure even data distribution and prevent "hot partitions" (a single partition receiving disproportionate traffic).
        * **Access Pattern:** It must match your most frequent `GetItem` or `Query` access patterns.
    * **Sort Key (Range Key):**
        * Orders items within a partition.
        * Allows for efficient range queries (e.g., "all orders for customer X between date A and date B").
        * Can be used to store different "types" of items within the same partition, differentiating them by their sort key prefix.
    * **Secondary Indexes (GSIs and LSIs):**
        * When your primary key doesn't support all your access patterns, you use secondary indexes.
        * **Local Secondary Indexes (LSIs):** Share the same partition key as the base table but have a different sort key. They are co-located with the base table data and support strongly consistent reads.
        * **Global Secondary Indexes (GSIs):** Can have a completely different partition key and sort key from the base table. They are physically separate tables, support only eventually consistent reads (unless Multi-Region Strong Consistency is enabled for Global Tables), and have their own throughput. GSIs are crucial for supporting diverse access patterns.

4.  **Flexible Schema (No Schema Enforcement beyond Primary Key):**
    * DynamoDB does not enforce a strict schema for attributes beyond the primary key. Different items in the same table can have different attributes. This flexibility allows for evolving your data model without disruptive schema migrations.

5.  **Understand Item Collections:**
    * An "item collection" refers to all items that share the same partition key in a table or index.
    * You can efficiently retrieve all items within an item collection using a single `Query` operation. This is key to achieving "join-like" behavior in DynamoDB without actual joins.

## Data Modeling Techniques/Patterns

Given the principles above, here are common techniques used in DynamoDB data modeling:

1.  **Generic Primary Keys (PK/SK):**
    * Instead of naming your primary key attributes like `CustomerId` and `OrderId`, it's common to use generic names like `PK` (Partition Key) and `SK` (Sort Key).
    * You then store compound values (e.g., `USER#<user_id>`, `ORDER#<order_id>`, `PRODUCT#<product_id>`) in these keys to identify different entity types within the same table and enable efficient lookups.
    * The `SK` can often store a specific type prefix combined with a unique identifier or timestamp (e.g., `DETAILS`, `ORDER_STATUS#<date>`, `REVIEW#<review_id>`).

2.  **Adjacency List Pattern (for Relationships):**
    * This is excellent for modeling many-to-many or one-to-many relationships within a single table.
    * Each entity (e.g., `User`, `Product`) is represented as an item. Relationships are also represented as items within the same partition using the sort key.
    * Example: For a social network, `PK = USER#<user_id>`, `SK = USER#<user_id>` (for user details), `SK = FRIEND#<friend_id>` (for friend relationships), `SK = POST#<post_id>` (for user's posts).

3.  **Composite Sort Keys:**
    * Concatenate multiple attributes into a single sort key using delimiters (e.g., `CITY#STATE#ZIP`).
    * This allows you to perform highly targeted queries with `begins_with` conditions on parts of the composite sort key, effectively mimicking hierarchical queries.
    * Example: For locations, `SK = Country#State#City#Street`. You can query all locations in a `Country`, or `Country#State`.

4.  **GSI Overloading:**
    * Using a single GSI to support multiple, different access patterns by varying the data stored in its PK and SK for different item types.
    * Example: A `GSI1_PK` might hold `USER#<user_id>` for user lookups, `PRODUCT#<product_id>` for product lookups, or `ORDER#<order_id>` for order lookups, depending on the item type in the base table. This consolidates indexes and saves cost.

5.  **Sparse Indexes:**
    * A GSI is "sparse" if only a subset of items from the base table are projected into the index. This happens if the GSI's partition key (or sort key) is only present on certain items in the base table.
    * Useful for indexing a smaller, more relevant subset of data (e.g., "all active users" if `IsActive` is part of the GSI's key).

6.  **Time Series Data:**
    * Often involves a partition key for the entity (e.g., `DEVICE#<device_id>`) and a sort key for the timestamp (e.g., `TIMESTAMP#<iso_date>`).
    * Allows querying a device's data within a specific time range.

## Data Modeling Workflow

1.  **Understand Your Application's Access Patterns:** This is the absolute first step. List every single way you expect to retrieve, add, update, and delete data.
    * *Example Access Pattern:* "Get all orders for a specific customer."
    * *Example Access Pattern:* "Find all products in a given category with a price range."
    * *Example Access Pattern:* "Get all comments for a particular blog post, sorted by date."

2.  **Sketch Out Your Primary Keys for Main Access Patterns:** For each access pattern, identify what attribute(s) you need to query on directly. These will become your `PK` and `SK`.

3.  **Consider Secondary Indexes for Other Access Patterns:** If your main table's primary key doesn't support a critical access pattern, design a GSI (or LSI if applicable) to fulfill it.

4.  **Visualize and Test with Sample Data:** Use tools like AWS NoSQL Workbench to visualize your data model with sample items. Run simulated queries to ensure your design works efficiently for your access patterns.

5.  **Iterate and Refine:** Data modeling is an iterative process. As your application evolves or new access patterns emerge, you might need to adjust your schema or add new indexes.

## Why is it different from Relational?

* **No Joins:** The biggest difference. You must pre-join data by denormalizing or using item collections.
* **Schema-on-Read vs. Schema-on-Write:** RDBMS are "schema-on-write" (you define schema first, then write data). DynamoDB is more "schema-on-read" (data can have flexible attributes, and your application interprets the schema when reading).
* **Fixed Throughput Cost:** In RDBMS, query complexity often dictates performance. In DynamoDB, performance and cost are directly tied to the efficiency of your key design and how effectively you avoid full table scans.
* **Optimized for Specific Access Patterns:** You design for the queries you *will* run, not for all possible queries.

By embracing these principles and thinking "access patterns first," you can unlock the full power of DynamoDB for highly scalable and performant applications.

## 10\. CRUD Examples in .NET and DynamoDB

To interact with DynamoDB in .NET, you'll use the AWS SDK for .NET, typically the `AWSSDK.DynamoDBv2` NuGet package. The `DynamoDBContext` (Object Persistence Model) simplifies operations.

First, define your C\# model class, mapping properties to DynamoDB attributes using annotations:

```csharp
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

// [DynamoDBTable("YourTableName")]
// Replace "YourTableName" with the actual name of your DynamoDB table
[DynamoDBTable("Products")]
public class Product
{
    // Partition Key (Hash Key) - required for every table
    [DynamoDBHashKey]
    public string ProductId { get; set; }

    // Sort Key (Range Key) - optional, for composite primary key
    // [DynamoDBRangeKey]
    // public string Category { get; set; }

    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Tags { get; set; } // Example of a List (set in DynamoDB)
    public bool IsAvailable { get; set; }
}
```

Now, the CRUD operations using `DynamoDBContext`:

```csharp
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime; // Required for BasicAWSCredentials
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DynamoDBProductService
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly DynamoDBContext _dbContext;

    // Constructor can take an existing IAmazonDynamoDB client,
    // or create a new one using credentials/region.
    public DynamoDBProductService(string accessKey, string secretKey, Amazon.RegionEndpoint region)
    {
        // Example: Using explicit credentials. In production, use IAM Roles/Environment Variables.
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        _dynamoDBClient = new AmazonDynamoDBClient(credentials, region);
        _dbContext = new DynamoDBContext(_dynamoDBClient);
    }

    // Constructor for dependency injection (e.g., ASP.NET Core)
    public DynamoDBProductService(IAmazonDynamoDB client)
    {
        _dynamoDBClient = client;
        _dbContext = new DynamoDBContext(_dynamoDBClient);
    }

    // --- CREATE / UPDATE (PutItem) ---
    // SaveAsync performs an upsert: creates if not exists, updates if exists.
    public async Task AddOrUpdateProductAsync(Product product)
    {
        try
        {
            await _dbContext.SaveAsync(product);
            Console.WriteLine($"Product '{product.Name}' (ID: {product.ProductId}) saved successfully.");
        }
        catch (AmazonDynamoDBException e)
        {
            Console.WriteLine($"DynamoDB Error saving product: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Generic Error saving product: {e.Message}");
        }
    }

    // --- READ (GetItem) ---
    // Loads a single item by its primary key.
    // If using a composite primary key, you'd pass both productId and category.
    public async Task<Product> GetProductByIdAsync(string productId)
    {
        try
        {
            // For a simple primary key (Partition Key only)
            return await _dbContext.LoadAsync<Product>(productId);

            // If Product had a composite primary key (e.g., ProductId and Category):
            // return await _dbContext.LoadAsync<Product>(productId, category);
        }
        catch (AmazonDynamoDBException e)
        {
            Console.WriteLine($"DynamoDB Error getting product {productId}: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Generic Error getting product {productId}: {e.Message}");
            return null;
        }
    }

    // --- QUERY (Efficiently retrieve items by Partition Key and optional Sort Key conditions) ---
    // Example: Assuming 'Category' is a GSI Hash Key on your Products table for this query.
    // If 'Category' was the base table's HashKey, ProductId as RangeKey, you'd query on Category directly.
    public async Task<List<Product>> GetProductsByCategory(string categoryName, string indexName = "Category-Index")
    {
        var products = new List<Product>();
        try
        {
            // Create a Query operation configuration for a GSI
            var config = new DynamoDBOperationConfig
            {
                // IndexName is crucial when querying a GSI
                IndexName = indexName,
                // You can also specify consistent reads for GSIs if enabled (Multi-Region Strong Consistency)
                // ConsistentRead = true
            };

            // QueryAsync takes the hash key value and optional operation config
            var search = _dbContext.QueryAsync<Product>(categoryName, config);

            // Get all results (pagination handled automatically by GetRemainingAsync)
            products = await search.GetRemainingAsync();
            Console.WriteLine($"Found {products.Count} products in category '{categoryName}'.");
        }
        catch (AmazonDynamoDBException e)
        {
            Console.WriteLine($"DynamoDB Error querying by category: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Generic Error querying by category: {e.Message}");
        }
        return products;
    }

    // --- SCAN (Retrieves all items, then filters - less efficient for large tables) ---
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();
        try
        {
            // ScanAsync without conditions will fetch all items
            var search = _dbContext.ScanAsync<Product>(new List<ScanCondition>());

            // Get all results (pagination handled automatically)
            products = await search.GetRemainingAsync();
            Console.WriteLine($"Scanned {products.Count} total products.");
        }
        catch (AmazonDynamoDBException e)
        {
            Console.WriteLine($"DynamoDB Error scanning products: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Generic Error scanning products: {e.Message}");
        }
        return products;
    }

    // --- DELETE ---
    // Deletes an item by its primary key.
    // If using a composite primary key, you'd pass both productId and category.
    public async Task DeleteProductAsync(string productId)
    {
        try
        {
            // For a simple primary key (Partition Key only)
            await _dbContext.DeleteAsync<Product>(productId);

            // If Product had a composite primary key:
            // await _dbContext.DeleteAsync<Product>(productId, category);

            Console.WriteLine($"Product with ID: {productId} deleted successfully.");
        }
        catch (AmazonDynamoDBException e)
        {
            Console.WriteLine($"DynamoDB Error deleting product {productId}: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Generic Error deleting product {productId}: {e.Message}");
        }
    }
}

// Example Program usage (in a console app's Main method)
public class Program
{
    public static async Task Main(string[] args)
    {
        // IMPORTANT: Replace with your actual AWS credentials and desired region.
        // For production, prefer IAM roles for EC2 instances or Lambda functions.
        // For local development, configure AWS credentials file or environment variables.
        // This example uses explicit credentials for clarity, but it's not a best practice for production.
        string awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        string awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        Amazon.RegionEndpoint region = Amazon.RegionEndpoint.APSouth1; // Mumbai region

        if (string.IsNullOrEmpty(awsAccessKey) || string.IsNullOrEmpty(awsSecretKey))
        {
            Console.WriteLine("AWS credentials not found. Please set AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY environment variables.");
            return;
        }

        var productService = new DynamoDBProductService(awsAccessKey, awsSecretKey, region);

        // 1. Add/Update a Product
        Console.WriteLine("\n--- Adding/Updating Products ---");
        var laptop = new Product
        {
            ProductId = "PROD_LAPTOP_001",
            // Category = "Electronics", // Uncomment if using a composite primary key
            Name = "SuperFast Laptop",
            Price = 1500.00m,
            StockQuantity = 50,
            Tags = new List<string> { "electronics", "computer", "high-end" },
            IsAvailable = true
        };
        await productService.AddOrUpdateProductAsync(laptop);

        var book = new Product
        {
            ProductId = "PROD_BOOK_002",
            // Category = "Books", // Uncomment if using a composite primary key
            Name = "The Sci-Fi Odyssey",
            Price = 25.50m,
            StockQuantity = 200,
            Tags = new List<string> { "fiction", "sci-fi" },
            IsAvailable = true
        };
        await productService.AddOrUpdateProductAsync(book);

        // Update an attribute
        laptop.StockQuantity = 45;
        await productService.AddOrUpdateProductAsync(laptop);

        // 2. Get a Product
        Console.WriteLine("\n--- Getting a Product ---");
        var retrievedLaptop = await productService.GetProductByIdAsync("PROD_LAPTOP_001");
        if (retrievedLaptop != null)
        {
            Console.WriteLine($"Retrieved: {retrievedLaptop.Name}, Price: {retrievedLaptop.Price}, Stock: {retrievedLaptop.StockQuantity}");
        }
        else
        {
            Console.WriteLine("Laptop not found.");
        }

        // 3. Query Products by Category (Requires a GSI named "Category-Index" on the Products table)
        Console.WriteLine("\n--- Querying Products by Category (assuming GSI exists) ---");
        // For this to work, ensure your "Products" table has a GSI named "Category-Index"
        // with 'Category' as its partition key. If your base table PK is Category, use that directly.
        // For this example to function directly, change `Product` class to have `Category` as `DynamoDBHashKey`
        // and `ProductId` as `DynamoDBRangeKey`. Or create a GSI as mentioned.
        var electronicsProducts = await productService.GetProductsByCategory("Electronics", "Category-Index");
        foreach (var p in electronicsProducts)
        {
            Console.WriteLine($"- Queried (GSI): {p.Name} (ID: {p.ProductId})");
        }


        // 4. Scan All Products (Use with caution on large tables)
        Console.WriteLine("\n--- Scanning All Products ---");
        var allProducts = await productService.GetAllProductsAsync();
        foreach (var p in allProducts)
        {
            Console.WriteLine($"- Scanned: {p.Name} (ID: {p.ProductId}) - Tags: {string.Join(", ", p.Tags ?? new List<string>())}");
        }

        // 5. Delete a Product
        Console.WriteLine("\n--- Deleting a Product ---");
        await productService.DeleteProductAsync("PROD_BOOK_002");

        // Verify deletion
        var deletedBook = await productService.GetProductByIdAsync("PROD_BOOK_002");
        if (deletedBook == null)
        {
            Console.WriteLine("Product PROD_BOOK_002 successfully deleted.");
        }
    }
}
```

This comprehensive guide covers the essential aspects of DynamoDB, providing a strong foundation for understanding its architecture, features, and practical application. Remember that effective DynamoDB design heavily relies on understanding your application's access patterns to optimize your primary key and index choices.
