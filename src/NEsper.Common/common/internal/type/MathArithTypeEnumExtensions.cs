///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
            switch (value) {
                case MathArithTypeEnum.ADD:
                    return "+";

                case MathArithTypeEnum.DIVIDE:
                    return "/";

                case MathArithTypeEnum.MODULO:
                    return "%";

                case MathArithTypeEnum.MULTIPLY:
                    return "*";

                case MathArithTypeEnum.SUBTRACT:
                    return "-";
            }

            throw new ArgumentException("invalid value for MathArithTypeEnum", nameof(value));
        }

        /// <summary>
        ///     Returns the math operator for the string.
        /// </summary>
        /// <param name="operator">to parse</param>
        /// <returns>math enum</returns>
        public static MathArithTypeEnum ParseOperator(string @operator)
        {
            switch (@operator) {
                case "+":
                    return MathArithTypeEnum.ADD;

                case "-":
                    return MathArithTypeEnum.SUBTRACT;

                case "*":
                    return MathArithTypeEnum.MULTIPLY;

                case "/":
                    return MathArithTypeEnum.DIVIDE;

                case "%":
                    return MathArithTypeEnum.MODULO;
            }

            throw new ArgumentException($"Unknown operator '{@operator}'");
        }
    }
}