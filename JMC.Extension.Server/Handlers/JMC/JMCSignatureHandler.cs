using JMC.Extension.Server.Datas;
using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Handlers.JMC
{
    internal class JMCSignatureHandler : SignatureHelpHandlerBase
    {
        public override async Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken cancellationToken)
        {
            var doc = ExtensionData.Workspaces.GetJMCFile(request.TextDocument.Uri);
            var workspace = ExtensionData.Workspaces.GetWorkspaceByDocument(request.TextDocument.Uri);
            if (doc == null || workspace == null) return null;

            var lexer = doc.Lexer;
            var token = lexer.GetJMCToken(request.Position);
            if (token == null) return null;
            var tokenIndex = lexer.Tokens.IndexOf(token);

            if (!request.Context.IsRetrigger && request.Context.TriggerCharacter == "(")
            {
                var preToken = lexer.Tokens[tokenIndex - 1];
                if (preToken == null || preToken.TokenType != JMCTokenType.LITERAL) return null;
                var splited = preToken.Value.Split('.').Where(v => !string.IsNullOrEmpty(v));
                if (splited.Count() > 2 || splited.Count() == 1) return null;
                var query = ExtensionData.JMCBuiltInFunctions.GetFunction(splited.ElementAt(0), splited.ElementAt(1));
                if (query == null) return null;
                var signatureHelp = new SignatureHelp()
                {
                    Signatures = new Container<SignatureInformation>(new SignatureInformation()
                    {
                        ActiveParameter = 0,
                        Documentation = query.Summary,
                        Label = query.ToDocumentFormat(),
                        Parameters = new Container<ParameterInformation>(query.Arguments.Select(v => new ParameterInformation()
                        {
                            Label = v.ToDocumentArg(),
                            Documentation = v.Summary
                        }))
                    }),
                    ActiveParameter = 0,
                    ActiveSignature = 0
                };
                return signatureHelp;
            }
            else if (request.Context.TriggerCharacter == "=")
            {
                //get current function's range
                var ranges = lexer.GetFunctionRanges();
                var range = ranges.FirstOrDefault(v => v.Value.Contains(request.Position));
                if (range.Value == null || range.Key == null) return null;

                //get builtin function
                var splited = range.Key.Split('.');
                var @class = splited[0];
                var func = splited[1];
                var builtin = ExtensionData.JMCBuiltInFunctions.Find(v => v.Class == @class && v.Function == func);
                if (builtin == null) return null;

                //get argIndex
                var argName = lexer.Tokens[tokenIndex - 1].Value;
                var arg = builtin.Arguments.FirstOrDefault(v => v.Name == argName);
                var argIndex = Array.IndexOf(builtin.Arguments, arg);
                if (argIndex == -1) return null;

                var signatureHelp = new SignatureHelp()
                {
                    Signatures = new Container<SignatureInformation>(new SignatureInformation()
                    {
                        ActiveParameter = argIndex,
                        Documentation = builtin.Summary,
                        Label = builtin.ToDocumentFormat(),
                        Parameters = new Container<ParameterInformation>(builtin.Arguments.Select(v => new ParameterInformation()
                        {
                            Label = v.ToDocumentArg(),
                            Documentation = v.Summary
                        }))
                    }),
                    ActiveParameter = argIndex,
                    ActiveSignature = 0
                };
                return signatureHelp;

            }
            else if (request.Context.IsRetrigger && request.Context.ActiveSignatureHelp != null)
            {
                var commaCount = GetCommaCount(lexer.Tokens, tokenIndex);
                var signatureHelp = request.Context.ActiveSignatureHelp;
                return new SignatureHelp()
                {
                    Signatures = signatureHelp.Signatures,
                    ActiveSignature = signatureHelp.ActiveSignature,
                    ActiveParameter = commaCount,
                };
            }
            else if (request.Context.TriggerCharacter == ",")
            {
                //get current function's range
                var ranges = lexer.GetFunctionRanges();
                var range = ranges.FirstOrDefault(v => v.Value.Contains(request.Position));
                if (range.Value == null || range.Key == null) return null;

                //get builtin function
                var splited = range.Key.Split('.');
                var @class = splited[0];
                var func = splited[1];
                var builtin = ExtensionData.JMCBuiltInFunctions.Find(v => v.Class == @class && v.Function == func);
                if (builtin == null) return null;

                var tokens = lexer.Tokens.Where(v => range.Value.Contains(v.Range.Start)).ToArray();
                var commaCount = GetCommaCount(tokens[1..]);

                var signatureHelp = new SignatureHelp()
                {
                    Signatures = new Container<SignatureInformation>(new SignatureInformation()
                    {
                        ActiveParameter = commaCount,
                        Documentation = builtin.Summary,
                        Label = builtin.ToDocumentFormat(),
                        Parameters = new Container<ParameterInformation>(builtin.Arguments.Select(v => new ParameterInformation()
                        {
                            Label = v.ToDocumentArg(),
                            Documentation = v.Summary
                        }))
                    }),
                    ActiveParameter = commaCount,
                    ActiveSignature = 0
                };
                return signatureHelp;
            }
            //TODO support for "=" "," for trigger and retrigger
            return null;
        }

        private int GetCommaCount(IEnumerable<JMCToken> tokens, int tokenIndex)
        {
            var arr = tokens.ToArray().AsSpan();
            var cpCount = 0;
            var mpCount = 0;
            var commaCount = 0;
            var parenCount = 1;
            for (int i = tokenIndex; i != -1; i--)
            {
                ref var token = ref arr[i];
                if (token.TokenType == JMCTokenType.LCP) cpCount--;
                else if (token.TokenType == JMCTokenType.RCP) cpCount++;
                else if (token.TokenType == JMCTokenType.LMP) mpCount--;
                else if (token.TokenType == JMCTokenType.RMP) mpCount++;
                else if (token.TokenType == JMCTokenType.LPAREN) parenCount--;
                else if (token.TokenType == JMCTokenType.RPAREN) parenCount++;
                else if (cpCount == 0 && mpCount == 0 && token.TokenType == JMCTokenType.COMMA) commaCount++;


                if (parenCount == 0) break;
            }

            return commaCount;
        }

        private int GetCommaCount(IEnumerable<JMCToken> tokens)
        {
            var arr = tokens.ToArray().AsSpan();
            var cpCount = 0;
            var mpCount = 0;
            var commaCount = 0;
            var parenCount = 1;
            for (int i = 0; i < tokens.Count() - 1; i++)
            {
                ref var token = ref arr[i];
                if (token.TokenType == JMCTokenType.LCP) cpCount++;
                else if (token.TokenType == JMCTokenType.RCP) cpCount--;
                else if (token.TokenType == JMCTokenType.LMP) mpCount++;
                else if (token.TokenType == JMCTokenType.RMP) mpCount--;
                else if (token.TokenType == JMCTokenType.LPAREN) parenCount++;
                else if (token.TokenType == JMCTokenType.RPAREN) parenCount--;
                else if (cpCount == 0 && mpCount == 0 && token.TokenType == JMCTokenType.COMMA) commaCount++;


                if (parenCount == 0) break;
            }

            return commaCount;
        }

        protected override SignatureHelpRegistrationOptions CreateRegistrationOptions(SignatureHelpCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = LanguageSelector.JMC,
            RetriggerCharacters = new string[]
            {
                " ",",","="
            },
            TriggerCharacters = new string[]
            {
                "(", "=", ","
            }
        };
    }
}
