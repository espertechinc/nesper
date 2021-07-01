///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.RegularExpressions;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Regular expression matcher.
    /// </summary>
    [Serializable]
    public class StringPatternSetRegex : StringPatternSet
    {
        private readonly string _patternText;
        private readonly Regex _pattern;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="patternText">regex to match</param>
        public StringPatternSetRegex(string patternText)
        {
            this._patternText = patternText;
            this._pattern = new Regex(patternText);
        }

        /// <summary>
        /// Match the string returning true for a match, using regular expression semantics.
        /// </summary>
        /// <param name="stringToMatch">string to match</param>
        /// <returns>true for match</returns>
        public bool Match(string stringToMatch)
        {
            return _pattern.IsMatch(stringToMatch);
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            StringPatternSetRegex that = (StringPatternSetRegex) o;

            if (!_patternText.Equals(that._patternText)) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _patternText.GetHashCode();
        }
    }
} // end of namespace