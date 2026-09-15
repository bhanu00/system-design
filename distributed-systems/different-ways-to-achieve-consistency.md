# How Distributed Systems achieve Consistency & Fault tolerance 



## 🔍 Overview

Distributed systems often need to **agree on a shared state**, maintain **consistency**, and be **fault-tolerant**. To achieve this, different algorithms and techniques are used. These include:

* **Consensus Algorithms** (like Raft, Paxos)
* **Gossip Protocols**
* **Vector Clocks**
* **Quorum Reads/Writes**

Let’s understand how each works, their purpose, use cases, and how they relate to each other.

---

## ✅ 1. Consensus Algorithms

Consensus algorithms help multiple nodes **agree on a single value/state** even in the presence of failures.

### 🔹 Raft

* **Purpose**: Leader election, log replication, maintaining strong consistency.
* **Used in**: etcd, CockroachDB, MongoDB (Replica Sets), Consul (partially).
* **Features**: Easier to understand than Paxos, maintains a single leader for coordination.

### 🔹 Paxos

* **Purpose**: Same as Raft, but harder to understand and implement.
* **Used in**: Google Chubby, FoundationDB, ScyllaDB (for LWT), Amazon Aurora (internal).
* **Features**: Fully decentralized, more theoretical, higher message complexity.

> These are **true consensus protocols** and ensure **linearizability** (strong consistency).

---

## ❌ 2. Gossip Protocol – Membership and Health

* **Purpose**: Nodes exchange state with peers to maintain cluster membership and health.
* **How it works**: Like a viral spread of information. Every node tells a few random peers what it knows.
* **Used in**: Cassandra, ScyllaDB, Consul, DynamoDB.
* **Key Use Case**: Detecting which nodes are alive or dead.
* **Not used for**: Reaching agreement on data values.

---

## ❌ 3. Vector Clocks – Conflict Resolution

* **Purpose**: Track **causal relationships** between updates across nodes.
* **How it works**: Each update is tagged with a vector clock. If updates from different nodes conflict, the vector clock helps detect and resolve it.
* **Used in**: Riak, DynamoDB, Cassandra.
* **Use Case**: "Last write wins" isn’t enough in distributed systems; vector clocks help preserve update history.

> Vector clocks help in **eventual consistency** scenarios. They are used for **conflict detection**, not consensus.

---

## ❌ 4. Quorum Reads/Writes – Tunable Consistency

* **Purpose**: Ensure that reads and writes overlap to increase consistency.
* **How it works**:

  * **W** = # of nodes written to.
  * **R** = # of nodes read from.
  * **N** = total replicas.
  * If `R + W > N`, you have a **quorum**.
* **Used in**: Cassandra, DynamoDB.
* **Example**: If `W=2`, `R=2`, `N=3`, then the read and write must intersect.

> This allows **tunable consistency**: choose speed vs. consistency as needed.

---

## 🔄 Relationship Summary

| Technique           | Type                | Purpose                              | Examples               |
| ------------------- | ------------------- | ------------------------------------ | ---------------------- |
| **Raft**            | Consensus           | Leader election, strong consistency  | etcd, CockroachDB      |
| **Paxos**           | Consensus           | Agreement without centralized leader | FoundationDB, ScyllaDB |
| **Gossip Protocol** | Membership          | Detect node liveness, share state    | Cassandra, Consul      |
| **Vector Clocks**   | Causal tracking     | Detect concurrent updates            | DynamoDB, Riak         |
| **Quorum R/W**      | Tunable consistency | Balance availability and consistency | Cassandra, DynamoDB    |

---

## 🤔 Real-world analogy

| Concept          | Analogy                                                                                        |
| ---------------- | ---------------------------------------------------------------------------------------------- |
| **Raft/Paxos**   | Parliament voting on a law (must agree on one bill)                                            |
| **Gossip**       | Spreading a rumor to your friends randomly                                                     |
| **Vector Clock** | Tracking changes to a shared Google Doc with timestamps per editor                             |
| **Quorum R/W**   | Having multiple copies of a will, and needing majority agreement to confirm its latest version |

---

## 🔁 Final Visualization

```
              +----------------------------+
              |     Consensus Algorithms   |
              |  (Agreement on a value)    |
              +----------------------------+
                 |                    |
              Raft               Paxos

              +----------------------------+
              |   Supportive Mechanisms     |
              +----------------------------+
               | Gossip Protocol – health/membership
               | Vector Clocks – versioning/causality
               | Quorum R/W – tunable consistency
```

---

## ✅ Conclusion

* Use **Raft/Paxos** when you need **strong consistency**.
* Use **Gossip Protocol** for **cluster state propagation**.
* Use **Vector Clocks** for **conflict detection**.
* Use **Quorum Reads/Writes** to **tune availability vs consistency**.

Each plays a different role in a robust, distributed, and scalable architecture.

Let me know if you’d like a system-specific walkthrough like how Cassandra combines Gossip + Quorum + Hinted Handoff etc.
