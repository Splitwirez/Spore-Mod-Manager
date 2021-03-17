namespace SporeMods.Core.ArgScript
{
    public class PragmaVisitor : ArgScriptBaseVisitor<string>
    {
        public override string VisitPragma(ArgScriptParser.PragmaContext context)
        {
            return context.GetText();
        }
    }
}