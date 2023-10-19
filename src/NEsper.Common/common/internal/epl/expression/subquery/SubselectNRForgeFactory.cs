///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    /// <summary>
    /// Factory for subselect evaluation strategies.
    /// </summary>
    public class SubselectNRForgeFactory
    {
        public static SubselectForgeNR CreateStrategyExists(ExprSubselectExistsNode subselectExpression)
        {
            var aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            var grouped = Grouped(subselectExpression.StatementSpecCompiled.Raw.GroupByExpressions);
            if (grouped) {
                if (subselectExpression.havingExpr != null) {
                    return new SubselectForgeNRExistsWGroupByWHaving(
                        subselectExpression,
                        subselectExpression.havingExpr);
                }

                return new SubselectForgeNRExistsWGroupBy(subselectExpression);
            }

            if (aggregated) {
                if (subselectExpression.havingExpr != null) {
                    return new SubselectForgeNRExistsAggregated(subselectExpression.havingExpr);
                }

                return SubselectForgeNRExistsAlwaysTrue.INSTANCE;
            }

            return new SubselectForgeNRExistsDefault(subselectExpression.filterExpr, subselectExpression.havingExpr);
        }

        public static SubselectForgeNR CreateStrategyAnyAllIn(
            ExprSubselectNode subselectExpression,
            bool isNot,
            bool isAll,
            bool isAny,
            RelationalOpEnum? relationalOp,
            ImportServiceCompileTime importService)
        {
            if (subselectExpression.ChildNodes.Length != 1) {
                throw new ExprValidationException("The Subselect-IN requires 1 child expression");
            }

            var valueExpr = subselectExpression.ChildNodes[0];

            var typeOne = subselectExpression.ChildNodes[0].Forge.EvaluationType.GetBoxedType();
            var typeOneClass = ExprNodeUtilityValidate.ValidateLHSTypeAnyAllSomeIn(typeOne);

            Type typeTwoClass;
            if (subselectExpression.SelectClause != null) {
                var selectType = subselectExpression.SelectClause[0].Forge.EvaluationType;
                if (selectType == null) {
                    throw new ExprValidationException(
                        "Null-type value not allowed for the IN, ANY, SOME or ALL keywords");
                }

                typeTwoClass = selectType;
            }
            else {
                typeTwoClass = subselectExpression.RawEventType.UnderlyingType;
            }

            var aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            var grouped = Grouped(subselectExpression.StatementSpecCompiled.Raw.GroupByExpressions);
            var selectEval = subselectExpression.SelectClause?[0].Forge;
            var valueEval = valueExpr.Forge;
            var filterEval = subselectExpression.filterExpr;
            var havingEval = subselectExpression.havingExpr;

            if (relationalOp != null) {
                if ((typeOne != typeof(string)) || (typeTwoClass != typeof(string))) {
                    if (!typeOne.IsTypeNumeric()) {
                        throw new ExprValidationException(
                            "Implicit conversion from datatype '" +
                            typeOne.CleanName() +
                            "' to numeric is not allowed");
                    }

                    if (!typeTwoClass.IsTypeNumeric()) {
                        throw new ExprValidationException(
                            "Implicit conversion from datatype '" +
                            typeTwoClass.CleanName() +
                            "' to numeric is not allowed");
                    }
                }

                var compareType = typeOneClass.GetCompareToCoercionType(typeTwoClass);
                var computer = relationalOp.Value.GetComputer(compareType, typeOneClass, typeTwoClass);

                if (isAny) {
                    if (grouped) {
                        return new SubselectForgeNRRelOpAnyWGroupBy(
                            subselectExpression,
                            valueEval,
                            selectEval,
                            false,
                            computer,
                            havingEval);
                    }

                    if (aggregated) {
                        return new SubselectForgeNRRelOpAllAnyAggregated(
                            subselectExpression,
                            valueEval,
                            selectEval,
                            false,
                            computer,
                            havingEval);
                    }

                    return new SubselectForgeStrategyNRRelOpAnyDefault(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        false,
                        computer,
                        filterEval);
                }

                // handle ALL
                if (grouped) {
                    return new SubselectForgeNRRelOpAllWGroupBy(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        true,
                        computer,
                        havingEval);
                }

                if (aggregated) {
                    return new SubselectForgeNRRelOpAllAnyAggregated(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        true,
                        computer,
                        havingEval);
                }

                return new SubselectForgeNRRelOpAllDefault(
                    subselectExpression,
                    valueEval,
                    selectEval,
                    true,
                    computer,
                    filterEval);
            }

            var coercer = GetCoercer(typeOneClass, typeTwoClass);
            if (isAll) {
                if (grouped) {
                    return new SubselectForgeNREqualsAllAnyWGroupBy(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        true,
                        isNot,
                        coercer,
                        havingEval,
                        true);
                }

                if (aggregated) {
                    return new SubselectForgeNREqualsAllAnyAggregated(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        true,
                        isNot,
                        coercer,
                        havingEval);
                }

                return new SubselectForgeNREqualsDefault(
                    subselectExpression,
                    valueEval,
                    selectEval,
                    true,
                    isNot,
                    coercer,
                    filterEval,
                    true);
            }
            else if (isAny) {
                if (grouped) {
                    return new SubselectForgeNREqualsAllAnyWGroupBy(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        false,
                        isNot,
                        coercer,
                        havingEval,
                        false);
                }

                if (aggregated) {
                    return new SubselectForgeNREqualsAllAnyAggregated(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        true,
                        isNot,
                        coercer,
                        havingEval);
                }

                return new SubselectForgeNREqualsDefault(
                    subselectExpression,
                    valueEval,
                    selectEval,
                    false,
                    isNot,
                    coercer,
                    filterEval,
                    false);
            }
            else {
                if (grouped) {
                    return new SubselectForgeNREqualsInWGroupBy(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        isNot,
                        isNot,
                        coercer,
                        havingEval);
                }

                if (aggregated) {
                    return new SubselectForgeNREqualsInAggregated(
                        subselectExpression,
                        valueEval,
                        selectEval,
                        isNot,
                        isNot,
                        coercer,
                        havingEval);
                }

                return new SubselectForgeNREqualsIn(
                    subselectExpression,
                    valueEval,
                    selectEval,
                    isNot,
                    isNot,
                    coercer,
                    filterEval);
            }
        }

        private static Coercer GetCoercer(
            Type typeOne,
            Type typeTwo)
        {
            // Get the common type such as Bool, String or Double and Long
            Type coercionType;
            bool mustCoerce;
            try {
                coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            }
            catch (CoercionException) {
                throw new ExprValidationException(
                    "Implicit conversion from datatype '" +
                    typeTwo +
                    "' to '" +
                    typeOne +
                    "' is not allowed");
            }

            // Check if we need to coerce
            mustCoerce = false;
            if (!coercionType.Equals(typeOne.GetBoxedType()) ||
                !coercionType.Equals(typeTwo.GetBoxedType())) {
                if (coercionType.IsTypeNumeric()) {
                    mustCoerce = true;
                }
            }

            return !mustCoerce ? null : SimpleNumberCoercerFactory.GetCoercer(null, coercionType);
        }

        private static bool Grouped(IList<GroupByClauseElement> groupByExpressions)
        {
            return groupByExpressions != null && !groupByExpressions.IsEmpty();
        }

        private static bool Aggregated(ExprSubselectNode.SubqueryAggregationType subqueryAggregationType)
        {
            return subqueryAggregationType != null &&
                   subqueryAggregationType != ExprSubselectNode.SubqueryAggregationType.NONE;
        }
    }
} // end of namespace