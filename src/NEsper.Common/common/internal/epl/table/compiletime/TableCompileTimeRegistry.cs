///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableCompileTimeRegistry : CompileTimeRegistry
    {
        public TableCompileTimeRegistry(IDictionary<string, TableMetaData> tables)
        {
            Tables = tables;
        }

        public IDictionary<string, TableMetaData> Tables { get; }

        public void NewTable(TableMetaData metaData)
        {
            if (!metaData.TableVisibility.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for tables");
            }

            var tableName = metaData.TableName;
            var existing = Tables.Get(tableName);
            if (existing != null) {
                throw new IllegalStateException("Duplicate table encountered for name '" + tableName + "'");
            }

            Tables.Put(tableName, metaData);
        }

        public TableMetaData GetTable(string tableName)
        {
            return Tables.Get(tableName);
        }
    }
} // end of namespace