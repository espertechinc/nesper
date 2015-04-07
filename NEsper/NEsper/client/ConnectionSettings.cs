///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Data;

namespace com.espertech.esper.client
{
    /// <summary>
	/// Supplies connection-level settings for a given database name.
	/// </summary>
    
	[Serializable]
	public class ConnectionSettings
    {
        //private bool? readOnly;

        /// <summary> Returns a bool indicating auto-commit, or null if not set and default accepted.</summary>
        /// <returns> true for auto-commit on, false for auto-commit off, or null to accept the default
        /// </returns>
        /// <summary> Indicates whether to set any new connections for this database to auto-commit.</summary>
        public bool? AutoCommit { get; set; }

        /// <summary> Gets the name of the catalog to set on new database connections, or null for default.</summary>
        /// <returns> name of the catalog to set, or null to accept the default
        /// </returns>
        /// <summary> Sets the name of the catalog on new database connections.</summary>
        public string Catalog { get; set; }

        /// <summary> Returns the connection settings for transaction isolation level.</summary>
        /// <returns> transaction isolation level
        /// </returns>
        /// <summary>
        /// Sets the transaction isolation level for new database connections,
        /// can be null to accept the default.
        /// </summary>
        public IsolationLevel? TransactionIsolation { get; set; }
    }
}
