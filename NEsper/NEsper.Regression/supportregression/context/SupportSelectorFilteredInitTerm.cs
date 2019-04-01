///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.context {
    public class SupportSelectorFilteredInitTerm : ContextPartitionSelectorFiltered {
        private readonly LinkedHashSet<int?> _cpids = new LinkedHashSet<int?>();
        private readonly string _matchP00Value;
        private readonly List<object> _p00PropertyValues = new List<object>();
        private readonly List<object> _startTimes = new List<object>();
        private ContextPartitionIdentifierInitiatedTerminated _lastValue;

        public SupportSelectorFilteredInitTerm(string matchP00Value) {
            _matchP00Value = matchP00Value;
        }

        public object[] ContextsStartTimes => _startTimes.ToArray();

        public object[] P00PropertyValues => _p00PropertyValues.ToArray();

        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
            var id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
            if (_matchP00Value == null && _cpids.Contains(id.ContextPartitionId)) {
                throw new EPRuntimeException("Already exists context id: " + id.ContextPartitionId);
            }

            _cpids.Add(id.ContextPartitionId);
            _startTimes.Add(id.StartTime);
            var p00Value = (string) ((EventBean) id.Properties.Get("s0")).Get("p00");
            _p00PropertyValues.Add(p00Value);
            _lastValue = id;
            return _matchP00Value != null && _matchP00Value.Equals(p00Value);
        }
    }
} // end of namespace