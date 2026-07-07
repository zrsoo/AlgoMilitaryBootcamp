# System Design — Study Curriculum

Self-contained explainers to internalize **what's best for what purpose** *before* mocking designs. Read in order; each is standalone but they build. Pair with [../decision-framework.md](../decision-framework.md) (the catalog) and [../method.md](../method.md) (the interview method).

Target level: **senior (L5)** — understand mechanisms, not just names. When a note says "X is faster," it will say *why* at the mechanism level, because that's what a senior deep-dive probes.

## Reading order

| # | Note | Covers | Maps to framework |
|---|------|--------|-------------------|
| 01 | [01-foundations.md](01-foundations.md) | Latency numbers, back-of-envelope estimation, CAP/PACELC, consistency models, the core trade-offs | Layer 0, 8 |
| 02 | 02-databases.md *(planned)* | Relational vs NoSQL families, storage engines (B-tree vs LSM), indexing, when each store wins | Layer 5, 9 |
| 03 | 03-replication-partitioning.md *(planned)* | Replication modes, sharding strategies, quorums, consensus (Raft), rebalancing | Layer 8 |
| 04 | 04-caching.md *(planned)* | Cache patterns, invalidation, stampede/hot keys, where to cache | Layer 6 |
| 05 | 05-api-communication.md *(planned)* | REST/gRPC/GraphQL/WebSocket/SSE, sync vs async, idempotency, pagination | Layer 3 |
| 06 | 06-edge-load-balancing.md *(planned)* | CDN, L4/L7 LBs, routing algorithms, gateways, meshes | Layer 1, 2 |
| 07 | 07-messaging-streaming.md *(planned)* | Queue vs log, delivery semantics, ordering, back-pressure, Kafka model | Layer 7 |
| 08 | 08-search-analytics.md *(planned)* | OLTP vs OLAP, columnar storage, full-text indexing, CDC | Layer 9 |
| 09 | 09-reliability.md *(planned)* | Failure modes, retries/timeouts/backoff, circuit breakers, idempotency, degradation | Layer 11 |
| 10 | 10-observability-security.md *(planned)* | Metrics/logs/traces, SLOs, authn/authz, encryption, rate limiting | Layer 11 |
| 11 | 11-data-platform-lakehouse.md *(planned)* | Ingestion, table formats (Delta/Iceberg/Hudi), medallion, feature store | Layer 12 |
| 12 | 12-distributed-systems-deepdive.md *(planned)* | Senior 2nd-round depth: query execution, shuffle, multi-tenant compute, exactly-once | senior deep-dive round |

## How to study each note

1. Read it once for the map.
2. Re-read and, for each option, be able to say the **"use when"** and the **"cost"** without looking.
3. Cross-check against the framework table for that layer — they should agree.
4. Only after a note is solid, use it in a mock; log gaps back into the note.

## Status

- 01 written (style anchor). Rest planned — will write in order, calibrating depth after 01 feedback.
