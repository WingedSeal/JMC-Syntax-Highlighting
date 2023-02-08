const SELECTORS: string[] = ["@p", "@a", "@r", "@s", "@e"];
export const NAMESPACE = "NAMESPACE";

interface CommandArgument {
	command: string;
	args: (string[] | string)[];
}

export const CommandArguments: CommandArgument[] = [
	{
		command: "advancement",
		args: [
			["grant", "revoke"],
			SELECTORS,
			["everything", "from", "only", "until", "through"],
			NAMESPACE,
		],
	},
];
