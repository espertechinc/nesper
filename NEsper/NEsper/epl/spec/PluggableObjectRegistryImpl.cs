///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.spec
{
    public class PluggableObjectRegistryImpl : PluggableObjectRegistry
    {
        private readonly PluggableObjectCollection[] _collections;
    
        public PluggableObjectRegistryImpl(PluggableObjectCollection[] collections)
        {
            _collections = collections;
        }
    
        public Pair<Type, PluggableObjectEntry> Lookup(string nameSpace, string name)
        {
            // Handle namespace-provided
            if (nameSpace != null) {
                for (int i = 0; i < _collections.Length; i++) {
                    IDictionary<string, Pair<Type, PluggableObjectEntry>> names = _collections[i].Pluggables.Get(nameSpace);
                    if (names == null) {
                        continue;
                    }
                    Pair<Type, PluggableObjectEntry> entry = names.Get(name);
                    if (entry == null) {
                        continue;
                    }
                    return entry;
                }
                return null;
            }
    
            // Handle namespace-not-provided
            ISet<string> entriesDuplicate = null;
            KeyValuePair<string, Pair<Type, PluggableObjectEntry>>? found = null;
            for (int i = 0; i < _collections.Length; i++) {
                foreach (var collEntry in _collections[i].Pluggables) {
                    foreach (var viewEntry in collEntry.Value) {
                        if (viewEntry.Key.Equals(name)) {
                            if (found != null) {
                                if (entriesDuplicate == null) {
                                    entriesDuplicate = new HashSet<string>();
                                }
                                entriesDuplicate.Add(viewEntry.Key);
                            } else {
                                found = viewEntry;
                            }
                        }
                    }
                }
            }
    
            if (entriesDuplicate != null) {
                entriesDuplicate.Add(found.Value.Key);
                throw new ViewProcessingException("Duplicate entries for view '" + name + "' found in namespaces " + entriesDuplicate.Render());
            }
    
            return found == null ? null : found.Value.Value;
        }
    }
} // end of namespace
