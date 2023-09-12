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
import * as os from "os";

export function activate(context: ExtensionContext) {
  var serverExe = os.platform() == "win32" ? "JMCServer.exe" : "JMCServer";

  let serverOptions: ServerOptions = {
    // run: { command: serverExe, args: ['-lsp', '-d'] },
    run: {
      command: path.join(__dirname, "server", serverExe),
      args: ["-lsp", "-d"],
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
        serverExe
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
