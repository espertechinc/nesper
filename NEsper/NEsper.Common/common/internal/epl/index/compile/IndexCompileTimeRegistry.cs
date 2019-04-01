///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
	public class IndexCompileTimeRegistry {
	    private readonly IDictionary<IndexCompileTimeKey, IndexDetailForge> indexes;

	    public IndexCompileTimeRegistry(IDictionary<IndexCompileTimeKey, IndexDetailForge> indexes) {
	        this.indexes = indexes;
	    }

	    public void NewIndex(IndexCompileTimeKey key, IndexDetailForge detail) {
	        IndexDetailForge existing = indexes.Get(key);
	        if (existing != null) {
	            throw new IllegalStateException("A duplicate index has been encountered for key '" + key + "'");
	        }
	        indexes.Put(key, detail);
	    }

	    public IDictionary<IndexCompileTimeKey, IndexDetailForge> GetIndexes() {
	        return indexes;
	    }
	}
} // end of namespace