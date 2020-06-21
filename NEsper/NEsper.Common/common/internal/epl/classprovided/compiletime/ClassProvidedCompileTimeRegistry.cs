///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedCompileTimeRegistry : CompileTimeRegistry
    {
        public IDictionary<string, ClassProvided> Classes { get; } = new Dictionary<string, ClassProvided>();

        public void NewClass(ClassProvided detail)
        {
            if (!detail.Visibility.IsModuleProvidedAccessModifier) {
                throw new IllegalStateException("Invalid visibility for contexts");
            }

            var key = detail.ClassName;
            var existing = Classes.Get(key);
            if (existing != null) {
                throw new IllegalStateException("Duplicate class-provided-by-application has been encountered for name '" + key + "'");
            }

            Classes[key] = detail;
        }

        public void AddTo(IDictionary<string, byte[]> additionalClasses)
        {
            foreach (var entry in Classes) {
                additionalClasses.PutAll(entry.Value.Bytes);
            }
        }
    }
} // end of namespace