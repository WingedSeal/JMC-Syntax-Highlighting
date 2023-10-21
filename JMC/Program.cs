using JMC.Extension.Server.Lexer.JMC;

namespace JMC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GeneralDebug();
            //Testing();
        }

        static void GeneralDebug()
        {
            var tree = new JMCSyntaxTree(" import \"amogus\"; function test() {do {} while(1 == 1)} class test {function test() {}} class test2 {}");
            tree.PrintPretty();
            Console.WriteLine($"Error Count: {tree.Errors.Count}");
            tree.Errors.ForEach(v => Console.WriteLine($"{v.Message}"));
        }

        static void Testing()
        {
            var tree = new JMCSyntaxTree(" import \"amogus\"; function test() {do {} while ()} class test {function test() {}} class test2 {}");
        }
    }
}