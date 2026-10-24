// SP-3 load scenarios. Usage (from this folder), with three gateways on 5201, 5202, 5203:
//   k6 run -e SCENARIO=shared  k6/limits.js     # limits hold across instances
//   k6 run -e SCENARIO=outage  k6/limits.js     # stop the Redis container while this runs (see README)
//   k6 run -e SCENARIO=admission k6/limits.js   # 900 orders/s against the 600/s global limit
import http from 'k6/http';
import { Counter, Trend } from 'k6/metrics';

const ports = [5201, 5202, 5203];
const scenario = __ENV.SCENARIO || 'shared';

const allowed = new Counter('allowed');
const limited = new Counter('limited');
const viaLocal = new Counter('decided_by_local');
const viaRedis = new Counter('decided_by_redis');
const rejectLatency = new Trend('reject_latency_ms', true);

const scenarios = {
  // One anonymous client IP, 150 req/s for 30 s, spread over three gateways.
  // Expected allowed: about 20/s * 30 s + burst 60 = ~660, NOT three times that.
  shared: {
    executor: 'constant-arrival-rate', rate: 150, timeUnit: '1s', duration: '30s',
    preAllocatedVUs: 50, maxVUs: 200,
  },
  // Same load for 60 s. Stop Redis at about 20 s. Expect decided_by_local to rise and the total to stay near
  // the same per-second figure, because local limit = global / 3 on each of the three gateways.
  outage: {
    executor: 'constant-arrival-rate', rate: 150, timeUnit: '1s', duration: '60s',
    preAllocatedVUs: 50, maxVUs: 200,
  },
  // 900 distinct users each placing one order per second-ish for 30 s.
  // Expected accepted: about 600/s * 30 s + burst 100 = ~18,100 of ~27,000. Excess answered 429 quickly.
  admission: {
    executor: 'constant-arrival-rate', rate: 900, timeUnit: '1s', duration: '30s',
    preAllocatedVUs: 300, maxVUs: 1000,
  },
};

export const options = {
  scenarios: { main: scenarios[scenario] },
  thresholds: scenario === 'admission' ? { reject_latency_ms: ['p(95)<20'] } : {},
};

export default function () {
  const port = ports[Math.floor(Math.random() * ports.length)];
  const base = `http://localhost:${port}`;
  let res;
  if (scenario === 'admission') {
    const user = `user-${Math.floor(Math.random() * 1000000)}`;
    res = http.post(`${base}/api/v1/orders`, '{}', { headers: { 'X-Spike-User': user, 'Content-Type': 'application/json' } });
  } else {
    res = http.get(`${base}/spike/ping`, { headers: { 'X-Forwarded-For': '203.0.113.7' } });
  }
  if (res.status === 429) {
    limited.add(1);
    rejectLatency.add(res.timings.duration);
  } else if (res.status === 200 || res.status === 202) {
    allowed.add(1);
  }
  const src = res.headers['X-Ratelimit-Source'];
  if (src === 'Local') viaLocal.add(1);
  if (src === 'Redis') viaRedis.add(1);
}
