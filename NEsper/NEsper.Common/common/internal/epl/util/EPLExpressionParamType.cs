///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Castle.MicroKernel.Registration;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace com.espertech.esper.common.@internal.epl.util
{
    public enum EPLExpressionParamType
    {
        /// <summary>
        /// Boolean-type parameter
        /// </summary>
        BOOLEAN,
        /// <summary>
        /// Numeric-type parameter / value
        /// </summary>
        NUMERIC,
        /// <summary>
        /// Any parameter / value (e.g. object)
        /// </summary>
        ANY,
        /// <summary>
        /// A specific class as indicated by a separate container
        /// </summary>
        SPECIFIC,
        /// <summary>
        /// Time-period or number of seconds.
        /// </summary>
        TIME_PERIOD_OR_SEC,
        /// <summary>
        /// Date-time value.
        /// </summary>
        DATETIME
    }

    public static class EPLExpressionParamTypeExtensions
    {
        public static Type GetMethodParamType(this EPLExpressionParamType value)
        {
            if (value == EPLExpressionParamType.BOOLEAN)
                return typeof(bool?);
            return typeof(object);
        } 
    } 
}