///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.context
{
    public class SupportSelectorFilteredInitTerm : ContextPartitionSelectorFiltered {
    
        private readonly string matchP00Value;
    
        private List<Object> startTimes = new List<Object>();
        private List<Object> p00PropertyValues = new List<Object>();
        private LinkedHashSet<int?> cpids = new LinkedHashSet<int?>();
        private ContextPartitionIdentifierInitiatedTerminated lastValue;
    
        public SupportSelectorFilteredInitTerm(string matchP00Value) {
            this.matchP00Value = matchP00Value;
        }
    
        public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
            ContextPartitionIdentifierInitiatedTerminated id = (ContextPartitionIdentifierInitiatedTerminated) contextPartitionIdentifier;
            if (matchP00Value == null && cpids.Contains(id.ContextPartitionId)) {
                throw new EPRuntimeException("Already Exists context id: " + id.ContextPartitionId);
            }
            cpids.Add(id.ContextPartitionId);
            startTimes.Add(id.StartTime);
            string p00Value = (string) ((EventBean) id.Properties.Get("s0")).Get("p00");
            p00PropertyValues.Add(p00Value);
            lastValue = id;
            return matchP00Value != null && matchP00Value.Equals(p00Value);
        }
    
        public Object[] GetContextsStartTimes() {
            return StartTimes.ToArray();
        }
    
        public Object[] GetP00PropertyValues() {
            return P00PropertyValues.ToArray();
        }
    }
} // end of namespace
