///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime.Tree;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>Helper class for administrative interface.</summary>
    public class EPAdministratorHelper
    {
        private static readonly ParseRuleSelector PatternParseRule;
        private static readonly ParseRuleSelector EPLParseRule;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static EPAdministratorHelper()
        {
            PatternParseRule = DoPatternParseRule;
            EPLParseRule = DoEPLParseRule;
        }

        #region Parse & Walk Rules
        private static ITree DoPatternParseRule(EsperEPL2GrammarParser parser)
        {
            return parser.startPatternExpressionRule();
        }

        private static ITree DoEPLParseRule(EsperEPL2GrammarParser parser)
        {
            return parser.startEPLExpressionRule();
        }
        #endregion

        /// <summary>
        /// Compile an EPL statement.
        /// </summary>
        /// <param name="eplStatement">to compile</param>
        /// <param name="eplStatementForErrorMsg">the statement to use for indicating error messages</param>
        /// <param name="addPleaseCheck">true to add please-check message text</param>
        /// <param name="statementName">the name of statement</param>
        /// <param name="services">engine services</param>
        /// <param name="defaultStreamSelector">stream selector</param>
        /// <returns>compiled statement</returns>
        public static StatementSpecRaw CompileEPL(
            string eplStatement,
            string eplStatementForErrorMsg,
            bool addPleaseCheck,
            string statementName,
            EPServicesContext services,
            SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            return CompileEPL(
                services.Container,
                eplStatement,
                eplStatementForErrorMsg,
                addPleaseCheck,
                statementName,
                defaultStreamSelector,
                services.EngineImportService,
                services.VariableService,
                services.SchedulingService,
                services.EngineURI,
                services.ConfigSnapshot,
                services.PatternNodeFactory,
                services.ContextManagementService,
                services.ExprDeclaredService,
                services.TableService);
        }

        public static StatementSpecRaw CompileEPL(
            IContainer container,
            string eplStatement,
            string eplStatementForErrorMsg,
            bool addPleaseCheck,
            string statementName,
            SelectClauseStreamSelectorEnum defaultStreamSelector,
            EngineImportService engineImportService,
            VariableService variableService,
            SchedulingService schedulingService,
            string engineURI,
            ConfigurationInformation configSnapshot,
            PatternNodeFactory patternNodeFactory,
            ContextManagementService contextManagementService,
            ExprDeclaredService exprDeclaredService,
            TableService tableService)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".createEPLStmt statementName=" + statementName + " eplStatement=" + eplStatement);
            }

            ParseResult parseResult = ParseHelper.Parse(
                eplStatement, eplStatementForErrorMsg, addPleaseCheck, EPLParseRule, true);
            ITree ast = parseResult.Tree;

            var walker = new EPLTreeWalkerListener(
                container,
                parseResult.TokenStream,
                engineImportService,
                variableService,
                schedulingService,
                defaultStreamSelector,
                engineURI,
                configSnapshot,
                patternNodeFactory,
                contextManagementService,
                parseResult.Scripts,
                exprDeclaredService,
                tableService);

            try
            {
                ParseHelper.Walk(ast, walker, eplStatement, eplStatementForErrorMsg);
            }
            catch (ASTWalkException ex)
            {
                Log.Error(".createEPL Error validating expression", ex);
                throw new EPStatementException(ex.Message, ex, eplStatementForErrorMsg);
            }
            catch (EPStatementSyntaxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = "Error in expression";
                Log.Debug(message, ex);
                throw new EPStatementException(GetNullableErrortext(message, ex.Message), ex, eplStatementForErrorMsg);
            }

            if (Log.IsDebugEnabled)
            {
                ASTUtil.DumpAST(ast);
            }

            StatementSpecRaw raw = walker.StatementSpec;
            raw.ExpressionNoAnnotations = parseResult.ExpressionWithoutAnnotations;
            return raw;
        }

        public static StatementSpecRaw CompilePattern(
            string expression,
            string expressionForErrorMessage,
            bool addPleaseCheck,
            EPServicesContext services,
            SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            // Parse
            ParseResult parseResult = ParseHelper.Parse(
                expression, expressionForErrorMessage, addPleaseCheck, PatternParseRule, true);
            ITree ast = parseResult.Tree;
            if (Log.IsDebugEnabled)
            {
                ASTUtil.DumpAST(ast);
            }

            // Walk
            var walker = new EPLTreeWalkerListener(
                services.Container,
                parseResult.TokenStream,
                services.EngineImportService,
                services.VariableService,
                services.SchedulingService,
                defaultStreamSelector,
                services.EngineURI,
                services.ConfigSnapshot,
                services.PatternNodeFactory,
                services.ContextManagementService,
                parseResult.Scripts,
                services.ExprDeclaredService,
                services.TableService);

            try
            {
                ParseHelper.Walk(ast, walker, expression, expressionForErrorMessage);
            }
            catch (ASTWalkException ex)
            {
                Log.Debug(".createPattern Error validating expression", ex);
                throw new EPStatementException(ex.Message, expression);
            }
            catch (EPStatementSyntaxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string message = "Error in expression";
                Log.Debug(message, ex);
                throw new EPStatementException(GetNullableErrortext(message, ex.Message), expression);
            }

            if (walker.StatementSpec.StreamSpecs.Count > 1)
            {
                throw new IllegalStateException("Unexpected multiple stream specifications encountered");
            }

            // Get pattern specification
            PatternStreamSpecRaw patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

            // Create statement spec, set pattern stream, set wildcard select
            var statementSpec = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            statementSpec.StreamSpecs.Add(patternStreamSpec);
            statementSpec.SelectClauseSpec.SelectExprList.Clear();
            statementSpec.SelectClauseSpec.SelectExprList.Add(new SelectClauseElementWildcard());
            statementSpec.Annotations = walker.StatementSpec.Annotations;
            statementSpec.ExpressionNoAnnotations = parseResult.ExpressionWithoutAnnotations;

            return statementSpec;
        }

        private static string GetNullableErrortext(string msg, string cause)
        {
            if (cause == null)
            {
                return msg;
            }
            else
            {
                return msg + ": " + cause;
            }
        }
    }
} // end of namespace
