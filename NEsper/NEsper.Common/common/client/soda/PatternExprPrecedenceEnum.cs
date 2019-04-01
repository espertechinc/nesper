///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Pattern precendences. </summary>
    public enum PatternExprPrecedenceEnum {
        /// <summary>Precedence. </summary>
        MAXIMIM = (int.MaxValue),
    
        /// <summary>Precedence. </summary>
        ATOM = (7),
        /// <summary>Precedence. </summary>
        GUARD = (6),
        /// <summary>Precedence. </summary>
        EVERY_NOT = (5),
        /// <summary>Precedence. </summary>
        MATCH_UNTIL = (4),
        /// <summary>Precedence. </summary>
        AND = (3),
        /// <summary>Precedence. </summary>
        OR = (2),
        /// <summary>Precedence. </summary>
        FOLLOWED_BY = (1),
    
        /// <summary>Precedence. </summary>
        MINIMUM = (int.MinValue)
    }

    public static class PatternExprPrecedenceEnumExtensions {
        /// <summary>Returns Precedence. </summary>
        /// <returns>Precedence</returns>
        public static int GetLevel(this PatternExprPrecedenceEnum value) {
            return (int) value;
        }
    }
}
