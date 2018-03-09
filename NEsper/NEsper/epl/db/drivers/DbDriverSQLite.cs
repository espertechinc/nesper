///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data.Common;

namespace com.espertech.esper.epl.db.drivers
{
    /// <summary>
    /// A database driver specific to the SQLite driver.
    /// </summary>
    [Serializable]
    public class DbDriverSQLite : DbDriverGeneric
    {
        private const string DRIVER_CLASS = "Microsoft.Data.Sqlite.Core.SqliteFactory";

        /// <summary>
        /// Initializes the <see cref="DbDriverMySQL"/> class.
        /// </summary>
        public DbDriverSQLite(DbProviderFactoryManager dbProviderFactoryManager)
            : base(dbProviderFactoryManager, DRIVER_CLASS)
        {
        }
    }
}
