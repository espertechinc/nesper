///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Text.RegularExpressions;

namespace com.espertech.esper.type
{
    /// <summary>Regular expression matcher. </summary>
    [Serializable]
    public class StringPatternSetRegex : StringPatternSet
    {
        private readonly Regex _pattern;
        private readonly String _patternText;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="patternText">regex to match</param>
        public StringPatternSetRegex(String patternText)
        {
            _patternText = patternText;
            _pattern = new Regex(patternText);
        }

        #region StringPatternSet Members

        /// <summary>
        /// Match the string returning true for a match, using regular expression semantics.
        /// </summary>
        /// <param name="stringToMatch">string to match</param>
        /// <returns>true for match</returns>
        public bool Match(String stringToMatch)
        {
            return _pattern.IsMatch(stringToMatch);
        }

        #endregion

        /// <summary>
        /// Equalses the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public bool Equals(StringPatternSetRegex obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj._patternText, _patternText);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (StringPatternSetRegex)) return false;
            return Equals((StringPatternSetRegex) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return (_patternText != null ? _patternText.GetHashCode() : 0);
        }
    }
}
