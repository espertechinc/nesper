///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerHelper;
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerWidthBasic; //planRemainingNodesBasic
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecCompilerIndexPlannerWidthWithConditions; //planRemainingNodesWithConditions
using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecPlanForge; //makePlanFromTriplets

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecCompilerIndexPlanner
    {
	    /// <summary>
	    ///     Assigned for filter parameters that are based on boolean expression and not on
	    ///     any particular property name.
	    ///     <para>
	    ///         Keeping this artificial property name is a simplification as optimized filter parameters
	    ///         generally keep a property name.
	    ///     </para>
	    /// </summary>
	    public const string PROPERTY_NAME_BOOLEAN_EXPRESSION = ".boolean_expression";

        public static FilterSpecPlanForge PlanFilterParameters(
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            var plan = PlanFilterParametersInternal(validatedNodes, args);
            PromoteControlConfirmSinglePathSingleTriplet(plan);
            return plan;
        }

        private static FilterSpecPlanForge PlanFilterParametersInternal(
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            if (validatedNodes.IsEmpty()) {
                return EMPTY;
            }

            if (args.compileTimeServices.Configuration.Compiler.Execution.FilterIndexPlanning == ConfigurationCompilerExecution.FilterIndexPlanningEnum.NONE) {
                DecomposeCheckAggregation(validatedNodes);
                return BuildNoPlan(validatedNodes, args);
            }

            var performConditionPlanning = HasLevelOrHint(FilterSpecCompilerIndexPlannerHint.CONDITIONS, args.statementRawInfo, args.compileTimeServices);
            var filterParamExprMap = new FilterSpecParaForgeMap();

            // Make filter parameter for each expression node, if it can be optimized.
            // Optionally receive a top-level control condition that negates
            var topLevelNegation = DecomposePopulateConsolidate(filterParamExprMap, performConditionPlanning, validatedNodes, args);

            // Use all filter parameter and unassigned expressions
            var countUnassigned = filterParamExprMap.CountUnassignedExpressions();

            // we are done if there are no remaining nodes
            if (countUnassigned == 0) {
                return MakePlanFromTriplets(filterParamExprMap.Triplets, topLevelNegation, args);
            }

            // determine max-width
            var filterServiceMaxFilterWidth = args.compileTimeServices.Configuration.Compiler.Execution.FilterServiceMaxFilterWidth;
            var hint = HintEnum.MAX_FILTER_WIDTH.GetHint(args.statementRawInfo.Annotations);
            if (hint != null) {
                var hintValue = HintEnum.MAX_FILTER_WIDTH.GetHintAssignedValue(hint);
                filterServiceMaxFilterWidth = int.Parse(hintValue);
            }

            FilterSpecPlanForge plan = null;
            if (filterServiceMaxFilterWidth > 0) {
                if (performConditionPlanning) {
                    plan = PlanRemainingNodesWithConditions(filterParamExprMap, args, filterServiceMaxFilterWidth, topLevelNegation);
                }
                else {
                    plan = PlanRemainingNodesBasic(filterParamExprMap, args, filterServiceMaxFilterWidth);
                }
            }

            if (plan != null) {
                return plan;
            }

            // handle no-plan
            var triplets = new List<FilterSpecPlanPathTripletForge>(filterParamExprMap.Triplets);
            var unassignedExpressions = filterParamExprMap.UnassignedExpressions;
            var triplet = MakeRemainingNode(unassignedExpressions, args);
            triplets.Add(triplet);
            return MakePlanFromTriplets(triplets, topLevelNegation, args);
        }

        private static FilterSpecPlanForge BuildNoPlan(
            IList<ExprNode> validatedNodes,
            FilterSpecCompilerArgs args)
        {
            var triplet = MakeRemainingNode(validatedNodes, args);
            FilterSpecPlanPathTripletForge[] triplets = {triplet};
            var path = new FilterSpecPlanPathForge(triplets, null);
            FilterSpecPlanPathForge[] paths = {path};
            return new FilterSpecPlanForge(paths, null, null, null);
        }

        private static void PromoteControlConfirmSinglePathSingleTriplet(FilterSpecPlanForge plan)
        {
            if (plan.Paths.Length != 1) {
                return;
            }

            var path = plan.Paths[0];
            if (path.Triplets.Length != 1) {
                return;
            }

            var controlConfirm = path.Triplets[0].TripletConfirm;
            if (controlConfirm == null) {
                return;
            }

            plan.FilterConfirm = controlConfirm;
            path.Triplets[0].TripletConfirm = null;
        }
    }
} // end of namespace