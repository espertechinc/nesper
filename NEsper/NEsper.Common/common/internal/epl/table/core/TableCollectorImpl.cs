///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableCollectorImpl : TableCollector
    {
        private readonly IDictionary<string, TableMetaData> _moduleTables;

        public TableCollectorImpl(IDictionary<string, TableMetaData> moduleTables)
        {
            _moduleTables = moduleTables;
        }

        public void RegisterTable(
            string tableName,
            TableMetaData table)
        {
            _moduleTables.Put(tableName, table);
        }
    }
} // end of namespace