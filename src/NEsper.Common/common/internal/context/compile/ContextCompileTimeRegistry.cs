///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.context.compile
{
    public class ContextCompileTimeRegistry : CompileTimeRegistry
    {
        public IDictionary<string, ContextMetaData> Contexts { get; } = new Dictionary<string, ContextMetaData>();

        public void NewContext(ContextMetaData detail)
        {
            if (!detail.ContextVisibility.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for contexts");
            }

            string name = detail.ContextName;
            var existing = Contexts.Get(name);
            if (existing != null) {
                throw new IllegalStateException(
                    "A duplicate definition of contexts was detected for name '" + name + "'");
            }

            Contexts.Put(name, detail);
        }
    }
} // end of namespace