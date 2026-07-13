# Databricks — Full Loop: Process Map (v0.1, working draft)

**Purpose.** A factual reconstruction of the Databricks onsite loop as described by (a) the recruiter's prep email and (b) your own recorded debrief of the call with Alex, plus the substantive links contained in the email. This is the reference we iterate on to restructure prep.

**Status.** Draft for iteration. Every factual claim below is tagged with a source marker `[S#]` resolved in the [Appendix](#appendix--sources). Anything I could not confirm from a source is placed under [Open questions](#open-questions-not-answered-by-the-sources) or [Assumptions](#assumptions-explicitly-flagged) rather than stated as fact.

> **Sourcing rule for this doc.** Nothing here is asserted from memory. Round mechanics come from Alex's email `[S1]` and your notes `[S2]`; the rate-limiter sample from `[S3]`; culture principles from `[S4]`; general SD-interview framing from `[S5]/[S6]`. Where `[S1]` and `[S2]` overlap they agree; where only one covers a point, it's marked with that single source.

---

## 1. Snapshot

| Item | Detail | Source |
|---|---|---|
| Total onsite interviews | **4 interviews over 2 days** | `[S2]` |
| Round types | Algorithms · Coding · Systems Programming · Conversational/Behavioral | `[S1]`, `[S2]` |
| Role / level | **L4**, described as **"lvl 63 equivalent MSFT"** | `[S2]` |
| Team | **Not targeting a specific team** | `[S2]` |
| References | **3 people you worked with previously** must vouch for you | `[S2]` |
| Coding tool | **CoderPad** (named for Systems Programming, Algorithms, Coding) | `[S1]` |
| Prior stage implied | Algorithms round is *"same format as the technical screen"* → a technical screen preceded this onsite | `[S1]` |

**Mapping the 4 interviews to the 4 email round-descriptions.** The email `[S1]` describes exactly four round *types* (Systems Programming, Algorithms, Coding, Conversational/Behavioral); your notes `[S2]` say there are four interviews (1 algo, 1 heavier-coding, 1 systems-programming, 1 behavioral with a manager). These line up 1:1, so the working assumption is **one of each** — see [Assumptions](#assumptions-explicitly-flagged) A1.

---

## 2. Logistics & ground truth

- **Structure:** 4 interviews, spread across **2 days** `[S2]`.
- **Level:** **L4 / "lvl 63 equivalent MSFT"** `[S2]`. Captured verbatim; I have not independently mapped Databricks L4 to a Microsoft band beyond what you wrote.
- **Team:** none specified — **generalist loop, not team-matched** `[S2]`.
- **References:** you need **3 former colleagues** to vouch `[S2]`.
- **Tooling:** **CoderPad** for the technical rounds `[S1]`. Note: the email does **not** name a diagramming/design tool (this was your Question 5 to Alex — see [Open questions](#open-questions-not-answered-by-the-sources)).

---

## 3. The four rounds

### 3.1 Algorithms (`CoderPad`)

- **What it is:** one question on classic **data structures & algorithms plus some math concepts**; *"same format as the technical screen."* `[S1]`
- **Code must actually compile and run** — you're evaluated on the real implementation, analysis, and communication. `[S1]`
- **They want:** appropriate **test cases** and clean handling of **corner/edge cases**; brief discussion of approach *before* coding; explanation of decisions; readiness for follow-ups on **time complexity / run-time performance**; responsiveness to hints. `[S1]`
- **Language:** agnostic — use what suits you/the problem. `[S1]`
- **Prep topics named:** hash maps, strings, binary search, graphs, arrays, sorting. `[S1]`
- **From your notes:** this is the **"1 algo — leetcode based"** round. `[S2]`

### 3.2 Coding (`CoderPad`)

- **What it is:** evaluates coding, implementation, and **debugging** in a "typical everyday working environment." `[S1]`
- **Emphasis vs Algorithms:** **more implementation-focused**, still needs *some* algorithms knowledge. **First priority: a complete solution**, then debugging and testing edge cases. `[S1]`
- **They look at:** how you implement components/classes/methods/functions, **coding style**, and **test cases**; clean, bug-free, well-tested code. `[S1]`
- **From your notes:** *"1 more coding — less algorithm heavy → more coding; code quality; how to ship code; how to test code; lighter on algo, still there but lighter."* `[S2]` — consistent with `[S1]`.

### 3.3 Systems Programming (`CoderPad`) — the concurrency round

This is the round that most changes our prep, and the one the old large-scale system-design curriculum does **not** directly target (see [Observations](#4-observations--implications-for-restructuring)).

- **What it is:** a **single-computer / single-machine** systems-programming problem — the email's explicit example is **"design and implement a single-machine cache."** `[S1]` Your notes call it a **"system programming interview — mix of coding and system design."** `[S2]`
- **Scope is explicitly NOT large-scale.** `[S2]` The question is *"usually on a single machine"* and *"likely a general question, not necessarily related to your technical background or to Databricks' core products."* `[S1]`
- **Deliverable:** an **actual pseudocode implementation** that is *reliable, fast, scalable* and meets stated requirements. **Code need not compile** and is **language-agnostic**, but must be **specific enough to discuss trade-offs, techniques, performance, and design.** `[S1]`, `[S2]`
- **Requirements you must reason about (email's list):** performance; **concurrency**; **threading vs multi-threading**; **order-dependent processing**; throughput; latency; **data durability**; **synchronization**; **network and disk I/O**. `[S1]`
- **Design/quality expectations:** class structuring, **strong cohesion & weak coupling**, **separation of concerns**, **single-responsibility principle**. `[S1]`
- **Failure & environment reasoning:** you must think about how things **fail under concurrency and environmental conditions** and how to handle them; the interviewer *may add requirements mid-interview*, e.g. **"how would you handle losing power on a regular basis."** `[S1]` Your notes echo: *"what if high load / what if power goes down / concurrency primitives → focus on concurrency."* `[S2]`
- **Your notes add concrete sub-topics:** *single-machine cache behaviour, memory, synchronization.* `[S2]`
- **You drive:** proactively explain and rationalize; listen to hints and steer accordingly. `[S1]`
- **Sample question named in the email:** a **rate limiter** `[S1]` (details in `[S3]`, summarized below). The email also notes **"concurrency and multi-threading questions on LeetCode can be helpful."** `[S1]`

**Rate-limiter sample — reference points `[S3]`** (linked from the email as the sample design question):
- Enforce **server-side**; track a **counter per user** and reject when the limit is hit; a hash-table `UserId -> {counter, startTime}` is the core structure; an in-memory store (e.g. Redis with `INCR` + `EXPIRE`) is suggested for speed and time-based expiry. `[S3]`
- Four standard algorithms: **Token Bucket**, **Leaky Bucket**, **Sliding Window Logs**, **Sliding Window Counters** (the last is an optimization of the logs approach to cut memory). `[S3]`
- *Note:* `[S3]` is written as a **distributed/API** rate limiter; for this round, translate it to a **single-machine, thread-safe** counter (synchronization, atomics/locks) — see [Observations](#4-observations--implications-for-restructuring).

### 3.4 Conversational / Behavioral — "Cross-Functional" (1 hour)

- **Who:** an **engineering manager** at Databricks. `[S1]`, `[S2]`
- **Focus:** how you operate in a working environment, interest in Databricks, **career growth/progression**, and your technical background. `[S1]`
- **Method:** the email explicitly says **"Read about the STAR method and use it."** `[S1]`
- **Communication:** clear, concise, structured answers; be conscious of audience and clarify jargon. `[S1]`
- **Topic areas (email):** `[S1]`
  - **Collaboration** — conflict resolution, mentorship of junior engineers, managing up, cross-functional work. Sample: *"a disagreement with a teammate and how you solved it"*; *"execute on a project with varying stakeholder priorities."*
  - **Ownership & Impact** — identify/solve problems, decision-making, end-to-end ownership. Sample: *"describe a project end to end — who was involved, how did you measure success"*; *"a project you're leading is set to miss a deadline — how do you course-correct."*
  - **Growth/Reflection** — learning from things that went wrong, giving/receiving feedback, seeking mentors, learning preferences (breadth vs deep mastery). Sample: *"something that didn't go as planned — what did you learn / do differently"*; *"a time you took feedback and applied it."*
  - **Technical Background** — explain your background and fit with Databricks; expect to discuss recent/impactful projects.
  - **Career** — overall history, e.g. why you left a company.
  - **Interest in Databricks** — be ready to answer *why Databricks.*
- **From your notes (topics to prepare):** career growth; learning mindset; conflict resolution (handling a **misunderstanding** and a **technical dispute**); what you'd do if a **project will miss its deadline**; **a time you received feedback and used it to improve.** `[S2]` — all consistent with `[S1]`.
- **Culture:** the email links Databricks' culture principles and says to work these themes into answers. `[S1]` The six principles are in [Section 5](#5-databricks-culture-principles-for-the-behavioral-round).

---

## 4. Observations & implications for restructuring

These are analytical observations grounded in the sources, not new facts — but they matter for re-planning.

1. **The design element is single-machine, not large-scale distributed.** Alex's Systems Programming round is explicitly single-machine (cache, concurrency, synchronization, power-loss) `[S1]`, `[S2]`. The **loop as described contains no large-scale distributed-system or data-platform design round.** Consequently, most of the existing `system_design/` curriculum (replication, partitioning, CDNs, lakehouse, etc.) is **background/safety depth, not the main event.** The main event is **single-machine concurrency & systems programming.**
2. **The email's generic prep links point at large-scale design and don't match the actual round.** `[S5]` (Meta system-design) and `[S6]` ("How NOT to design Netflix in 45 min") are about **45-minute large-scale distributed design** (design Uber/Netflix, high-level components). That is a **different format** from Databricks' single-machine systems-programming round `[S1]`. Their *transferable* advice still applies (ask clarifying questions; don't rush; don't bluff; discuss trade-offs) `[S5]`, `[S6]`, but do **not** treat "design Netflix in 45 min" as representative of the Databricks loop.
3. **The rate-limiter sample must be re-cast for a single machine.** `[S3]` is a distributed/API rate limiter; for this round the emphasis shifts to **thread-safety, atomics/locks, memory, and failure-under-concurrency** rather than Redis/sharding `[S1]`.
4. **Two of four rounds are pure coding** (Algorithms + Coding) `[S1]`, `[S2]` — so LeetCode-style DS&A **and** clean, tested, complete implementation both carry weight; the Coding round rewards *finished, debugged, well-tested* code over cleverness `[S1]`.
5. **Concurrency shows up twice**: as the core of Systems Programming, and as a suggested LeetCode topic `[S1]`, `[S2]`. Concurrency primitives are a high-leverage study target.
6. **Behavioral is STAR-driven and culture-aware** `[S1]` — prepared STAR stories mapped to the six culture principles and the named sample prompts is directly actionable.

---

## 5. Databricks culture principles (for the behavioral round)

The six principles as published on the Databricks culture page `[S4]`:

1. **We are customer obsessed** — the customer is at the center of everything; what's best for the customer is best for Databricks.
2. **We raise the bar** — every day is a chance to do better, as individuals and as a company.
3. **We are truth seeking** — decisions based on data; adapt as it changes.
4. **We operate from first principles** — start with *why*; first-principles thinking drives innovation.
5. **We bias for action** — speed matters; debate, plan, execute, iterate with urgency.
6. **We put the company first** — do what's best for the company overall.

> Actionable: prepare STAR stories that naturally evidence several of these (e.g. a truth-seeking/data-driven decision; a first-principles redesign; a bias-for-action delivery under deadline pressure — which also answers the "missing deadline" prompt `[S1]`, `[S2]`).

---

## 6. General tips from the email `[S1]`

Condensed from the email's 16-point list (all `[S1]`):

- **Ask clarifying questions** before diving in; avoid assumptions; gather requirements on deliberately open-ended problems.
- **Think out loud**; verbalize your process throughout the day.
- **Be mindful of bugs; use test-driven development.**
- Consider **implementability, maintainability, operability**; discuss **trade-offs** and justify technology choices.
- **Use hints** when given; iterate and refine toward a reasonable end state.
- Be ready to discuss **career motivations, passions, relevant research.**
- **Research the panel** (e.g. their GitHub); watch the Databricks YouTube channel; **try the product** (free); **analyze the codebases** named — **Delta Lake, Koalas, and "DataFlow"** — to reflect coding culture and share findings.
- Recommended reading: **Cracking the Coding Interview (CTCI)** and **"Programming Pearls" (Jon Bentley)**; practice away from the keyboard (whiteboard/notebook).

*(Codebase names captured verbatim from the email `[S1]`; I have not independently verified their current repository status.)*

---

## 7. Open questions (not answered by the sources)

These were **your own questions to Alex** `[S2]`; the sources don't clearly answer them, so they remain open:

- **Q2 — Design style & Spark internals:** whether the design element leans "product/web service" vs "data pipeline/platform," and whether there's a deep dive into e.g. Apache Spark internals. *Partially addressed:* the Systems Programming round is single-machine and *"not necessarily related to Databricks' core products"* `[S1]`, which implies **no Spark-internals deep dive** and **no data-platform design round** — but there is no explicit statement confirming this. **Confirm with Alex.**
- **Q5 — Design tool:** the email names **CoderPad** for the coding rounds `[S1]` but does **not** state what you'd *diagram/design* in (Google Doc vs drawing tool vs interviewer's choice). **Confirm whether the Systems Programming round expects any diagramming and in what tool.**
- **Q6 — The bar:** how a strong candidate is separated from a weak one. **Not captured in the sources.** `[S1]` gives indirect signals (complete + tested code; drive; trade-off reasoning; communication) but no explicit bar. **Ask Alex directly.**
- **Q1 count confirmation:** confirm there is exactly **one** of each round type (see Assumption A1).

---

## 8. Assumptions (explicitly flagged)

- **A1 — One of each round.** The email lists four round *types* `[S1]`; your notes say four interviews `[S2]`. I've assumed a **1:1 mapping (one Algorithms, one Coding, one Systems Programming, one Behavioral)**. Not explicitly stated that there's exactly one of each — **verify.**
- **A2 — CoderPad for all three technical rounds.** The email attaches "Tooling: CoderPad" to Systems Programming, Algorithms, and Coding individually `[S1]`; I've treated that as covering all technical rounds. The behavioral round's tool/medium isn't stated.
- **A3 — "Technical screen already happened."** Inferred from *"Algorithms … same format as the technical screen"* `[S1]`. Assumed you've passed a prior screen; not independently confirmed here.
- **A4 — Level mapping.** "L4 / lvl 63 equivalent MSFT" is captured **verbatim from your notes** `[S2]`; I have **not** independently verified the Databricks↔Microsoft level mapping.

---

## Appendix — Sources

All web pages fetched **2026-07-13**. Local files read **2026-07-13**.

- **[S1] Recruiter prep email** — *"Databricks (Full loop interview prep)"*, from Alex Verkunich (Sr. Technical Recruiter), dated **Mon, 13 Jul 2026**. Local file: `loop_details/Databricks (Full loop interview prep).eml`. Source of all four round descriptions, tooling (CoderPad), the rate-limiter sample reference, the general-tips list, and the culture-principles link.
- **[S2] Your recorded call notes** — Local file: `loop_details/alex_questions.txt`. Source of: 4 interviews / 2 days; L4 ("lvl 63 equivalent MSFT"); no specific team; 3 references; the per-round breakdown in your words (algo leetcode-based; lighter-algo coding; single-machine concurrency systems-programming; behavioral with a manager); and your six questions to Alex.
- **[S3] GeeksforGeeks — "How to Design a Rate Limiter API | Learn System Design"** — linked from `[S1]` as the sample design question. Page "Last Updated: 28 Mar 2026." URL: https://www.geeksforgeeks.org/how-to-design-a-rate-limiter-api-learn-system-design/ . Used for the rate-limiter algorithms and counter structure. *(Framed as distributed/API; re-cast to single-machine for this loop.)*
- **[S4] Databricks — "How we work" / culture principles** — © Databricks 2026. URL: https://www.databricks.com/company/careers/culture . Source of the six culture principles.
- **[S5] HackerNoon — "I Led Dozens of Meta System Design Interviews…" (a.k.a. "Anatomy of a System Design Interview")**, Fahim ul Haq, published 2024-08-05. Linked from `[S1]`. URL: https://hackernoon.com/anatomy-of-a-system-design-interview-4cb57d75a53 . **General/Meta large-scale design framing — transferable advice only; not representative of the Databricks single-machine round.**
- **[S6] HackerNoon — "How NOT to design Netflix in your 45-minute System Design Interview?"**, Fahim ul Haq, published 2017-02-26. Linked from `[S1]`. URL: https://hackernoon.com/how-not-to-design-netflix-in-your-45-minute-system-design-interview-64953391a054 . **Large-scale design pitfalls (ask questions, don't rush, don't bluff, discuss trade-offs) — transferable, not format-matching.**

### Links in the email NOT independently reviewed for this doc
Listed for completeness; their content is **not** cited above and should be treated as **not independently verified** here:
- **Pramp — "How to Succeed in a System Design Interview"** (https://blog.pramp.com/how-to-succeed-in-a-system-design-interview-27b35de0df26) — **fetch failed to extract content**; not reviewed.
- **Educative — "Grokking the System Design Interview"** (https://www.educative.io/courses/grokking-the-system-design-interview) — course/paywall; not reviewed.
- Generic resource links in the email: LeetCode, GeeksforGeeks (home), InfoWorld "best open source software," and Databricks Home / Careers / Blog / Demo / YouTube / try-databricks — not individually reviewed.

---

*Next step: iterate on this map, confirm the open questions with Alex, then rebuild the study plan around the single-machine Systems Programming (concurrency) round + two coding rounds + STAR behavioral, keeping the existing large-scale `system_design/` notes as secondary depth.*
