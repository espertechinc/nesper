///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Data.Common;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Minor abstraction on top of the IDbCommand.  The DbDriverCommand
    /// provides callers (above the driver command) to obtain information
    /// about the command using a notation that is similar to JDBC (i.e. it
    /// uses ? for parameters); below it ensures that the underlying
    /// connection uses proper ADO.NET conventions for the driver.  It also
    /// handles certain other nuances that differ between ADO.NET driver
    /// implementations, encapsulating that behavior within the driver.
    /// </summary>

    public interface DbDriverCommand : IDisposable
    {
        /// <summary>
        /// Clones the driver command.
        /// </summary>
        /// <returns></returns>
        DbDriverCommand Clone();

        /// <summary>
        /// Gets the driver associated with this command.
        /// </summary>
        DbDriver Driver { get; }

        /// <summary>
        /// Gets the actual database command.
        /// </summary>
        /// <value>The command.</value>
        DbCommand Command { get; }

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        /// <returns>The meta data.</returns>
        QueryMetaData GetMetaData();

        /// <summary>
        /// Gets the meta data settings associated with this command.
        /// </summary>
        ColumnSettings MetaDataSettings { get; }

        /// <summary>
        /// Gets a list of parameters.
        /// </summary>
        /// <value>The parameters.</value>
        SQLParameterDesc ParameterDescription { get; }

        /// <summary>
        /// Gets the fragments.  If the command was not created through
        /// supplied fragments, this method will throw an exception.
        /// </summary>
        /// <value>The fragments.</value>
        IEnumerable<PlaceholderParser.Fragment> Fragments { get;}

        /// <summary>
        /// Gets the actual SQL that is sent to the driver.
        /// </summary>
        String CommandText { get; }

        /// <summary>
        /// Gets the pseudo SQL that is provided to and from the client.
        /// </summary>
        String PseudoText { get; }
    }
}
