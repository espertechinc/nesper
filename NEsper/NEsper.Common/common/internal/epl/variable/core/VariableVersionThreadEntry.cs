///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// Thread-specific state in regards to variable versions.
    /// </summary>
    public class VariableVersionThreadEntry
    {
        /// <summary>Ctor.</summary>
        /// <param name="version">
        /// current version number of the variables visible to thread
        /// </param>
        /// <param name="uncommitted">
        /// the uncommitted values of variables for the thread, if any
        /// </param>
        public VariableVersionThreadEntry(
            int version,
            IDictionary<int, Pair<int, object>> uncommitted)
        {
            Version = version;
            Uncommitted = uncommitted;
        }

        /// <summary>Gets or sets the version visible for a thread.</summary>
        /// <returns>version number</returns>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a map of variable number and uncommitted value, or empty
        /// map or null if none exist
        /// </summary>
        /// <returns>uncommitted values</returns>
        public IDictionary<int, Pair<int, object>> Uncommitted { get; set; }
    }
} // End of namespace