namespace JMC.Extension.Server.Datas.BuiltIn
{
    internal record JMCBuiltInFunction(string Class, string Function, string Summary, JMCFunctionArgument[] Arguments, JMCFunctionReturnType ReturnType);

    internal record JMCFunctionArgument(JMCFunctionArgumentType ArgumentType, string Name, string? Summary, string? DefaultValue);
}
