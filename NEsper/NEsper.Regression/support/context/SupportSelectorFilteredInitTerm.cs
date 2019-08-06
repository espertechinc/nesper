///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportSelectorFilteredInitTerm : ContextPartitionSelectorFiltered
    {
        private readonly string matchP00Value;
        private readonly LinkedHashSet<int?> cpids = new LinkedHashSet<int?>();
        private ContextPartitionIdentifierInitiatedTerminated lastValue;
        private readonly IList<object> p00PropertyValues = new List<object>();

        private readonly IList<object> startTimes = new List<object>();

        public SupportSelectorFilteredInitTerm(string matchP00Value)
        {
            this.matchP00Value = matchP00Value;
        }

        public object[] ContextsStartTimes => startTimes.ToArray();

        public object[] P00PropertyValues => p00PropertyValues.ToArray();

        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
        {
            var id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
            if (matchP00Value == null && cpids.Contains(id.ContextPartitionId)) {
                throw new EPException("Already exists context Id: " + id.ContextPartitionId);
            }

            cpids.Add(id.ContextPartitionId);
            startTimes.Add(id.StartTime);
            var p00Value = (string) ((EventBean) id.Properties.Get("s0")).Get("P00");
            p00PropertyValues.Add(p00Value);
            lastValue = id;
            return matchP00Value != null && matchP00Value.Equals(p00Value);
        }
    }
} // end of namespace