import { MC_ITEMS } from "../data/mcDatas";
import { END_TOKEN, TokenData, TokenType } from "../lexer";

export function toSingleToken(tokens: TokenData[]): TokenData | null {
	if (tokens.length <= 0) return null;
	let text = "";

	for (let i = 0; i < tokens.length; i++) {
		const token = tokens[i];
		const preToken = tokens[i - 1];
		if (preToken) {
			const prePos = preToken.pos + preToken.value.length;
			text += " ".repeat(token.pos - prePos) + token.value;
		} else text += token.value;
	}

	return {
		type: TokenType.COMMAND,
		pos: tokens[0].pos,
		value: text,
	};
}

export function splitToken(token: TokenData): TokenData[] {
	const text = token.value;
	const datas: TokenData[] = [];
	const splited = text
		.split(/(\s+|;|@[parse](?:\[.*\])?|\{.*\})/)
		.filter((v) => v !== "" && v !== undefined);
	let pos = token.pos;
	for (let i = 0; i < splited.length; i++) {
		const current = splited[i];

		if (current === ";") {
			datas.push({
				type: TokenType.SEMI,
				pos: pos,
				value: current,
			});
		} else if (/^\s+$/.test(current)) {
			pos += current.length;
			continue;
		} else {
			const t: TokenData = {
				type: TokenType.COMMAND_UNKNOWN,
				pos: pos,
				value: current,
			};
			t.type = tokenizeCommand(t);
			datas.push(t);
			if (t.type === TokenType.COMMAND_VARIABLECALL) {
				const s = current
					.split(/(\.)|[()]/)
					.filter((v) => v !== "" && v !== undefined);

				datas.push({
					type: TokenType.COMMAND_VARIABLE,
					pos: pos,
					value: s[0],
				});

				datas.push({
					type: TokenType.COMMAND_FUNCCALL,
					pos: pos + 1 + s[0].length,
					value: s[2],
				});
			}
		}
		pos += current.length;
	}

	return datas;
}

export function tokenizeCommand(token: TokenData): TokenType {
	const value = token.value;
	//mc values
	if (
		MC_ITEMS.find(
			(v) => value.startsWith(v) || value.startsWith("minecraft:" + v)
		)
	)
		return TokenType.COMMAND_ITEM_STACK;
	else if (value.includes(":")) return TokenType.COMMAND_NAMESAPCE;
	else if (value.startsWith("{")) return TokenType.COMMAND_JSON;
	else if (/^@[parse]/.test(value)) return TokenType.COMMAND_SELECTOR;
	//jmc values
	else if (/^\$\S+\.get\(\)$/.test(value))
		return TokenType.COMMAND_VARIABLECALL;
	else if (/^\S+\.\S+\(\)$/.test(value)) return TokenType.COMMAND_FUNCCALL;
	else if (/^\$/.test(value)) return TokenType.COMMAND_VARIABLE;
	//values
	else if (/^-?(?:(\d*\.\d+)|\d+)[lsb]?$/.test(token.value))
		return TokenType.COMMAND_NUMBER;
	else if (/^\w+$/.test(value)) return TokenType.COMMAND_LITERAL;
	else if (/^\S+$/.test(value)) return TokenType.COMMAND_VALUE;
	//invalid
	else return TokenType.COMMAND_INVALID;
}

export function joinCommands(tokens: TokenData[]): TokenData[] {
	let datas: TokenData[] = [];

	for (let i = 0; i < tokens.length; i++) {
		const token = tokens[i];
		if (!TokenType[token.type].startsWith("COMMAND")) datas.push(token);
		else {
			const cTemp: TokenData[] = [];
			for (; i < tokens.length; i++) {
				const c = tokens[i];
				cTemp.push(c);
				if (!TokenType[c.type].startsWith("COMMAND")) break;
			}

			const command = toSingleToken(cTemp);
			if (command) {
				const cData = splitToken(command);
				datas = datas.concat(cData);
			}
		}
	}

	return datas;
}
