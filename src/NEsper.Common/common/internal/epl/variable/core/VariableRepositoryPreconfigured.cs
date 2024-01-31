///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableRepositoryPreconfigured
    {
        public IDictionary<string, VariableMetaData> Metadata { get; } = new HashMap<string, VariableMetaData>();

        public void AddVariable(
            string name,
            VariableMetaData meta)
        {
            Metadata.Put(name, meta);
        }

        public VariableMetaData GetMetadata(string name)
        {
            return Metadata.Get(name);
        }

        public void MergeFrom(VariableRepositoryPreconfigured other)
        {
            foreach (var entry in other.Metadata) {
                if (Metadata.ContainsKey(entry.Key)) {
                    continue;
                }

                Metadata.Put(entry.Key, entry.Value);
            }
        }
    }
} // end of namespace