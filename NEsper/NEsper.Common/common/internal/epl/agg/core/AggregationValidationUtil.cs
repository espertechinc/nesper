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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationValidationUtil
    {
        public static void ValidateAggregationInputType(
            Type requiredParam,
            Type providedParam)
        {
            Type boxedRequired = requiredParam.GetBoxedType();
            Type boxedProvided = providedParam.GetBoxedType();
            if (boxedRequired != boxedProvided &&
                !TypeHelper.IsSubclassOrImplementsInterface(boxedProvided, boxedRequired)) {
                throw new ExprValidationException(
                    "The required parameter type is " +
                    requiredParam.CleanName() +
                    " and provided is " +
                    providedParam.CleanName());
            }
        }

        public static void ValidateAggregationFilter(
            bool requireFilter,
            bool provideFilter)
        {
            if (requireFilter != provideFilter) {
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (requireFilter ? "a" : "no") +
                    " filter expression and provided is " +
                    (provideFilter ? "a" : "no") +
                    " filter expression");
            }
        }

        public static void ValidateAggregationUnbound(
            bool requiredHasDataWindows,
            bool providedHasDataWindows)
        {
            if (requiredHasDataWindows != providedHasDataWindows) {
                throw new ExprValidationException(
                    "The table declares " +
                    (requiredHasDataWindows ? "use with data windows" : "unbound") +
                    " and provided is " +
                    (providedHasDataWindows ? "use with data windows" : "unbound"));
            }
        }

        public static void ValidateAggregationType(
            AggregationPortableValidation tableDeclared,
            string tableExpression,
            AggregationPortableValidation intoTableDeclared,
            string intoExpression)
        {
            if (tableDeclared.GetType() != intoTableDeclared.GetType()) {
                throw new ExprValidationException(
                    "The table declares '" +
                    tableExpression +
                    "' and provided is '" +
                    intoExpression +
                    "'");
            }
        }

        public static void ValidateAggFuncName(
            string requiredName,
            string providedName)
        {
            if (!requiredName.ToLowerInvariant().Equals(providedName)) {
                throw new ExprValidationException(
                    "The required aggregation function name is '" +
                    requiredName +
                    "' and provided is '" +
                    providedName +
                    "'");
            }
        }

        public static void ValidateDistinct(
            bool required,
            bool provided)
        {
            if (required != provided) {
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (required ? "a" : "no") +
                    " distinct and provided is " +
                    (provided ? "a" : "no") +
                    " distinct");
            }
        }

        public static void ValidateEventType(
            EventType requiredType,
            EventType providedType)
        {
            if (!EventTypeUtility.IsTypeOrSubTypeOf(providedType, requiredType)) {
                throw new ExprValidationException(
                    "The required event type is '" +
                    requiredType.Name +
                    "' and provided is '" +
                    providedType.Name +
                    "'");
            }
        }
    }
} // end of namespace