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
        private readonly string _matchP00Value;
        private readonly LinkedHashSet<int?> _cpids = new LinkedHashSet<int?>();
        private ContextPartitionIdentifierInitiatedTerminated _lastValue;
        private readonly IList<object> _p00PropertyValues = new List<object>();

        private readonly IList<object> _startTimes = new List<object>();

        public SupportSelectorFilteredInitTerm(string matchP00Value)
        {
            _matchP00Value = matchP00Value;
        }

        public object[] ContextsStartTimes => _startTimes.ToArray();

        public object[] P00PropertyValues => _p00PropertyValues.ToArray();

        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
        {
            var id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
            if (_matchP00Value == null && _cpids.Contains(id.ContextPartitionId)) {
                throw new EPException("Already exists context Id: " + id.ContextPartitionId);
            }

            _cpids.Add(id.ContextPartitionId);
            _startTimes.Add(id.StartTime);
            var p00Value = (string) ((EventBean) id.Properties.Get("s0")).Get("P00");
            _p00PropertyValues.Add(p00Value);
            _lastValue = id;
            return _matchP00Value != null && _matchP00Value.Equals(p00Value);
        }
    }
} // end of namespace