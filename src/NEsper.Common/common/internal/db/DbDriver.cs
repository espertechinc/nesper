///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.Common;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.db
{
    /// <summary>
    ///     Database driver semantics are captured in the DbDriver.  Each
    ///     driver instance is completely separate from other instances.
    ///     Drivers encapsulate management of the connection, so specific
    ///     properties are given to it so that it can build its connection
    ///     string.
    /// </summary>
    public interface DbDriver
    {
        /// <summary>
        ///     Gets the default meta origin policy.
        /// </summary>
        /// <value>The default meta origin policy.</value>
        MetadataOriginEnum DefaultMetaOriginPolicy { get; }

        /// <summary>
        ///     Gets or sets the properties for the driver.
        /// </summary>
        /// <value>The properties.</value>
        Properties Properties { get; set; }

        /// <summary>
        ///     Gets the connection string associated with this driver.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        ///     Creates a database driver command from a collection of fragments.
        /// </summary>
        /// <param name="sqlFragments">The SQL fragments.</param>
        /// <param name="metadataSettings">The metadata settings.</param>
        /// <param name="contextAttributes">The context attributes.</param>
        /// <returns></returns>
        DbDriverCommand CreateCommand(
            IEnumerable<PlaceholderParser.Fragment> sqlFragments,
            ColumnSettings metadataSettings,
            IEnumerable<Attribute> contextAttributes);

        /// <summary>
        ///     Creates a database connection; this should be used sparingly if possible.
        /// </summary>
        /// <returns></returns>
        DbConnection CreateConnection();
    }
}