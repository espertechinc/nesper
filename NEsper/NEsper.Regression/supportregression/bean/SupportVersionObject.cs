///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.bean
{
    public class SupportVersionObject
    {
        /// <summary>
        /// Returns version A.
        /// </summary>
        public SupportVersion VersionA { get; set; }

        /// <summary>
        /// Returns version B.
        /// </summary>
        public SupportVersion VersionB { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVersionObject"/> class.
        /// </summary>
        public SupportVersionObject() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportVersionObject"/> class.
        /// </summary>
        /// <param name="versionA">The version a.</param>
        /// <param name="versionB">The version b.</param>
        public SupportVersionObject(SupportVersion versionA, SupportVersion versionB)
        {
            VersionA = versionA;
            VersionB = versionB;
        }
    }
}
