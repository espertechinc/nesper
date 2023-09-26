///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedCollectorCompileTime : ClassProvidedCollector
    {
        private readonly IDictionary<string, ClassProvided> moduleClassProvideds;
        private readonly TypeResolver parentTypeResolver;

        public ClassProvidedCollectorCompileTime(
            IDictionary<string, ClassProvided> moduleClassProvideds,
            TypeResolver parentTypeResolver)
        {
            this.moduleClassProvideds = moduleClassProvideds;
            this.parentTypeResolver = parentTypeResolver;
        }

        public void RegisterClass(
            string className,
            ClassProvided meta)
        {
            moduleClassProvideds.Put(className, meta);
            meta.LoadClasses(parentTypeResolver);
        }
    }
} // end of namespace