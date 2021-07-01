///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Enumeration for the type of arithmetic to use.
    /// </summary>
    public enum MinMaxTypeEnum
    {
        /// <summary>
        ///     Max.
        /// </summary>
        MAX,

        /// <summary>
        ///     Min.
        /// </summary>
        MIN
    }

    public static class MinMaxTypeEnumExtensions {
        /// <summary>
        ///     Returns textual representation of enum.
        /// </summary>
        /// <returns>text for enum</returns>
        public static string GetExpressionText(this MinMaxTypeEnum value)
        {
            switch (value)
            {
                case MinMaxTypeEnum.MAX:
                    return "max";
                case MinMaxTypeEnum.MIN:
                    return "min";
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
} // end of namespace