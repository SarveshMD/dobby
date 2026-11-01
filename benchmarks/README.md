# Benchmark Baselines: PostgreSQL (Pre-Redis)

## 1. Two-VM Isolated Baseline

**Environment**:
- **Load Generator:**
  - Dedicated VM running k6 in Docker.
  - Azure VM Config:
    - Standard D2s v3 (2 vcpus, 2 threads per core)
    - 8 GiB memory
    - OS: ubuntu-24_04-lts server
- **Application Host:**
  - Dedicated VM running ASP.NET Core (`dobby`) and PostgreSQL (`dobby-postgres`) via Docker Compose.
  - Azure VM Config:
    - Standard D4s v4 (4 vcpus, 2 threads per core)
    - 16 GiB memory
    - OS: ubuntu-24_04-lts server
- **Network:** Private Virtual Network (VNet) peering via internal private IP.

| Benchmark Scenario                             | Concurrency   | Total Requests | Throughput (req/s) | Median Latency | p(90) Latency | p(95) Latency | Max Latency | Error Rate |
| :--------------------------------------------- | :------------ | :------------- | :----------------- | :------------- | :------------ | :------------ | :---------- | :--------- |
| **Redirect Load** (`redirect_random.js`)       | 50 VUs        | 76,748         | 730.60 req/s       | 2.55 ms        | 5.51 ms       | 6.85 ms       | 65.49 ms    | 0.00%      |
| **Redirect Stress** (`redirect_stress.js`)     | Up to 400 VUs | 335,281        | 2,234.58 req/s     | 7.38 ms        | 41.06 ms      | 53.47 ms      | 174.31 ms   | 0.00%      |
| **Redirect Spike** (`redirect_spike.js`)       | Up to 300 VUs | 140,703        | 2,008.46 req/s     | 20.72 ms       | 52.93 ms      | 63.08 ms      | 135.69 ms   | 0.00%      |
| **Mixed Traffic (Total)** (`mixed_traffic.js`) | 100 VUs       | 138,631        | 1,539.90 req/s     | 2.69 ms        | 6.39 ms       | 8.26 ms       | 37.55 ms    | 0.00%      |
| > *Redirects (95% reads)*                      | 95 VUs        | 131,699        | 1,462.90 req/s     | 2.59 ms        | 6.11 ms       | 7.91 ms       | 37.55 ms    | 0.00%      |
| > *Creations (5% writes)*                      | 5 VUs         | 6,932          | 77.00 req/s        | 5.31 ms        | 9.84 ms       | 13.01 ms      | 30.89 ms    | 0.00%      |
| **Write Baseline** (`create_url.js`)           | 50 VUs        | 66,413         | 632.48 req/s       | 9.82 ms        | 17.69 ms      | 26.31 ms      | 286.79 ms   | 0.00%      |

---

## 2. Local Docker Baseline (Single Host Machine - For Reference)

> These performance tests were run on a single machine with `k6`, `dobby`, and `postgres` running through `docker compose`. All three resource-intensive containers competed for the same CPU cores and memory bandwidth, potentially limiting the app's performance. To fix this, I moved the tests to Azure, running two VMs on the same VNet, one for `dobby`+`postgres`, other for `k6`.

- **Host Machine Config**:
  - 11th Gen Intel(R) Core(TM) i3-1115G4 @ 3.00GHz
  - 2 physical cores, 4 logical processors, 2 threads per core
  - 16 GiB memory
  - OS: Windows 11 Home

| Benchmark Scenario                   | Concurrency   | Total Requests | Throughput (req/s) | Median Latency | p(90) Latency | p(95) Latency | Max Latency | Error Rate |
| :----------------------------------- | :------------ | :------------- | :----------------- | :------------- | :------------ | :------------ | :---------- | :--------- |
| **Redirect Load**                    | 50 VUs        | 72,642         | 643.42 req/s       | 3.18 ms        | 11.92 ms      | 18.51 ms      | 1,240.00 ms | 0.00%      |
| **Redirect Stress**                  | Up to 400 VUs | 242,924        | 1,574.70 req/s     | 28.22 ms       | 86.87 ms      | 111.88 ms     | 707.96 ms   | 0.00%      |
| **Redirect Spike**                   | Up to 300 VUs | 102,762        | 1,432.16 req/s     | 41.49 ms       | 94.72 ms      | 131.65 ms     | 779.78 ms   | 0.00%      |
| **Mixed Traffic (Total)**            | 100 VUs       | 108,208        | 1,164.85 req/s     | 10.82 ms       | 39.72 ms      | 55.34 ms      | 660.47 ms   | 0.00%      |
| > *Redirects (95% reads)*            | 95 VUs        | 102,797        | 1,106.60 req/s     | 10.60 ms       | 39.21 ms      | 54.44 ms      | 656.93 ms   | 0.00%      |
| > *Creations (5% writes)*            | 5 VUs         | 5,411          | 58.25 req/s        | 16.17 ms       | 50.94 ms      | 71.20 ms      | 660.47 ms   | 0.00%      |
| **Write Baseline**                   | 50 VUs        | 63,989         | 598.10 req/s       | 10.48 ms       | 26.57 ms      | 31.86 ms      | 475.45 ms   | 0.00%      |

---

## 3. Impact of Performance Test Isolation (Local vs. Dedicated VM)

| Metric                       | Local Shared Host   | Dedicated 2-VM VNet | Improvement                   |
| :--------------------------- | :------------------ | :------------------ | :---------------------------- |
| **Peak Throughput (Stress)** | 1,574.70 req/s      | 2,234.58 req/s      | **+41.9% higher throughput**  |
| **Stress Median Latency**    | 28.22 ms            | 7.38 ms             | **-73.8% lower latency**      |
| **Spike Peak Latency (Max)** | 779.78 ms           | 135.69 ms           | **-82.6% tail reduction**     |
| **Mixed Redirects p(95)**    | 54.44 ms *(Failed)* | 7.91 ms *(Passed)*  | **-85.5% lower tail latency** |
| **Write Throughput**         | 598.10 req/s        | 632.48 req/s        | **+5.7% higher throughput**   |
| **Write p(95) Latency**      | 31.86 ms            | 26.31 ms            | **-17.4% lower tail latency** |
