# [Refer from this , By Ashish](https://github.com/ashishps1/awesome-system-design-resources)
# Fundamentals of System Design
- [Cloud native - Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/definition)
- [Cloud native - AWS](https://aws.amazon.com/what-is/cloud-native/)
- [What is System Design](https://www.youtube.com/watch?v=quLrc3PbuIw)
- [Algorithms you must know before system design](https://www.youtube.com/watch?v=xbgzl2maQUU)
- [SPOF](https://www.youtube.com/watch?v=-BOysyYErLY)
- [Latency - Why it matters?, Latency, Bandwidth and Throughput](https://www.ibm.com/topics/latency)
- [Scalability](https://www.youtube.com/watch?v=xpDnVSmNFX0) -
Below are some additional docs to read about scalability.
    - [Scalability - doc](https://blog.algomaster.io/p/scalability)
    - [Scalability - doc](https://systemdesignprep.com/scalability)
    - [Architecting for Reliable Scalability by AWS](https://aws.amazon.com/blogs/architecture/architecting-for-reliable-scalability/)
- [Reliability](https://www.youtube.com/watch?v=jwsyC9CQKA4) - has 2 pillors Availability and Resiliency. Fault tolerance and fault isolation are important concepts when we think about availability.
    - [Availability](https://www.youtube.com/watch?v=LdvduBxZRLs)
        - [Understanding availability - AWS](https://docs.aws.amazon.com/whitepapers/latest/availability-and-beyond-improving-resilience/understanding-availability.html)
    - [Fault Tolerance & Fault isolation](https://docs.aws.amazon.com/whitepapers/latest/availability-and-beyond-improving-resilience/fault-tolerance-and-fault-isolation.html)
    - Resiliency
        - [Patterns for Resilient Architecture - Adrian Hornsby - Video](https://www.youtube.com/watch?v=gET51_C3k5s) It covers Resilancy patterns, Famous 9s of availibility
        - [Five Design Patterns to build resilient Applications - Derek Bingham - NDC Melbourne 2022 - Video](https://www.youtube.com/watch?v=gE4Bo5ZjfgY)
        - [AWS re:Invent 2023 - Resilient architectures at scale: Real-world use cases from Amazon.com - Video](https://www.youtube.com/watch?v=fQgaR-iQrTY)
        - [How to verify resiliancy of a cloud-native application](https://www.ibm.com/blog/a-four-step-approach-to-verifying-the-resiliency-of-cloud-native-applications/)
        - [Understand resiliency patterns in AWS](https://aws.amazon.com/blogs/architecture/understand-resiliency-patterns-and-trade-offs-to-architect-efficiently-in-the-cloud/)
        - [Azure Application Reliliency Patterns](https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/application-resiliency-patterns)
        - [Cloud Infrastructure resiliency with Azure](https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/infrastructure-resiliency-azure)
        - [AWS Reliability](https://docs.aws.amazon.com/wellarchitected/latest/reliability-pillar/reliability.html)
        - [Azure Reliability](https://learn.microsoft.com/en-us/azure/reliability/overview)
- [CAP Theoram](https://www.youtube.com/watch?v=eWMgsk7mpFc)
    - [CAP & PACELC Theoram](https://blog.algomaster.io/p/cap-theorem-explained)
    - [CAP](https://www.ibm.com/topics/cap-theorem)
- [BASE]()
- [Idempotency](https://serverlessland.com/event-driven-architecture/idempotency)
- [Implement idempotent AWS Lambda functions](https://aws.amazon.com/blogs/compute/implementing-idempotent-aws-lambda-functions-with-powertools-for-aws-lambda-typescript/)
- [distributed Messaging Queue]()
- [distributed locking]()
- [Cascading Failures](https://github.com/bhanu00/system-design/blob/main/cascading-failure.md)
- [Polling Long vs Short]()
- [Gossip Protocol]()
- [Indexing]()
- [Partition and Sharding]()
- [Sparse Index]()
- [Bloom Filters]()
- [Consensus Algorithm]()
- [Replication vs Redundancy]()
- [Messaging Queue]()
  - [Guarante Delivery]()
  - [At least once / At most once]()
  - [Ordering]()
  - [Dead Lettering]
- [Gateway]()
- [Service Discovery]()
- [Change Data Capture]()

- ### CDN
- ### Edge Computing
- ### Subnetting
This document explains about Consistency ([Strong vs Week](https://www.geeksforgeeks.org/eventual-vs-strong-consistency-in-distributed-databases/)), Availability, Partition tolerance and detailed about CAP theorem.

## Load Balancing and algorithms
- Algorithms used in Load balancing
    - Layman's Approach
    - Least Connections
    - Least Response Time
    - Round Robin
    - Weighted Round Robin
    - IP Hash
    - Consistent Hashing

## API Development
  - [Rate Limiting](fixed window, sliding window, token bucket, concurrency)
  - [When to Use Webhooks, WebSocket, Pub/Sub, and Polling] (https://hookdeck.com/webhooks/guides/when-to-use-webhooks)
  - [Webhook vs. API](https://zapier.com/blog/webhook-vs-api/)
  - [Different Ways to create API]
  - [API Proxy]
  - [Reverse Proxy vs. API Gateway vs. Load Balancer vs. Forward Proxy]
  - [Open API Spec] (https://swagger.io/specification/)

## Security 
- Authentication/Authorization
- [TLS Termination - Types, Uses, Benefits]()
- [How SSL/TLS works when connecting to anything]()
- mTLS for internal services
- OAuth2 / OIDC for clients
- Encryption at rest (AWS KMS) & in transit (TLS 1.2/1.3)
- Secrets Management
- [Token Types](https://cloud.google.com/docs/authentication/token-types)
- [JWT Tokens](https://jwt.io/introduction/)

## [SQL vs NoSQL](https://www.ibm.com/blog/sql-vs-nosql/)
## Caching
Know more about Caching, Cache Hit, Cache Miss, Cache Invalidation Strategy
- [Caching](https://medium.com/must-know-computer-science/system-design-caching-acbd1b02ca01)
- [Common Caching missunderstandings](https://medium.com/geekculture/system-design-basics-5-common-caching-misunderstandings-explained-2f19b1c88373)
- [Different levels of caching](https://medium.com/@abhishekranjandev/caching-in-system-design-an-in-depth-exploration-b51e2c2e4dbd)

## Disaster Recovery Solutions
- [Disaster Recovery (DR) Architecture on AWS](https://aws.amazon.com/blogs/architecture/disaster-recovery-dr-architecture-on-aws-part-i-strategies-for-recovery-in-the-cloud/)
- Dive deep into [Disaster Recovery of Workloads on AWS: Recovery in the Cloud](https://docs.aws.amazon.com/whitepapers/latest/disaster-recovery-workloads-on-aws/introduction.html)

## Software Diagrams
- [Sequence Diagrams] - TODO , Lucide Chart we can use to create sequence diagrams [Sequence daigram with Lucid](https://www.youtube.com/watch?v=pCK6prSq8aw).
- [UML Diagrams]
- []
## [Some Good Engineering Blogs](https://interviewready.io/blog)

##[Videos]()
- Dos and Don't Do - https://www.youtube.com/watch?v=CtmBGH8MkX4

### Non Functional Requiremets
  To design a system, there are some non-functional requirements which helps to make decisions.
- Letency
- Consistency & Availability - CAP theoram will help you to decide CP vs AP.
- 

Make a proper plan to become master in System Design, Make this schedule topic wise then complexity wise. Basic system Design topics are available every where so make it with proper AWS and Microsoft Blogs reference , for example ([Disaster Recovery (DR) Architecture on AWS](https://aws.amazon.com/blogs/architecture/disaster-recovery-dr-architecture-on-aws-part-i-strategies-for-recovery-in-the-cloud/)). Post 1 topic with (summary and example) daily on Linkedin once this system design is almost complete and  focus should be more on System design instead of DSA. For senior positions System Design, HLD, LLD matters. and provide some examples as well like How netflix scaled his system, etc.

## Good To know before jumping to System Design
- [Google India Engineers in a Mock System Design Interview](https://www.youtube.com/watch?v=S1DvEdR0iUo)

## System Design Mocks
- [Exponent - System Design Interview: Design Amazon Prime Video](https://www.youtube.com/watch?v=PuU_0esYyhg)

## Last minute URLs
- https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/resilient-communications
- https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/containers/aks-mission-critical/mission-critical-app-platform
- https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/
