import http from "k6/http";
import { check, sleep } from "k6";

// 1. Scenario Options & Thresholds
export const options = {
	// Prevent k6 from following the 302 Found redirect to external domains
	maxRedirects: 0,

	stages: [
		{ duration: "30s", target: 50 }, // Ramp-up to 50 concurrent virtual users
		{ duration: "1m", target: 50 }, // Stay at 50 VUs for 1 minute (steady state)
		{ duration: "15s", target: 0 }, // Ramp-down to 0 VUs
	],

	thresholds: {
		// 95% of redirects should return under 50ms (Postgres baseline)
		http_req_duration: ["p(95)<50", "p(99)<100"],
		// Less than 1% failed requests (excluding 302s)
		http_req_failed: ["rate<0.01"],
	},
};

// 2. Configuration Parameters
const BASE_URL = __ENV.BASE_URL || "http://localhost:5297";
// Replace with a known valid short URL generated in your database
const SHORT_CODE = __ENV.SHORT_CODE || "b4a1c";

export default function () {
	const url = `${BASE_URL}/${SHORT_CODE}`;

	const response = http.get(url);

	// 3. Validation: Verify response is HTTP 302 Found and has a Location header
	check(response, {
		"status is 302": (r) => r.status === 302,
		"has location header": (r) => r.headers["Location"] !== undefined,
	});

	// Short pacing to simulate realistic user click intervals
	sleep(0.05); // 50ms pause per VU iteration
}
