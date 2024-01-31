///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text.RegularExpressions;

namespace com.espertech.esper.compat
{
    public static class RegexExtensions
    {
        public static Regex Compile(string text, out string patternText)
        {
            patternText = text;

            // test compiling the expression without 'bracketing' - once we bracket
            // it allows some things to work that would not work by themselves.
            // - throws ArgumentException
            new Regex(patternText, RegexOptions.None);

            if (!patternText.StartsWith("^")) {
                patternText = "^" + patternText;
            }

            if (!patternText.EndsWith("$")) {
                patternText += "$";
            }

            return new Regex(patternText, RegexOptions.None);
        }

        public static bool IsMatchDebug(
            this Regex regex,
            string textToMatch)
        {
            return regex.IsMatch(textToMatch);
        }
    }
}
