///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerAdvancedIndex; //handleAdvancedIndexDescProvider;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerBooleanLimited; //handleBooleanLimited;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerEquals; //handleEqualsAndRelOp;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerInSetOfValues; //handleInSetNode;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerOrToInRewrite; //rewriteOrToInIfApplicable;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerPlugInSingleRow; //handlePlugInSingleRow;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerRange; //handleRangeNode;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	/// <summary>
	/// Helper to compile (validate and optimize) filter expressions as used in pattern and filter-based streams.
	/// </summary>
	public class FilterSpecCompilerIndexPlannerConstituent
	{
		/// <summary>
		/// For a given expression determine if this is optimizable and create the filter parameter
		/// representing the expression, or null if not optimizable.
		/// </summary>
		/// <param name="constituent">is the expression to look at</param>
		/// <param name="performConditionPlanning"></param>
		/// <param name="taggedEventTypes">event types that provide non-array values</param>
		/// <param name="arrayEventTypes">event types that provide array values</param>
		/// <param name="statementName">statement name</param>
		/// <param name="streamTypeService">stream type service</param>
		/// <returns>filter parameter representing the expression, or null</returns>
		/// <throws>ExprValidationException if the expression is invalid</throws>
		internal static FilterSpecPlanPathTripletForge MakeFilterParam(
			ExprNode constituent,
			bool performConditionPlanning,
			IDictionary<string, Pair<EventType, string>> taggedEventTypes,
			IDictionary<string, Pair<EventType, string>> arrayEventTypes,
			ISet<string> allTagNamesOrdered,
			string statementName,
			StreamTypeService streamTypeService,
			StatementRawInfo raw,
			StatementCompileTimeServices services)
		{
			// Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
			if ((constituent is ExprEqualsNode) || (constituent is ExprRelationalOpNode)) {
				var param = HandleEqualsAndRelOp(
					constituent,
					taggedEventTypes,
					arrayEventTypes,
					allTagNamesOrdered,
					statementName,
					raw,
					services);
				if (param != null) {
					return new FilterSpecPlanPathTripletForge(param, null);
				}
			}

			constituent = RewriteOrToInIfApplicable(constituent, false);

			// Is this expression node a simple compare, i.e. a=5 or b<4; these can be indexed
			if (constituent is ExprInNode) {
				var param = HandleInSetNode((ExprInNode) constituent, taggedEventTypes, arrayEventTypes, allTagNamesOrdered, raw, services);
				if (param != null) {
					return new FilterSpecPlanPathTripletForge(param, null);
				}
			}

			if (constituent is ExprBetweenNode) {
				var param = HandleRangeNode(
					(ExprBetweenNode) constituent,
					taggedEventTypes,
					arrayEventTypes,
					allTagNamesOrdered,
					statementName,
					raw,
					services);
				if (param != null) {
					return new FilterSpecPlanPathTripletForge(param, null);
				}
			}

			if (constituent is ExprPlugInSingleRowNode) {
				var param = HandlePlugInSingleRow((ExprPlugInSingleRowNode) constituent);
				if (param != null) {
					return new FilterSpecPlanPathTripletForge(param, null);
				}
			}

			if (constituent is FilterSpecCompilerAdvIndexDescProvider) {
				var param = HandleAdvancedIndexDescProvider(
					(FilterSpecCompilerAdvIndexDescProvider) constituent,
					arrayEventTypes,
					statementName);
				if (param != null) {
					return new FilterSpecPlanPathTripletForge(param, null);
				}
			}

			if (constituent is ExprOrNode && performConditionPlanning) {
				return HandleOrAlternateExpression(
					(ExprOrNode) constituent,
					performConditionPlanning,
					taggedEventTypes,
					arrayEventTypes,
					allTagNamesOrdered,
					statementName,
					streamTypeService,
					raw,
					services);
			}

			var paramX = HandleBooleanLimited(
				constituent,
				taggedEventTypes,
				arrayEventTypes,
				allTagNamesOrdered,
				streamTypeService,
				raw,
				services);
			if (paramX != null) {
				return new FilterSpecPlanPathTripletForge(paramX, null);
			}

			return null;
		}

		private static FilterSpecPlanPathTripletForge HandleOrAlternateExpression(
			ExprOrNode orNode,
			bool performConditionPlanning,
			IDictionary<string, Pair<EventType, string>> taggedEventTypes,
			IDictionary<string, Pair<EventType, string>> arrayEventTypes,
			ISet<string> allTagNamesOrdered,
			string statementName,
			StreamTypeService streamTypeService,
			StatementRawInfo raw,
			StatementCompileTimeServices services)
		{
			IList<ExprNode> valueExpressions = new List<ExprNode>(orNode.ChildNodes.Length);
			foreach (var child in orNode.ChildNodes) {
				var visitor = new FilterSpecExprNodeVisitorValueLimitedExpr();
				child.Accept(visitor);
				if (visitor.IsLimited) {
					valueExpressions.Add(child);
				}
			}

			// The or-node must have a single constituent and one or more value expressions
			if (orNode.ChildNodes.Length != valueExpressions.Count + 1) {
				return null;
			}

			IList<ExprNode> constituents = new List<ExprNode>(orNode.ChildNodes);
			constituents.RemoveAll(valueExpressions);
			if (constituents.Count != 1) {
				throw new IllegalStateException("Found multiple constituents");
			}

			var constituent = constituents[0];

			var triplet = MakeFilterParam(
				constituent,
				performConditionPlanning,
				taggedEventTypes,
				arrayEventTypes,
				allTagNamesOrdered,
				statementName,
				streamTypeService,
				raw,
				services);
			if (triplet == null) {
				return null;
			}

			var controlConfirm = ExprNodeUtilityMake.ConnectExpressionsByLogicalOrWhenNeeded(valueExpressions);
			return new FilterSpecPlanPathTripletForge(triplet.Param, controlConfirm);
		}
	}
} // end of namespace
