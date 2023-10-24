namespace JMC.Shared.Datas.BuiltIn
{
    internal record JMCBuiltInFunction(string Class, string Function, string Summary, JMCFunctionArgument[] Arguments, JMCFunctionReturnType ReturnType);

    internal record JMCFunctionArgument(string ArgumentType, string Name, string? Summary, string? DefaultValue);
}
