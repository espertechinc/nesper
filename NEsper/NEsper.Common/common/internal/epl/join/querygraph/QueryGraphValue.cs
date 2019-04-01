///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    /// <summary>
    ///     Property lists stored as a value for each stream-to-stream relationship, for use by
    ///     <seealso cref="QueryGraphForge" />.
    /// </summary>
    public class QueryGraphValue
    {
        public QueryGraphValue(IList<QueryGraphValueDesc> items)
        {
            Items = items;
        }

        public IList<QueryGraphValueDesc> Items { get; }

        public QueryGraphValuePairHashKeyIndex HashKeyProps {
            get {
                IList<QueryGraphValueEntryHashKeyed> keys = new List<QueryGraphValueEntryHashKeyed>();
                Deque<string> indexed = new ArrayDeque<string>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryHashKeyed) {
                        var keyprop = (QueryGraphValueEntryHashKeyed) desc.Entry;
                        keys.Add(keyprop);
                        indexed.Add(desc.IndexExprs[0]);
                    }
                }

                return new QueryGraphValuePairHashKeyIndex(indexed.ToArray(), keys);
            }
        }

        public QueryGraphValuePairRangeIndex RangeProps {
            get {
                Deque<string> indexed = new ArrayDeque<string>();
                IList<QueryGraphValueEntryRange> keys = new List<QueryGraphValueEntryRange>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryRange) {
                        var keyprop = (QueryGraphValueEntryRange) desc.Entry;
                        keys.Add(keyprop);
                        indexed.Add(desc.IndexExprs[0]);
                    }
                }

                return new QueryGraphValuePairRangeIndex(indexed.ToArray(), keys);
            }
        }

        public QueryGraphValuePairInKWSingleIdx InKeywordSingles {
            get {
                IList<string> indexedProps = new List<string>();
                IList<QueryGraphValueEntryInKeywordSingleIdx> single =
                    new List<QueryGraphValueEntryInKeywordSingleIdx>();
                foreach (var desc in Items) {
                    if (desc.Entry is QueryGraphValueEntryInKeywordSingleIdx) {
                        var keyprop = (QueryGraphValueEntryInKeywordSingleIdx) desc.Entry;
                        single.Add(keyprop);
                        indexedProps.Add(desc.IndexExprs[0]);
                    }
                }

                return new QueryGraphValuePairInKWSingleIdx(indexedProps.ToArray(), single);
            }
        }
    }
} // end of namespace