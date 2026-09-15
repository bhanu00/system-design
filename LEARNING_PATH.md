# Learning Path

This is the guided route through the Software Engineering Handbook. Use the [book index](INDEX.md) whenever you want to explore beyond the suggested sequence.

The path is intentionally flexible: finish each outcome, rather than treating the time estimates as a deadline.

## Choose Your Starting Point

| Your goal | Start here |
| --- | --- |
| Build programming and interview fundamentals | [Path A: Foundations to Senior](#path-a-foundations-to-senior) |
| Improve system-design ability | [Path B: System Design and Distributed Systems](#path-b-system-design-and-distributed-systems) |
| Become stronger in cloud/platform engineering | [Path C: Cloud Platform and Reliability](#path-c-cloud-platform-and-reliability) |
| Prepare for Staff, Principal, or PMTS roles | [Path D: Staff and Principal Engineer](#path-d-staff-and-principal-engineer) |
| Prepare for interviews | [Path E: Interview Preparation](#path-e-interview-preparation) |

## Core Sequence

```text
Foundations -> DSA -> Object-Oriented Design -> System Design ->
Distributed Systems and Architecture -> Cloud and Reliability ->
Security -> Applied Projects -> Interviews
```

## Path A: Foundations to Senior

Estimated pace: 12 to 16 weeks.

### Phase 1: Engineering foundations

1. [Foundations](foundations/README.md)
2. [.NET and C# fundamentals](dotnet/README.md), if C# is your primary language

Outcome: you can decompose a problem, explain SOLID, and write small maintainable programs.

### Phase 2: Data structures and algorithms

Follow the [DSA section](dsa/README.md), starting with complexity, arrays, strings, linked lists, trees, and recursion. Then cover DFS, dynamic programming, greedy algorithms, and the pattern files.

Outcome: you can recognize common problem patterns and solve medium-level coding problems with clear complexity analysis.

### Phase 3: Object-oriented design

Follow the [Object-Oriented Design section](oops/README.md), including its case studies.

Outcome: you can model a small domain, apply patterns for a reason, and explain alternatives.

### Phase 4: Application development

Use [.NET and frontend](dotnet/README.md) or [Backend](backend/README.md) for technology-specific depth, then implement or modify a sample in [Solutions and POCs](solutions/README.md).

Outcome: you can turn a design into a working, tested application.

## Path B: System Design and Distributed Systems

Estimated pace: 12 to 16 weeks. Start after you are comfortable with basic APIs, databases, and programming.

### Phase 1: System-design foundations

1. [System-design foundations](foundations/fundamentals.md)
2. [System Design section](system-designs/README.md)
3. [Non-functional requirements](system-designs/nfrs-for-hld.md)
4. [High-level design process](resources/high-lvel-design-process.md)
5. [System-design trade-offs](system-designs/system-design-trade-offs.md)

### Phase 2: Performance, APIs, and data

1. [Scalability](system-designs/scalability.md)
2. [API performance](system-designs/api-performance.md)
3. [Caching](system-designs/caching.md)
4. [Rate limiting](system-designs/rate-limiting.md)
5. [Databases and Data Systems](database/README.md)
6. [API guidelines](resources/public-api-guidelines.md)

### Phase 3: Distributed systems

Follow [Distributed Systems](distributed-systems/README.md), then read [network protocols and load balancing](system-designs/network-protocols-and-load-balancer.md).

### Phase 4: Architecture and events

1. [Architecture](architectures/README.md)
2. [Event sourcing and materialized views](distributed-systems/event-sourcing-materialized-view.md)
3. [Event non-delivery](distributed-systems/event-non-delivery.md)
4. [Background jobs](distributed-systems/background-jobs.md)
5. [Batch versus stream processing](distributed-systems/batch-vs-stream-processing.md)

Outcome: you can lead an end-to-end design discussion, defend its trade-offs, and explain how it behaves when dependencies fail.

## Path C: Cloud Platform and Reliability

Estimated pace: 6 to 10 weeks.

1. [DevOps and Operations](devops/README.md)
2. [GitOps](gitops/README.md)
3. [Container fundamentals](dotnet/container-fundamentals.md)
4. [Cloud monitoring](devops/monitoring-cloud-based.md)
5. [Operational excellence](devops/operational-excellence.md)
6. [Disaster recovery](devops/disaster-recovery.md)
7. [Security](security/README.md)
8. [Cascading failures](distributed-systems/cascading-failure.md)
9. [Resiliency versus fault tolerance](distributed-systems/resiliency-vs-fault-tolerance.md)

Apply this work to the GitOps sample or another POC in [Solutions](solutions/README.md): add health checks, structured logs, metrics, a deployment pipeline, and a failure-mode test.

Outcome: you can design, deploy, observe, and operate a service rather than only implement one.

## Path D: Staff and Principal Engineer

Estimated pace: ongoing; spend 12 focused weeks preparing before interviews.

Use [Career Preparation](career/README.md) as the role-requirements source of truth, then track progress in the [target-role competency matrix](career/target-role-competency-matrix.md).

### Technical depth

1. Complete [Path B](#path-b-system-design-and-distributed-systems) and [Path C](#path-c-cloud-platform-and-reliability).
2. Study [System Design](system-designs/README.md), [Architecture](architectures/README.md), [Distributed Systems](distributed-systems/README.md), and [Use Cases](use-cases/README.md) as design-review exercises.
3. Build one portfolio-quality system in [Solutions](solutions/README.md), with a design document, ADRs, load tests, SLOs, dashboards, and a cost/reliability analysis.
4. Review [Agentic AI](agentic-ai/README.md) for modern system concerns.

### Leadership depth

1. Read [Senior engineer](foundations/senior-engineer.md).
2. Write 10 career stories using: context, constraints, alternatives, decision, alignment, result, and learning.
3. Practice explaining architecture to three audiences: an engineer, a product leader, and an executive.
4. Turn one POC into a multi-team style proposal: roadmap, migration plan, risks, ownership boundaries, and success metrics.

Outcome: you can demonstrate technical judgment and organizational impact, not only individual coding ability.

## Path E: Interview Preparation

Estimated pace: 6 to 8 weeks; run it alongside the relevant technical path.

1. Follow [Interview Preparation](interviews/README.md).
2. Revisit [DSA](dsa/README.md) for coding rounds.
3. Use [System Design](system-designs/README.md) and [Use Cases](use-cases/README.md) for design rounds.
4. Practice company-focused material in [interviews/](interviews/).
5. Use [modernization mock interviews](interviews/my-modernization-mock-interviews.md) to rehearse communication and migration trade-offs.

Weekly cadence:

- Two coding sessions
- Two system-design sessions
- One behavioral or leadership mock
- One design-document review or implementation improvement

## Progress Checklist

- [ ] I can solve common DSA patterns and communicate complexity.
- [ ] I can model a domain and justify object-oriented design decisions.
- [ ] I can design an API, data model, cache, and asynchronous workflow.
- [ ] I can explain consistency, retries, idempotency, and failure handling.
- [ ] I can deploy, monitor, and operate a service.
- [ ] I can articulate security and reliability trade-offs.
- [ ] I have one end-to-end POC or application I can discuss deeply.
- [ ] I have evidence-based stories of technical leadership and influence.

## Continue Exploring

The [book index](INDEX.md) is the complete topic map. Use it to find focused references, extra practice material, and adjacent topics after completing a path.
