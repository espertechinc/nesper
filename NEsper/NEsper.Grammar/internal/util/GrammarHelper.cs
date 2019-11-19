using Antlr4.Runtime;

using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.grammar.@internal.util
{
    public class GrammarHelper
    {
        public static EsperEPL2GrammarLexer CreateLexer(string input)
        {
            return CreateLexer(new CaseInsensitiveInputStream(input));
        }

        public static EsperEPL2GrammarLexer CreateLexer(ICharStream input)
        {
            var lex = new EsperEPL2GrammarLexer(input);
            lex.RemoveErrorListeners();
            lex.AddErrorListener(Antlr4ErrorListener<int>.INSTANCE);
            return lex;
        }
        
        public static EsperEPL2GrammarParser CreateParser(ITokenStream tokens)
        {
            var g = new EsperEPL2GrammarParser(tokens);
            g.RemoveErrorListeners();
            g.AddErrorListener(Antlr4ErrorListener<IToken>.INSTANCE);
            g.ErrorHandler = new Antlr4ErrorStrategy();
            return g;
        }
    }
}