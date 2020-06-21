///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertUtil
    {
        public static ExprValidationException MakeEventTypeCastException(
            EventType sourceType,
            EventType targetType)
        {
            return new ExprValidationException(
                "Expression-returned event type '" +
                sourceType.Name +
                "' with underlying type '" +
                sourceType.UnderlyingType.CleanName() +
                "' cannot be converted to target event type '" +
                targetType.Name +
                "' with underlying type '" +
                targetType.UnderlyingType.CleanName() +
                "'");
        }

        public static ExprValidationException MakeEventTypeCastException(
            Type sourceType,
            EventType targetType)
        {
            return new ExprValidationException(
                "Expression-returned value of type '" +
                sourceType.CleanName() +
                "' cannot be converted to target event type '" +
                targetType.Name +
                "' with underlying type '" +
                targetType.UnderlyingType.CleanName() +
                "'");
        }
    }
} // end of namespace