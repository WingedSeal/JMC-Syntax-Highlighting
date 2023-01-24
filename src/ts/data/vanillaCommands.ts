const selectors: string[] = ["@p", "@a", "@r", "@s", "@e"];

interface CommandArgument {
	command: string;
	args: string[][];
}

export const CommandArguments: CommandArgument[] = [
	{
		command: "advancement",
		args: [
			["grant", "revoke"],
			selectors,
			["everything", "from", "only", "until", "through"],
		],
	}
];
