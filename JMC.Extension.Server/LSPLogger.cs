using Microsoft.Extensions.Logging;

namespace JMC.Extension.Server
{
    internal class LSPLogger
    {
        private readonly ILogger<LSPLogger> _logger;

        public LSPLogger(ILogger<LSPLogger> logger)
        {
            _logger = logger;
        }
    }
}
