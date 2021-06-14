///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Factory for subselect evaluation strategies.
    /// </summary>
    public class SubselectNRForgeFactory
    {
        public static SubselectForgeNR CreateStrategyExists(ExprSubselectExistsNode subselectExpression)
        {
            var aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            var grouped = Grouped(subselectExpression.StatementSpecCompiled.Raw.GroupByExpressions);
            if (grouped) {
                if (subselectExpression.HavingExpr != null) {
                    return new SubselectForgeNRExistsWGroupByWHaving(
                        subselectExpression,
                        subselectExpression.HavingExpr);
                }

                return new SubselectForgeNRExistsWGroupBy(subselectExpression);
            }

            if (aggregated) {
                if (subselectExpression.HavingExpr != null) {
                    return new SubselectForgeNRExistsAggregated(subselectExpression.HavingExpr);
                }

                return SubselectForgeNRExistsAlwaysTrue.INSTANCE;
            }

            return new SubselectForgeNRExistsDefault(subselectExpression.FilterExpr, subselectExpression.HavingExpr);
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

            // Must be the same boxed type returned by expressions under this
            var typeOne = subselectExpression.ChildNodes[0].Forge.EvaluationType.GetBoxedType();

            // collections, array or map not supported
            if (typeOne.IsArray ||
                typeOne.IsGenericCollection() ||
                typeOne.IsGenericDictionary()) {
                throw new ExprValidationException(
                    "Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            Type typeTwo;
            if (subselectExpression.SelectClause != null) {
                typeTwo = subselectExpression.SelectClause[0].Forge.EvaluationType;
            }
            else {
                typeTwo = subselectExpression.RawEventType.UnderlyingType;
            }

            var aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            var grouped = Grouped(subselectExpression.StatementSpecCompiled.Raw.GroupByExpressions);
            var selectEval = subselectExpression.SelectClause == null
                ? null
                : subselectExpression.SelectClause[0].Forge;
            var valueEval = valueExpr.Forge;
            var filterEval = subselectExpression.FilterExpr;
            var havingEval = subselectExpression.HavingExpr;

            if (relationalOp != null) {
                if (typeOne != typeof(string) || typeTwo != typeof(string)) {
                    if (!typeOne.IsNumeric()) {
                        throw new ExprValidationException(
                            "Implicit conversion from datatype '" +
                            typeOne.CleanName() +
                            "' to numeric is not allowed");
                    }

                    if (!typeTwo.IsNumeric()) {
                        throw new ExprValidationException(
                            "Implicit conversion from datatype '" +
                            typeTwo.CleanName() +
                            "' to numeric is not allowed");
                    }
                }

                var compareType = typeOne.GetCompareToCoercionType(typeTwo);
                var computer = relationalOp.Value.GetComputer(compareType, typeOne, typeTwo);

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

            var coercer = GetCoercer(typeOne, typeTwo);
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

            if (isAny) {
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
                    typeTwo.CleanName() +
                    "' to '" +
                    typeOne.CleanName() +
                    "' is not allowed");
            }

            // Check if we need to coerce
            mustCoerce = false;
            if (coercionType != typeOne.GetBoxedType() ||
                coercionType != typeTwo.GetBoxedType()) {
                if (coercionType.IsNumeric()) {
                    mustCoerce = true;
                }
            }

            return !mustCoerce ? null : SimpleNumberCoercerFactory.GetCoercer(null, coercionType);
        }

        private static bool Grouped(IList<GroupByClauseElement> groupByExpressions)
        {
            return groupByExpressions != null && !groupByExpressions.IsEmpty();
        }

        private static bool Aggregated(ExprSubselectNode.SubqueryAggregationType? subqueryAggregationType)
        {
            return subqueryAggregationType != null &&
                   subqueryAggregationType != ExprSubselectNode.SubqueryAggregationType.NONE;
        }
    }
} // end of namespace