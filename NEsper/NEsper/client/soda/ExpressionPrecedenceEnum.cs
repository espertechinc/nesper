///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Precendence levels for expressions.
    /// </summary>
    public enum ExpressionPrecedenceEnum
    {
        /// <summary>Precedence. </summary>
        UNARY = (11),
        /// <summary>Precedence. </summary>
        MULTIPLY = (10),
        /// <summary>Precedence. </summary>
        ADDITIVE = (9),
        /// <summary>Precedence. </summary>
        CONCAT = (8),
        /// <summary>Precedence. </summary>
        RELATIONAL_BETWEEN_IN = (7),
        /// <summary>Precedence. </summary>
        EQUALS = (6),
        /// <summary>Precedence. </summary>
        NEGATED = (5),
        /// <summary>Precedence. </summary>
        BITWISE = (4),
        /// <summary>Precedence. </summary>
        AND = (3),
        /// <summary>Precedence. </summary>
        OR = (2),
        /// <summary>Precedence. </summary>
        CASE = (1),
    
        /// <summary>Precedence. </summary>
        MINIMUM = (int.MinValue)
    }
}
