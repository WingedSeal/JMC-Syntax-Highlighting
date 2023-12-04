using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC.Error.Base
{
    internal abstract class JMCBaseError
        (string message, ErrorType errorType, Range range, DiagnosticSeverity diagnosticSeverity = DiagnosticSeverity.Error)
    {
        public string Message { get; private set; } = message;
        public ErrorType ErrorType { get; private set; } = errorType;
        public DiagnosticSeverity DiagnosticSeverity { get; private set; } = diagnosticSeverity;
        public Range Range { get; private set; } = range;
    }
}
