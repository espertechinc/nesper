///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.@internal.parse;
using com.espertech.esper.grammar.@internal.generated;

using static com.espertech.esper.common.@internal.epl.classprovided.compiletime.ClassProvidedPrecompileUtil;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerHelperSingleEPL
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ParseRuleSelector EPL_PARSE_RULE = new ProxyParseRuleSelector();

		internal static CompilerHelperSingleResult ParseCompileInlinedClassesWalk(
			Compilable compilable,
			StatementCompileTimeServices compileTimeServices)
		{
			CompilerHelperSingleResult result;
			try {
				if (compilable is CompilableEPL) {
					var compilableEPL = (CompilableEPL) compilable;

					// parse
					var parseResult = Parse(compilableEPL.Epl);

					// compile application-provided classes (both create-class as well as just class-keyword)
					var classesInlined = CompileAddExtensions(parseResult.Classes, compilable, compileTimeServices);

					// walk - this may use the new classes already such as for extension-single-row-function
					var raw = Walk(parseResult, compilableEPL.Epl, compileTimeServices.StatementSpecMapEnv);
					result = new CompilerHelperSingleResult(raw, classesInlined);
				}
				else if (compilable is CompilableSODA) {
					var soda = ((CompilableSODA) compilable).Soda;

					// compile application-provided classes (both create-class as well as just class-keyword)
					ClassProvidedPrecompileResult classesInlined;
					if ((soda.ClassProvidedExpressions != null && !soda.ClassProvidedExpressions.IsEmpty()) || soda.CreateClass != null) {
						IList<string> classTexts = new List<string>();
						if (soda.ClassProvidedExpressions != null) {
							foreach (var inlined in soda.ClassProvidedExpressions) {
								classTexts.Add(inlined.ClassText);
							}
						}

						if (soda.CreateClass != null) {
							classTexts.Add(soda.CreateClass.ClassProvidedExpression.ClassText);
						}

						classesInlined = CompileAddExtensions(classTexts, compilable, compileTimeServices);
					}
					else {
						classesInlined = ClassProvidedPrecompileResult.EMPTY;
					}

					// map from soda to raw
					var raw = StatementSpecMapper.Map(soda, compileTimeServices.StatementSpecMapEnv);
					result = new CompilerHelperSingleResult(raw, classesInlined);
				}
				else {
					throw new IllegalStateException("Unrecognized compilable " + compilable);
				}
			}
			catch (StatementSpecCompileException) {
				throw;
			}
			catch (Exception ex) {
				throw new StatementSpecCompileException("Exception processing statement: " + ex.Message, ex, compilable.ToEPL());
			}

			return result;
		}

		public static StatementSpecRaw ParseWalk(
			string epl,
			StatementSpecMapEnv mapEnv)
		{
			var parseResult = Parse(epl);
			return Walk(parseResult, epl, mapEnv);
		}

		private static ClassProvidedPrecompileResult CompileAddExtensions(
			IList<string> classes,
			Compilable compilable,
			StatementCompileTimeServices compileTimeServices)
		{
			ClassProvidedPrecompileResult classesInlined;
			try {
				classesInlined = CompileClassProvided(classes, compileTimeServices, null);
				// add inlined classes including create-class
				compileTimeServices.ClassProvidedExtension.Add(classesInlined.Classes, classesInlined.Bytes);
			}
			catch (ExprValidationException ex) {
				throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
			}

			return classesInlined;
		}

		private static StatementSpecRaw Walk(
			ParseResult parseResult,
			string epl,
			StatementSpecMapEnv mapEnv)
		{
			var ast = parseResult.Tree;

			var defaultStreamSelector =
				StatementSpecMapper.MapFromSODA(mapEnv.Configuration.Compiler.StreamSelection.DefaultStreamSelector);
			var walker = new EPLTreeWalkerListener(
				parseResult.TokenStream,
				defaultStreamSelector,
				parseResult.Scripts,
				parseResult.Classes,
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

		private static ParseResult Parse(string epl)
		{
			return ParseHelper.Parse(epl, epl, true, EPL_PARSE_RULE, true);
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
