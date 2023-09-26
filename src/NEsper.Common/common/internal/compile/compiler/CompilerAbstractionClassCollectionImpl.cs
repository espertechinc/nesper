///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public class CompilerAbstractionClassCollectionImpl : CompilerAbstractionClassCollection
    {
        private readonly IDictionary<string, byte[]> classes = new ConcurrentDictionary<string, byte[]>();

        public IDictionary<string, byte[]> Classes => classes;

        public void Add(IDictionary<string, byte[]> bytes)
        {
            classes.PutAll(bytes);
        }

        public void Remove(string name)
        {
            classes.Remove(name);
        }
    }
} // end of namespace