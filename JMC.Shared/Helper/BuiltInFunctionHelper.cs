using JMC.Shared.Datas.BuiltIn;

namespace JMC.Shared.Helper
{
    internal static class BuiltInFunctionHelper
    {
        public static string ToDocumentFormat(this JMCBuiltInFunction func)
        {
            var args = string.Join(", ", func.ToDucumentArgs());
            return $"{func.Class}.{func.Function}({args}) -> {func.ReturnType}";
        }

        public static IEnumerable<string> ToDucumentArgs(this JMCBuiltInFunction func) => func.Arguments.Select(v => v.ToDocumentArg());
        public static string ToDocumentArg(this JMCFunctionArgument arg)
        {
            var defaultValue = arg.DefaultValue == null ? "" : $" = {arg.DefaultValue}";
            return $"{arg.Name}: {arg.ArgumentType}{defaultValue}";
        }
    }
}
