# 01 — Foundations

This lesson assumes you know **nothing** about how large software systems are built, and explains every term from the ground up. It covers the two things every later lesson depends on:

1. **How to reason about numbers** — how fast is fast, how big is big, and how to estimate the size of a system on the back of an envelope.
2. **The consistency/availability trade-offs** — the deep reason you can't just "make everything correct and always-on," and how to choose.

Take your time with it. Everything else in the curriculum is an application of these ideas.

---

## Part 0 — The absolute basics (vocabulary you need first)

Before any of the interesting ideas, we need a shared vocabulary. Skip nothing here.

**Server / machine / node / box / instance.** These words all mean roughly the same thing: **one computer that runs your software**. It has a CPU (does the computing), RAM (fast temporary memory), and a disk (slow permanent storage). In system design, people say "a box" or "a node" casually to mean "one such computer." When we say a service "runs on a server," we mean your program is running on one of these computers, waiting for requests and answering them.

**Client and server.** A **client** is whatever sends a request (a phone app, a browser, another program). A **server** is the computer that receives the request and sends back a response. The typical shape:

```
  Client (your phone)                 Server (a computer in a datacenter)
        |                                        |
        |  ---- "give me user 42's profile" ---> |
        |                                        |  (looks it up)
        |  <------ "here is the profile" ------- |
        |                                        |
```

**Request / response.** One round of "client asks, server answers." A big system might handle millions of these per second.

**A read vs a write.** Two kinds of operations on data:
- A **read** = "tell me something" (fetch a profile, load a page). It does **not** change stored data.
- A **write** = "change something" (post a comment, place an order, update a setting). It **does** change stored data.
This distinction matters enormously, because reads are easy to scale (you can copy data and let many computers answer reads) while writes are hard (everyone has to agree on the new value).

**Latency.** How **long one operation takes**, from start to finish — a *duration*. "This request has 200 ms latency" means it took 200 milliseconds to get an answer. Lower is better. Think: *how long do I wait?*

**Throughput.** How **many operations you can do per unit time** — a *rate*. "This system has a throughput of 50,000 requests per second." Higher is better. Think: *how much can I get through?*
> Latency and throughput are different axes. A checkout line: latency = how long *one* customer waits; throughput = how many customers get served *per hour*. You can improve one and hurt the other (batching customers into groups raises throughput but each waits longer).

**QPS (Queries Per Second).** The most common throughput unit in system design: how many requests hit your system each second. "100K QPS" = 100,000 requests every second. (You'll also see RPS, requests per second — same idea.)

**Datacenter.** A building full of thousands of these server computers, wired together with very fast local networking. Cloud providers (AWS, Azure, Google Cloud) own many datacenters.

**Region.** A geographic location containing one or more datacenters (e.g. "US-East," "Europe-West"). Two servers in the *same* region can talk to each other in well under a millisecond. Two servers in *different* regions (say Virginia and Frankfurt) are separated by thousands of kilometers of cable, so talking between them takes ~100+ milliseconds — an eternity in computing. This geography will drive many decisions.

---

## Part 1 — The three ways to grow a system: single box, fleet, sharded

When your system is tiny, **one computer** runs everything. As load grows, one computer stops being enough, and you have exactly three structural moves. You must know these cold because estimates constantly conclude "…so this can't be a single box, it needs to be sharded," and that sentence has to mean something concrete to you.

### 1a. Single box (one computer)

Everything — your program and its data — lives on **one machine**.

```
        ┌──────────────────────────┐
        │        ONE SERVER        │
        │   your app + the data    │
        └──────────────────────────┘
                    ▲
                    │  all requests
                 clients
```

- **Pro:** simplest possible thing. No coordination, no network between parts, easy to reason about. There is only one copy of the data, so it's automatically consistent.
- **Con:** it has a ceiling. A single machine has a maximum CPU, maximum RAM (say a few hundred GB to a few TB), and maximum disk. When traffic or data exceeds what one machine can hold or handle, you're stuck. Also, if that one machine dies, your whole system is down (a **single point of failure** — one component whose failure takes everything down).

**Scaling a single box is called *vertical scaling* (scaling up):** buy a bigger machine — more CPU, more RAM. It works until you hit the biggest machine money can buy, and it does nothing for the "one machine dies = everything down" problem. It's the first tool, not the last.

### 1b. Fleet (many identical computers behind a load balancer)

Instead of one big machine, run **many identical copies of your program** on many machines, and put a **load balancer** in front to spread requests across them.

A **load balancer** is a component (often itself a specialized server) whose only job is: a request comes in, it picks one of the backend machines and forwards the request there. It "balances the load" so no single machine is overwhelmed.

```
                         ┌─────────────────┐
        clients  ──────▶ │  LOAD BALANCER  │
                         └───────┬─────────┘
                 ┌───────────────┼───────────────┐
                 ▼               ▼               ▼
          ┌───────────┐   ┌───────────┐   ┌───────────┐
          │  app copy │   │  app copy │   │  app copy │   ← identical
          │    #1     │   │    #2     │   │    #3     │      "app servers"
          └───────────┘   └───────────┘   └───────────┘
                 └───────────────┼───────────────┘
                                 ▼
                        ┌─────────────────┐
                        │    the database  │   ← still shared
                        └─────────────────┘
```

- **Pro:** now you can handle far more requests — need double the capacity? Add more machines. This is called **horizontal scaling (scaling out):** add more machines rather than a bigger machine. It also removes the app tier as a single point of failure: if app copy #2 dies, the load balancer just stops sending it traffic and the others carry on.
- **Key requirement:** for this to work, each app copy must be **stateless** — meaning it keeps no important data of its own between requests; it fetches everything it needs from a shared database or cache. That way any copy can handle any request, and they're interchangeable. (If a copy remembered things only it knew, you couldn't freely route the next request to a different copy.)
- **The catch:** notice the database is *still shared and still one thing* in the diagram above. A fleet solves "too many requests for the app to compute," but all those app copies still read and write the same database. Eventually **the database itself** becomes the bottleneck — too much data to fit, or too many reads/writes for one DB machine. That's when you need the third move.

### 1c. Sharded (the data itself is split across machines)

**Sharding** (also called **partitioning**) means **splitting your data into pieces and putting each piece on a different machine**. Each piece is called a **shard** (or partition). No single machine holds all the data; each holds a slice.

Example: users A–H live on shard 1, I–P on shard 2, Q–Z on shard 3.

```
        ┌───────────┐   ┌───────────┐   ┌───────────┐
        │  SHARD 1  │   │  SHARD 2  │   │  SHARD 3  │
        │ users A–H │   │ users I–P │   │ users Q–Z │
        └───────────┘   └───────────┘   └───────────┘
   "give me user 'Maria'"  → hash/route → goes to SHARD 2 only
```

- **Pro:** now there is no single-machine data ceiling. 90 TB of data won't fit on one machine? Split it into 30 shards of 3 TB each. Each shard also handles only its slice of the read/write traffic, so write load spreads out too.
- **Con — this is where real complexity enters:** the data is now spread out, so:
  - You need a rule to decide *which shard* holds a given piece of data (e.g. "hash the user id"), and every request must route to the right shard.
  - Operations that span shards get hard. "Count all users" now has to ask every shard and combine results. A transaction that touches two users on two different shards is genuinely difficult (there's no single machine that can lock both atomically).
  - Some shards can get hotter than others (if one celebrity user gets all the traffic — a **hot shard / hot partition**).
- **Replication vs sharding — don't confuse them:**
  - **Replication** = keeping **full copies** of the *same* data on multiple machines (for reliability and for scaling reads). Every replica has everything.
  - **Sharding** = splitting **different** data onto different machines (for scaling size and writes). Each shard has a slice.
  - Real systems do **both**: shard the data into slices, and replicate each slice a few times so no slice is lost if a machine dies. (Replication gets its own lesson, note 03.)

**So the mental ladder is:** one box → make it bigger (vertical) → put many app copies behind a load balancer (fleet / horizontal) → when the data outgrows one DB, split the data across machines (sharding), replicating each piece for safety.

Now when an estimate ends in "…so it can't be a single box, it needs to be sharded," you know *exactly* what that sentence is claiming and what it costs.

---

## Part 2 — Reasoning about numbers, part 1: the latency hierarchy

You cannot design what you cannot size. The first sizing skill is knowing **how long different operations take**, because these times differ by factors of *millions*, and that dictates where data should live.

### The units (so the table means something)

Time gets small fast. Each step down is 1000× smaller:
- **1 second (s)**
- **1 millisecond (ms)** = one thousandth of a second (0.001 s). A blink is ~100–400 ms.
- **1 microsecond (µs)** = one millionth of a second (0.000001 s).
- **1 nanosecond (ns)** = one billionth of a second. Light travels about 30 cm (one foot) in a nanosecond.

### The pieces of hardware (what's actually being timed)

- **CPU cache (L1/L2/L3).** Tiny, extremely fast memory *inside the CPU chip* itself. L1 is the smallest and fastest. The CPU keeps the data it's actively using here.
- **RAM (main memory).** The computer's fast working memory — gigabytes of it. Fast to access, but **volatile**: everything in RAM is *lost when the power goes off*. This is where a running program keeps its live data.
- **SSD (Solid-State Drive).** Fast permanent storage (a flash-memory disk). Slower than RAM, but data **survives a reboot / power loss** (it's *persistent* / *durable*).
- **HDD (Hard Disk Drive).** Older permanent storage with a physically spinning platter and a moving head. Much slower than SSD, especially for random access, because a physical arm has to move. Being phased out for hot data, still used for cheap bulk storage.
- **Network round trip.** The time for a message to travel from one computer to another *and back*. Within a datacenter it's short; across the planet it's long, because it's limited by the speed of light through cable plus routing.

### The hierarchy (memorize the *ratios*, not the exact numbers)

The absolute numbers change with new hardware, but the **ratios between them barely change** — and the ratios are what drive decisions.

| Operation | Approx. time | How it compares |
|---|---|---|
| Read from **L1 CPU cache** | ~1 ns | the baseline: "instant" |
| Read from **RAM** | ~100 ns | ~**100× slower** than L1 cache |
| Read 1 MB sequentially from **RAM** | ~10 µs | — |
| **SSD** random read | ~100 µs (100,000 ns) | ~**1000× slower** than RAM |
| Read 1 MB from **SSD** | ~1 ms | — |
| **Network** round trip *inside one datacenter* | ~0.5 ms | about the same order as an SSD read |
| Read 1 MB from **HDD** (spinning disk) | ~20 ms | ~**20× slower** than SSD; avoid on hot paths |
| **Network** round trip *across continents* | ~150 ms | geography dominates everything |

Read that top to bottom as a cliff: going from CPU cache → RAM → SSD → cross-continent network, each big step is roughly **100–1000× slower** than the one above.

### What this actually forces you to do

Because the differences are so enormous, a few rules fall out and you'll apply them in almost every design:

1. **Keep hot data in RAM (cache it).** RAM is ~1000× faster than SSD and vastly faster than a network hop to a database. So the data you touch most often should be held in fast memory close to the computation, not fetched from disk or another machine every time. This is the entire reason **caches** exist (whole lesson on this: note 04).
2. **Colocate compute and data.** "Colocate" = put them in the same place (same datacenter/region). A cross-continent round trip is ~150 ms *by itself* — that alone busts most latency targets. So you keep the computation physically near the data it uses, and you **replicate data near users** who are far away rather than making them reach across the planet.
3. **Avoid chatty cross-region calls on the hot path.** The "hot path" is the code that runs on every user request (as opposed to rare background work). If serving one request requires 10 sequential cross-region calls, that's 10 × 150 ms = 1.5 seconds of pure waiting. Batch the calls, or don't cross regions at all on the hot path.
4. **Sequential access beats random access, especially on disk.** Reading data that's laid out in order is much faster than jumping around, because disks (even SSDs) and memory prefetching are optimized for sequential runs. This is *why* many databases are built to turn random writes into sequential ones (you'll meet "LSM trees" in note 02 — this is their whole trick).

---

## Part 3 — Reasoning about numbers, part 2: back-of-envelope estimation

"Back-of-envelope" estimation means doing **rough math, in your head or on scratch paper, to size a system** — could this run on one machine, or does it need a fleet, or must it be sharded? The goal is **not precision**. It's to land in the right *order of magnitude* so you can make structural decisions. You round aggressively and say your assumptions out loud.

### The three numbers worth memorizing

1. **Seconds in a day ≈ 86,400, which we round to ≈ 100,000 = 10⁵.**
   Why this is the single handiest constant: if you know how many events happen *per day*, dividing by 10⁵ gives you *events per second* (roughly QPS). Rounding 86,400 up to 100,000 keeps the mental math trivial and errs on the safe (slightly higher-load) side.

2. **Powers of two, for data sizes.** Computers measure storage in powers of 2, but they line up almost exactly with the "thousands" you already know:
   - 2¹⁰ = 1,024 ≈ **1 thousand** → a **KB** (kilobyte) is ~1000 bytes
   - 2²⁰ ≈ **1 million** → an **MB** (megabyte) is ~1000 KB
   - 2³⁰ ≈ **1 billion** → a **GB** (gigabyte) is ~1000 MB
   - then **TB** (terabyte) ≈ 1000 GB, and **PB** (petabyte) ≈ 1000 TB.
   So each step up — KB → MB → GB → TB → PB — is about **×1000**. A single machine today holds up to a few TB of RAM-ish scale and can *store* many TB on disk; once you're talking hundreds of TB or PB, you're firmly in "must be sharded" territory.

3. **Peak ≈ average × 2 to 5.** Traffic is never flat — there are busy hours and quiet hours. Whatever *average* rate you compute, the *peak* (the worst moment you must survive) is a few times higher. A common rule of thumb is to multiply the average by 2–5 to get a peak you should design for. (If you know the traffic is very spiky — flash sales, a sports event — use a bigger multiplier.)

### Worked example — sizing a URL shortener (done slowly, like a proof)

A **URL shortener** is a service like bit.ly: you give it a long link, it stores that link and gives you back a short code (e.g. `bit.ly/xY7g`); later, when someone visits the short code, it looks up the original long link and redirects them there. So:
- Creating a short link = a **write** (you're storing new data).
- Visiting a short link = a **read** (you're looking up existing data).

Suppose the requirement is: **100 million (100M) new short links created per day.** Let's derive the architecture purely from numbers. We'll establish four things: write rate, read rate, total storage, and from those, the shape of the system.

**Step 1 — Write rate (QPS of creating links).**
We create 100M links per day. To turn "per day" into "per second," divide by seconds-in-a-day, which we memorized as ≈ 10⁵:

$$
\text{writes/sec} = \frac{100{,}000{,}000 \text{ writes/day}}{100{,}000 \text{ sec/day}} = \frac{10^8}{10^5} = 10^3 = 1{,}000 \text{ writes/sec (average).}
$$

Let me spell out that arithmetic so nothing is hidden: 100 million is $10^8$ (1 followed by 8 zeros). Seconds per day is $10^5$. Dividing powers of ten means subtracting the exponents: $10^{8} \div 10^{5} = 10^{8-5} = 10^{3} = 1{,}000$. So on an *average* second, about **1,000 create-requests** arrive.

Now apply the peak rule (×2–5). Taking ×5 to be safe:

$$
\text{peak writes/sec} \approx 1{,}000 \times 5 = 5{,}000 \text{ writes/sec.}
$$

**Interpretation:** 1,000–5,000 writes per second is a *moderate* write load. A single well-tuned database machine can often handle a few thousand writes per second. So writes alone do **not** yet force us to shard. Keep going.

**Step 2 — Read rate (QPS of visiting links).**
People *visit* short links far more often than they *create* them — a link is made once and then clicked many times. We don't know the exact ratio, so we **state an assumption**: assume **100 reads for every 1 write** (a 100:1 read:write ratio — reasonable for a link that gets shared and clicked repeatedly). Then:

$$
\text{reads/sec (average)} = 1{,}000 \text{ writes/sec} \times 100 = 100{,}000 \text{ reads/sec} = 100\text{K QPS.}
$$

And at peak (×5): up to ~**500K reads/sec**.

**Interpretation:** 100,000+ reads per second is a *lot*. This is well beyond what one database machine can serve comfortably. Two facts about this workload rescue us:
- It is **read-heavy** (100× more reads than writes). Read-heavy workloads are the *easy* kind to scale, because reads don't change data, so you can safely serve them from **many copies** (replicas) and from **caches**.
- The data barely changes after it's written (a short link's destination is basically fixed). Data that is written once and read many times is *ideal for caching* — you can hold the popular links in fast memory and answer most reads without touching the database at all.

So the read numbers point us at: **a cache in front of the database, plus read replicas** to absorb 100K+ QPS.

**Step 3 — Total storage (does the data fit on one machine?).**
Now, how much data piles up? Links don't get deleted quickly, so we accumulate them. Let's size for **5 years** of retention.

First, how many links total? 100M per day, 365 days per year, 5 years:

$$
100{,}000{,}000 \times 365 \times 5.
$$

Do it in pieces. $100\text{M} \times 365 = 36{,}500\text{M} = 3.65 \times 10^{10}$ links per year (that's 36.5 billion). Over 5 years:

$$
3.65 \times 10^{10} \times 5 = 1.825 \times 10^{11} \approx 1.8 \times 10^{11} \text{ links} \;(\approx 180 \text{ billion}).
$$

Now, how big is one link record? We store at least: the short code, the original long URL, a creation timestamp, maybe an owner id. Long URLs can be a few hundred characters. Round the whole record to **~500 bytes** each (a deliberately generous round number). Total bytes:

$$
1.8 \times 10^{11} \text{ links} \times 500 \text{ bytes} = 1.8 \times 10^{11} \times 5 \times 10^{2} = 9 \times 10^{13} \text{ bytes.}
$$

Convert $9 \times 10^{13}$ bytes into human units. Recall a **TB** (terabyte) ≈ $10^{12}$ bytes. So:

$$
\frac{9 \times 10^{13} \text{ bytes}}{10^{12} \text{ bytes/TB}} = 90 \text{ TB.}
$$

**Interpretation:** ~**90 terabytes**. A single machine cannot hold 90 TB of actively-served database (recall single machines top out around a few TB of fast storage before it gets impractical/expensive). Therefore the storage **must be split across many machines** — i.e. it **must be sharded**. This is the number that forces the third scaling move from Part 1.

**Step 4 — Put it together (the numbers chose the architecture).**
From four short calculations we now *know*, without having drawn a single box yet:
- **~1–5K writes/sec:** moderate; a sharded database can absorb this easily.
- **~100–500K reads/sec:** heavy, but the workload is read-heavy and rarely-changing → **put a cache in front, add read replicas.**
- **~90 TB over 5 years:** too big for one machine → **shard the data across many machines.**
- **Access pattern:** every read is "given this short code, get the one matching long URL" — a simple *look up one value by its key*. That's the ideal shape for a **key-value store** (a database optimized for exactly "give me the value for this key"; full treatment in note 02), and it also shards naturally (route by hashing the short code).

So the estimate alone tells us: **a sharded key-value store as the source of truth, a cache in front to absorb the read-heavy load, and read replicas for extra read capacity.** We derived the skeleton of the design from arithmetic, before discussing any specific technology. *That* is the point of back-of-envelope estimation.

### The interview move (what to actually do)

For almost any design prompt, compute these four numbers early and out loud:
1. **Write QPS** (how many writes per second, average and peak),
2. **Read QPS** (using a stated read:write assumption),
3. **Storage over the retention window** (does it fit on one machine?),
4. and note the **read:write ratio and access pattern**.

Those four numbers point directly at cache/replica/shard decisions — *before* you draw any boxes. Doing this signals that you size systems with math, not vibes.

---

## Part 4 — Consistency and availability: the deep trade-off

The second foundational skill is understanding *why you can't just make a distributed system both perfectly correct and perfectly always-on.* This is the single most important theoretical idea in system design, and it decides most storage choices. We'll build it up slowly.

### The setup: why copies of data create a problem

Recall from Part 1 that for reliability and read-scaling we keep **multiple copies (replicas)** of data on different machines. Say we have two copies of your bank balance, on machine A and machine B, and they're supposed to always agree.

```
        write "balance = $100"
                 │
        ┌────────┴────────┐
        ▼                 ▼
   ┌─────────┐       ┌─────────┐
   │ Node A  │       │ Node B  │
   │ bal=$100│ ????? │ bal=$100│      ← they must be kept in sync
   └─────────┘       └─────────┘
```

Keeping copies in sync requires the machines to **talk to each other over the network** ("I just updated the balance to $100, you update too"). Almost always, that works fine. But networks sometimes fail: a cable breaks, a switch dies, a datacenter link goes down. When that happens, A and B **can't talk**. This situation has a name.

**Network partition.** A **partition** is when the network splits so that some nodes **cannot communicate** with others, even though each is still running. The group is "partitioned" into islands that can't reach each other.

```
        ┌─────────┐   ✂  network   ┌─────────┐
        │ Node A  │  X  broken  X   │ Node B  │
        │ bal=$100│ <-- can't --->  │ bal=$100│
        └─────────┘    talk         └─────────┘
```

Now a customer sends a write ("withdraw $40, balance should become $60") and it reaches **only node A**, because B is unreachable. You are forced into a dilemma, and this dilemma is the whole ballgame.

### The CAP theorem

CAP is about three properties, and it's easiest to define them precisely:
- **C — Consistency:** every read returns the *most recent* write. All copies agree; nobody ever sees stale or contradictory data. (This specific "all copies agree, reads see the latest" notion is more precisely called *linearizability* — see the models below.)
- **A — Availability:** every request receives a (non-error) response — the system stays *up and answering*, even if some machines can't be reached.
- **P — Partition tolerance:** the system keeps working *despite* network partitions (messages between nodes being lost/delayed).

**The theorem:** in the presence of a network **partition (P)**, you can guarantee **either Consistency or Availability, but not both.** During the partition above, node A has two choices when the withdrawal arrives:

- **Choose Consistency (refuse the write): "I can't reach B to keep us in sync, so to avoid the copies disagreeing, I'll reject/stall this request."** The data stays correct everywhere, but the request fails — you sacrificed **Availability**. A system that makes this choice is called **CP** (Consistent under Partition).
- **Choose Availability (accept the write): "I'll accept the withdrawal on A now and reconcile with B once the network heals."** The request succeeds, but for a while A says $60 and B still says $100 — the copies **disagree**, so a read from B is stale. You sacrificed **Consistency**. A system that makes this choice is called **AP** (Available under Partition).

```
   During a partition, node A must pick ONE:

   ┌── CP: "refuse to answer" ──►  data stays correct, request FAILS   (lose Availability)
   │
   A ─┤
   │
   └── AP: "answer anyway"     ──►  request SUCCEEDS, copies disagree   (lose Consistency)
```

**Why "partition tolerance" isn't really optional.** People sometimes phrase CAP as "pick 2 of the 3." That's misleading. In any real system that runs across multiple machines, **network partitions *will* happen** — you don't get to opt out of them; they're a fact of physics and hardware. So P is a given, and the real, live choice is always **C vs A, and only *during* a partition.** The rest of the time (network healthy) you can have both. That's the honest reading of CAP:
> **When a partition happens, you must choose between staying consistent and staying available. The rest of the time you don't have to choose.**

**How you decide, per use case:**
- Choose **CP (consistency)** when a *wrong* answer is worse than *no* answer: money, bank balances, inventory ("did we sell the last item twice?"), unique usernames, anything where two copies disagreeing causes real damage. Better to say "try again in a moment" than to double-spend.
- Choose **AP (availability)** when being *down* is worse than being *slightly stale*: social feeds, like counts, view counts, product recommendations. If your like count is briefly off by one during a network blip, nobody is harmed; but showing an error page loses users.

### PACELC — the part CAP leaves out

CAP only tells you what happens *during a partition*. But partitions are rare. What about the 99.9% of the time when the network is healthy — is there no trade-off then? There is, and **PACELC** captures it. Read the name as two clauses:

- **P-A-C:** "if **P**artition, then choose **A**vailability or **C**onsistency" — that's just CAP restated.
- **E-L-C:** "**E**lse (= otherwise, when there is **no** partition and everything is healthy), choose between **L**atency and **C**onsistency."

The word **"Else"** here simply means **"in the normal case, when the network is fine."** The letter **E stands for "Else,"** as in "otherwise." So the full sentence is:
> **If** there's a partition → trade **A**vailability vs **C**onsistency. **Else** (normal times) → trade **L**atency vs **C**onsistency.

**Why is there *still* a trade-off when everything is healthy?** Because keeping copies perfectly consistent costs *time* even when nothing is broken. To guarantee a read sees the very latest write, the machines have to **coordinate**: before confirming a write, the leader may wait for several replicas to acknowledge it; before answering a strongly-consistent read, a node may have to check with others that it isn't about to serve stale data. All that back-and-forth is **network round trips**, and from Part 2 we know round trips cost real milliseconds. So:

- Want **strong consistency**? You pay **extra latency** on every operation (waiting for replicas to agree), even on a perfectly healthy day.
- Willing to accept **weaker consistency** (a read might occasionally be a little stale)? You can answer immediately from the nearest copy — **lower latency**.

This is why "just make everything strongly consistent everywhere" is not free advice: it makes every read and write slower, all the time, not just during failures. PACELC forces you to admit that and choose deliberately.

### Consistency models — the menu between "perfect" and "eventually"

"Consistency" isn't binary. There's a **ladder of guarantees** from strongest (and most expensive) to weakest (and cheapest/fastest). The senior skill is to pick, **for each kind of data**, the **weakest model that is still correct** — because weaker models are faster, cheaper, and more available.

| Model (strong → weak) | What it guarantees (plain language) | Good for | What it costs |
|---|---|---|---|
| **Linearizable / strong** | Reads always see the latest write. The system behaves as if there were a single copy of the data, even though there are many. | Money, inventory, locks, unique constraints | Coordination on every op → higher latency; unavailable during partition (the "C" of CP) |
| **Sequential** | Everyone sees operations in the **same order**, though not necessarily the instant they happen. | Replicated logs, some consensus systems | Still needs global ordering → not free |
| **Read-your-own-writes** | *You* always see *your own* recent writes immediately (others might see them slightly later). | "I edited my profile / posted a comment, and I expect to see it right away" | Must route your reads to a copy that has your write (session stickiness) |
| **Monotonic reads** | Once you've seen a value, you'll never later see an *older* value — time never appears to go backwards for you. | Feeds, timelines (you won't see a comment vanish then reappear) | Requires sticking you to a consistent copy |
| **Eventual** | If writes stop, all copies will *eventually* converge to the same value — but for a while they may disagree. | Like counts, view counts, DNS, caches | Temporary staleness and possible conflicting writes to reconcile |

**Crucial senior point — you mix these within one product.** A single application deliberately uses *different* models for *different* data, based on what each can tolerate. Classic example, one social app:
- **Account balance / payments:** *strong* — must never be wrong.
- **"Likes" count on a post:** *eventual* — being off by one for a second harms nobody, and this data is enormous and constant, so cheap+fast+available wins.
- **Your own just-posted comment:** *read-your-own-writes* — you must see your comment immediately after posting, or the app feels broken (even if your friend sees it a half-second later).

Applying one global model everywhere is a rookie move: strong-everywhere is needlessly slow and fragile; eventual-everywhere corrupts your money. Choose per data type.

### Where conflicts come from, and how to resolve them

If you chose **AP / eventual** (copies allowed to diverge temporarily), you can get **conflicting writes**: two users update the same thing at "the same time" on two copies that couldn't talk, and now there are two different values. When the network heals, the system must decide which value wins — this is **conflict resolution**. The main strategies:

- **Last-Write-Wins (LWW):** keep whichever write has the latest timestamp; discard the other. *Simple, but lossy* — it silently throws away a real update (and clocks on different machines don't perfectly agree, so "latest" is fuzzy).
- **Version vectors / causal tracking:** attach bookkeeping to each write so the system can *detect* that two writes were truly concurrent (neither happened before the other) and flag a genuine conflict rather than blindly picking one. More correct, more machinery.
- **CRDTs (Conflict-free Replicated Data Types):** special data structures *designed* so that concurrent updates **merge automatically and deterministically** with no conflict — e.g. a counter that just adds up all increments, or a set that unions all additions. Great for collaborative editing and offline-capable apps. The constraint: only certain data types have a clean merge rule.
- **Application-level merge:** let *business logic* decide how to combine. The famous example is a shopping cart: if two copies of your cart diverged, **union** the items (better to resurrect a removed item than to lose an added one). The right rule depends entirely on the domain.

---

## Part 5 — The trade-off vocabulary (say these out loud in every design)

Everything above distills into a handful of **tensions** that recur in *every* design. In an interview, the graded skill is not knowing the terms — it's **naming which side you're choosing and *why*** as you design. Keep these loaded:

- **Latency vs throughput** — doing work in big batches gets *more* done per second (higher throughput) but makes each individual item wait longer (higher latency). You usually optimize for one; say which.
- **Consistency vs availability** (this is CAP — the choice *during a partition*).
- **Consistency vs latency** (this is the "Else" of PACELC — the choice during *normal* operation).
- **Read-optimized vs write-optimized** — adding caches, extra indexes, and pre-computed copies makes **reads** faster but makes **writes** slower and more complex (every write must now update all those copies/indexes). And vice versa.
- **Normalization vs denormalization** — *normalized* data is stored once with no duplication (simple, correct writes, but reads must join pieces together); *denormalized* data pre-combines and duplicates it (fast join-free reads, but writes must update every duplicate). (Detailed in note 02.)
- **Synchronous vs asynchronous** — *sync* = do it now and wait for the result (simple, immediately consistent, but the caller is blocked and failures cascade); *async* = hand the work to a queue and move on (decoupled, resilient to spikes, survives downstream failures, but the result is eventual and harder to trace). (Detailed in notes 05 & 07.)
- **Cost vs performance vs complexity** — the eternal triangle. More performance usually costs more money and/or more complexity. Name the corner you're deliberately sacrificing for this problem.
- **Push vs pull** — *push* = the server proactively sends updates to clients (low latency, but the server bears the fan-out cost of contacting everyone); *pull* = clients periodically ask for updates (simple, client-controlled, but wasteful and laggy). (Detailed in note 05.)

When you can look at any design decision and immediately say "this is really a latency-vs-consistency call, and I'm choosing latency because this is a like-count, not money" — you're operating at the level this round is testing.

---

## Self-check (answer out loud, without looking; answers follow each question)

Try to *explain* each one in a few sentences, as if teaching it — not just recall the keyword. This is exactly how you'll be quizzed at the start of the next session.

1. **In your own words, what's the difference between vertical scaling and horizontal scaling, and what's the ceiling on each?**
   *Vertical = bigger single machine (up to the biggest/most expensive box, and it stays a single point of failure). Horizontal = more machines behind a load balancer (scales much further, and removes the app tier as a single point of failure — but requires the app to be stateless).*

2. **What's the difference between replication and sharding? Can you use both at once?**
   *Replication = full copies of the same data on multiple machines (for reliability + read scaling). Sharding = splitting different data across machines (for size + write scaling). Real systems do both: shard the data, then replicate each shard.*

3. **Why is "seconds in a day ≈ 10⁵" the handiest constant, and how do you use it?**
   *Because dividing a per-day event count by 10⁵ gives per-second QPS. e.g. 100M/day ÷ 10⁵ = 1,000/sec.*

4. **The latency hierarchy: roughly how much slower is RAM than L1 cache, and SSD than RAM? What single design rule does this justify?**
   *RAM ≈ 100× slower than L1; SSD random read ≈ 1000× slower than RAM. Justifies caching hot data in RAM and colocating compute with data.*

5. **Walk through, step by step, why a 100M-links/day URL shortener ends up "read-heavy, cached, sharded." Show the three calculations.**
   *Writes: 10⁸/10⁵ = 1,000/s (moderate). Reads at 100:1 = 100K/s (heavy but read-heavy → cache + replicas). Storage: 1.8×10¹¹ links × 500 B ≈ 90 TB over 5 yr → too big for one machine → shard.*

6. **Define a network partition, and state exactly what CAP forces you to choose — and precisely *when*.**
   *Partition = network split so some nodes can't talk to others. CAP: during a partition you must choose Consistency (refuse writes to stay correct → lose availability) or Availability (accept writes → copies temporarily disagree → lose consistency). Only forced during the partition.*

7. **What does the "E" in PACELC stand for, and what trade-off does that clause describe?**
   *E = "Else" = the normal case with no partition. In that case the trade-off is Latency vs Consistency: stronger consistency costs coordination round trips (more latency); weaker consistency lets you answer from the nearest copy faster.*

8. **Name a single product that deliberately uses three different consistency models, and say which data gets which and why.**
   *e.g. a social app: balance = strong (must be correct); like-count = eventual (cheap/fast, harmless if briefly off); your own new post = read-your-own-writes (you must see it immediately).*

9. **In an eventual-consistency system, two copies took conflicting writes during a partition. Name three ways to resolve the conflict when they reconnect, with a trade-off for each.**
   *LWW (simple, but silently drops a write); version vectors (detect true conflicts, more machinery); CRDTs / app-merge (auto-merge like counter-sum or cart-union, but only for suitable data types / domain rules).*

10. **Give one example each of latency-vs-throughput and push-vs-pull, and when you'd pick which side.**
    *Latency/throughput: batching writes raises throughput but each write waits longer — batch for analytics ingestion, don't batch for an interactive checkout. Push/pull: push notifications (low latency, server fan-out cost) vs polling for new email (simple, wasteful/laggy) — push when freshness matters and fan-out is manageable.*

---

## Changelog

- **v0.2 — 2026-07-08:** Full rewrite to zero-prior-knowledge depth per user directive. Added Part 0 (basic vocabulary), Part 1 (single box / fleet / sharded, with diagrams and vertical/horizontal scaling, replication vs sharding). Expanded latency section with units and hardware explained. Rewrote the URL-shortener estimate as a step-by-step proof with explicit arithmetic. Expanded CAP/PACELC from scratch (what a partition is, why copies conflict, what "E/Else" means and why the trade-off persists when healthy). Expanded consistency-model and conflict sections. Expanded self-check to 10 explain-it-back questions with answers.
- **v0.1 — 2026-07-07:** First foundations note (style anchor, too concise).
