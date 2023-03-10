import { languages } from "vscode";
import { SELECTOR } from "../data/common";
import { getCurrentCommand } from "../helpers/documentHelper";
import { BuiltInFunctions, methodInfoToDoc } from "../data/builtinFunctions";

export function RegisterSignatureSign() {
	languages.registerSignatureHelpProvider(
		SELECTOR,
		{
			async provideSignatureHelp(document, position, token, ctx) {
				const linePrefix = await getCurrentCommand(
					document.getText(),
					document.offsetAt(position)
				);
				if (ctx.triggerCharacter === "(") {
					const methods = BuiltInFunctions.flatMap((v) => {
						const target = v.methods.filter((value) => {
							return linePrefix.endsWith(
								`${v.class}.${value.name}(`
							);
						});
						return target;
					});
					const method = methods[0];

					return {
						signatures: [
							{
								label: methodInfoToDoc(method),
								parameters: method.args.flatMap((v) => {
									const def =
										v.default !== undefined
											? ` = ${v.default}`
											: "";
									const arg = `${v.name}: ${v.type}${def}`;
									return {
										label: arg,
										documentation: v.doc,
									};
								}),
								documentation: method.doc,
							},
						],
						activeSignature: 0,
						activeParameter: 0,
					};
				}
				return undefined;
			},
		},
		"(",
		","
	);
}
