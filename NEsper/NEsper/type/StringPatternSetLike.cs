///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.type
{
    [Serializable]
    public class StringPatternSetLike : StringPatternSet
    {
        private readonly String likeString;
        private readonly LikeUtil likeUtil;
    
        /// <summary>Ctor. </summary>
        /// <param name="likeString">pattern to match</param>
        public StringPatternSetLike(String likeString)
        {
            this.likeString = likeString;
            likeUtil = new LikeUtil(likeString, '\\', false);
        }
    
        /// <summary>Match the string returning true for a match, using SQL-like semantics. </summary>
        /// <param name="stringToMatch">string to match</param>
        /// <returns>true for match</returns>
        public bool Match(String stringToMatch)
        {
            if (stringToMatch == null)
            {
                return false;
            }
            return likeUtil.Compare(stringToMatch);
        }

        /// <summary>
        /// Equalses the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public bool Equals(StringPatternSetLike obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.likeString, likeString);
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
            if (obj.GetType() != typeof (StringPatternSetLike)) return false;
            return Equals((StringPatternSetLike) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return (likeString != null ? likeString.GetHashCode() : 0);
        }
    }
}
