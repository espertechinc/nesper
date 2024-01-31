///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.index.unindexed;

namespace com.espertech.esper.regressionlib.support.util
{
    public interface IndexBackingTableInfo
    {
        public static readonly string INDEX_CALLBACK_HOOK =
            "@Hook(HookType=" +
            typeof(HookType).FullName +
            ".INTERNAL_QUERY_PLAN,Hook='" +
            SupportQueryPlanIndexHook.ResetGetClassName() +
            "')";

        public static readonly string BACKING_UNINDEXED = nameof(UnindexedEventTable);

        public static readonly string BACKING_SINGLE_UNIQUE = nameof(PropertyHashedEventTableUnique);
        public static readonly string BACKING_SINGLE_DUPS = nameof(PropertyHashedEventTable);
        public static readonly string BACKING_MULTI_UNIQUE = nameof(PropertyHashedEventTableUnique);
        public static readonly string BACKING_MULTI_DUPS = nameof(PropertyHashedEventTable);
        public static readonly string BACKING_SORTED = nameof(PropertySortedEventTable);
        public static readonly string BACKING_COMPOSITE = nameof(PropertyCompositeEventTable);
    }
} // end of namespace