using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Datas;
using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.JMC;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Handlers.JMC
{
    internal class JMCCompletionHandler : CompletionHandlerBase
    {
        private readonly ILogger<JMCCompletionHandler> _logger;

        public JMCCompletionHandler(ILogger<JMCCompletionHandler> logger)
        {
            _logger = logger;
        }
        public override async Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) => request;
        public override async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var file = ExtensionData.Workspaces.GetJMCFile(request.TextDocument.Uri);
            if (file == null)
            {
                return new();
            }

            var list = new List<CompletionItem>();

            var lexer = file.Lexer;
            var workspace = ExtensionData.Workspaces.GetWorkspaceByDocument(request.TextDocument.Uri);
            if (workspace == null) return list;
            var funcs = workspace.GetJMCFunctionDefines()
                .SelectMany(v => v.Tokens)
                .DistinctBy(v => v.Value);

            //function completion
            if (request.Context?.TriggerCharacter == ".")
            {
                var requestToken = lexer.GetJMCToken(request.Position);
                if (requestToken != null)
                {
                    var index = lexer.Tokens.IndexOf(requestToken);
                    var token = lexer.Tokens[index];
                    if (token.TokenType == Lexer.JMC.Types.JMCTokenType.LITERAL)
                    {
                        var splited = token.Value
                            .Split(' ')
                            .Last()
                            .Split('.')
                            .Where(v => !string.IsNullOrWhiteSpace(v));
                        if (splited.Count() == 1)
                        {
                            var builtinFuncs = ExtensionData.JMCBuiltInFunctions.GetFunctions(splited.ElementAt(0));
                            if (builtinFuncs.Any())
                            {
                                return CompletionList.From(builtinFuncs.Select(v => new CompletionItem()
                                {
                                    Label = v.Function,
                                    Kind = CompletionItemKind.Function,
                                    InsertText = $"{v.Function}"
                                }));
                            }
                        }

                        var result = FunctionHierarchy.GetHierachy(funcs.Select(v => v.Value), splited);
                        return CompletionList.From(result.Select(value => new CompletionItem()
                        {
                            Label = value.FuncName,
                            Kind = value.Type == FunctionHierarchyType.CLASS ? CompletionItemKind.Class : CompletionItemKind.Function,
                            InsertText = value.Type == FunctionHierarchyType.CLASS ? value.FuncName : $"{value.FuncName}()",
                        }));
                    }
                    else if (JMCLexer.VariableTypes.Contains(token.TokenType))
                    {
                        list.Add(new()
                        {
                            Label = "get",
                            Kind = CompletionItemKind.Function,
                            InsertText = "get()"
                        });
                        return list;
                    }
                }
            }
            var vars = workspace.GetJMCVariables()
                .SelectMany(v => v.Tokens)
                .Where(v => !v.Value.EndsWith(".get", StringComparison.CurrentCulture))
                .DistinctBy(v => v.Value);

            //normal completion
            var funcValue = FunctionHierarchy.GetFirstHierarchy(funcs.Select(v => v.Value).ToArray());
            foreach (var value in funcValue)
            {
                list.Add(new()
                {
                    Label = value.FuncName,
                    Kind = value.Type == FunctionHierarchyType.CLASS ? CompletionItemKind.Class : CompletionItemKind.Function,
                    InsertText = value.Type == FunctionHierarchyType.CLASS ? value.FuncName : $"{value.FuncName}()",
                });
            }

            var jmcFuncs = ExtensionData.JMCBuiltInFunctions.Select(v => v.Class).Distinct();
            foreach (var jmcFunc in jmcFuncs)
            {
                list.Add(new()
                {
                    Label = jmcFunc,
                    Kind = CompletionItemKind.Class,
                    InsertText = jmcFunc,
                });
            }

            foreach (var v in vars)
            {
                list.Add(new()
                {
                    Label = v.Value[1..],
                    Kind = CompletionItemKind.Variable,
                    InsertText = v.Value
                });
            }

            return new(list);
        }


        protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            ResolveProvider = true,
            TriggerCharacters = new string[]
                {
                    ".", "#", " ", "/", "$"
                },
            DocumentSelector = LanguageSelector.JMC
        };
    }
}
