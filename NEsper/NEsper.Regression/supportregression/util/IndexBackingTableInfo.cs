///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.supportregression.epl;

namespace com.espertech.esper.supportregression.util
{
    public class IndexBackingTableInfo
    {
        public static String INDEX_CALLBACK_HOOK = string.Format("@Hook(Type={0}.INTERNAL_QUERY_PLAN,Hook='{1}')\n",
            typeof(HookType).FullName,
            SupportQueryPlanIndexHook.ResetGetClassName());

        public static readonly String BACKING_SINGLE_UNIQUE = typeof (PropertyIndexedEventTableSingleUnique).Name;
        public static readonly String BACKING_SINGLE_DUPS = typeof (PropertyIndexedEventTableSingle).Name;

        public static readonly String BACKING_MULTI_UNIQUE = typeof (PropertyIndexedEventTableUnique).Name;
        public static readonly String BACKING_MULTI_DUPS = typeof (PropertyIndexedEventTableUnadorned).Name;
        public static readonly String BACKING_SORTED_COERCED = typeof (PropertySortedEventTableCoerced).Name;
        public static readonly String BACKING_SORTED = typeof (PropertySortedEventTable).Name;
        public static readonly String BACKING_UNINDEXED = typeof (UnindexedEventTable).Name;
        public static readonly String BACKING_COMPOSITE = typeof (PropertyCompositeEventTable).Name;
    }
}