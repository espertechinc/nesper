///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class OperatorDependencyEntry
    {
        public OperatorDependencyEntry()
        {
            Incoming = new LinkedHashSet<int>();
            Outgoing = new LinkedHashSet<int>();
        }

        public void AddIncoming(int num)
        {
            Incoming.Add(num);
        }

        public void AddOutgoing(int num)
        {
            Outgoing.Add(num);
        }

        public ICollection<int> Incoming { get; private set; }

        public ICollection<int> Outgoing { get; private set; }

        public override string ToString()
        {
            return "OperatorDependencyEntry{" +
                   "incoming=" + Incoming.Render() +
                   ", outgoing=" + Outgoing.Render() +
                   '}';
        }
    }
}