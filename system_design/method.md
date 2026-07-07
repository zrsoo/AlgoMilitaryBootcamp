# System Design — Method

> A repeatable method for attacking any system-design question under interview time pressure.
> This is a **living document**: refine the phases, timings, and checklists after every practice rep.
> The goal is a groove I can fall into on autopilot so working memory is spent on the *problem*, not on *what to do next*.
>
> **Target level: Senior SWE (L5 / IC4).** The method below is the spine for *any* SD prompt; the senior bar (§0) demands more depth, more autonomy, and a second design-flavored round.

Status: v0.2 (2026-07-07) — retargeted to senior. See [Changelog](#changelog) + [Open questions](#open-questions-iterate-here) at the bottom.

---

## 0. What this round actually is (Databricks context)

Grounding facts (sourced below — re-verify before the loop):

- **Format:** one open-ended problem, ~45–60 min, single interviewer. I drive; they probe deeper as I go. (systemdesignhandbook, interviewing.io — as of 2026-07-07)
- **Medium:** often a **Google Doc**, not a whiteboard/diagram tool. Some interviewers do allow drawing — confirm at the start. Practice designing in *text*: ASCII boxes, bulleted component lists, labeled arrows written as `A --REST--> B`. (interviewing.io, systemdesignhandbook)
- **Two flavors it can take:**
  1. **Standard product/architecture design** — e.g. "design a service that finds the cheapest copy of a book across distributors" (real Databricks report). Classic web-service design.
  2. **Data-platform design** — ingestion pipelines, streaming vs batch, storage/compute separation, lakehouse-flavored. More likely for platform/compute/infra teams.
  - Prepare the *general method* to serve both; keep a thin data-platform vocabulary layer ready (see §7).
- **What they grade (repeated across sources):** structured problem-solving, clarifying before building, **driving the conversation**, articulating **trade-offs** (latency vs throughput, batch vs stream, cost vs complexity), **MVP-first then scale**, and communication clarity ("explain the *why*"). Technical correctness alone is not enough.

### Senior (L5 / IC4) bar — what changes vs mid-level

- **Usually two design-flavored rounds, not one:** (1) a **system-design** round (often *multi-tenant data platform* or *query routing*), and (2) a **distributed-systems / Spark-internals deep-dive** round. Budget prep for both. (ophyai 2026 — as of 2026-07-07)
- **Depth is the differentiator.** Senior candidates who can't go deep on execution internals struggle. Expect probes on: wide vs narrow dependencies, shuffle boundaries, stage construction, adaptive query execution (AQE), Catalyst plan phases, vectorized (Photon-style) execution — *if the role touches compute-core*. For other teams, the equivalent depth is on *my* domain's internals (consistency protocols, partitioning, failure recovery). Know one stack cold.
- **More autonomy expected.** I drive harder, propose the problem framing myself, and volunteer trade-offs without being asked. Less hand-holding from the interviewer.
- **Strategic framing counts.** Be ready to justify architecture choices against alternatives (lakehouse vs warehouse; Delta vs Iceberg/Hudi; batch vs streaming) — the "why this and not that" at a systems level.
- **Multi-tenancy, isolation, and cost at scale** show up more: tenant isolation (keyspaces, RBAC, quotas), noisy-neighbor control, elastic autoscaling, scale-to-zero economics.
- Still an IC design round — I own the design end-to-end; "I'd delegate that" is not an answer.

**The one-line mantra:** *Clarify → scope an MVP → make the data flow → then scale it, narrating trade-offs the whole way — and at senior, go one level deeper than feels necessary on the hard component.*

---

## 1. The phased loop (the spine)

Default budget for a 50-minute round. Adjust ±, but always **timebox** — running out of time before the deep-dive is the classic failure.

| # | Phase | Time | Output on the "page" |
|---|-------|------|----------------------|
| 1 | **Requirements & scope** | 5–8 min | Bulleted functional + non-functional list, explicit out-of-scope |
| 2 | **Estimation (back-of-envelope)** | 3–5 min | QPS, storage/day, read:write ratio, bandwidth |
| 3 | **API / data contract** | 3–5 min | Core endpoints or interfaces + key entities |
| 4 | **High-level architecture** | 8–10 min | Component list + data-flow arrows; the "happy path" end-to-end |
| 5 | **Deep dive (1–2 components)** | 12–15 min | The hard part: schema, algorithm, consistency, the interviewer's pushes |
| 6 | **Scale / bottlenecks / failure** | 8–10 min | What breaks at 10×, mitigations, failure modes |
| 7 | **Wrap / trade-off summary** | 2–3 min | Recap decisions + what I'd do next with more time |

Rule: **do not start drawing boxes until requirements are on the page.** Jumping to architecture is the #1 tell of a junior candidate.

---

## 2. Phase-by-phase playbook

### Phase 1 — Requirements & scope
- **Ask, don't assume.** Who are the users? What's the core use case? What's explicitly *not* in scope?
- Separate **functional** (what it does: features, endpoints) from **non-functional** (how well: scale, latency, availability, consistency, durability, cost).
- Force a **scale number** early: "How many users / events / QPS are we targeting?" This drives every later decision.
- Nail **one primary metric to optimize** (latency? throughput? cost? consistency?). Databricks rewards choosing the right SLA for the problem, not gold-plating everything.
- Write out-of-scope items explicitly ("assuming auth is handled, skipping payments") so I get credit for knowing they exist without spending time.

### Phase 2 — Estimation
- Only what informs a decision. Typical set:
  - Read QPS, write QPS, read:write ratio.
  - Storage/day and storage over retention window.
  - Peak vs average (× 2–5 for peak).
  - Bandwidth if media/large payloads.
- Round aggressively. State assumptions out loud. The number matters less than showing I can size the system (single box vs fleet vs sharded).

### Phase 3 — API / data contract
- Define the **interface first** — a few core endpoints (`POST /x`, `GET /y`) or class methods for a design-coding-style prompt.
- Sketch the **key entities** and their relationships (this is often where the real problem hides).
- The contract frames everything downstream and shows product sense.

### Phase 4 — High-level architecture
- Draw the **happy path end-to-end** before optimizing anything: client → gateway/LB → service(s) → storage → back.
- Label each arrow with the **protocol** (REST / gRPC / Kafka stream / JDBC) — signals I understand latency and coupling implications.
- Name the **storage choice and why** (SQL vs NoSQL vs object store vs cache vs search index) tied to the access pattern, not to fashion.
- **Separate storage from compute** where relevant — cloud-native default; lets each scale independently.
- Start **monolith/simple**, then note the seams where I'd split into services as it grows. Pragmatism + vision.

### Phase 5 — Deep dive
- The interviewer will steer here. Have a **default target**: the component with the hardest correctness or scale property.
- Go deep on **one or two**: data schema, the core algorithm/data structure, the concurrency/consistency model, indexing, partitioning key.
- Respond to pushes ("now make it real-time", "now 100× the data", "what if a node dies") with a concrete change, not hand-waving.

### Phase 6 — Scale, bottlenecks, failure
- Walk the request path and name each bottleneck: single points of failure, hot partitions, the DB, the cache, the network.
- Standard toolkit to reach for (justify each): horizontal scaling, load balancing, caching (and invalidation), read replicas, sharding/partitioning, async queues, CDN, rate limiting, back-pressure.
- **Failure modes:** what happens when each component dies? Retries, timeouts, idempotency, checkpointing, replication, multi-region (only if the SLA needs it).
- **Cost awareness** (Databricks explicitly grades this): don't run hot/expensive resources idle; match storage tier and compute to the SLA.

### Phase 7 — Wrap
- Recap the 3–4 key decisions and the trade-off behind each.
- State what I'd tackle next with more time. Leaving a clear "here's the roadmap" ending lands better than trailing off mid-diagram.

---

## 3. Trade-offs I should always be naming out loud

Databricks weights this heavily. Keep these tensions loaded and reach for the relevant ones:

- **Latency vs throughput** — optimize for one; you rarely max both.
- **Consistency vs availability** (CAP) — strong vs eventual; pick per data type, not globally.
- **Batch vs streaming** — cost/simplicity vs freshness. Don't build real-time if daily reports suffice; don't build batch if the use case is sub-second.
- **Normalization vs denormalization** — write simplicity vs read speed.
- **SQL vs NoSQL** — transactions/joins vs scale/flexible schema.
- **Sync vs async** — simplicity/consistency vs decoupling/resilience.
- **Cost vs performance vs complexity** — the eternal triangle; name which corner I'm sacrificing.
- **Monolith vs microservices** — operational simplicity now vs independent scaling later.

Phrasing pattern (avoid the "tool-first trap"):
> ❌ "I'll use Kafka because it's fast."
> ✅ "Because we need durable, replayable ingestion with back-pressure, a log like Kafka fits; the cost is another system to operate."

---

## 4. Anti-patterns (my personal failure list — grow this)

- Jumping to architecture before requirements are written down.
- Naming a technology before naming the requirement that justifies it.
- Gold-plating: designing for petabytes / multi-region when the prompt implies one region and modest scale.
- Going silent while thinking — I must **narrate**; the interviewer grades reasoning, not just the final box diagram.
- Losing the timebox and never reaching the deep-dive or scale discussion.
- Ignoring cost and operational concerns.
- (Data-platform prompts) forgetting governance / schema / lineage entirely.
- _add real ones after each mock →_

---

## 5. Pre-flight checklist (read before every practice rep)

- [ ] Confirmed the medium (Google Doc text vs drawing allowed)?
- [ ] Wrote functional + non-functional requirements before any component?
- [ ] Pinned a scale number and the one primary metric?
- [ ] Defined the API/entities before the architecture?
- [ ] Drew the happy path end-to-end before optimizing?
- [ ] Labeled arrows with protocols?
- [ ] Named a storage choice with a *why*?
- [ ] Did a real deep-dive on the hard component?
- [ ] Covered failure modes + a scale bottleneck?
- [ ] Named trade-offs out loud throughout?
- [ ] Left a wrap-up with next steps?

---

## 6. Generic building-block vocabulary (the toolbox)

Keep these one-liners crisp so I can deploy without stalling:

- **Load balancer** — distributes traffic; L4 vs L7; health checks.
- **Cache** — Redis/Memcached; cache-aside vs write-through; TTL + invalidation; hot-key risk.
- **CDN** — edge caching for static/media.
- **Message queue / log** — Kafka (durable, replayable, ordered per partition) vs SQS (simple queue); decoupling + back-pressure.
- **SQL DB** — ACID, joins, secondary indexes; scale via replicas + partitioning.
- **NoSQL** — KV (DynamoDB/Cassandra), document, wide-column; pick partition key carefully to avoid hot spots.
- **Object storage** — S3/ADLS/GCS; cheap, durable, the storage half of storage/compute separation.
- **Search index** — Elasticsearch for full-text / faceted queries.
- **Sharding/partitioning** — by key/range/hash; consistent hashing to minimize reshuffles.
- **Replication** — leader-follower, quorum; sync vs async trade-off.
- **Rate limiter** — token bucket / sliding window.
- **Idempotency** — keys + dedup for exactly-once effects on at-least-once delivery.

---

## 7. Data-platform layer (pull in only if the prompt is lakehouse/data-flavored)

Thin vocabulary so a data-platform prompt doesn't catch me flat. Explain *why*, never name-drop.

- **Medallion architecture:** Bronze (raw) → Silver (cleaned/conformed) → Gold (aggregated for BI). Good default framing for an ingestion→analytics pipeline.
- **Storage/compute separation:** object store (cheap, scales on data) under elastic compute clusters (expensive, scales on load). Autoscale compute; scale to zero when idle (cost).
- **Table format / ACID over object store** (Delta/Iceberg/Hudi idea): a transaction log over Parquet files gives ACID, schema enforcement/evolution, and time travel — fixes the raw-data-lake consistency + "small file" problems.
- **Ingestion:** streaming (Kafka/Kinesis → structured streaming, checkpointing for exactly-once, **watermarking** for late data) vs batch ETL; **CDC** to sync operational DBs into the lake.
- **Lambda vs Kappa:** dual batch+speed paths vs one unified stream path. Modern default leans unified/Kappa.
- **Query performance:** partitioning by common filter keys, clustering/Z-order for data-skipping, caching hot data, materialized views for heavy aggregates. Watch **over-partitioning** → small-file problem.
- **Governance (don't forget — graded):** catalog for schema/lineage/RBAC, encryption in transit + at rest.
- **ML platform (only if asked):** feature store (offline for training w/ point-in-time correctness + online KV for low-latency serving → prevents train/serve skew); experiment tracking + model registry; batch scoring vs real-time serving endpoints (scale-to-zero).

Only reach into this box when the problem calls for it. For a "cheapest book" prompt, §1–§6 is the whole game.

---

## 8. Practice backlog (drill these; log reps in the changelog)

Product/architecture style (both flavors possible):
- Cheapest-copy-of-a-book search service (real Databricks report) — integrations, search, purchase flow.
- Rate limiter (ties to a coding round too).
- Design a metrics / monitoring pipeline.
- URL shortener (warmup for the method groove).
- Web crawler.
- Notification / fan-out service.

Data-platform style:
- Real-time ingestion pipeline (Kafka → lake, exactly-once, late data).
- Multi-tenant query engine / query routing.
- Feature store at scale (offline + online).
- CDC pipeline syncing an operational DB into analytics.

Senior distributed-systems / internals deep-dive (the *second* round — verbal, less about boxes):
- Walk through what happens when a distributed join runs over two huge tables (logical plan → physical plan → stages → shuffle → execution).
- Design a multi-tenant compute platform: tenant isolation, quotas, noisy-neighbor control, autoscaling, scale-to-zero.
- Explain an ACID-over-object-store table format's transaction log; compare Delta vs Iceberg vs Hudi.
- Consensus / replication deep-dive: leader election, quorum reads/writes, how the system survives node loss.
- Exactly-once in streaming: checkpointing, idempotent writes, watermarking, failure recovery.

> For each rep, do a full timed pass against the phased loop, then log: what phase I fumbled, what trade-off I missed, one method tweak.

---

## Open questions (iterate here)

- Confirm with recruiter (if round happens): drawing allowed, or strictly Google Doc? How many design rounds (senior often 2: system design **+** distributed-systems/Spark-internals)? Which team → how compute/data-heavy → which internals to drill?
- Is 50 min the right budget, or should I compress requirements to 5 to buy deep-dive time? (tune after first mock)
- At senior, §7's thin data-platform layer is **not enough** — decide which internals stack to master cold (compute-core Spark internals vs my own domain's distributed-systems depth). Expand into a dedicated `internals.md` once chosen.
- Prep a distributed-systems deep-dive checklist (consistency, replication, consensus, partitioning, failure recovery) as a sibling to this method — senior needs it as its own round.

## Changelog

- **v0.2 — 2026-07-07:** Retargeted to **senior (L5 / IC4)**. Added the senior bar (§0): two design-flavored rounds, internals depth expectation, more autonomy, multi-tenancy/isolation/cost-at-scale, strategic framing. Flagged need for a separate internals + distributed-systems deep-dive doc.
- **v0.1 — 2026-07-07:** Initial method drafted from sourced Databricks research. Phased loop, trade-off list, anti-patterns, checklists, generic toolbox, thin data-platform layer, practice backlog. Not yet battle-tested against a rep.

---

## Sources (as of 2026-07-07 — re-verify before the loop)

- interviewing.io — *Databricks Interview Process & Questions* (updated 2025-10-10): loop structure; system design "pretty standard, expands deeper"; cheapest-book example; **Google Docs** medium.
- systemdesignhandbook.com — *Databricks System Design Interview: The Complete Guide*: 45–60 min single open problem; clarify-first; MVP→scale; trade-off emphasis; medallion architecture; storage/compute separation; Delta/ingestion/governance details; common mistakes (ignoring cost/governance).
- ophyai.com — *Databricks Interview Process 2026*: distributed-systems + ML-platform emphasis; system design "multi-tenant data platforms / query routing"; feature-store sample; role-specific depth.
- Cross-reference with existing repo note: `algorithmics/docs/01-databricks-signal.md` (loop structure, L4 = ~1 SD round).

> Sourcing dogma: every factual claim above is from a source checked this run. Nothing here is battle-verified against an actual Databricks SD round for *my* team/level — treat team-flavor and drawing-medium as **to-confirm with recruiter**, not asserted fact.
