///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.lookup
{
    public class SubordPropRangeKey
    {
        public SubordPropRangeKey(QueryGraphValueEntryRange rangeInfo, Type coercionType)
        {
            RangeInfo = rangeInfo;
            CoercionType = coercionType;
        }

        public Type CoercionType { get; private set; }

        public QueryGraphValueEntryRange RangeInfo { get; private set; }

        public String ToQueryPlan()
        {
            return string.Format(" info {0} coercion {1}", RangeInfo.ToQueryPlan(), CoercionType);
        }

        public static String ToQueryPlan(ICollection<SubordPropRangeKey> rangeDescs)
        {
            var writer = new StringWriter();
            var delimiter = "";
            foreach (var key in rangeDescs)
            {
                writer.Write(delimiter);
                writer.Write(key.ToQueryPlan());
                delimiter = ", ";
            }
            return writer.ToString();
        }
    }
}
