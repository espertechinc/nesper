///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;
using com.espertech.esper.common.@internal.epl.agg.rollup;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class EPTypeCollectionConst
    {
        public static readonly Type EPTYPE_MAP_OBJECT_EVENTBEANARRAY = typeof(IDictionary<object, EventBean[]>);
        public static readonly Type EPTYPE_MAP_OBJECT_AGGROW = typeof(IDictionary<object, AggregationRow>);

        public static readonly Type EPTYPE_MAPARRAY_OBJECT_AGGROW = typeof(IDictionary<object, AggregationRow>[]);

        public static readonly Type EPTYPE_SET_MULTIKEYARRAYOFKEYS_EVENTBEAN =
            typeof(ISet<MultiKeyArrayOfKeys<EventBean>>);

        public static readonly Type EPTYPE_COLLECTION_EVENTBEAN = typeof(ICollection<EventBean>);

        public static readonly Type EPTYPE_LIST_UNIFORMPAIR_EVENTBEANARRAY = typeof(IList<UniformPair<EventBean[]>>);

        public static readonly Type EPTYPE_LIST_GROUPBYROLLUPKEY = typeof(IList<GroupByRollupKey>);

        public static readonly Type EPTYPE_LIST_UNIFORMPAIR_SET_MKARRAY_EVENTBEAN =
            typeof(IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>>);

        public static readonly Type EPTYPE_LIST_AFFLOCALGROUPPAIR = typeof(IList<AggSvcLocalGroupLevelKeyPair>);
    }
} // end of namespace