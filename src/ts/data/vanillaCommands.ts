const SELECTORS: string[] = ["@p", "@a", "@r", "@s", "@e"];
export const NAMESPACE = "NAMESPACE";
const valuePattern = /[\w.]+/g;

enum ArgType {
	KEYWORD, VALUE
}

interface CommandArgument {
	command: string;
	args: { value: string[] | string; type: ArgType }[];
}

export const CommandArguments: CommandArgument[] = [
	{
		command: "advancement",
		args: [
			{
				value: ["grant", "revoke"],
				type: ArgType.KEYWORD
			},
			{
				value: SELECTORS,
				type: ArgType.KEYWORD
			},
			{
				value: ["everything", "from", "only", "until", "through"],
				type: ArgType.KEYWORD
			},
			{
				value: NAMESPACE,
				type: ArgType.VALUE
			}
		],
	},
];

// export function getCommand(text: string): RegExp[] {
// 	const results: RegExp[] = [];

// 	for (const command of CommandArguments) {
		
// 	}

// 	return results;
// }
