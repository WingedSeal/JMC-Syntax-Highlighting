using JMC.Parser.JMC;
using JMC.Shared;
using System.Diagnostics;

namespace JMC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            GeneralDebug();
            sw.Stop();
            Console.WriteLine($"Time escalped: {sw.ElapsedMilliseconds}ms");
        }
        static void GeneralDebug()
        {
            var extensionData = new ExtensionData();
            var content = File.ReadAllText("test/workspace/main.jmc");
            var sw = new Stopwatch();
            sw.Start();
            var tree = new JMCSyntaxTree(content);
            sw.Stop();
            Console.WriteLine($"Time escalped (Parsing): {sw.ElapsedMilliseconds}ms");
            tree.PrintPretty();
            Console.WriteLine($"Error Count: {tree.Errors.Count}");
            tree.Errors.ForEach(v => Console.WriteLine($"{v.Message}"));
        }

        static void Testing()
        {
            var tree = new JMCSyntaxTree("~ 3 3");
            var query = tree.AsParseQuery();
            var r = query.ExpectAsync(JMCSyntaxNodeType.VEC2).Result;
        }
    }
}