# G. Daily workflow, progression rules, readiness rubric, rescue plan

## 1. Daily workflow — the contract

3 hours, no exceptions, in 4 blocks (see [05-calendar.md](05-calendar.md) for context).

| Block | Time | Activity | Output artifact |
|---|---|---|---|
| 1 | 15–20 min | Review yesterday's notes + 1 template | none |
| 2 | 90–120 min | New solving, timed (Easy 10 min, Medium 25, Hard 45) | code committed to `solutions/<topic>/<id>.py` |
| 3 | 30–45 min | Write-up | `notes/<topic>/<id>.md` |
| 4 | 20–30 min | Spaced-repetition redo (no peeking) | row appended to `tracker.md` |

### Write-up template (paste into every `notes/<topic>/<id>.md`)

```
# LC <id> — <title>
- **Pattern:** <category from taxonomy>
- **Brute force:** <one line> — O(?)
- **Optimized:** <one line> — O(?)
- **Key insight:** <single sentence: what makes the optimization work>
- **Edge cases I had to handle:** <list>
- **Where I got stuck and for how long:** <minutes, root cause>
- **Template fragments I reused:** <list>
- **Would I solve this in 25 min cold next week? Y/N>
```

If you cannot fill the *Key insight* line in one sentence, you have not understood the problem and must redo.

### Spaced repetition queue

Use the cadence **D+3, D+7, D+14**. Track in `tracker.md`:

```
| LC id | first solve date | D+3 | D+7 | D+14 | status |
```

If you fail (could not solve cleanly in time budget) at D+3 or D+7, reset that schedule: schedule a fresh D+3 from the failure date.

---

## 2. Progression rules per problem

Strict rules. Stop improvising.

| Time elapsed on problem | What to do |
|---|---|
| 0–10 min | Re-read problem; clarify constraints; restate; identify pattern candidates. |
| 10–25 min (Medium) / 10–35 min (Hard) | Attempt solution. Try 2 approaches max. |
| At 25 min Medium / 35 min Hard | **Allow yourself a hint.** Look only at the *Approach name* (e.g. "Topological sort with Kahn"), not the code. Resume. |
| At 40 min Medium / 60 min Hard | **Read editorial.** Do not retry yet. |
| Immediately after reading editorial | Close it. Type the solution from memory. If you cannot, re-read, close, retype. Repeat. |
| After typing it correctly | Write the *Key insight* line. Schedule a D+3 redo. |

**Do not** spend 2 hours on one problem. That is a learning anti-pattern at this stage. The goal is breadth × depth, not heroism.

---

## 3. When to redo

Redo a problem if **any** of the following is true:
- You needed an editorial.
- You needed >25 min on Medium / >45 min on Hard.
- You wrote messy code that you would not want in code review.
- You could not state the complexity unprompted.
- You could not enumerate edge cases unprompted.
- At spaced-repetition D+3 or D+7 you needed >150% of the original time.

Redo means: **from a blank file, with timer, with editorial closed.**

---

## 4. Mistake tracking

Maintain `mistakes.md` with categories:

- **Conceptual:** "I didn't recognize this as binary-search-on-answer."
- **Template:** "I forgot to dedup in the 3Sum inner loop."
- **Implementation:** "I used `<` instead of `<=` in the histogram pop."
- **Communication:** "Didn't state complexity until prompted."
- **Edge case:** "Forgot empty input."

At the end of each week, count mistakes per category. The dominant category is your **next week's focus**.

---

## 5. Readiness rubric

Score yourself at end of Phase 3 (Jun 12), end of Phase 5 (Jun 26), and Day 43 (Jul 4).

| Dimension | 1 (not ready) | 3 (borderline) | 5 (ready) |
|---|---|---|---|
| **Pattern recognition** | I have to think 5+ min to spot the pattern | I spot the pattern in 2 min | I spot the pattern in <60 s and name an alternative |
| **Brute-force articulation** | I jump to the optimized solution and skip brute force | I mention brute force but not its complexity | I describe brute force with complexity in one sentence, unprompted |
| **Medium cold-solve time** | >35 min | 25–35 min | <25 min, clean code |
| **Hard cold-solve time** | >70 min or fails | 45–70 min with one hint | <45 min with at most one hint |
| **Code quality** | Messy, single-letter names, no helpers | Acceptable | Names are clear, small helpers, no dead branches |
| **Edge-case discipline** | I miss them until prompted | I name them after solving | I list them while planning |
| **Complexity statements** | I confuse O(n) and O(n log n) | I get them right when asked | I state them unprompted, both time and space |
| **Communication** | I go silent or panic | I narrate but pause too long | Continuous narration; I explain tradeoffs while typing |
| **Optimization-on-pushback** | I stall when interviewer says "make it faster" | I try a known optimization | I name 2 alternatives and pick one with justification |
| **Databricks-flavor coverage** | I have not done calc / snapshot / N-ary serialize / Bus Routes / LFU | I've done some | All ⚑ problems done and most ★-redone |

**Total readiness score:** sum / 50.

- **≥ 42 (84%): green.** You're ready.
- **35–41: yellow.** You will probably pass with adrenaline + 1–2 strong rounds, but a bad-luck Hard could fail you.
- **< 35: red.** Trigger the rescue plan below.

Score honestly. Faking it here only fails you in the real loop.

---

## 6. Red-zone rescue plan

Triggered if any of:
- End of Phase 3 (Day 21, Jun 12) you've solved < 45 of the planned problems.
- End of Phase 5 (Day 35, Jun 26) you score < 30 on the rubric.
- You're failing 3 consecutive D+7 spaced repetitions.

### Rescue protocol

1. **Cut everything that isn't C or ⚑.** Drop all R and S problems immediately. New target = ~88 problems (Core only).
2. **Cut concurrency entirely** unless your recruiter explicitly mentioned it.
3. **Drop one DP Hard and one backtracking Hard** (LC 312, 51). They are low-yield for Databricks.
4. **Switch problem strategy:** for every problem you have not solved yet, allow only **20 min attempt → editorial → retype**. The goal becomes *exposure to all ⚑ patterns*, not solo solves.
5. **Increase block-3 write-up time.** Understanding > volume. A well-understood editorialed solution beats an unsolved one.
6. **Double the mock interview cadence:** 2 mocks per week instead of 1. Force performance under time.
7. **Daily 10-minute "talk-through" drill:** pick a random ★ problem and explain it out loud for 5 min. Tests communication independently of solving.
8. **Last-7-day all-redo lockdown:** in the final 7 days, no new problems. Only redo of ★ + ⚑ set + mocks.

### What to *not* do in the red zone

- Do not panic-buy a course.
- Do not start a different list (Grind 169, NeetCode All).
- Do not skip writing notes "to save time" — notes are how you compound learning.
- Do not do >3.5 hours on any day; fatigue compounds against you.
- Do not skip sleep. 7 hours minimum. Sleep-deprived practice mostly hurts retention.

---

## 7. Pre-interview-day protocol (Day 44, Jul 5)

- No new problems.
- Re-read your `mistakes.md`.
- Talk-through (no coding) 3 ★ problems from 3 different categories.
- Re-read the **template section** of [03-taxonomy.md](03-taxonomy.md).
- Re-skim the **edge-case checklist** from §0 of the taxonomy.
- Sleep 8 hours.
- The morning of the interview: solve one Easy you've already done, just to warm fingers. **Not a new problem.**
