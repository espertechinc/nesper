///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.rowregex
{
	/// <summary>
	/// Precendence levels for expressions.
	/// </summary>
	public enum RowRegexExprNodePrecedenceEnum
    {
	    /// <summary>
	    /// Precedence.
	    /// </summary>
	    UNARY = 4,
	    /// <summary>
	    /// Precedence.
	    /// </summary>
	    GROUPING = 3,
	    /// <summary>
	    /// Precedence.
	    /// </summary>
	    CONCATENATION = 2,
	    /// <summary>
	    /// Precedence.
	    /// </summary>
	    ALTERNATION = 1,

	    /// <summary>
	    /// Precedence.
	    /// </summary>
	    MINIMUM = int.MinValue
    };

    public static class RowRegexExprNodePrecedenceEnumExtensions
    {
	    /// <summary>
	    /// Level.
	    /// </summary>
	    /// <returns>level</returns>
	    public static int GetLevel(this RowRegexExprNodePrecedenceEnum value)
        {
            switch (value)
            {
                case RowRegexExprNodePrecedenceEnum.UNARY:
                    return 4;
                case RowRegexExprNodePrecedenceEnum.GROUPING:
                    return 3;
                case RowRegexExprNodePrecedenceEnum.CONCATENATION:
                    return 2;
                case RowRegexExprNodePrecedenceEnum.ALTERNATION:
                    return 1;
                case RowRegexExprNodePrecedenceEnum.MINIMUM:
                    return int.MinValue;
            }

            throw new ArgumentException();
	    }
	}
} // end of namespace
