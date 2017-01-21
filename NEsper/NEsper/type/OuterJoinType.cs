///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.type
{
	/// <summary> Enum for the type of outer join.</summary>
    public enum OuterJoinType
    {
	    /// <summary> Left outer join.</summary>
	    LEFT,
        /// <summary> Right outer join.</summary>
        RIGHT,
        /// <summary> Full outer join.</summary>
        FULL,
        /// <summary> Inner join.</summary>
        INNER
    }

    public static class OuterJoinTypeExtensions
    {
        public static String GetText(this OuterJoinType joinType)
        {
            switch (joinType)
            {
                case OuterJoinType.LEFT:
                    return "left";
                case OuterJoinType.RIGHT:
                    return "right";
                case OuterJoinType.FULL:
                    return "full";
                case OuterJoinType.INNER:
                    return "inner";
                default:
                    throw new ArgumentException("Unknown joinType " + joinType);
            }
        }
    }
}
