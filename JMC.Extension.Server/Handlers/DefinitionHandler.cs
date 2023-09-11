using JMC.Extension.Server.Datas;
using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.JMC;
using JMC.Extension.Server.Lexer.JMC.Types;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace JMC.Extension.Server.Handlers
{
    internal sealed class DefinitionHandler : DefinitionHandlerBase
    {
        private readonly ILogger<DefinitionHandler> _logger;

        public DefinitionHandler(ILogger<DefinitionHandler> logger) => _logger = logger;

        public override async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            var file = ExtensionData.Workspaces.GetJMCFile(request.TextDocument.Uri);
            if (file == null)
            {
                var link = new List<LocationOrLocationLink>();
                return link;
            }
            else
            {
                var link = new List<LocationOrLocationLink>();
                var lexer = file.Lexer;
                var lexerTokens = lexer.Tokens;
                var currentToken = lexer.GetJMCToken(request.Position);
                if (currentToken == null) return link;

                var tokenIndex = file.Lexer.Tokens.IndexOf(currentToken);

                //function defines
                if (currentToken.TokenType == JMCTokenType.LITERAL &&
                    lexerTokens[tokenIndex - 1].TokenType == JMCTokenType.FUNCTION)
                {
                    var funcs = ExtensionData.Workspaces.GetJMCFunctionCalls();
                    var query = currentToken.Value.Split(' ').Last();
                    foreach (var func in funcs)
                    {
                        var matches = func.Tokens.Where(v => v.Value == query);
                        if (matches.Any())
                        {
                            foreach (var match in matches)
                            {
                                var location = new LocationLink()
                                {
                                    OriginSelectionRange = currentToken.Range,
                                    TargetRange = match.Range,
                                    TargetSelectionRange = match.Range,
                                    TargetUri = func.DocumentUri
                                };
                                link.Add(new(location));
                            }
                        }
                    }
                }
                //function calls
                else if (currentToken.TokenType == JMCTokenType.LITERAL &&
                    lexerTokens[tokenIndex + 1].TokenType == JMCTokenType.LPAREN)
                {
                    var defines = ExtensionData.Workspaces.GetJMCFunctionDefines();
                    foreach (var define in defines)
                    {
                        var result = define.Tokens.Find(v => v.Value.Split(' ').Last() == currentToken.Value);
                        if (result != null)
                        {
                            var location = new LocationLink
                            {
                                OriginSelectionRange = currentToken.Range,
                                TargetRange = result.Range,
                                TargetSelectionRange = result.Range,
                                TargetUri = define.DocumentUri
                            };
                            link.Add(location);
                            break;
                        }
                    }
                }
                //variables
                else if (JMCLexer.VariableTypes.Contains(currentToken.TokenType))
                {
                    if (JMCLexer.VariableCallTypes.Contains(currentToken.TokenType))
                    {
                        var start = currentToken.Range.Start;
                        var newOffset = currentToken.Offset + currentToken.Value.Length - 4;
                        var end = JMCLexer.OffsetToPosition(newOffset, lexer.RawText);
                        var range = new Range(start, end);
                        var vars = ExtensionData.Workspaces.GetJMCVariables();
                        vars.ForEach(v =>
                        {
                            var matches = v.Tokens.Where(x => x.Value.Split('.').ElementAt(0) == currentToken.Value[..^4]);
                            var arr = matches.ToArray().AsSpan();
                            for (var i = 0; i < arr.Length; i++)
                            {
                                ref var match = ref arr[i];
                                if (JMCLexer.VariableCallTypes.Contains(match.TokenType))
                                {
                                    var fileLexer = ExtensionData.Workspaces.GetJMCFile(v.DocumentUri)?.Lexer;
                                    if (fileLexer == null) continue;
                                    var offset = match.Offset + match.Value.Length - 4;
                                    var start = match.Range.Start;
                                    var end = JMCLexer.OffsetToPosition(offset, fileLexer.RawText);
                                    var newRange = new Range(start, end);
                                    var location = new LocationLink()
                                    {
                                        OriginSelectionRange = range,
                                        TargetRange = newRange,
                                        TargetSelectionRange = newRange,
                                        TargetUri = v.DocumentUri
                                    };
                                    link.Add(new(location));
                                }
                                else
                                {
                                    var location = new LocationLink()
                                    {
                                        OriginSelectionRange = range,
                                        TargetRange = match.Range,
                                        TargetSelectionRange = match.Range,
                                        TargetUri = v.DocumentUri
                                    };
                                    link.Add(new(location));
                                }
                            }
                        });
                        return link;
                    }
                    else
                    {
                        var vars = ExtensionData.Workspaces.GetJMCVariables();
                        vars.ForEach(v =>
                        {
                            var matches = v.Tokens.Where(v => v.Value.Split('.').ElementAt(0) == currentToken.Value);
                            var arr = matches.ToArray().AsSpan();
                            for (var i = 0; i < arr.Length; i++)
                            {
                                ref var match = ref arr[i];
                                if (JMCLexer.VariableCallTypes.Contains(match.TokenType))
                                {
                                    var fileLexer = ExtensionData.Workspaces.GetJMCFile(v.DocumentUri)?.Lexer;
                                    if (fileLexer == null) continue;
                                    var offset = match.Offset + match.Value.Length - 4;
                                    var start = match.Range.Start;
                                    var end = JMCLexer.OffsetToPosition(offset, fileLexer.RawText);
                                    var newRange = new Range(start, end);
                                    var location = new LocationLink()
                                    {
                                        OriginSelectionRange = currentToken.Range,
                                        TargetRange = newRange,
                                        TargetSelectionRange = newRange,
                                        TargetUri = v.DocumentUri
                                    };
                                    link.Add(new(location));
                                }
                                else
                                {
                                    var location = new LocationLink()
                                    {
                                        OriginSelectionRange = currentToken.Range,
                                        TargetRange = match.Range,
                                        TargetSelectionRange = match.Range,
                                        TargetUri = v.DocumentUri
                                    };
                                    link.Add(new(location));
                                }
                            }
                        });
                        return link;
                    }
                }
                _logger.LogInformation($"Definition Token: {LoggerHelper.ObjectToJson(currentToken)}");
                return link;
            }
        }

        protected override DefinitionRegistrationOptions CreateRegistrationOptions(
            DefinitionCapability capability, ClientCapabilities clientCapabilities)
            => new()
            {
                DocumentSelector = LanguageSelector.JMC,
            };
    }
}
