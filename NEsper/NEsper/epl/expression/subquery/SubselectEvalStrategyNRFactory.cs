///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    using RelationalComputer = Func<object, object, bool>;

    /// <summary>Factory for subselect evaluation strategies.</summary>
    public static class SubselectEvalStrategyNRFactory
    {
        public static SubselectEvalStrategyNR CreateStrategyExists(ExprSubselectExistsNode subselectExpression) {
            bool aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            bool grouped = Grouped(subselectExpression.StatementSpecCompiled.GroupByExpressions);
            if (grouped) {
                if (subselectExpression.HavingExpr != null) {
                    return new SubselectEvalStrategyNRExistsWGroupByWHaving(subselectExpression.HavingExpr);
                }
                return SubselectEvalStrategyNRExistsWGroupBy.INSTANCE;
            }
            if (aggregated) {
                if (subselectExpression.HavingExpr != null) {
                    return new SubselectEvalStrategyNRExistsAggregated(subselectExpression.HavingExpr);
                }
                return SubselectEvalStrategyNRExistsAlwaysTrue.INSTANCE;
            }
            return new SubselectEvalStrategyNRExistsDefault(subselectExpression.FilterExpr, subselectExpression.HavingExpr);
        }
    
        public static SubselectEvalStrategyNR CreateStrategyAnyAllIn(ExprSubselectNode subselectExpression,
                                                                     bool isNot,
                                                                     bool isAll,
                                                                     bool isAny,
                                                                     RelationalOpEnum? relationalOp)
        {
            if (subselectExpression.ChildNodes.Count != 1) {
                throw new ExprValidationException("The Subselect-IN requires 1 child expression");
            }
            ExprNode valueExpr = subselectExpression.ChildNodes[0];
    
            // Must be the same boxed type returned by expressions under this
            Type typeOne = subselectExpression.ChildNodes[0].ExprEvaluator.ReturnType.GetBoxedType();
    
            // collections, array or map not supported
            if ((typeOne.IsArray) ||
                (typeOne.IsImplementsInterface(typeof(ICollection<object>))) || 
                (typeOne.IsImplementsInterface(typeof(IDictionary<string, object>)))) {
                throw new ExprValidationException("Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }
    
            Type typeTwo;
            if (subselectExpression.SelectClause != null) {
                typeTwo = subselectExpression.SelectClause[0].ExprEvaluator.ReturnType;
            } else {
                typeTwo = subselectExpression.RawEventType.UnderlyingType;
            }
    
            bool aggregated = Aggregated(subselectExpression.SubselectAggregationType);
            bool grouped = Grouped(subselectExpression.StatementSpecCompiled.GroupByExpressions);
            ExprEvaluator selectEval = subselectExpression.SelectClause == null ? null : subselectExpression.SelectClause[0].ExprEvaluator;
            ExprEvaluator valueEval = valueExpr.ExprEvaluator;
            ExprEvaluator filterEval = subselectExpression.FilterExpr;
            ExprEvaluator havingEval = subselectExpression.HavingExpr;
    
            if (relationalOp != null) {
                if ((typeOne != typeof(string)) || (typeTwo != typeof(string))) {
                    if (!typeOne.IsNumeric()) {
                        throw new ExprValidationException("Implicit conversion from datatype '" + Name.Clean(typeOne) + "' to numeric is not allowed");
                    }
                    if (!typeTwo.IsNumeric()) {
                        throw new ExprValidationException("Implicit conversion from datatype '" + Name.Clean(typeTwo) + "' to numeric is not allowed");
                    }
                }
    
                Type compareType = typeOne.GetCompareToCoercionType(typeTwo);
                RelationalComputer computer = relationalOp.Value.GetComputer(compareType, typeOne, typeTwo);
    
                if (isAny) {
                    if (grouped) {
                        return new SubselectEvalStrategyNRRelOpAnyWGroupBy(valueEval, selectEval, false, computer, havingEval);
                    }
                    if (aggregated) {
                        return new SubselectEvalStrategyNRRelOpAllAnyAggregated(valueEval, selectEval, false, computer, havingEval);
                    }
                    return new SubselectEvalStrategyNRRelOpAnyDefault(valueEval, selectEval, false, computer, filterEval);
                }
    
                // handle ALL
                if (grouped) {
                    return new SubselectEvalStrategyNRRelOpAllWGroupBy(valueEval, selectEval, true, computer, havingEval);
                }
                if (aggregated) {
                    return new SubselectEvalStrategyNRRelOpAllAnyAggregated(valueEval, selectEval, true, computer, havingEval);
                }
                return new SubselectEvalStrategyNRRelOpAllDefault(valueEval, selectEval, true, computer, filterEval);
            }
    
            Coercer coercer = GetCoercer(typeOne, typeTwo);
            if (isAll) {
                if (grouped) {
                    return new SubselectEvalStrategyNREqualsAllWGroupBy(valueEval, selectEval, true, isNot, coercer, havingEval);
                }
                if (aggregated) {
                    return new SubselectEvalStrategyNREqualsAllAnyAggregated(valueEval, selectEval, true, isNot, coercer, havingEval);
                }
                return new SubselectEvalStrategyNREqualsAllDefault(valueEval, selectEval, true, isNot, coercer, filterEval);
            } else if (isAny) {
                if (grouped) {
                    return new SubselectEvalStrategyNREqualsAnyWGroupBy(valueEval, selectEval, false, isNot, coercer, havingEval);
                }
                if (aggregated) {
                    return new SubselectEvalStrategyNREqualsAllAnyAggregated(valueEval, selectEval, true, isNot, coercer, havingEval);
                }
                return new SubselectEvalStrategyNREqualsAnyDefault(valueEval, selectEval, false, isNot, coercer, filterEval);
            } else {
                if (grouped) {
                    return new SubselectEvalStrategyNREqualsInWGroupBy(valueEval, selectEval, isNot, coercer, havingEval);
                }
                if (aggregated) {
                    return new SubselectEvalStrategyNREqualsInAggregated(valueEval, selectEval, isNot, coercer, havingEval);
                }
                if (filterEval == null) {
                    return new SubselectEvalStrategyNREqualsInUnfiltered(valueEval, selectEval, isNot, coercer);
                }
                return new SubselectEvalStrategyNREqualsInFiltered(valueEval, selectEval, isNot, coercer, filterEval);
            }
        }
    
        private static Coercer GetCoercer(Type typeOne, Type typeTwo) {
            // Get the common type such as Bool, string or double? and Long
            Type coercionType;
            bool mustCoerce;
            try {
                coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            } catch (CoercionException ) {
                throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to '{1}' is not allowed", Name.Clean(typeTwo), Name.Clean(typeOne)));
            }
    
            // Check if we need to coerce
            mustCoerce = false;
            if ((coercionType != typeOne.GetBoxedType()) ||
                    (coercionType != typeTwo.GetBoxedType())) {
                if (coercionType.IsNumeric()) {
                    mustCoerce = true;
                }
            }
            return !mustCoerce ? null : CoercerFactory.GetCoercer(null, coercionType);
        }
    
        private static bool Grouped(GroupByClauseExpressions groupByExpressions) {
            return groupByExpressions != null && groupByExpressions.GroupByNodes != null && groupByExpressions.GroupByNodes.Length != 0;
        }
    
        private static bool Aggregated(ExprSubselectNode.SubqueryAggregationType subqueryAggregationType) {
            return subqueryAggregationType != ExprSubselectNode.SubqueryAggregationType.NONE;
        }
    }
} // end of namespace
