///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regression.context
{
    public class SupportSelectorFilteredInitTerm : ContextPartitionSelectorFiltered
    {
        private readonly String _matchP00Value;

        private readonly List<Object> _startTimes = new List<Object>();
        private readonly List<Object> _p00PropertyValues = new List<Object>();
        private readonly LinkedHashSet<int> _cpids = new LinkedHashSet<int>();
        private ContextPartitionIdentifierInitiatedTerminated _lastValue;

        public SupportSelectorFilteredInitTerm(String matchP00Value)
        {
            _matchP00Value = matchP00Value;
        }

        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
        {
            var id = (ContextPartitionIdentifierInitiatedTerminated)contextPartitionIdentifier;
            if (id.ContextPartitionId == null)
            {
                throw new ArgumentException("invalid context partition identifier");
            }
            if (_matchP00Value == null && _cpids.Contains(id.ContextPartitionId.Value))
            {
                throw new Exception("Already exists context id: " + id.ContextPartitionId);
            }
            _cpids.Add(id.ContextPartitionId.Value);
            _startTimes.Add(id.StartTime);

            var p00Value = (String)((EventBean)id.Properties.Get("s0")).Get("p00");
            _p00PropertyValues.Add(p00Value);
            _lastValue = id;
            return _matchP00Value != null && _matchP00Value.Equals(p00Value);
        }

        public object[] ContextsStartTimes
        {
            get { return _startTimes.ToArray(); }
        }

        public object[] P00PropertyValues
        {
            get { return _p00PropertyValues.ToArray(); }
        }
    }
}
