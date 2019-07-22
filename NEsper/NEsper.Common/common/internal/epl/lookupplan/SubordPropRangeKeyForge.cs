///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.epl.@join.querygraph;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    [Serializable]
    public class SubordPropRangeKeyForge
    {
        public SubordPropRangeKeyForge(
            QueryGraphValueEntryRangeForge rangeInfo,
            Type coercionType)
        {
            RangeInfo = rangeInfo;
            CoercionType = coercionType;
        }

        public Type CoercionType { get; }

        public QueryGraphValueEntryRangeForge RangeInfo { get; }

        public string ToQueryPlan()
        {
            return " info " + RangeInfo.ToQueryPlan() + " coercion " + CoercionType;
        }

        public static string ToQueryPlan(ICollection<SubordPropRangeKeyForge> rangeDescs)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var key in rangeDescs) {
                writer.Write(delimiter);
                writer.Write(key.ToQueryPlan());
                delimiter = ", ";
            }

            return writer.ToString();
        }
    }
} // end of namespace