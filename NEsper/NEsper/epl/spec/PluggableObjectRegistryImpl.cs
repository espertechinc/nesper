///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.spec
{
    public class PluggableObjectRegistryImpl : PluggableObjectRegistry
    {
        private readonly PluggableObjectCollection[] _collections;

        public PluggableObjectRegistryImpl(PluggableObjectCollection[] collections)
        {
            _collections = collections;
        }

        public Pair<Type, PluggableObjectEntry> Lookup(String nameSpace, String name)
        {
            for (int ii = 0; ii < _collections.Length; ii++)
            {
                var names = _collections[ii].Pluggables.Get(nameSpace);
                if (names == null)
                {
                    continue;
                }

                var entry = names.Get(name);
                if (entry == null)
                {
                    continue;
                }

                return entry;
            }

            return null;
        }
    }
}
