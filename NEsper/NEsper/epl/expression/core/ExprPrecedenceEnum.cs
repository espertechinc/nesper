///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>Precendence levels for expressions. </summary>
    public enum ExprPrecedenceEnum
    {
        /// <summary>Precedence. </summary>
        UNARY,
        /// <summary>Precedence. </summary>
        MULTIPLY,
        /// <summary>Precedence. </summary>
        ADDITIVE,
        /// <summary>Precedence. </summary>
        CONCAT,
        /// <summary>Precedence. </summary>
        RELATIONAL_BETWEEN_IN,
        /// <summary>Precedence. </summary>
        EQUALS,
        /// <summary>Precedence. </summary>
        NEGATED,
        /// <summary>Precedence. </summary>
        BITWISE,
        /// <summary>Precedence. </summary>
        AND,
        /// <summary>Precedence. </summary>
        OR,
        /// <summary>Precedence. </summary>
        CASE,
        /// <summary>Precedence. </summary>
        MINIMUM
    }

    public static class ExprPrecedenceEnumExtensions
    {
        public static int GetLevel(this ExprPrecedenceEnum value)
        {
            switch (value)
            {
                case ExprPrecedenceEnum.UNARY:
                    return (11);
                case ExprPrecedenceEnum.MULTIPLY:
                    return (10);
                case ExprPrecedenceEnum.ADDITIVE:
                    return (9);
                case ExprPrecedenceEnum.CONCAT:
                    return (8);
                case ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN:
                    return (7);
                case ExprPrecedenceEnum.EQUALS:
                    return (6);
                case ExprPrecedenceEnum.NEGATED:
                    return (5);
                case ExprPrecedenceEnum.BITWISE:
                    return (4);
                case ExprPrecedenceEnum.AND:
                    return (3);
                case ExprPrecedenceEnum.OR:
                    return (2);
                case ExprPrecedenceEnum.CASE:
                    return (1);
                case ExprPrecedenceEnum.MINIMUM:
                    return (Int32.MinValue);
            }

            throw new ArgumentException();
        }
    }
}
