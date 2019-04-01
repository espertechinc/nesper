///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.statement.helper
{
	public class EPStatementStartMethodHelperValidate {
	    // Special-case validation: When an on-merge query in the not-matched clause uses a subquery then
	    // that subquery should not reference any of the stream's properties which are not-matched
	    public static void ValidateSubqueryExcludeOuterStream(ExprNode matchCondition) {
	        ExprNodeSubselectDeclaredDotVisitor visitorSubselects = new ExprNodeSubselectDeclaredDotVisitor();
	        matchCondition.Accept(visitorSubselects);
	        if (visitorSubselects.Subselects.IsEmpty()) {
	            return;
	        }
	        ExprNodeIdentifierCollectVisitor visitorProps = new ExprNodeIdentifierCollectVisitor();
	        foreach (ExprSubselectNode node in visitorSubselects.Subselects) {
	            if (node.StatementSpecCompiled.Raw.WhereClause != null) {
	                node.StatementSpecCompiled.Raw.WhereClause.Accept(visitorProps);
	            }
	        }
	        foreach (ExprIdentNode node in visitorProps.ExprProperties) {
	            if (node.StreamId == 1) {
	                throw new ExprValidationException("On-Merge not-matched filter expression may not use properties that are provided by the named window event");
	            }
	        }
	    }

	    public static ExprNode ValidateExprNoAgg(ExprNodeOrigin exprNodeOrigin, ExprNode exprNode, StreamTypeService streamTypeService, string errorMsg, bool allowTableConsumption, StatementRawInfo raw, StatementCompileTimeServices compileTimeServices) {
	        ExprValidationContext validationContext = new ExprValidationContextBuilder(streamTypeService, raw, compileTimeServices)
	                .WithAllowBindingConsumption(allowTableConsumption).Build();
	        ExprNode validated = ExprNodeUtilityValidate.GetValidatedSubtree(exprNodeOrigin, exprNode, validationContext);
	        ValidateNoAggregations(validated, errorMsg);
	        return validated;
	    }

	    public static void ValidateNoDataWindowOnNamedWindow(IList<ViewFactoryForge> forges) {
	        AtomicBoolean hasDataWindow = new AtomicBoolean();
	        ViewForgeVisitor visitor = forge => {
	            if (forge is DataWindowViewForge) {
	                hasDataWindow.Set(true);
	            }
	        };
	        foreach (ViewFactoryForge forge in forges) {
	            forge.Accept(visitor);
	        }
	        if (hasDataWindow.Get()) {
	            throw new ExprValidationException(NamedWindowManagementService.ERROR_MSG_NO_DATAWINDOW_ALLOWED);
	        }
	    }

	    public static ExprNode ValidateNodes(StatementSpecRaw statementSpec, StreamTypeService typeService, ViewResourceDelegateExpr viewResourceDelegate, StatementRawInfo statementRawInfo,
	                                         StatementCompileTimeServices compileTimeServices) {
	        string intoTableName = statementSpec.IntoTableSpec == null ? null : statementSpec.IntoTableSpec.Name;

	        ExprNode whereClauseValidated = null;
	        if (statementSpec.WhereClause != null) {
	            ExprNode whereClause = statementSpec.WhereClause;

	            // Validate where clause, initializing nodes to the stream ids used
	            try {
	                ExprValidationContext validationContext = new ExprValidationContextBuilder(typeService, statementRawInfo, compileTimeServices)
	                        .WithViewResourceDelegate(viewResourceDelegate)
	                        .WithAllowBindingConsumption(true)
	                        .WithIntoTableName(intoTableName)
	                        .Build();
	                whereClause = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.FILTER, whereClause, validationContext);
	                if (whereClause.Forge.EvaluationType != typeof(bool) && whereClause.Forge.EvaluationType != typeof(bool?)) {
	                    throw new ExprValidationException("The where-clause filter expression must return a boolean value");
	                }
	                whereClauseValidated = whereClause;

	                // Make sure there is no aggregation in the where clause
	                IList<ExprAggregateNode> aggregateNodes = new LinkedList<ExprAggregateNode>();
	                ExprAggregateNodeUtil.GetAggregatesBottomUp(whereClause, aggregateNodes);
	                if (!aggregateNodes.IsEmpty()) {
	                    throw new ExprValidationException("An aggregate function may not appear in a WHERE clause (use the HAVING clause)");
	                }
	            } catch (ExprValidationException ex) {
	                throw new ExprValidationException("Error validating expression: " + ex.Message, ex);
	            }
	        }

	        if ((statementSpec.OutputLimitSpec != null) && ((statementSpec.OutputLimitSpec.WhenExpressionNode != null) || (statementSpec.OutputLimitSpec.AndAfterTerminateExpr != null))) {
	            // Validate where clause, initializing nodes to the stream ids used
	            EventType outputLimitType = OutputConditionExpressionTypeUtil.GetBuiltInEventType(statementRawInfo.ModuleName, compileTimeServices.BeanEventTypeFactoryPrivate);
	            StreamTypeService typeServiceOutputWhen = new StreamTypeServiceImpl(new EventType[]{outputLimitType}, new string[]{null}, new bool[]{true}, false, false);
	            ExprValidationContext validationContext = new ExprValidationContextBuilder(typeServiceOutputWhen, statementRawInfo, compileTimeServices)
	                    .WithIntoTableName(intoTableName).Build();

	            ExprNode outputLimitWhenNode = statementSpec.OutputLimitSpec.WhenExpressionNode;
	            if (outputLimitWhenNode != null) {
	                outputLimitWhenNode = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, outputLimitWhenNode, validationContext);
	                statementSpec.OutputLimitSpec.WhenExpressionNode = outputLimitWhenNode;

	                if (Boxing.GetBoxedType(outputLimitWhenNode.Forge.EvaluationType) != typeof(bool?)) {
	                    throw new ExprValidationException("The when-trigger expression in the OUTPUT WHEN clause must return a boolean-type value");
	                }
	                EPStatementStartMethodHelperValidate.ValidateNoAggregations(outputLimitWhenNode, "An aggregate function may not appear in a OUTPUT LIMIT clause");
	            }

	            // validate and-terminate expression if provided
	            if (statementSpec.OutputLimitSpec.AndAfterTerminateExpr != null) {
	                if (statementSpec.OutputLimitSpec.RateType != OutputLimitRateType.WHEN_EXPRESSION && statementSpec.OutputLimitSpec.RateType != OutputLimitRateType.TERM) {
	                    throw new ExprValidationException("A terminated-and expression must be used with the OUTPUT WHEN clause");
	                }
	                ExprNode validated = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.OUTPUTLIMIT, statementSpec.OutputLimitSpec.AndAfterTerminateExpr, validationContext);
	                statementSpec.OutputLimitSpec.AndAfterTerminateExpr = validated;

	                if (Boxing.GetBoxedType(validated.Forge.EvaluationType) != typeof(bool?)) {
	                    throw new ExprValidationException("The terminated-and expression must return a boolean-type value");
	                }
	                EPStatementStartMethodHelperValidate.ValidateNoAggregations(validated, "An aggregate function may not appear in a terminated-and clause");
	            }

	            // validate then-expression
	            ValidateThenSetAssignments(statementSpec.OutputLimitSpec.ThenExpressions, validationContext);

	            // validate after-terminated then-expression
	            ValidateThenSetAssignments(statementSpec.OutputLimitSpec.AndAfterTerminateThenExpressions, validationContext);
	        }

	        for (int outerJoinCount = 0; outerJoinCount < statementSpec.OuterJoinDescList.Count; outerJoinCount++) {
	            OuterJoinDesc outerJoinDesc = statementSpec.OuterJoinDescList.Get(outerJoinCount);

	            // validate on-expression nodes, if provided
	            if (outerJoinDesc.OptLeftNode != null) {
	                UniformPair<int> streamIdPair = ValidateOuterJoinPropertyPair(outerJoinDesc.OptLeftNode, outerJoinDesc.OptRightNode, outerJoinCount, typeService, viewResourceDelegate, statementRawInfo, compileTimeServices
	                );

	                if (outerJoinDesc.AdditionalLeftNodes != null) {
	                    ISet<int> streamSet = new HashSet<int>();
	                    streamSet.Add(streamIdPair.First);
	                    streamSet.Add(streamIdPair.Second);
	                    for (int i = 0; i < outerJoinDesc.AdditionalLeftNodes.Length; i++) {
	                        UniformPair<int> streamIdPairAdd = ValidateOuterJoinPropertyPair(outerJoinDesc.AdditionalLeftNodes[i], outerJoinDesc.AdditionalRightNodes[i], outerJoinCount, typeService, viewResourceDelegate, statementRawInfo, compileTimeServices
	                        );

	                        // make sure all additional properties point to the same two streams
	                        if (!streamSet.Contains(streamIdPairAdd.First) || (!streamSet.Contains(streamIdPairAdd.Second))) {
	                            string message = "Outer join ON-clause columns must refer to properties of the same joined streams" +
	                                    " when using multiple columns in the on-clause";
	                            throw new ExprValidationException("Error validating outer-join expression: " + message);
	                        }

	                    }
	                }
	            }
	        }

	        return whereClauseValidated;
	    }

	    protected internal static UniformPair<int> ValidateOuterJoinPropertyPair(
	            ExprIdentNode leftNode, ExprIdentNode rightNode, int outerJoinCount, StreamTypeService typeService,
	            ViewResourceDelegateExpr viewResourceDelegate, StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        // Validate the outer join clause using an artificial equals-node on top.
	        // Thus types are checked via equals.
	        // Sets stream ids used for validated nodes.
	        ExprNode equalsNode = new ExprEqualsNodeImpl(false, false);
	        equalsNode.AddChildNode(leftNode);
	        equalsNode.AddChildNode(rightNode);
	        try {
	            ExprValidationContext validationContext = new ExprValidationContextBuilder(typeService, statementRawInfo, compileTimeServices)
	                    .WithViewResourceDelegate(viewResourceDelegate).WithAllowBindingConsumption(true).WithIsFilterExpression(true).Build();
	            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.JOINON, equalsNode, validationContext);
	        } catch (ExprValidationException ex) {
	            throw new ExprValidationException("Error validating outer-join expression: " + ex.Message, ex);
	        }

	        // Make sure we have left-hand-side and right-hand-side refering to different streams
	        int streamIdLeft = leftNode.StreamId;
	        int streamIdRight = rightNode.StreamId;
	        if (streamIdLeft == streamIdRight) {
	            string message = "Outer join ON-clause cannot refer to properties of the same stream";
	            throw new ExprValidationException("Error validating outer-join expression: " + message);
	        }

	        // Make sure one of the properties refers to the acutual stream currently being joined
	        int expectedStreamJoined = outerJoinCount + 1;
	        if ((streamIdLeft != expectedStreamJoined) && (streamIdRight != expectedStreamJoined)) {
	            string message = "Outer join ON-clause must refer to at least one property of the joined stream" +
	                    " for stream " + expectedStreamJoined;
	            throw new ExprValidationException("Error validating outer-join expression: " + message);
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
	            string message = "Outer join ON-clause invalid scope for property" +
	                    " '" + badPropertyName + "', expecting the current or a prior stream scope";
	            throw new ExprValidationException("Error validating outer-join expression: " + message);
	        }

	        return new UniformPair<int>(streamIdLeft, streamIdRight);
	    }

	    public static void ValidateNoAggregations(ExprNode exprNode, string errorMsg)
	            {
	        // Make sure there is no aggregation in the where clause
	        IList<ExprAggregateNode> aggregateNodes = new LinkedList<ExprAggregateNode>();
	        ExprAggregateNodeUtil.GetAggregatesBottomUp(exprNode, aggregateNodes);
	        if (!aggregateNodes.IsEmpty()) {
	            throw new ExprValidationException(errorMsg);
	        }
	    }

	    private static void ValidateThenSetAssignments(IList<OnTriggerSetAssignment> assignments, ExprValidationContext validationContext)
	            {
	        if (assignments == null || assignments.IsEmpty()) {
	            return;
	        }
	        foreach (OnTriggerSetAssignment assign in assignments) {
	            ExprNode node = ExprNodeUtilityValidate.GetValidatedAssignment(assign, validationContext);
	            assign.Expression = node;
	            EPStatementStartMethodHelperValidate.ValidateNoAggregations(node, "An aggregate function may not appear in a OUTPUT LIMIT clause");
	        }
	    }
	}
} // end of namespace