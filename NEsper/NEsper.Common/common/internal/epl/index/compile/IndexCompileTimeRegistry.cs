///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
    public class IndexCompileTimeRegistry
    {
        public IndexCompileTimeRegistry(IDictionary<IndexCompileTimeKey, IndexDetailForge> indexes)
        {
            Indexes = indexes;
        }

        public IDictionary<IndexCompileTimeKey, IndexDetailForge> Indexes { get; }

        public void NewIndex(
            IndexCompileTimeKey key,
            IndexDetailForge detail)
        {
            var existing = Indexes.Get(key);
            if (existing != null) {
                throw new IllegalStateException("A duplicate index has been encountered for key '" + key + "'");
            }

            Indexes.Put(key, detail);
        }
    }
} // end of namespace