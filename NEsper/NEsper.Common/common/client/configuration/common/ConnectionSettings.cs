///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Supplies connection-level settings for a given database name.
    /// </summary>
    [Serializable]
    public class ConnectionSettings
    {
        /// <summary>
        ///     Returns a boolean indicating auto-commit, or null if not set and default accepted.
        /// </summary>
        /// <returns>true for auto-commit on, false for auto-commit off, or null to accept the default</returns>
        public bool AutoCommit { get; set; }

        /// <summary>
        ///     Gets the name of the catalog to set on new database connections, or null for default.
        /// </summary>
        /// <returns>name of the catalog to set, or null to accept the default</returns>
        public string Catalog { get; set; }

        /// <summary>
        ///     Returns a boolean indicating read-only, or null if not set and default accepted.
        /// </summary>
        /// <returns>true for read-only on, false for read-only off, or null to accept the default</returns>
        public bool ReadOnly { get; set; }

        /// <summary>
        ///     Returns the connection settings for transaction isolation level.
        /// </summary>
        /// <value>transaction isolation level</value>
        public IsolationLevel? TransactionIsolation { get; set; }
    }
}