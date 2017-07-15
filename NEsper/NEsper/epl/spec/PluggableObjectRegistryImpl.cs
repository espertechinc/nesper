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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.spec
{
    public class PluggableObjectRegistryImpl : PluggableObjectRegistry {
        private PluggableObjectCollection[] collections;
    
        public PluggableObjectRegistryImpl(PluggableObjectCollection[] collections) {
            this.collections = collections;
        }
    
        public Pair<Type, PluggableObjectEntry> Lookup(string nameSpace, string name) {
    
            // Handle namespace-provided
            if (nameSpace != null) {
                for (int i = 0; i < collections.Length; i++) {
                    IDictionary<string, Pair<Type, PluggableObjectEntry>> names = collections[i].Pluggables.Get(nameSpace);
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
            var found = null;
            for (int i = 0; i < collections.Length; i++) {
                for (var collEntry : collections[i].Pluggables) {
                    foreach (var viewEntry in collEntry.Value) {
                        if (viewEntry.Key.Equals(name)) {
                            if (found != null) {
                                if (entriesDuplicate == null) {
                                    entriesDuplicate = new HashSet<>();
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
                entriesDuplicate.Add(found.Key);
                throw new ViewProcessingException("Duplicate entries for view '" + name + "' found in namespaces " + Arrays.ToString(entriesDuplicate.ToArray()));
            }
    
            return found == null ? null : found.Value;
        }
    }
} // end of namespace
