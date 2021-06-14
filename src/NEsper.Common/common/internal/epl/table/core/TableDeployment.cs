///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableDeployment
    {
        private readonly IDictionary<string, Table> tables = new Dictionary<string, Table>(4);

        public void Add(
            string tableName,
            TableMetaData metadata,
            EPStatementInitServices services)
        {
            Table existing = tables.Get(tableName);
            if (existing != null) {
                throw new IllegalStateException("Table already found for name '" + tableName + "'");
            }

            Table table = services.TableManagementService.AllocateTable(metadata);
            tables.Put(tableName, table);
        }

        public Table GetTable(string tableName)
        {
            return tables.Get(tableName);
        }

        public void Remove(string tableName)
        {
            tables.Remove(tableName);
        }

        public bool IsEmpty()
        {
            return tables.IsEmpty();
        }
    }
} // end of namespace