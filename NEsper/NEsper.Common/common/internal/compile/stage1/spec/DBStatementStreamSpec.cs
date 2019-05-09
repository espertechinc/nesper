///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification object for historical data poll via database SQL statement.
    /// </summary>
    [Serializable]
    public class DBStatementStreamSpec : StreamSpecBase,
        StreamSpecRaw,
        StreamSpecCompiled
    {
        /// <summary>Ctor. </summary>
        /// <param name="optionalStreamName">is a stream name optionally given to stream</param>
        /// <param name="viewSpecs">is a list of views onto the stream</param>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL with placeholder parameters</param>
        /// <param name="metadataSQL">is the sample SQL to retrieve statement metadata, if any was supplied</param>
        public DBStatementStreamSpec(
            String optionalStreamName,
            ViewSpec[] viewSpecs,
            String databaseName,
            String sqlWithSubsParams,
            String metadataSQL)
            : base(optionalStreamName, viewSpecs, StreamSpecOptions.DEFAULT)
        {
            DatabaseName = databaseName;
            SqlWithSubsParams = sqlWithSubsParams;
            MetadataSQL = metadataSQL;
        }

        /// <summary>Returns the database name. </summary>
        /// <value>name of database.</value>
        public string DatabaseName { get; private set; }

        /// <summary>Returns the SQL with substitution parameters. </summary>
        /// <value>SQL with parameters embedded as ${stream.param}</value>
        public string SqlWithSubsParams { get; private set; }

        /// <summary>Returns the optional sample metadata SQL </summary>
        /// <value>null if not supplied, or SQL to fire to retrieve metadata</value>
        public string MetadataSQL { get; private set; }

        public StreamSpecCompiled Compile(
            StatementContext statementContext,
            ICollection<string> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger,
            string optionalStreamName)
        {
            return this;
        }
    }
}