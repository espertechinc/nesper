///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Indicates how the runtime retrieves metadata about a statement's output columns.
    /// </summary>
    public enum MetadataOriginEnum
    {
        /// <summary>
        ///     By default, get output column metadata from the prepared statement, unless
        ///     an Oracle connection class is used in which case the behavior is SAMPLE.
        /// </summary>
        DEFAULT,

        /// <summary>
        ///     Always get output column metadata from the prepared statement regardless of what driver
        ///     or connection is used.
        /// </summary>
        METADATA,

        /// <summary>
        ///     Obtain output column metadata by executing a sample query statement at statement
        ///     compilation time. The sample statement
        ///     returns the same columns as the statement executed during event processing.
        ///     See the documentation for the generation or specication of the sample query statement.
        /// </summary>
        SAMPLE
    }
}