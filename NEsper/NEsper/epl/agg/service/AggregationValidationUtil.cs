///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationValidationUtil
    {
        public static void ValidateAggregationType(
            AggregationMethodFactory requiredFactory,
            AggregationMethodFactory providedFactory)
        {
            if (!TypeHelper.IsSubclassOrImplementsInterface(providedFactory.GetType(), requiredFactory.GetType()))
                throw new ExprValidationException(
                    "Not a '" + requiredFactory.AggregationExpression.AggregationFunctionName + "' aggregation");
            ExprAggregateNode aggNodeRequired = requiredFactory.AggregationExpression;
            ExprAggregateNode aggNodeProvided = providedFactory.AggregationExpression;
            if (aggNodeRequired.IsDistinct != aggNodeProvided.IsDistinct)
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (aggNodeRequired.IsDistinct ? "a" : "no") + " distinct and provided is " +
                    (aggNodeProvided.IsDistinct ? "a" : "no") + " distinct");
        }

        public static void ValidateAggregationInputType(
            Type requiredParam,
            Type providedParam)
        {
            var boxedRequired = requiredParam.GetBoxedType();
            var boxedProvided = providedParam.GetBoxedType();
            if (boxedRequired != boxedProvided &&
                !TypeHelper.IsSubclassOrImplementsInterface(boxedProvided, boxedRequired))
                throw new ExprValidationException(
                    "The required parameter type is " +
                    requiredParam.GetCleanName() +
                    " and provided is " +
                    providedParam.GetCleanName());
        }

        public static void ValidateAggregationFilter(
            bool requireFilter,
            bool provideFilter)
        {
            if (requireFilter != provideFilter)
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (requireFilter ? "a" : "no") + " filter expression and provided is " +
                    (provideFilter ? "a" : "no") + " filter expression");
        }

        public static void ValidateAggregationUnbound(
            bool requiredHasDataWindows, 
            bool providedHasDataWindows)
        {
            if (requiredHasDataWindows != providedHasDataWindows)
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (requiredHasDataWindows ? "use with data windows" : "unbound") +
                    " and provided is " +
                    (providedHasDataWindows ? "use with data windows" : "unbound"));
        }

        public static void ValidateEventType(
            EventType requiredType, 
            EventType providedType)
        {
            if (!EventTypeUtility.IsTypeOrSubTypeOf(providedType, requiredType))
                throw new ExprValidationException(
                    "The required event type is '" +
                    requiredType.Name + "' and provided is '" + providedType.Name + "'");
        }

        public static void ValidateAggFuncName(
            string requiredName,
            string providedName)
        {
            if (requiredName.ToLowerInvariant() != providedName)
                throw new ExprValidationException(
                    "The required aggregation function name is '" +
                    requiredName + "' and provided is '" + providedName + "'");
        }

        public static void ValidateStreamNumZero(int streamNum)
        {
            if (streamNum != 0)
                throw new ExprValidationException(
                    "The from-clause order requires the stream in position zero");
        }
    }
} // end of namespace