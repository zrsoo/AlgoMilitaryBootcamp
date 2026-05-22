# Databricks L4 Coding Interview — 44-Day Bootcamp

**Window:** May 23, 2026 → July 5, 2026 (44 days × 3h = 132 h)
**Target:** Pass Databricks L4 SWE coding rounds (LC Medium / Hard band)

This is not a generic FAANG grind. It is engineered around what Databricks actually asks. Read the files in order the first time, then use them as reference.

## File index

| # | File | Purpose |
|---|---|---|
| A | [Executive summary](00-README.md#executive-summary) | TL;DR + how to use this plan |
| B | [Databricks interview signal](01-databricks-signal.md) | What rounds, what topics, source-ranked |
| C | [Curated list evaluation](02-list-evaluation.md) | Blind 75 vs NeetCode 150 vs Grind 169 vs Databricks-tagged |
| D | [Algorithm taxonomy](03-taxonomy.md) | Patterns, templates, traps, recognition cues |
| E | [Final problem set](04-problem-set.md) | 122 problems, grouped, labeled Core / Reinforcement / Stretch |
| F | [44-day calendar](05-calendar.md) | Day-by-day plan, phases, mocks, post-mortems |
| G | [Daily workflow + rubric + rescue](06-workflow-rubric-rescue.md) | Time blocks, progression rules, readiness scoring, red-zone plan |
| H | [Language guidance](07-language-guidance.md) | Python vs Java vs C++ vs Scala, traps |
| I | [Final advice](08-final-advice.md) | What to ignore, what to drill hard |

---

## Executive summary

**The brutal version.** A Databricks L4 coding loop is a graphs-heavy, parsing-heavy, design-heavy LC Medium/Hard environment with one occasional concurrency / "implement-a-system" twist. The bar is not raw difficulty (it is not Google L5). The bar is **clean implementation, articulated tradeoffs, and not freezing on the Hard**. Roughly 50–60% of your loop is coding; the rest is system design + behavioral. This plan only covers coding.

**Concrete signals that shape this plan** (cited in [01](01-databricks-signal.md)):
- OA = 70 min, 4 problems, 2 easy + 2 medium-hard.
- Tech screen = 1 LC Medium/Hard, 1 hour, often graphs / strings / concurrency.
- Onsite coding rounds (2–3 of them) are biased toward: graphs / BFS variants, calculators & expression parsing, tree serialization, interval / scheduling, design-style coding (snapshot, iterator, file system, LFU/LRU), and CIDR / IP problems.
- Concurrency comes up but is not guaranteed; treat as a stretch topic, not a core one.
- Pass rate at OA ~30%, tech screen ~20% — clean code and complexity discipline matter more than exotic algorithms.

**Strategy in one paragraph.** Stop chasing problem counts. Build a tight 122-problem set mapped to the patterns Databricks actually fires, with a deliberate 25% Hard ratio so you do not get blindsided. Spend ~60% of your hours on patterns Databricks loves (graphs, parsing, intervals, design), ~25% on universal coverage (DP, sliding window, heap, two pointers), ~10% on Databricks-flavor specials (CIDR, calculators, file system, snapshot), and ~5% on a thin concurrency layer. Three hours/day = 90 min new problem + 30–45 min write-up + 30–45 min spaced-repetition redo + 15 min review. Mock interview every Saturday. Final 9 days = pure simulation + targeted redo.

**Default language: Python.** Falls back to Java if you already know it cold; do not switch languages now. See [07](07-language-guidance.md).

**Readiness target by July 5:** can solve a *cold* LC Medium in ≤25 min, a *cold* LC Hard in ≤45 min with one hint at most, in any of the 18 pattern categories in [03](03-taxonomy.md), while explaining tradeoffs and edge cases unprompted. If you cannot, follow the rescue plan in [06](06-workflow-rubric-rescue.md).

**What this plan refuses to do:**
- Recommend Blind 75 alone. It under-covers Databricks design + graph BFS variants.
- Recommend grinding NeetCode 150 in full. It over-invests in easy reps you do not need at L4.
- Pretend concurrency is core. The evidence is mixed; do not burn 20 hours on it.
- Give you motivational filler. Every line in these docs is supposed to change what you do tomorrow.
