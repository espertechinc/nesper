///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Enum for match recognize pattern atom types.
    /// </summary>
    public enum MatchRecogizePatternElementType
    {
        // For single multiplicity.
        SINGLE,

        // For greedy '*' multiplicity.
        ZERO_TO_MANY,

        // For greedy '+' multiplicity.
        ONE_TO_MANY,

        // For greedy '?' multiplicity.
        ONE_OPTIONAL,

        // For reluctant '*' multiplicity.
        ZERO_TO_MANY_RELUCTANT,

        // For reluctant '+' multiplicity.
        ONE_TO_MANY_RELUCTANT,

        // For reluctant '?' multiplicity.
        ONE_OPTIONAL_RELUCTANT
    }

    public static class MatchRecogizePatternElementTypeExtensions
    {
        public static string GetText(this MatchRecogizePatternElementType value)
        {
            switch (value) {
                case MatchRecogizePatternElementType.SINGLE:
                    return "";

                case MatchRecogizePatternElementType.ZERO_TO_MANY:
                    return "*";

                case MatchRecogizePatternElementType.ONE_TO_MANY:
                    return "+";

                case MatchRecogizePatternElementType.ONE_OPTIONAL:
                    return "?";

                case MatchRecogizePatternElementType.ZERO_TO_MANY_RELUCTANT:
                    return "*?";

                case MatchRecogizePatternElementType.ONE_TO_MANY_RELUCTANT:
                    return "+?";

                case MatchRecogizePatternElementType.ONE_OPTIONAL_RELUCTANT:
                    return "??";
            }

            throw new ArgumentException("invalid value", nameof(value));
        }
    }
} // end of namespace