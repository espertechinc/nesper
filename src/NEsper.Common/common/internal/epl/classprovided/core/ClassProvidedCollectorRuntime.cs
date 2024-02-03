///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvidedCollectorRuntime : ClassProvidedCollector
    {
        private readonly IDictionary<string, ClassProvided> _classProvidedList;

        public ClassProvidedCollectorRuntime(IDictionary<string, ClassProvided> classProvidedList)
        {
            _classProvidedList = classProvidedList;
        }

        public void RegisterClass(
            string className,
            ClassProvided meta)
        {
            if (_classProvidedList.ContainsKey(className)) {
                throw new IllegalStateException("Application-inlined class already found '" + className + "'");
            }

            _classProvidedList.Put(className, meta);
        }
    }
} // end of namespace