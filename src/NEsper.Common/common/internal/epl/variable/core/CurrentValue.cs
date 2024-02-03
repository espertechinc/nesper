///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// A holder for versioned values that holds a current version-value and a prior version-value pair.
    /// </summary>
    public class CurrentValue<T>
    {
        private readonly VersionedValue<T> currentVersion;
        private readonly VersionedValue<T> priorVersion;

        /// <summary>Ctor.</summary>
        /// <param name="currentVersion">current version and value</param>
        /// <param name="priorVersion">prior version and value</param>
        public CurrentValue(
            VersionedValue<T> currentVersion,
            VersionedValue<T> priorVersion)
        {
            this.currentVersion = currentVersion;
            this.priorVersion = priorVersion;
        }

        /// <summary>Returns the current version.</summary>
        /// <returns>current version</returns>
        public VersionedValue<T> CurrentVersion => currentVersion;

        /// <summary>Returns the prior version.</summary>
        /// <returns>prior version</returns>
        public VersionedValue<T> PriorVersion => priorVersion;
    }
} // End of namespace