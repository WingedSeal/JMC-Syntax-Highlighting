using JMC.Parser.JMC;
using JMC.Shared;
using System.Diagnostics;

namespace JMC
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var ext = new ExtensionData();
            var sw = new Stopwatch();
            sw.Start();
            await GeneralDebugAsync();
            sw.Stop();
            Console.WriteLine($"Time escalped: {sw.ElapsedMilliseconds}ms");
            return 1;
        }
        static void LexerDebug()
        {
            var content = File.ReadAllText("test/workspace/main.jmc");
            var lexer = new JMCLexer(content);
            var result = lexer.StartLexing();
        }

        static async Task GeneralDebugAsync()
        {
            var content = File.ReadAllText("test/workspace/main.jmc");
            var sw = new Stopwatch();
            sw.Start();
            var tree = await new JMCSyntaxTree().InitializeAsync(content);
            sw.Stop();
            tree.PrintPretty();
            Console.WriteLine($"Error Count: {tree.Errors.Count}");
            tree.Errors.ForEach(v => Console.WriteLine($"{v.Message}"));
            Console.WriteLine($"Time escalped (Parsing): {sw.ElapsedMilliseconds}ms");
        }

        static async Task TestingAsync()
        {
            var tree = await new JMCSyntaxTree().InitializeAsync(@"()=>{}");
            var q = tree.AsParseQuery();
            var m = q.ExpectArrowFunction(out var s);
        }
    }
}