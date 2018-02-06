///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.util;
using com.espertech.esper.view;
using com.espertech.esper.view.std;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodHelperValidate
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void ValidateNoDataWindowOnNamedWindow(IList<ViewFactory> viewFactories)
        {
            foreach (var viewFactory in viewFactories)
            {
                if ((viewFactory is GroupByViewFactory) || ((viewFactory is MergeViewFactory)))
                {
                    continue;
                }
                if (viewFactory is DataWindowViewFactory)
                {
                    throw new ExprValidationException(NamedWindowMgmtServiceConstants.ERROR_MSG_NO_DATAWINDOW_ALLOWED);
                }
            }
        }

        /// <summary>
        /// Validate filter and join expression nodes.
        /// </summary>
        /// <param name="statementSpec">the compiled statement</param>
        /// <param name="statementContext">the statement services</param>
        /// <param name="typeService">the event types for streams</param>
        /// <param name="viewResourceDelegate">the delegate to verify expressions that use view resources</param>
        internal static void ValidateNodes(
            StatementSpecCompiled statementSpec,
            StatementContext statementContext,
            StreamTypeService typeService,
            ViewResourceDelegateUnverified viewResourceDelegate)
        {
            var engineImportService = statementContext.EngineImportService;
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var intoTableName = statementSpec.IntoTableSpec == null ? null : statementSpec.IntoTableSpec.Name;

            if (statementSpec.FilterRootNode != null)
            {
                var optionalFilterNode = statementSpec.FilterRootNode;

                // Validate where clause, initializing nodes to the stream ids used
                try
                {
                    var validationContext = new ExprValidationContext(
                        statementContext.Container,
                        typeService, engineImportService, 
                        statementContext.StatementExtensionServicesContext,
                        viewResourceDelegate, 
                        statementContext.SchedulingService, 
                        statementContext.VariableService,
                        statementContext.TableService,
                        evaluatorContextStmt,
                        statementContext.EventAdapterService,
                        statementContext.StatementName, 
                        statementContext.StatementId,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, true, false, intoTableName, false);
                    optionalFilterNode = ExprNodeUtility.GetValidatedSubtree(
                        ExprNodeOrigin.FILTER, optionalFilterNode, validationContext);
                    if (optionalFilterNode.ExprEvaluator.ReturnType != typeof (bool) &&
                        optionalFilterNode.ExprEvaluator.ReturnType != typeof (bool?))
                    {
                        throw new ExprValidationException("The where-clause filter expression must return a boolean value");
                    }
                    statementSpec.FilterExprRootNode = optionalFilterNode;

                    // Make sure there is no aggregation in the where clause
                    var aggregateNodes = new List<ExprAggregateNode>();
                    ExprAggregateNodeUtil.GetAggregatesBottomUp(optionalFilterNode, aggregateNodes);
                    if (!aggregateNodes.IsEmpty())
                    {
                        throw new ExprValidationException(
                            "An aggregate function may not appear in a WHERE clause (use the HAVING clause)");
                    }
                }
                catch (ExprValidationException ex)
                {
                    Log.Debug(
                        ".validateNodes Validation exception for filter=" +
                        ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(optionalFilterNode), ex);
                    throw new EPStatementException(
                        "Error validating expression: " + ex.Message, ex, statementContext.Expression);
                }
            }

            if ((statementSpec.OutputLimitSpec != null) &&
                ((statementSpec.OutputLimitSpec.WhenExpressionNode != null) ||
                 (statementSpec.OutputLimitSpec.AndAfterTerminateExpr != null)))
            {
                // Validate where clause, initializing nodes to the stream ids used
                try
                {
                    var outputLimitType =
                        OutputConditionExpressionFactory.GetBuiltInEventType(statementContext.EventAdapterService);
                    var typeServiceOutputWhen = new StreamTypeServiceImpl(
                        new EventType[] { outputLimitType },
                        new string[] { null }, 
                        new bool[] { true }, 
                        statementContext.EngineURI, false);
                    var validationContext = new ExprValidationContext(
                        statementContext.Container,
                        typeServiceOutputWhen, 
                        engineImportService, 
                        statementContext.StatementExtensionServicesContext, null, 
                        statementContext.SchedulingService, 
                        statementContext.VariableService,
                        statementContext.TableService, 
                        evaluatorContextStmt, 
                        statementContext.EventAdapterService,
                        statementContext.StatementName, 
                        statementContext.StatementId, 
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ScriptingService,
                        false, false, false, false, intoTableName, false);

                    var outputLimitWhenNode = statementSpec.OutputLimitSpec.WhenExpressionNode;
                    if (outputLimitWhenNode != null)
                    {
                        outputLimitWhenNode = ExprNodeUtility.GetValidatedSubtree(
                            ExprNodeOrigin.OUTPUTLIMIT, outputLimitWhenNode, validationContext);
                        statementSpec.OutputLimitSpec.WhenExpressionNode = outputLimitWhenNode;

                        if (outputLimitWhenNode.ExprEvaluator.ReturnType.GetBoxedType() != typeof (bool?))
                        {
                            throw new ExprValidationException(
                                "The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value");
                        }
                        EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                            outputLimitWhenNode, "An aggregate function may not appear in a OUTPUT LIMIT clause");
                    }

                    // validate and-terminate expression if provided
                    if (statementSpec.OutputLimitSpec.AndAfterTerminateExpr != null)
                    {
                        if (statementSpec.OutputLimitSpec.RateType != OutputLimitRateType.WHEN_EXPRESSION &&
                            statementSpec.OutputLimitSpec.RateType != OutputLimitRateType.TERM)
                        {
                            throw new ExprValidationException(
                                "A terminated-and expression must be used with the OUTPUT WHEN clause");
                        }
                        var validated = ExprNodeUtility.GetValidatedSubtree(
                            ExprNodeOrigin.OUTPUTLIMIT, statementSpec.OutputLimitSpec.AndAfterTerminateExpr,
                            validationContext);
                        statementSpec.OutputLimitSpec.AndAfterTerminateExpr = validated;

                        if (validated.ExprEvaluator.ReturnType.GetBoxedType() != typeof (bool?))
                        {
                            throw new ExprValidationException(
                                "The terminated-and expression must return a boolean-type value");
                        }
                        EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                            validated, "An aggregate function may not appear in a terminated-and clause");
                    }

                    // validate then-expression
                    ValidateThenSetAssignments(statementSpec.OutputLimitSpec.ThenExpressions, validationContext);

                    // validate after-terminated then-expression
                    ValidateThenSetAssignments(
                        statementSpec.OutputLimitSpec.AndAfterTerminateThenExpressions, validationContext);
                }
                catch (ExprValidationException ex)
                {
                    throw new EPStatementException(
                        "Error validating expression: " + ex.Message, statementContext.Expression);
                }
            }

            for (var outerJoinCount = 0; outerJoinCount < statementSpec.OuterJoinDescList.Length; outerJoinCount++)
            {
                var outerJoinDesc = statementSpec.OuterJoinDescList[outerJoinCount];

                // validate on-expression nodes, if provided
                if (outerJoinDesc.OptLeftNode != null)
                {
                    var streamIdPair = ValidateOuterJoinPropertyPair(
                        statementContext, outerJoinDesc.OptLeftNode, outerJoinDesc.OptRightNode, outerJoinCount,
                        typeService, viewResourceDelegate);

                    if (outerJoinDesc.AdditionalLeftNodes != null)
                    {
                        var streamSet = new HashSet<int?>();
                        streamSet.Add(streamIdPair.First);
                        streamSet.Add(streamIdPair.Second);
                        for (var i = 0; i < outerJoinDesc.AdditionalLeftNodes.Length; i++)
                        {
                            var streamIdPairAdd = ValidateOuterJoinPropertyPair(
                                statementContext, outerJoinDesc.AdditionalLeftNodes[i],
                                outerJoinDesc.AdditionalRightNodes[i], outerJoinCount,
                                typeService, viewResourceDelegate);

                            // make sure all additional properties point to the same two streams
                            if (!streamSet.Contains(streamIdPairAdd.First) ||
                                (!streamSet.Contains(streamIdPairAdd.Second)))
                            {
                                const string message =
                                    "Outer join ON-clause columns must refer to properties of the same joined streams" +
                                    " when using multiple columns in the on-clause";
                                throw new EPStatementException(
                                    "Error validating expression: " + message, statementContext.Expression);
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateThenSetAssignments(
            IList<OnTriggerSetAssignment> assignments,
            ExprValidationContext validationContext)
        {
            if (assignments == null || assignments.IsEmpty())
            {
                return;
            }
            foreach (var assign in assignments)
            {
                var node = ExprNodeUtility.GetValidatedAssignment(assign, validationContext);
                assign.Expression = node;
                EPStatementStartMethodHelperValidate.ValidateNoAggregations(
                    node, "An aggregate function may not appear in a OUTPUT LIMIT clause");
            }
        }

        internal static UniformPair<int> ValidateOuterJoinPropertyPair(
                StatementContext statementContext,
                ExprIdentNode leftNode,
                ExprIdentNode rightNode,
                int outerJoinCount,
                StreamTypeService typeService,
                ViewResourceDelegateUnverified viewResourceDelegate) {
            // Validate the outer join clause using an artificial equals-node on top.
            // Thus types are checked via equals.
            // Sets stream ids used for validated nodes.
            var equalsNode = new ExprEqualsNodeImpl(false, false);
            equalsNode.AddChildNode(leftNode);
            equalsNode.AddChildNode(rightNode);
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            try
            {
                var validationContext = new ExprValidationContext(
                    statementContext.Container,
                    typeService,
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext, viewResourceDelegate,
                    statementContext.SchedulingService,
                    statementContext.VariableService,
                    statementContext.TableService,
                    evaluatorContextStmt, 
                    statementContext.EventAdapterService,
                    statementContext.StatementName,
                    statementContext.StatementId,
                    statementContext.Annotations,
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, true, false, null, false);
                ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.JOINON, equalsNode, validationContext);
            } catch (ExprValidationException ex) {
                Log.Debug("Validation exception for outer join node=" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(equalsNode), ex);
                throw new EPStatementException("Error validating expression: " + ex.Message, statementContext.Expression);
            }
    
            // Make sure we have left-hand-side and right-hand-side refering to different streams
            var streamIdLeft = leftNode.StreamId;
            var streamIdRight = rightNode.StreamId;
            if (streamIdLeft == streamIdRight)
            {
                const string message = "Outer join ON-clause cannot refer to properties of the same stream";
                throw new EPStatementException("Error validating expression: " + message, statementContext.Expression);
            }

            // Make sure one of the properties refers to the acutual stream currently being joined
            var expectedStreamJoined = outerJoinCount + 1;
            if ((streamIdLeft != expectedStreamJoined) && (streamIdRight != expectedStreamJoined)) {
                var message = "Outer join ON-clause must refer to at least one property of the joined stream" +
                        " for stream " + expectedStreamJoined;
                throw new EPStatementException("Error validating expression: " + message, statementContext.Expression);
            }
    
            // Make sure neither of the streams refer to a 'future' stream
            string badPropertyName = null;
            if (streamIdLeft > outerJoinCount + 1) {
                badPropertyName = leftNode.ResolvedPropertyName;
            }
            if (streamIdRight > outerJoinCount + 1) {
                badPropertyName = rightNode.ResolvedPropertyName;
            }
            if (badPropertyName != null) {
                var message = "Outer join ON-clause invalid scope for property" +
                        " '" + badPropertyName + "', expecting the current or a prior stream scope";
                throw new EPStatementException("Error validating expression: " + message, statementContext.Expression);
            }
    
            return new UniformPair<int>(streamIdLeft, streamIdRight);
        }

        internal static ExprNode ValidateExprNoAgg(
            ExprNodeOrigin exprNodeOrigin,
            ExprNode exprNode,
            StreamTypeService streamTypeService,
            StatementContext statementContext,
            ExprEvaluatorContext exprEvaluatorContext,
            string errorMsg,
            bool allowTableConsumption)
        {
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypeService, 
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null, 
                statementContext.SchedulingService,
                statementContext.VariableService,
                statementContext.TableService, 
                exprEvaluatorContext,
                statementContext.EventAdapterService, 
                statementContext.StatementName, 
                statementContext.StatementId,
                statementContext.Annotations, 
                statementContext.ContextDescriptor, 
                statementContext.ScriptingService,
                false, false, allowTableConsumption,
                false, null, false);
            var validated = ExprNodeUtility.GetValidatedSubtree(exprNodeOrigin, exprNode, validationContext);
            ValidateNoAggregations(validated, errorMsg);
            return validated;
        }

        internal static void ValidateNoAggregations(ExprNode exprNode, string errorMsg)
        {
            // Make sure there is no aggregation in the where clause
            var aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(exprNode, aggregateNodes);
            if (!aggregateNodes.IsEmpty())
            {
                throw new ExprValidationException(errorMsg);
            }
        }

        // Special-case validation: When an on-merge query in the not-matched clause uses a subquery then
        // that subquery should not reference any of the stream's properties which are not-matched
        internal static void ValidateSubqueryExcludeOuterStream(ExprNode matchCondition)
        {
            var visitorSubselects = new ExprNodeSubselectDeclaredDotVisitor();
            matchCondition.Accept(visitorSubselects);
            if (visitorSubselects.Subselects.IsEmpty())
            {
                return;
            }
            var visitorProps = new ExprNodeIdentifierCollectVisitor();
            foreach (var node in visitorSubselects.Subselects)
            {
                if (node.StatementSpecCompiled.FilterRootNode != null)
                {
                    node.StatementSpecCompiled.FilterRootNode.Accept(visitorProps);
                }
            }
            foreach (var node in visitorProps.ExprProperties)
            {
                if (node.StreamId == 1)
                {
                    throw new ExprValidationException(
                        "On-Merge not-matched filter expression may not use properties that are provided by the named window event");
                }
            }
        }
    }
} // end of namespace
