import { CommandData } from "../lexer";
import * as mcc from "./minecraft/command.json";

export const Commands = mcc.root.children as unknown as CommandData[];
export const StartCommand = Commands.map((v) => v.name);
