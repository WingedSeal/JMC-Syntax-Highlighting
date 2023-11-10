using JMC.Parser.JMC;
using JMC.Parser.JMC.Command;
using JMC.Shared;
using System.Diagnostics;

namespace JMC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ext = new ExtensionData();
            var sw = new Stopwatch();
            sw.Start();
            GeneralDebug();
            sw.Stop();
            Console.WriteLine($"Time escalped: {sw.ElapsedMilliseconds}ms");
        }
        static void GeneralDebug()
        {
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
            var content = File.ReadAllText("test/workspace/main.jmc");
            var tree = new JMCSyntaxTree(content);
            var sw = new Stopwatch();
            sw.Start();
            tree.ModifyAsync(new()
            {
                Range = new(1,1,2,2),
                Text = "amogus",
            });
            sw.Stop();
            Console.WriteLine($"Time escalped (Parsing): {sw.ElapsedMilliseconds}ms");
        }
    }
}