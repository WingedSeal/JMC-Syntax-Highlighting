import { START_COMMAND } from "./data/commands";
import { StatementRange, joinCommandData } from "./helpers/general";
import { TokenData, TokenType } from "./lexer";
import ExtensionLogger from "./server/extlogger";

export enum CommandType {
	INVALID,
	LITERAL,
	VALUE,
	FUNCTION,
	SELECTOR,
	FLOAT,
	INT,
	NAMESPACE,
	POS,
	END,
}

export interface CommandToken {
	type: CommandType;
	pos: number;
	value: string;
}

export class CommandLexer {
	tokens: CommandToken[][];

	private logger: ExtensionLogger;

	constructor(lexerTokens: TokenData[]) {
		this.logger = new ExtensionLogger("CommandLexer");
		this.tokens = this.init(lexerTokens);
	}

	private init(tokens: TokenData[]): CommandToken[][] {
		const datas: CommandToken[][] = [];
		for (let i = 0; i < tokens.length; i++) {
			const token = tokens[i];
			const preToken = tokens[i - 1];
			//check if it is not inside a bracket
			if (
				(!preToken || preToken.type !== TokenType.LPAREN) &&
				START_COMMAND.includes(token.value)
			) {
				//get tokens till ";"
				const temp: TokenData[] = [];
				let t = token;
				while (t && t.type !== TokenType.SEMI) {
					t = tokens[i];
					temp.push(t);
					i++;
				}
				//to avoid bugs, move 1 backward
				i--;
				const joined = joinCommandData(temp.filter((v) => v)).map(
					this.parseCommand.bind(this)
				);
				datas.push(joined);
			}
		}
		return datas;
	}

	getRanges(): StatementRange[] {
		const ranges: StatementRange[] = [];

		for (const command of this.tokens) {
			const first = command[0];
			const last = command[command.length - 1];
			if (first && last && first !== last)
				ranges.push({
					start: first.pos,
					end: last.pos + last.value.length,
				});
		}

		return ranges;
	}

	update(tokens: TokenData[]) {
		this.tokens = this.init(tokens);
	}

	parseCommand(token: TokenData): CommandToken {
		const type = this.tokenize(token);
		return {
			type: type,
			pos: token.pos,
			value: token.value,
		};
	}

	tokenize(token: TokenData): CommandType {
		if (token.type === TokenType.LITERAL && !token.value.match(/^\w+$/))
			return CommandType.VALUE;
		else if (token.type === TokenType.LITERAL) return CommandType.LITERAL;
		else if (token.type === TokenType.COMMAND_NAMESAPCE)
			return CommandType.NAMESPACE;
		else if (token.type === TokenType.COMMAND_FC)
			return CommandType.FUNCTION;
		else if (
			token.type === TokenType.SELECTOR_INFO ||
			token.type === TokenType.COMMAND_SELECTOR
		)
			return CommandType.SELECTOR;
		else if (token.type === TokenType.FLOAT) return CommandType.FLOAT;
		else if (token.type === TokenType.INT) return CommandType.INT;
		else if (/^(~|\^)?-?\d*(?:\.\d+)?(?!\.)$/.test(token.value))
			return CommandType.POS;
		else if (token.type === TokenType.SEMI) return CommandType.END;
		else return CommandType.INVALID;
	}
}
