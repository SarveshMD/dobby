import http from "k6/http";
import { check, sleep } from "k6";
import { SharedArray } from "k6/data";
import exec from "k6/execution";

const shortCodes = new SharedArray("short codes", function () {
	return JSON.parse(open("../data/short_codes.json"));
});

export const options = {
	maxRedirects: 0,

	scenarios: {
		process_all_codes: {
			executor: "shared-iterations",
			vus: 50,
			iterations: shortCodes.length,
			maxDuration: "10m",
		},
	},

	thresholds: {
		http_req_duration: ["p(95)<50", "p(99)<100"],
		http_req_failed: ["rate<0.01"],
	},
};

const BASE_URL = __ENV.BASE_URL || "http://localhost:5297";

export default function () {
	const itemIndex = exec.scenario.iterationInTest;
	const code = shortCodes[itemIndex];
	const url = `${BASE_URL}/${code}`;

	const response = http.get(url, {
		tags: { name: "/{shortCode}" },
	});

	check(response, {
		"status is 302": (r) => r.status === 302,
		"has location header": (r) => r.headers["Location"] !== undefined,
	});

	sleep(0.05);
}
