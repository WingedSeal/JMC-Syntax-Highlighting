using JMC.Extension.Server;
using JMC.Parser.JMC;
using JMC.Shared;

namespace JMC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeneralDebug();
            static void GeneralDebug()
            {
                var extensionData = new ExtensionData();
                var content = File.ReadAllText("test/workspace/main.jmc");
                var tree = new JMCSyntaxTree(content);
                tree.PrintPretty();
                Console.WriteLine($"Error Count: {tree.Errors.Count}");
                tree.Errors.ForEach(v => Console.WriteLine($"{v.Message}"));
            }

            static void Testing()
            {
                var tree = new JMCSyntaxTree(" import \"amogus\"; function test() {do {} while ()} class test {function test() {}} class test2 {}");
                //Testing();
            }
        }
    }
}