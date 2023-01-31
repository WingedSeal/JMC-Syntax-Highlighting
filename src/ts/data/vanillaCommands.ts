const selectors: string[] = ["@p", "@a", "@r", "@s", "@e"];
export const NAMESPACE: string = "NAMESPACE";

interface CommandArgument {
	command: string;
	args: (string[] | string)[];
}

export const CommandArguments: CommandArgument[] = [
	{
		command: "advancement",
		args: [
			["grant", "revoke"],
			selectors,
			["everything", "from", "only", "until", "through"],
			NAMESPACE
		],
	}
];
