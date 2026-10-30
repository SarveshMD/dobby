import http from "k6/http";
import { check, sleep } from "k6";
import { SharedArray } from "k6/data";
import { generateSummary } from "../helpers/reporter.js";

// 1. Scenario Options & Thresholds

const shortCodes = new SharedArray("short codes", function () {
	return JSON.parse(open("../data/short_codes.json"));
});

export const options = {
	maxRedirects: 0,

	scenarios: {
		redirects: {
			executor: "ramping-vus",
			stages: [
				{ duration: "20s", target: 95 },
				{ duration: "1m", target: 95 },
				{ duration: "10s", target: 0 },
			],
			exec: "handleRedirects",
		},
		creations: {
			executor: "ramping-vus",
			stages: [
				{ duration: "20s", target: 5 },
				{ duration: "1m", target: 5 },
				{ duration: "10s", target: 0 },
			],
			exec: "handleCreations",
		},
	},
	thresholds: {
		"http_req_duration{scenario:redirects}": ["p(95)<50", "p(99)<100"],

		"http_req_duration{scenario:creations}": ["p(95)<80", "p(99)<150"],

		http_req_failed: ["rate<0.01"],
	},
};

// 2. Configuration Parameters
const BASE_URL = __ENV.BASE_URL || "http://localhost:5297";

export function handleRedirects() {
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

export function handleCreations() {
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
	return generateSummary("mixed_traffic", data);
}
