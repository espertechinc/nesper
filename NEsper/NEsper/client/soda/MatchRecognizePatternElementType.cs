///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>Enum for match recognize pattern atom types. </summary>
    public enum MatchRecognizePatternElementType
    {
        /// <summary>For single multiplicity. </summary>
        SINGLE,

        /// <summary>For greedy '*' multiplicity. </summary>
        ZERO_TO_MANY,

        /// <summary>For greedy '+' multiplicity. </summary>
        ONE_TO_MANY,

        /// <summary>For greedy '?' multiplicity. </summary>
        ONE_OPTIONAL,

        /// <summary>For reluctant '*' multiplicity. </summary>
        ZERO_TO_MANY_RELUCTANT,

        /// <summary>For reluctant '+' multiplicity. </summary>
        ONE_TO_MANY_RELUCTANT,

        /// <summary>For reluctant '?' multiplicity. </summary>
        ONE_OPTIONAL_RELUCTANT
    }

    public static class MatchRecognizePatternElementTypeExtensions
    {
        /// <summary>Returns the multiplicity text. </summary>
        /// <returns>text</returns>
        public static string GetText(this MatchRecognizePatternElementType value)
        {
            switch(value)
            {
                case MatchRecognizePatternElementType.SINGLE:
                    return ("");
                case MatchRecognizePatternElementType.ZERO_TO_MANY:
                    return ("*");
                case MatchRecognizePatternElementType.ONE_TO_MANY:
                    return ("+");
                case MatchRecognizePatternElementType.ONE_OPTIONAL:
                    return ("?");
                case MatchRecognizePatternElementType.ZERO_TO_MANY_RELUCTANT:
                    return ("*?");
                case MatchRecognizePatternElementType.ONE_TO_MANY_RELUCTANT:
                    return ("+?");
                case MatchRecognizePatternElementType.ONE_OPTIONAL_RELUCTANT:
                    return ("??");
            }

            throw new ArgumentException("invalid value", "value");
        }
    }
}