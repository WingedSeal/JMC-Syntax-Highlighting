/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */
// tslint:disable
"use strict";

import { workspace, ExtensionContext } from "vscode";
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
  createServerPipeTransport,
} from "vscode-languageclient/node";
import { Trace } from "vscode-jsonrpc/node";
import path = require("path");

export function activate(context: ExtensionContext) {
  // The server is implemented in node
  let serverExe = "dotnet";

  // let serverExe = "D:\\Development\\Omnisharp\\csharp-language-server-protocol\\sample\\SampleServer\\bin\\Debug\\netcoreapp2.0\\win7-x64\\SampleServer.exe";
  // let serverExe = "D:/Development/Omnisharp/omnisharp-roslyn/artifacts/publish/OmniSharp.Stdio.Driver/win7-x64/OmniSharp.exe";
  // The debug options for the server
  // let debugOptions = { execArgv: ['-lsp', '-d' };5

  // If the extension is launched in debug mode then the debug server options are used
  // Otherwise the run options are used
  let serverOptions: ServerOptions = {
    // run: { command: serverExe, args: ['-lsp', '-d'] },
    run: {
      command: serverExe,
      args: [
        "C:\\Users\\User\\Documents\\Github\\JMC-Syntax-Highlighting-cslsp\\server\\jmcserver\\bin\\Debug\\net6.0\\win7-x64\\JMCLSP.exe",
      ],
      transport: TransportKind.pipe,
    },
    // debug: { command: serverExe, args: ['-lsp', '-d'] }
    debug: {
      command: path.join(
        __dirname,
        "..",
        "..",
        "..",
        "JMC.Extension.Server",
        "bin",
        "Debug",
        "net7.0",
        "JMC.Extension.Server.exe"
      ),
      args: ["-lsp", "-d"],
      transport: TransportKind.pipe,
      runtime: "",
    },
  };

  // Options to control the language client
  let clientOptions: LanguageClientOptions = {
    // Register the server for plain text documents
    documentSelector: [
      {
        pattern: "**/*.jmc",
        language: "jmc",
      },
    ],
    progressOnInitialization: true,
    synchronize: {
      // Synchronize the setting section 'languageServerExample' to the server
      configurationSection: "jmc",
      fileEvents: workspace.createFileSystemWatcher("**/*.jmc"),
    },
  };

  // Create the language client and start the client.
  const client = new LanguageClient(
    "jmcLog",
    "JMC Logs",
    serverOptions,
    clientOptions
  );
  client.registerProposedFeatures();
  client.trace = Trace.Verbose;
  let disposable = client.start();

  // Push the disposable to the context's subscriptions so that the
  // client can be deactivated on extension deactivation
  context.subscriptions.push(disposable);
}
