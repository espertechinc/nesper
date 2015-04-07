///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime.Tree;

using com.espertech.esper.client;
using com.espertech.esper.compat;
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
    /// <summary>Helper class for administrative interface. </summary>
    public class EPAdministratorHelper
    {
        private static readonly ParseRuleSelector PatternParseRule;
        private static readonly ParseRuleSelector EPLParseRule;
    
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

        /// <summary>Compile an EPL statement. </summary>
        /// <param name="eplStatement">to compile</param>
        /// <param name="eplStatementForErrorMsg">the statement to use for indicating error messages</param>
        /// <param name="addPleaseCheck">true to add please-check message text</param>
        /// <param name="statementName">the name of statement</param>
        /// <param name="services">engine services</param>
        /// <param name="defaultStreamSelector">stream selector</param>
        /// <returns>compiled statement</returns>
        public static StatementSpecRaw CompileEPL(
            String eplStatement,
            String eplStatementForErrorMsg,
            bool addPleaseCheck,
            String statementName,
            EPServicesContext services,
            SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            return CompileEPL(
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

        /// <summary>
        /// Compile the EPL.
        /// </summary>
        /// <param name="eplStatement">expression to compile</param>
        /// <param name="eplStatementForErrorMsg">use this text for the error message</param>
        /// <param name="addPleaseCheck">indicator to add a "please check" wording for stack paraphrases</param>
        /// <param name="statementName">is the name of the statement</param>
        /// <param name="defaultStreamSelector">the configuration for which insert or remove streams (or both) to produce</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="variableService">The variable service.</param>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="configSnapshot">The config snapshot.</param>
        /// <param name="patternNodeFactory">The pattern node factory.</param>
        /// <param name="contextManagementService">The context management service.</param>
        /// <param name="exprDeclaredService">The expr declared service.</param>
        /// <param name="tableService">The table service.</param>
        /// <returns>
        /// statement specification
        /// </returns>
        /// <exception cref="EPStatementException">
        /// </exception>
        public static StatementSpecRaw CompileEPL(
            String eplStatement,
            String eplStatementForErrorMsg,
            bool addPleaseCheck,
            String statementName,
            SelectClauseStreamSelectorEnum defaultStreamSelector,
            EngineImportService engineImportService,
            VariableService variableService,
            SchedulingService schedulingService,
            String engineURI,
            ConfigurationInformation configSnapshot,
            PatternNodeFactory patternNodeFactory,
            ContextManagementService contextManagementService,
            ExprDeclaredService exprDeclaredService,
            TableService tableService)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".createEPLStmt StatementName=" + statementName + " eplStatement=" + eplStatement);
            }
    
            var parseResult = ParseHelper.Parse(eplStatement, eplStatementForErrorMsg, addPleaseCheck, EPLParseRule, true);
            var ast = parseResult.Tree;

            EPLTreeWalkerListener walker = new EPLTreeWalkerListener(
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
                throw new EPStatementException(ex.Message, eplStatementForErrorMsg, ex);
            }
            catch (EPStatementSyntaxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                const string message = "Error in expression";
                Log.Debug(message, ex);
                throw new EPStatementException(GetNullableErrortext(message, ex.Message), eplStatementForErrorMsg, ex);
            }
    
            if (Log.IsDebugEnabled)
            {
                ASTUtil.DumpAST(ast);
            }
    
            StatementSpecRaw raw = walker.GetStatementSpec();
            raw.ExpressionNoAnnotations = parseResult.ExpressionWithoutAnnotations;
            return raw;
        }

        public static StatementSpecRaw CompilePattern(String expression, String expressionForErrorMessage, bool addPleaseCheck, EPServicesContext services, SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            // Parse
            ParseResult parseResult = ParseHelper.Parse(expression, expressionForErrorMessage, addPleaseCheck, PatternParseRule, true);
            var ast = parseResult.Tree;
            if (Log.IsDebugEnabled)
            {
                ASTUtil.DumpAST(ast);
            }

            // Walk
            EPLTreeWalkerListener walker = new EPLTreeWalkerListener(
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
            catch (EPStatementSyntaxException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                String message = "Error in expression";
                Log.Debug(message, ex);
                throw new EPStatementException(GetNullableErrortext(message, ex.Message), expression);
            }

            var walkerStatementSpec = walker.GetStatementSpec();
            if (walkerStatementSpec.StreamSpecs.Count > 1)
            {
                throw new IllegalStateException("Unexpected multiple stream specifications encountered");
            }
    
            // Get pattern specification
            var patternStreamSpec = (PatternStreamSpecRaw)walkerStatementSpec.StreamSpecs[0];
    
            // Create statement spec, set pattern stream, set wildcard select
            var statementSpec = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            statementSpec.StreamSpecs.Add(patternStreamSpec);
            statementSpec.SelectClauseSpec.SelectExprList.Clear();
            statementSpec.SelectClauseSpec.SelectExprList.Add(new SelectClauseElementWildcard());
            statementSpec.Annotations = walkerStatementSpec.Annotations;
            statementSpec.ExpressionNoAnnotations = parseResult.ExpressionWithoutAnnotations;
    
            return statementSpec;
        }
    
        private static String GetNullableErrortext(String msg, String cause)
        {
            if (cause == null)
            {
                return msg;
            }
            return msg + ": " + cause;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
