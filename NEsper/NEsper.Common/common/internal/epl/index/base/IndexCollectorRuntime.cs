///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.index.compile;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public class IndexCollectorRuntime : IndexCollector
    {
        private readonly ISet<ModuleIndexMeta> moduleIndexes;

        public IndexCollectorRuntime(ISet<ModuleIndexMeta> moduleIndexes)
        {
            this.moduleIndexes = moduleIndexes;
        }

        public void RegisterIndex(
            IndexCompileTimeKey indexKey,
            IndexDetail indexDetail)
        {
            if (indexKey.Visibility == NameAccessModifier.PUBLIC) {
                moduleIndexes.Add(
                    new ModuleIndexMeta(
                        indexKey.IsNamedWindow, indexKey.InfraName, indexKey.InfraModuleName, indexKey.IndexName,
                        indexKey.InfraModuleName));
            }
        }
    }
} // end of namespace