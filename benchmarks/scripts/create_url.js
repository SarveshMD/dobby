import http from "k6/http";
import { check, sleep } from "k6";
import { generateSummary } from "../helpers/reporter.js";

// 1. Scenario Options & Thresholds
export const options = {
	stages: [
		{ duration: "30s", target: 50 }, // Ramp-up to 50 concurrent virtual users
		{ duration: "1m", target: 50 }, // Stay at 50 VUs for 1 minute (steady state)
		{ duration: "15s", target: 0 }, // Ramp-down to 0 VUs
	],

	thresholds: {
		// 95% of writes should return under 50ms (Postgres baseline)
		http_req_duration: ["p(95)<60", "p(99)<120"],
		// Less than 1% failed requests
		http_req_failed: ["rate<0.01"],
	},
};

// 2. Configuration Parameters
const BASE_URL = __ENV.BASE_URL || "http://dobby:5000";

export default function () {
	const url = `${BASE_URL}/api/urls/`;
	const currUrl = `https://example.com/test-${__VU}-${__ITER}-${Date.now()}`;

	let payload = JSON.stringify({
		originalUrl: currUrl,
	});

	const res = http.post(url, payload, {
		headers: {
			"Content-Type": "application/json",
		},
	});

	let jsonBody;
	try {
		jsonBody = res.json();
	} catch (_) {}

	check(res, {
		"status is 201": (r) => r.status === 201,
		"originalUrl is preserved": () => jsonBody?.originalUrl === currUrl,
		"has location header": (r) => Boolean(r.headers["Location"]),
		"shortCode returned": () => Boolean(jsonBody?.shortUrl),
	});

	// Short pacing to simulate realistic user click intervals
	sleep(0.05); // 50ms pause per VU iteration
}

export function handleSummary(data) {
	return generateSummary("create_url", data);
}
