///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.contained;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.filterspec;
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

        public static FilterSpecCompiled MakeFilterSpec(
            EventType eventType,
            string eventTypeName,
            IList<ExprNode> filterExpessions,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // Validate all nodes, make sure each returns a boolean and types are good;
            // Also decompose all AND super nodes into individual expressions
            var validatedNodes = ValidateAllowSubquery(
                ExprNodeOrigin.FILTER, filterExpessions, streamTypeService, taggedEventTypes, arrayEventTypes,
                statementRawInfo, services);
            return Build(
                validatedNodes, eventType, eventTypeName, optionalPropertyEvalSpec, taggedEventTypes, arrayEventTypes,
                streamTypeService, optionalStreamName, statementRawInfo, services);
        }

        public static FilterSpecCompiled Build(
            IList<ExprNode> validatedNodes,
            EventType eventType,
            string eventTypeName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            string optionalStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return BuildNoStmtCtx(
                validatedNodes, eventType, eventTypeName, optionalStreamName, optionalPropertyEvalSpec,
                taggedEventTypes, arrayEventTypes, streamTypeService, statementRawInfo, compileTimeServices);
        }

        public static FilterSpecCompiled BuildNoStmtCtx(
            IList<ExprNode> validatedNodes,
            EventType eventType,
            string eventTypeName,
            string optionalStreamName,
            PropertyEvalSpec optionalPropertyEvalSpec,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices
        )
        {
            PropertyEvaluatorForge optionalPropertyEvaluator = null;
            if (optionalPropertyEvalSpec != null) {
                optionalPropertyEvaluator = PropertyEvaluatorForgeFactory.MakeEvaluator(
                    optionalPropertyEvalSpec, eventType, optionalStreamName, statementRawInfo, compileTimeServices);
            }

            var args = new FilterSpecCompilerArgs(
                taggedEventTypes, arrayEventTypes, streamTypeService, null, statementRawInfo, compileTimeServices);
            IList<FilterSpecParamForge>[] spec = FilterSpecCompilerPlanner.PlanFilterParameters(validatedNodes, args);

            if (Log.IsDebugEnabled) {
                Log.Debug(".makeFilterSpec spec=" + spec);
            }

            return new FilterSpecCompiled(eventType, eventTypeName, spec, optionalPropertyEvaluator);
        }

        public static IList<ExprNode> ValidateAllowSubquery(
            ExprNodeOrigin exprNodeOrigin,
            IList<ExprNode> exprNodes,
            StreamTypeService streamTypeService,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            IList<ExprNode> validatedNodes = new List<ExprNode>();

            ExprValidationContext validationContext =
                new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services)
                    .WithAllowBindingConsumption(true).WithIsFilterExpression(true).Build();
            foreach (var node in exprNodes) {
                // Determine subselects
                var visitor = new ExprNodeSubselectDeclaredDotVisitor();
                node.Accept(visitor);

                // Compile subselects
                if (!visitor.Subselects.IsEmpty()) {
                    // The outer event type is the filtered-type itself
                    foreach (var subselect in visitor.Subselects) {
                        try {
                            SubSelectHelperFilters.HandleSubselectSelectClauses(
                                subselect,
                                streamTypeService.EventTypes[0], streamTypeService.StreamNames[0],
                                streamTypeService.StreamNames[0],
                                taggedEventTypes, arrayEventTypes, statementRawInfo, services);
                        }
                        catch (ExprValidationException ex) {
                            throw new ExprValidationException(
                                "Failed to validate " + ExprNodeUtilityMake.GetSubqueryInfoText(subselect) + ": " +
                                ex.Message, ex);
                        }
                    }
                }

                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, node, validationContext);
                validatedNodes.Add(validated);

                if (validated.Forge.EvaluationType != typeof(bool?) && validated.Forge.EvaluationType != typeof(bool)) {
                    throw new ExprValidationException(
                        "Filter expression not returning a boolean value: '" +
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validated) + "'");
                }
            }

            return validatedNodes;
        }
    }
} // end of namespace