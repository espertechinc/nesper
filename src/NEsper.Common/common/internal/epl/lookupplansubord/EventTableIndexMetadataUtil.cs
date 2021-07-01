///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.join.lookup;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class EventTableIndexMetadataUtil
    {
        public static string[][] GetUniqueness(
            EventTableIndexMetadata indexMetadata,
            string[] optionalViewUniqueness)
        {
            IList<string[]> unique = null;

            foreach (var index in indexMetadata.Indexes.Keys) {
                if (!index.IsUnique) {
                    continue;
                }

                var uniqueKeys = IndexedPropDesc.GetIndexProperties(index.HashIndexedProps);
                if (unique == null) {
                    unique = new List<string[]>();
                }

                unique.Add(uniqueKeys);
            }

            if (optionalViewUniqueness != null) {
                if (unique == null) {
                    unique = new List<string[]>();
                }

                unique.Add(optionalViewUniqueness);
            }

            return unique?.ToArray();
        }
    }
} // end of namespace