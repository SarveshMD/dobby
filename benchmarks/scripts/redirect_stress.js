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
		{ duration: "30s", target: 50 },
		{ duration: "30s", target: 100 },
		{ duration: "30s", target: 200 },
		{ duration: "30s", target: 400 },
		{ duration: "30s", target: 0 },
	],

	thresholds: {
		http_req_duration: ["p(95)<250", "p(99)<750"],
		http_req_failed: ["rate<0.02"],
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
	return generateSummary("redirect_stress", data);
}
