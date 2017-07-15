///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    public class JoinSetComposerUtil {
        private static readonly EventTable[] EMPTY = new EventTable[0];
    
        public static EventTable[][] ToArray(IDictionary<TableLookupIndexReqKey, EventTable>[] repositories) {
            return ToArray(repositories, repositories.Length);
        }
    
        public static EventTable[][] ToArray(IDictionary<TableLookupIndexReqKey, EventTable>[] repositories, int length) {
            if (repositories == null) {
                return GetDefaultTablesArray(length);
            }
            var tables = new EventTable[repositories.Length][];
            for (int i = 0; i < repositories.Length; i++) {
                tables[i] = ToArray(repositories[i]);
            }
            return tables;
        }
    
        private static EventTable[] ToArray(IDictionary<TableLookupIndexReqKey, EventTable> repository) {
            if (repository == null) {
                return EMPTY;
            }
            var tables = new EventTable[repository.Count];
            int count = 0;
            foreach (var entries in repository) {
                tables[count] = entries.Value;
                count++;
            }
            return tables;
        }
    
        private static EventTable[][] GetDefaultTablesArray(int length) {
            var result = new EventTable[length][];
            for (int i = 0; i < result.Length; i++) {
                result[i] = EMPTY;
            }
            return result;
        }
    }
} // end of namespace
