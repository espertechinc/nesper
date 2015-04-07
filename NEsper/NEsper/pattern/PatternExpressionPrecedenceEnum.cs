///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    public enum PatternExpressionPrecedenceEnum
    {
        /// <summary>Precedence. </summary>
        MINIMUM,

        /// <summary>Precedence. </summary>
        FOLLOWEDBY,

        /// <summary>Precedence. </summary>
        OR,

        /// <summary>Precedence. </summary>
        AND,

        /// <summary>Precedence. </summary>
        REPEAT_UNTIL,

        /// <summary>Precedence. </summary>
        UNARY,

        /// <summary>Precedence. </summary>
        GUARD_POSTFIX,

        /// <summary>Precedence. </summary>
        ATOM
    }

    public static class PatternExpressionPrecedenceEnumExtensions
    {
        public static int GetLevel(this PatternExpressionPrecedenceEnum value)
        {
            switch (value)
            {
                case PatternExpressionPrecedenceEnum.MINIMUM:
                    return int.MinValue;
                case PatternExpressionPrecedenceEnum.FOLLOWEDBY:
                    return 1;
                case PatternExpressionPrecedenceEnum.OR:
                    return 2;
                case PatternExpressionPrecedenceEnum.AND:
                    return 3;
                case PatternExpressionPrecedenceEnum.REPEAT_UNTIL:
                    return 4;
                case PatternExpressionPrecedenceEnum.UNARY:
                    return 5;
                case PatternExpressionPrecedenceEnum.GUARD_POSTFIX:
                    return 6;
                case PatternExpressionPrecedenceEnum.ATOM:
                    return int.MaxValue;
            }

            throw new ArgumentException("invalid value");
        }
    }
}
