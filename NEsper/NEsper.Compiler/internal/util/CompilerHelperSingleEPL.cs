///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.@internal.parse;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperSingleEPL
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ParseRuleSelector EPL_PARSE_RULE = new ProxyParseRuleSelector();

        internal static StatementSpecRaw ParseWalk(
            Compilable compilable,
            StatementCompileTimeServices compileTimeServices)
        {
            StatementSpecRaw specRaw;
            try {
                if (compilable is CompilableEPL) {
                    var compilableEPL = (CompilableEPL) compilable;
                    specRaw = ParseWalk(compilableEPL.Epl, compileTimeServices.StatementSpecMapEnv);
                }
                else if (compilable is CompilableSODA) {
                    var soda = ((CompilableSODA) compilable).Soda;
                    specRaw = StatementSpecMapper.Map(soda, compileTimeServices.StatementSpecMapEnv);
                }
                else {
                    throw new IllegalStateException("Unrecognized compilable " + compilable);
                }
            }
            catch (StatementSpecCompileException) {
                throw;
            }
            catch (Exception ex) {
                throw new StatementSpecCompileException(
                    "Unexpected exception parsing statement: " + ex.Message,
                    ex,
                    compilable.ToEPL());
            }

            return specRaw;
        }

        public static StatementSpecRaw ParseWalk(
            string epl,
            StatementSpecMapEnv mapEnv)
        {
            var parseResult = ParseHelper.Parse(epl, epl, true, EPL_PARSE_RULE, true);
            var ast = parseResult.Tree;

            var defaultStreamSelector =
                StatementSpecMapper.MapFromSODA(mapEnv.Configuration.Compiler.StreamSelection.DefaultStreamSelector);
            var walker = new EPLTreeWalkerListener(
                parseResult.TokenStream,
                defaultStreamSelector,
                parseResult.Scripts,
                mapEnv);

            try {
                ParseHelper.Walk(ast, walker, epl, epl);
            }
            catch (ASTWalkException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, epl);
            }
            catch (ValidationException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, epl);
            }
            catch (Exception ex) {
                var message = "Invalid expression encountered";
                throw new StatementSpecCompileException(GetNullableErrortext(message, ex.Message), ex, epl);
            }

            if (Log.IsDebugEnabled) {
                ASTUtil.DumpAST(ast);
            }

            return walker.StatementSpec;
        }

        private static string GetNullableErrortext(
            string msg,
            string cause)
        {
            if (cause == null) {
                return msg;
            }

            return cause;
        }

        private class ProxyParseRuleSelector : ParseRuleSelector
        {
            public ITree InvokeParseRule(EsperEPL2GrammarParser parser)
            {
                return parser.startEPLExpressionRule();
            }
        }
    }
} // end of namespace