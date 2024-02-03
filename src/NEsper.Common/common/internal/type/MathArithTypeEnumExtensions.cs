///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
////////////////////

using System;

namespace com.espertech.esper.common.@internal.type
{
    public static class MathArithTypeEnumExtensions
    {
        /// <summary>
        ///     Returns string representation of enum.
        /// </summary>
        /// <returns>text for enum</returns>
        public static string GetExpressionText(this MathArithTypeEnum value)
        {
            return value switch {
                MathArithTypeEnum.ADD => "+",
                MathArithTypeEnum.DIVIDE => "/",
                MathArithTypeEnum.MODULO => "%",
                MathArithTypeEnum.MULTIPLY => "*",
                MathArithTypeEnum.SUBTRACT => "-",
                _ => throw new ArgumentException("invalid value for MathArithTypeEnum", nameof(value))
            };
        }

        /// <summary>
        ///     Returns the math operator for the string.
        /// </summary>
        /// <param name="operator">to parse</param>
        /// <returns>math enum</returns>
        public static MathArithTypeEnum ParseOperator(string @operator)
        {
            return @operator switch {
                "+" => MathArithTypeEnum.ADD,
                "-" => MathArithTypeEnum.SUBTRACT,
                "*" => MathArithTypeEnum.MULTIPLY,
                "/" => MathArithTypeEnum.DIVIDE,
                "%" => MathArithTypeEnum.MODULO,
                _ => throw new ArgumentException($"Unknown operator '{@operator}'")
            };
        }
    }
}