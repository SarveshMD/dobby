# Benchmarks

## 1. PostgreSQL Baseline (Isolated Two-VM, Pre-Cache)

**Environment**

- **Load Generator**
    - Dedicated VM running k6 in Docker.
    - **Azure VM Config**
        - **CPU**: Standard D2s v3 (2 vcpus, 2 threads per core)
        - **Memory**: 8 GiB
        - **OS**: ubuntu-24_04-lts server
- **Application Host**
    - Dedicated VM running ASP.NET Core (`dobby`) and PostgreSQL (`dobby-postgres`) via Docker Compose.
    - **Azure VM Config**:
        - **CPU**: Standard D4s v4 (4 vcpus, 2 threads per core)
        - **Memory**: 16 GiB
        - **OS**: ubuntu-24_04-lts server
    - **Image**: `sarveshmd/dobby:1.0.0` (`docker-compose.prod.yml`)
- **Network:** Private Virtual Network (VNet) peering via internal private IP.

| Benchmark Scenario                             | Concurrency   | Total Requests | Throughput (req/s) | Median Latency | p(90) Latency | p(95) Latency | Max Latency | Error Rate |
| :--------------------------------------------- | :------------ | :------------- | :----------------- | :------------- | :------------ | :------------ | :---------- | :--------- |
| **Redirect Load** (`redirect_load.js`)         | 50 VUs        | 77,427         | 737.04 req/s       | 2.28 ms        | 4.74 ms       | 5.91 ms       | 34.99 ms    | 0.00%      |
| **Redirect Stress** (`redirect_stress.js`)     | Up to 400 VUs | 341,753        | 2,277.64 req/s     | 7.48 ms        | 37.66 ms      | 48.84 ms      | 233.47 ms   | 0.00%      |
| **Redirect Spike** (`redirect_spike.js`)       | Up to 300 VUs | 144,251        | 2,060.42 req/s     | 19.86 ms       | 48.84 ms      | 56.57 ms      | 230.01 ms   | 0.00%      |
| **Mixed Traffic (Total)** (`mixed_traffic.js`) | 100 VUs       | 139,738        | 1,552.32 req/s     | 2.33 ms        | 6.03 ms       | 7.69 ms       | 151.40 ms   | 0.00%      |
| > _Redirects (95% reads)_                      | 95 VUs        | 132,751        | 1,474.70 req/s     | 2.24 ms        | 5.84 ms       | 7.48 ms       | 84.14 ms    | 0.00%      |
| > _Creations (5% writes)_                      | 5 VUs         | 6,987          | 77.62 req/s        | 4.51 ms        | 8.44 ms       | 10.11 ms      | 151.40 ms   | 0.00%      |
| **Write Baseline** (`create_url.js`)           | 50 VUs        | 70,008         | 666.44 req/s       | 7.99 ms        | 13.31 ms      | 15.16 ms      | 32.71 ms    | 0.00%      |

---

## 2. Redis Cache-Aside Performance (Isolated Two-VM)

**Environment**

- **Load Generator**
    - Dedicated VM running k6 in Docker.
    - **Azure VM Config**
        - **CPU**: Standard D2s v3 (2 vcpus, 2 threads per core)
        - **Memory**: 8 GiB
        - **OS**: ubuntu-24_04-lts server
- **Application Host**
    - Dedicated VM running ASP.NET Core (`dobby`) and PostgreSQL (`dobby-postgres`) via Docker Compose.
    - **Azure VM Config**
        - **CPU**: Standard D4s v4 (4 vcpus, 2 threads per core)
        - **Memory**: 16 GiB
        - **OS**: ubuntu-24_04-lts server
    - **Image**: `sarveshmd/dobby:v2` (`docker-compose.prod.yml`)
- **Network:** Private Virtual Network (VNet) peering via internal private IP.

**Notes**

- `benchmarks/scripts/warm_up_cache.js` is initally run to get every `shortUrl` in `benchmarks/data/short_codes.json` written to Redis.
- When the k6 benchmark scripts are run, Redis is hot, and the hit ratio is essentially 100%
- This is done to prevent accidentally inflating cache hit ratio, and therefore performance, of benchmark scripts run later in the sequence.

| Benchmark Scenario        | Concurrency   | Total Requests | Throughput (req/s) | Median Latency | p(90) Latency | p(95) Latency | Max Latency | Error Rate |
| :------------------------ | :------------ | :------------- | :----------------- | :------------- | :------------ | :------------ | :---------- | :--------- |
| **Redirect Load**         | 50 VUs        | 78,454         | 746.83 req/s       | 1.57 ms        | 3.55 ms       | 4.59 ms       | 22.41 ms    | 0.00%      |
| **Redirect Stress**       | Up to 400 VUs | 385,494        | 2,569.66 req/s     | 2.66 ms        | 16.97 ms      | 26.06 ms      | 143.19 ms   | 0.00%      |
| **Redirect Spike**        | Up to 300 VUs | 173,074        | 2,470.24 req/s     | 7.44 ms        | 22.55 ms      | 38.72 ms      | 93.84 ms    | 0.00%      |
| **Mixed Traffic (Total)** | 100 VUs       | 142,461        | 1,582.05 req/s     | 1.53 ms        | 3.85 ms       | 4.92 ms       | 164.53 ms   | 0.00%      |
| > _Redirects (95% reads)_ | 95 VUs        | 135,338        | 1,502.94 req/s     | 1.47 ms        | 3.37 ms       | 4.40 ms       | 28.21 ms    | 0.00%      |
| > _Creations (5% writes)_ | 5 VUs         | 7,123          | 79.11 req/s        | 4.20 ms        | 6.43 ms       | 7.83 ms       | 164.53 ms   | 0.00%      |
| **Write Baseline**        | 50 VUs        | 70,143         | 667.73 req/s       | 7.58 ms        | 13.55 ms      | 15.47 ms      | 37.68 ms    | 0.00%      |

## 3. Impact of Redis Cache-Aside (PostgreSQL vs. Redis)

_Both benchmarks executed on isolated Azure Two-VM VNet with `docker-compose.prod.yml`_

| Benchmark Scenario / Metric       | PostgreSQL Baseline | With Redis Cache | Delta / Impact                     |
| :-------------------------------- | :------------------ | :--------------- | :--------------------------------- |
| **Peak Throughput (Stress)**      | 2,277.64 req/s      | 2,569.66 req/s   | **+12.8% throughput** (+292 req/s) |
| **Stress Median Latency**         | 7.48 ms             | 2.66 ms          | **-64.4% lower median**            |
| **Stress Tail Latency p(95)**     | 48.84 ms            | 26.06 ms         | **-46.6% lower tail**              |
| **Spike Peak Throughput**         | 2,060.42 req/s      | 2,470.24 req/s   | **+19.9% throughput** (+410 req/s) |
| **Spike Median Latency**          | 19.86 ms            | 7.44 ms          | **-62.5% lower median**            |
| **Spike Tail Latency p(95)**      | 56.57 ms            | 38.72 ms         | **-31.6% lower tail**              |
| **Spike Max Latency (Peak)**      | 230.01 ms           | 93.84 ms         | **-59.2% max spike reduction**     |
| **Mixed Traffic Reads p(95)**     | 7.48 ms             | 4.40 ms          | **-41.2% read tail reduction**     |
| **Mixed Traffic Writes p(95)**    | 10.11 ms            | 7.83 ms          | **-22.6% write tail reduction**    |
| **Redirect Load (50 VUs) Median** | 2.28 ms             | 1.57 ms          | **-31.1% lower median**            |

### Key Observations

1. Under low load of 50VUs in `redirect_load.js` test, there was no meaningful difference in p(95) latency between PostgreSQL and Redis, but there is an improvement of 0.71ms in median latency. The total response time at low load is bottlenecked only by Azure VNet round-trip latency.
2. Under heavy concurrent load of upto 400VUs in `redirect_stress.js` and 300VUs in `redirect_spike.js`, the real advantage of Redis is clearly demonstrated:
    - **Stress Test (400 VUs):** Redis cuts median latency by **-64.4%** (7.48 ms down to 2.66 ms) and p(95) tail latency by **-46.6%** (48.84 ms down to 26.06 ms).
    - **Spike Test (300 VUs):** Max latency drops from **230.01 ms down to 93.84 ms (-59.2%)**.
3. Write performance improved in `mixed_traffic.js`
    - Offloading 95% of redirect reads to Redis dropped write tail latency `p(95)` during mixed traffic from `10.11 ms down to 7.83 ms (-22.6%)`.
    - Postgres had zero read requests on its connection pool and WAL flushes, letting the 5% writes commit with less queuing.

## Local Single-Host Baseline (Reference)

- **Notes**
    - These performance tests were run on a single machine with `k6`, `dobby`, and `postgres` running through `docker compose`.
    - Initial runs showed unusually high latency because I ran the app in development environment (`docker-compose.yml`) instead of production (`docker-compose.prod.yml`).
    - Development environment logs every SQL query executed by EF Core to the console
    - The below statistics show a clean run re-tested with logging disabled.

- **Host Configuration**:
    - **CPU**: 11th Gen Intel(R) Core(TM) i3-1115G4 @ 3.00GHz (2 cores, 4 threads)
    - **Memory**: 16 GiB memory
    - **OS**: Windows 11 Home
    - **Image**: `sarveshmd/dobby:1.0.0` (`docker-compose.prod.yml`)

| Benchmark Scenario        | Concurrency   | Total Requests | Throughput (req/s) | Median Latency | p(90) Latency | p(95) Latency | Max Latency | Error Rate |
| :------------------------ | :------------ | :------------- | :----------------- | :------------- | :------------ | :------------ | :---------- | :--------- |
| **Redirect Load**         | 50 VUs        | 77,104         | 735.85 req/s       | 2.41 ms        | 4.67 ms       | 5.75 ms       | 46.89 ms    | 0.00%      |
| **Redirect Stress**       | Up to 400 VUs | 324,705        | 2,169.39 req/s     | 9.22 ms        | 40.81 ms      | 54.00 ms      | 299.84 ms   | 0.00%      |
| **Redirect Spike**        | Up to 300 VUs | 149,094        | 2,135.30 req/s     | 17.19 ms       | 38.10 ms      | 45.39 ms      | 172.24 ms   | 0.00%      |
| **Mixed Traffic (Total)** | 100 VUs       | 137,395        | 1,529.24 req/s     | 2.97 ms        | 7.30 ms       | 9.45 ms       | 645.11 ms   | 0.00%      |
| > _Redirects (95% reads)_ | 95 VUs        | 130,525        | 1,452.78 req/s     | 2.86 ms        | 7.07 ms       | 9.21 ms       | 100.44 ms   | 0.00%      |
| > _Creations (5% writes)_ | 5 VUs         | 6,870          | 76.46 req/s        | 5.17 ms        | 10.26 ms      | 12.66 ms      | 645.11 ms   | 0.00%      |
| **Write Baseline**        | 50 VUs        | 65,414         | 624.40 req/s       | 11.87 ms       | 17.31 ms      | 20.27 ms      | 306.70 ms   | 0.00%      |

---
