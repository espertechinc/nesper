///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
	public class TableCompileTimeRegistry : CompileTimeRegistry {
	    private readonly IDictionary<string, TableMetaData> tables;

	    public TableCompileTimeRegistry(IDictionary<string, TableMetaData> tables) {
	        this.tables = tables;
	    }

	    public void NewTable(TableMetaData metaData) {
	        if (!metaData.TableVisibility.IsModuleProvidedAccessModifier) {
	            throw new IllegalStateException("Invalid visibility for tables");
	        }
	        string tableName = metaData.TableName;
	        TableMetaData existing = tables.Get(tableName);
	        if (existing != null) {
	            throw new IllegalStateException("Duplicate table encountered for name '" + tableName + "'");
	        }
	        tables.Put(tableName, metaData);
	    }

	    public IDictionary<string, TableMetaData> GetTables() {
	        return tables;
	    }

	    public TableMetaData GetTable(string tableName) {
	        return tables.Get(tableName);
	    }
	}
} // end of namespace