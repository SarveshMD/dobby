import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.2/index.js";

/**
 * Standard summary generator for k6 benchmarks.
 * Saves a human-readable .txt snapshot and a machine-readable .json metrics file.
 *
 * @param {string} testName - Name prefix for output files (e.g., 'redirect_random')
 * @param {object} data - The k6 metrics data object from handleSummary
 */

export function generateSummary(testName, data) {
	const timestamp = new Date().toISOString().replace(/[:.]/g, "-");
	const jsonPath = `/benchmarks/results/${testName}_${timestamp}.json`;
	const textPath = `/benchmarks/results/${testName}_${timestamp}.txt`;

	return {
		stdout: textSummary(data, { indent: " ", enableColors: true }),
		[textPath]: textSummary(data, { indent: " ", enableColors: false }),
		[jsonPath]: JSON.stringify(data, null, 2),
	};
}
