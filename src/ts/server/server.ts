import { ProposedFeatures, createConnection } from "vscode-languageserver/node";
import { JMCServer } from "./jmcserver";

const server = new JMCServer(createConnection(ProposedFeatures.all));
server.register();
server.start();
