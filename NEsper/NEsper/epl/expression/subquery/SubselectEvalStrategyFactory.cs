///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.subquery
{
    using DataCollection = System.Collections.Generic.ICollection<object>;
    using DataMap = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>Factory for subselect evaluation strategies. </summary>
    public class SubselectEvalStrategyFactory
    {
        /// <summary>Create a strategy. </summary>
        /// <param name="subselectExpression">expression node</param>
        /// <param name="isNot">true if negated</param>
        /// <param name="isAll">true for ALL</param>
        /// <param name="isAny">true for ANY</param>
        /// <param name="relationalOp">relational op, if any</param>
        /// <returns>strategy</returns>
        /// <throws>ExprValidationException if expression validation fails</throws>
        public static SubselectEvalStrategy CreateStrategy(ExprSubselectNode subselectExpression,
                                                           bool isNot,
                                                           bool isAll,
                                                           bool isAny,
                                                           RelationalOpEnum? relationalOp)
        {
            if (subselectExpression.ChildNodes.Length != 1)
            {
                throw new ExprValidationException("The Subselect-IN requires 1 child expression");
            }
            ExprNode valueExpr = subselectExpression.ChildNodes[0];
    
            // Must be the same boxed type returned by expressions under this
            Type typeOne = subselectExpression.ChildNodes[0].ExprEvaluator.ReturnType.GetBoxedType();
    
            // collections, array or map not supported
            if ((typeOne.IsArray) ||
                (typeOne.IsImplementsInterface(typeof(DataCollection))) ||
                (typeOne.IsImplementsInterface(typeof(DataMap))))
            {
                throw new ExprValidationException("Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            var typeTwo = subselectExpression.SelectClause != null 
                ? subselectExpression.SelectClause[0].ExprEvaluator.ReturnType 
                : subselectExpression.RawEventType.UnderlyingType;
    
            if (relationalOp != null)
            {
                if ((typeOne != typeof(String)) || (typeTwo != typeof(String)))
                {
                    if (!typeOne.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", typeOne.FullName));
                    }
                    if (!typeTwo.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", typeTwo.FullName));
                    }
                }
    
                Type compareType = typeOne.GetCompareToCoercionType(typeTwo);
                RelationalOpEnumExtensions.Computer computer = relationalOp.Value.GetComputer(compareType, typeOne, typeTwo);
    
                ExprEvaluator selectClause = subselectExpression.SelectClause == null ? null : subselectExpression.SelectClause[0].ExprEvaluator;
                ExprEvaluator filterExpr = subselectExpression.FilterExpr;
                if (isAny)
                {
                    return new SubselectEvalStrategyRelOpAny(computer, valueExpr.ExprEvaluator,selectClause ,filterExpr);
                }
                return new SubselectEvalStrategyRelOpAll(computer, valueExpr.ExprEvaluator, selectClause, filterExpr);
            }
    
            // Get the common type such as Bool, String or Double and Long
            Type coercionType;
            try
            {
                coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            }
            catch (CoercionException)
            {
                throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to '{1}' is not allowed", typeTwo.FullName, typeOne.FullName));
            }
    
            // Check if we need to coerce
            bool mustCoerce = false;
            if ((coercionType != typeOne.GetBoxedType()) || (coercionType != typeTwo.GetBoxedType()))
            {
                if (coercionType.IsNumeric())
                {
                    mustCoerce = true;
                }
            }
    
            var value = valueExpr.ExprEvaluator;
            var select = subselectExpression.SelectClause == null ? null : subselectExpression.SelectClause[0].ExprEvaluator;
            var filter = subselectExpression.FilterExpr;

            if (isAll)
            {
                return new SubselectEvalStrategyEqualsAll(isNot, mustCoerce, coercionType, value, select, filter);
            }
            else if (isAny)
            {
                return new SubselectEvalStrategyEqualsAny(isNot, mustCoerce, coercionType, value, select, filter);
            }
            else
            {
                return new SubselectEvalStrategyEqualsIn(isNot, mustCoerce, coercionType, value, select, filter);
            }
        }
    }
}
