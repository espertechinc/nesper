///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
	public class EventTableIndexMetadataUtil {
	    public static string[][] GetUniqueness(EventTableIndexMetadata indexMetadata, string[] optionalViewUniqueness) {
	        IList<string[]> unique = null;

	        ISet<IndexMultiKey> indexDescriptors = indexMetadata.Indexes.Keys;
	        foreach (IndexMultiKey index in indexDescriptors) {
	            if (!index.IsUnique) {
	                continue;
	            }
	            string[] uniqueKeys = IndexedPropDesc.GetIndexProperties(index.HashIndexedProps);
	            if (unique == null) {
	                unique = new List<>();
	            }
	            unique.Add(uniqueKeys);
	        }
	        if (optionalViewUniqueness != null) {
	            if (unique == null) {
	                unique = new List<>();
	            }
	            unique.Add(optionalViewUniqueness);
	        }
	        if (unique == null) {
	            return null;
	        }
	        return unique.ToArray();
	    }
	}
} // end of namespace