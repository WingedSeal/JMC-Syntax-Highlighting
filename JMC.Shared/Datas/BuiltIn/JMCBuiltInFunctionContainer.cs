namespace JMC.Shared.Datas.BuiltIn
{
    internal class JMCBuiltInFunctionContainer : List<JMCBuiltInFunction>
    {
        public JMCBuiltInFunctionContainer(IEnumerable<JMCBuiltInFunction> list) : base(list) { }
        public IEnumerable<JMCBuiltInFunction> GetFunctions(string @class) => this.Where(v => v.Class == @class);
        public JMCBuiltInFunction? GetFunction(string @class, string func) => Find(v => v.Class == @class && v.Function == func);
        public IEnumerable<JMCFunctionArgument> GetRequiredArgs(JMCBuiltInFunction builtInFunction)
        {
            var func = Find(v => v.Equals(builtInFunction));
            if (func == null) return [];
            return func.Arguments.Where(v => v.DefaultValue == null);
        }
    }
}
