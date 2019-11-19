///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableCompileTimeResolverEmpty : TableCompileTimeResolver
    {
        public readonly static TableCompileTimeResolverEmpty INSTANCE = new TableCompileTimeResolverEmpty();

        private TableCompileTimeResolverEmpty()
        {
        }

        public TableMetaData Resolve(string tableName)
        {
            return null;
        }

        public TableMetaData ResolveTableFromEventType(EventType containedType)
        {
            return null;
        }
    }
} // end of namespace