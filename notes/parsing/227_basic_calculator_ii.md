# LC 227 — Basic Calculator II

- **Pattern:** Parsing — operator stack. Scan left-to-right; **defer** additive terms onto a stack, apply **multiplicative** operators eagerly to the stack top; answer = sum of the stack.
- **Brute force:** Two passes — first sweep to resolve every `*` and `/` (collapsing runs into single values), then a second sweep to add/subtract what remains. Correct but two-pass and clumsy with the intermediate token list.
- **Optimized:** Single pass, O(n) time. Keep a running integer `nr` (digit accumulation `nr = nr*10 + c-'0'`) and the operator that **preceded** it (`sgn`, seeded `+`). On hitting the next operator (or end of string) flush `nr` by `sgn`:
  - `+` → push `nr`
  - `-` → push `-nr`
  - `*` → push `pop() * nr`
  - `/` → push `pop() / nr`

  Return `stack.Sum()`.
- **Key insight:** The stack invariant is *"every value on it is a finished additive term, so the result is always their sum."* With no parentheses there are exactly two precedence levels, so `*` and `/` only ever **reach back and mutate the top of the stack immediately**, while `+`/`-` merely **defer** a term. That's why precedence falls out for free — no explicit precedence table needed.
- **Edge cases I had to handle:**
  - Multi-digit numbers (accumulate; don't treat each digit as a token).
  - Spaces (stripped up front with `Replace(" ", "")`; alternatively `continue` on space to avoid the extra allocation).
  - Integer division **truncates toward zero** — C#'s `/` already does this, and it stays correct even when the popped term is negative (`(-a)/b == -(a/b)` under truncation).
  - **Flushing the last number** after the loop ends (no trailing operator triggers it inside the loop).
- **Where I got stuck and for how long:** Solved first try, but took **L1 + L2 hints** to get moving. L1 = "the whole difficulty is operator precedence; what lets you defer additive terms while resolving multiplicative ones eagerly?" L2 = the stack-of-finished-terms invariant + the `prevOp`-flush rules. No implementation stumbles once the structure was clear.
- **Template fragments I reused:** Digit accumulation `nr = nr*10 + (c-'0')` (from #113 atoi). The operator-stack flush transfers to the calculator family — #224 (parens → recurse/second stack), #772/#227 III (both operators + parens).
- **O(1)-space follow-up (retry idea):** The stack is unnecessary — `*`/`/` only touch the top, so track just a running `result` (sum of committed terms) + `prev` (the one term still revisable). `+`/`-` commit `prev` into `result` and set the new `prev`; `*`/`/` revise `prev` in place. Same O(n) time, O(1) space — the standard interview follow-up.
- **Would I solve this in 25 min cold next week? Y/N** — Y (needed the precedence nudge this time; the invariant is sticky now).
