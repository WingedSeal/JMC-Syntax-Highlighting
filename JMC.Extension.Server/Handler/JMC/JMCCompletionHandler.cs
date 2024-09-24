using JMC.Shared;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;

namespace JMC.Extension.Server.Handler.JMC
{
    internal class JMCCompletionHandler(ILogger<JMCCompletionHandler> logger) : CompletionHandlerBase
    {
        private readonly ILogger<JMCCompletionHandler> _logger = logger;
        public static readonly ImmutableArray<string> TriggerChars = [".", "#", " ", "/", "$"];

        public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) =>
            Task.FromResult(request);

        public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var documentUri = request.TextDocument.Uri;

            if (documentUri.Path.EndsWith(".jmc"))
                return GetJMCAsync(request, cancellationToken);
            else if (documentUri.Path.EndsWith(".hjmc"))
                return GetHJMCAsync(request, cancellationToken);
            else
                throw new NotImplementedException();
        }

        private string GetDocumentText(Uri uri)
        {
            var text = File.ReadAllText(uri.LocalPath);
            return text;
        }

        private string GetContext(string text, int cursorPosition,int line)
        {
            string l = text.Split(Environment.NewLine)[line].Trim();
            string beforeCursor = l.Substring(0, Math.Min(cursorPosition,l.Length)).TrimStart();
            int lastDotIndex = beforeCursor.LastIndexOf('.');
            int lastspace = Math.Max(0, beforeCursor.LastIndexOf(' '));
            string word1 = beforeCursor.Substring(lastspace,lastDotIndex - lastspace);

            return word1;
        }

        string[] JMCKeywords = 
        {
            "true","false","import","class","if","else","while","do","for","switch","case","new",
            "advancement","attribute","ban","ban-ip","banlist","bossbar","clear","clone","damage","data","datapack","debug","defaultgamemode","deop","difficulty","effect","enchant","execute","experience","fill","fillbiome","forceload","function","gamemode","gamerule","give","help","item","jfr","kick","kill","list","locate","loot","me","msg","op","pardon","pardon-ip","particle","perf","place","playsound","publish","random","recipe","reload","return","ride","save-all","save-off","save-on","say","schedule","scoreboard","seed","setblock","setidletimeout","setworldspawn","spawnpoint","spectate","spreadplayers","stop","stopsound","summon","tag","team","teammsg","teleport","tell","tellraw","tick","time","title","tm","tp","transfer","trigger","w","weather","whitelist","worldborder","xp"
        };

        Dictionary<string, string> JMCSnippets = new()
        {
            {"__tick__", "function __tick__() {\r\n    say \"Loop\"; // This will run every tick.\r\n}"},
            {"function", "function name() {\r\n    say \"hello\";\r\n}"},
            {"class", "class name {\r\n}"},
            {"if", "if (true) {\r\n}"},
            {"elif", "else if (true) {\r\n}"},
            {"else", "else {\r\n}"},
            {"while", "while (condition) {\r\n}"},
            {"dowhile", "do {\r\n} while (condition)"},
            {"for", "for (condition) {\r\n}"},
            {"switch", "switch (variable) {\r\ncase 1:\r\n    say \"hi!\";\r\n}"}
        };


        private Task<CompletionList> GetJMCAsync(CompletionParams request, CancellationToken cancellationToken)
        {
            var list = new List<CompletionItem>();
            var file = ExtensionDatabase.Workspaces.GetJMCFile(request.TextDocument.Uri);

            if (request.Context == null || file == null)
                return Task.FromResult(CompletionList.From(list));

            var workspace = ExtensionDatabase.Workspaces.GetWorkspaceByUri(file.DocumentUri);
            if (workspace == null)
                return Task.FromResult(CompletionList.From(list));

            var triggerChar = request.Context.TriggerCharacter;
            var triggerType = request.Context.TriggerKind;
            //variables
            if (triggerChar != null &&
                triggerChar == "$" &&
                triggerType == CompletionTriggerKind.TriggerCharacter)
            {
                var arr = workspace.GetAllJMCVariableNames().AsSpan();
                for (var i = 0; i < arr.Length; i++)
                {
                    ref var v = ref arr[i];
                    list.Add(new()
                    {
                        Label = v[1..],
                        Kind = CompletionItemKind.Variable
                    });
                }
            }
            
            //normal case
            else if (triggerChar != null &&
                triggerChar == "$" &&
                triggerType == CompletionTriggerKind.TriggerCharacter)
                return GetJMCHirechyCompletionAsync(request, cancellationToken);
            //class case
            else if (triggerChar != null &&
                triggerChar == "." &&
                triggerType == CompletionTriggerKind.TriggerCharacter)
            {
                var text = workspace.GetJMCFile(request.TextDocument.Uri).SyntaxTree.RawText;//GetDocumentText(request.TextDocument.Uri.ToUri());
                var ctx = GetContext(text, request.Position.Character, request.Position.Line);
                if (FunctionsContextManager.contexts.ContainsKey(ctx))
                foreach (var item in FunctionsContextManager.contexts[ctx])
                {
                    list.Add(new()
                    {
                        Label = item.name,
                        Detail = item.usage,
                        Kind = CompletionItemKind.Function,
                    });
                }
            }
            else
            {
                //from .jmc
                var funcs = workspace.GetAllJMCFunctionNames().AsSpan();
                for (var i = 0; i < funcs.Length; i++)
                {
                    ref var v = ref funcs[i];
                    list.Add(new()
                    {
                        Label = v,
                        Kind = CompletionItemKind.Function,
                        InsertText = v +"();"
                    });
                }
                
                var cls = workspace.GetAllJMCClassNames().AsSpan();
                for (var i = 0; i < cls.Length; i++)
                {
                    ref var v = ref cls[i];
                    list.Add(new()
                    {
                        Label = v,
                        Kind = CompletionItemKind.Class
                    });
                }
                //from built-in
                var builtIn = ExtensionData.JMCBuiltInFunctions.DistinctBy(v => v.Class).ToArray().AsSpan();
                //classes
                for (var i = 0; i < builtIn.Length; i++)
                {
                    ref var v = ref builtIn[i];
                    list.Add(new()
                    {
                        Label = v.Class,
                        Kind = CompletionItemKind.Class,
                    });
                }

                //snippets
                foreach (var item in JMCSnippets)
                {
                    list.Add(new()
                    {
                        Label = item.Key,
                        Kind = CompletionItemKind.Snippet,
                        InsertText = item.Value
                    });
                }

                //keywords
                foreach (var item in JMCKeywords)
                {
                    list.Add(new()
                    {
                        Label = item,
                        Kind = CompletionItemKind.Keyword,
                    });
                }

                //functions
                list.Add(new()
                {
                    Label = "print",
                    Kind = CompletionItemKind.Function
                });
                list.Add(new()
                {
                    Label = "printf",
                    Kind = CompletionItemKind.Function
                });
            }

            return Task.FromResult(CompletionList.From(list));
        }

        private Task<CompletionList> GetJMCHirechyCompletionAsync(CompletionParams request, CancellationToken cancellationToken)
        {
            var list = new List<CompletionItem>();

            return Task.FromResult(new CompletionList(list));
        }

        private Task<CompletionList> GetHJMCAsync(CompletionParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CompletionList());
        }

        protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            ResolveProvider = true,
            TriggerCharacters = TriggerChars,
            DocumentSelector = ExtensionSelector.JMC
        };
    }
}
