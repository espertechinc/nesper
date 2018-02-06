///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportVersion : IComparable
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Release { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVersion"/> class.
        /// </summary>
        public SupportVersion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVersion"/> class.
        /// </summary>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="release">The release.</param>
        public SupportVersion(int major, int minor, int release)
        {
            Major = major;
            Minor = minor;
            Release = release;
        }

        protected bool Equals(SupportVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Release == other.Release;
        }

        public int CompareTo(object obj)
        {
            var that = (SupportVersion) obj;
            if (this.Major != that.Major)
                return this.Major.CompareTo(that.Major);
            if (this.Minor != that.Minor)
                return this.Minor.CompareTo(that.Minor);

            return this.Release.CompareTo(that.Release);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SupportVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Major;
                hashCode = (hashCode*397) ^ Minor;
                hashCode = (hashCode*397) ^ Release;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Release);
        }
    }
}
