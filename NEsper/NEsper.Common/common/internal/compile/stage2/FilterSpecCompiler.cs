///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    /// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
    /// </summary>
    internal class FilterSpecCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly String NEWLINE = Environment.NewLine;

        public static FilterSpecCompiledDesc MakeFilterSpec(
            EventType eventType,
            string eventTypeName,
            IList<ExprNode> filterExpessions,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // Validate all nodes, make sure each returns a boolean and types are good;
            // Also decompose all AND super nodes into individual expressions
            var validatedDesc = ValidateAllowSubquery(
                ExprNodeOrigin.FILTER,
                filterExpessions,
                streamTypeService,
                taggedEventTypes,
                arrayEventTypes,
                statementRawInfo,
                services);
            return Build(
                validatedDesc,
                eventType,
                eventTypeName,
                optionalPropertyEvalSpec,
                taggedEventTypes,
                arrayEventTypes,
                allTagNamesOrdered,
                streamTypeService,
                optionalStreamName,
                statementRawInfo,
                services);
        }

        public static FilterSpecCompiledDesc Build(
            FilterSpecValidatedDesc validatedDesc,
            EventType eventType,
            string eventTypeName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var compiled = BuildNoStmtCtx(
                validatedDesc.Expressions,
                eventType,
                eventTypeName,
                optionalStreamName,
                optionalPropertyEvalSpec,
                taggedEventTypes,
                arrayEventTypes,
                allTagNamesOrdered,
                streamTypeService,
                statementRawInfo,
                compileTimeServices);
            return new FilterSpecCompiledDesc(compiled, validatedDesc.AdditionalForgeables);
        }

        public static FilterSpecCompiled BuildNoStmtCtx(
            IList<ExprNode> validatedNodes,
            EventType eventType,
            string eventTypeName,
            string optionalStreamName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTagNamesOrdered,
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            PropertyEvaluatorForge optionalPropertyEvaluator = null;
            if (optionalPropertyEvalSpec != null) {
                optionalPropertyEvaluator = PropertyEvaluatorForgeFactory.MakeEvaluator(
                    optionalPropertyEvalSpec,
                    eventType,
                    optionalStreamName,
                    statementRawInfo,
                    compileTimeServices);
            }

            // unwind "and" and "or"
            var unwound = FilterSpecCompilerIndexPlannerUnwindAndOr.UnwindAndOr(validatedNodes);

            var args = new FilterSpecCompilerArgs(
                taggedEventTypes,
                arrayEventTypes,
                allTagNamesOrdered,
                streamTypeService,
                null,
                statementRawInfo,
                compileTimeServices);
            var plan = FilterSpecCompilerIndexPlanner.PlanFilterParameters(unwound, args);

            var hook = (FilterSpecCompileHook) ImportUtil.GetAnnotationHook(
                statementRawInfo.Annotations,
                HookType.INTERNAL_FILTERSPEC,
                typeof(FilterSpecCompileHook),
                compileTimeServices.ImportServiceCompileTime);
            hook?.FilterIndexPlan(eventType, unwound, plan);

            if (compileTimeServices.Configuration.Compiler.Logging.IsEnableFilterPlan) {
                LogFilterPlans(unwound, plan, eventType, optionalStreamName, statementRawInfo);
            }

            if (Log.IsDebugEnabled) {
                Log.Debug(".makeFilterSpec spec=" + plan);
            }

            return new FilterSpecCompiled(eventType, eventTypeName, plan, optionalPropertyEvaluator);
        }

        public static FilterSpecValidatedDesc ValidateAllowSubquery(
            ExprNodeOrigin exprNodeOrigin,
            IList<ExprNode> exprNodes,
            StreamTypeService streamTypeService,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IList<ExprNode> validatedNodes = new List<ExprNode>();
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>();

            ExprValidationContext validationContext =
                new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .WithIsFilterExpression(true)
                    .Build();
            foreach (var node in exprNodes) {
                // Determine subselects
                var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                node.Accept(visitor);

                // Compile subselects
                if (!visitor.Subselects.IsEmpty()) {
                    // The outer event type is the filtered-type itself
                    foreach (var subselect in visitor.Subselects) {
                        try {
                            var subselectAdditionalForgeables = SubSelectHelperFilters.HandleSubselectSelectClauses(
                                subselect,
                                streamTypeService.EventTypes[0],
                                streamTypeService.StreamNames[0],
                                streamTypeService.StreamNames[0],
                                taggedEventTypes,
                                arrayEventTypes,
                                statementRawInfo,
                                services);
                            additionalForgeables.AddAll(subselectAdditionalForgeables);
                        }
                        catch (ExprValidationException ex) {
                            throw new ExprValidationException(
                                "Failed to validate " +
                                ExprNodeUtilityMake.GetSubqueryInfoText(subselect) +
                                ": " +
                                ex.Message,
                                ex);
                        }
                    }
                }

                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, node, validationContext);
                validatedNodes.Add(validated);

                if (validated.Forge.EvaluationType != typeof(bool?) && validated.Forge.EvaluationType != typeof(bool)) {
                    throw new ExprValidationException(
                        "Filter expression not returning a boolean value: '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validated) +
                        "'");
                }
            }

            return new FilterSpecValidatedDesc(validatedNodes, additionalForgeables);
        }
        
        private static void LogFilterPlans(
            IList<ExprNode> validatedNodes,
            FilterSpecPlanForge plan,
            EventType eventType,
            string optionalStreamName,
            StatementRawInfo statementRawInfo)
        {
            var buf = new StringBuilder();
            buf
                .Append("Filter plan for statement '")
                .Append(statementRawInfo.StatementName)
                .Append("' filtering event type '")
                .Append(eventType.Name + "'");

            if (optionalStreamName != null) {
                buf.Append(" alias '" + optionalStreamName + "'");
            }
            if (validatedNodes.IsEmpty()) {
                buf.Append(" empty");
            } else {
                var andNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAndWhenNeeded(validatedNodes);
                var expression = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(andNode);
                buf
                    .Append(" expression '")
                    .Append(expression)
                    .Append("' for ")
                    .Append(plan.Paths.Length)
                    .Append(" paths");
            }
            buf.Append(Environment.NewLine);

            plan.AppendPlan(buf);

            Log.Info(buf.ToString());
        }
    }
} // end of namespace