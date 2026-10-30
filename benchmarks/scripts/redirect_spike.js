import http from "k6/http";
import { check, sleep } from "k6";
import { SharedArray } from "k6/data";
import { generateSummary } from "../helpers/reporter.js";

// 1. Scenario Options & Thresholds

const shortCodes = new SharedArray("short codes", function () {
	return JSON.parse(open("../data/short_codes.json"));
});

export const options = {
	// Prevent k6 from following the 302 Found redirect to external domains
	maxRedirects: 0,

	stages: [
		{ duration: "10s", target: 10 }, // Low baseline
		{ duration: "5s", target: 300 }, // Sudden sharp spike
		{ duration: "30s", target: 300 }, // Sustain spike
		{ duration: "5s", target: 10 }, // Immediate recovery drop
		{ duration: "20s", target: 10 }, // Check if latency returns to baseline
	],

	thresholds: {
		http_req_duration: ["p(90)<200", "p(95)<500", "p(99)<1500"],
		http_req_failed: ["rate<0.01"],
	},
};

// 2. Configuration Parameters
const BASE_URL = __ENV.BASE_URL || "http://localhost:5297";

export default function () {
	const code = shortCodes[Math.floor(Math.random() * shortCodes.length)];
	const url = `${BASE_URL}/${code}`;

	const response = http.get(url, {
		tags: { name: "/{shortCode}" },
	});

	// 3. Validation: Verify response is HTTP 302 Found and has a Location header
	check(response, {
		"status is 302": (r) => r.status === 302,
		"has location header": (r) => r.headers["Location"] !== undefined,
	});

	// Short pacing to simulate realistic user click intervals
	sleep(0.05); // 50ms pause per VU iteration
}

export function handleSummary(data) {
	return generateSummary("redirect_spike", data);
}
