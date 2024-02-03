///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

        public ICollection<int> Incoming { get; }

        public ICollection<int> Outgoing { get; }

        public void AddIncoming(int num)
        {
            Incoming.Add(num);
        }

        public void AddOutgoing(int num)
        {
            Outgoing.Add(num);
        }

        public override string ToString()
        {
            return "OperatorDependencyEntry{" +
                   "incoming=" +
                   Incoming.RenderAny() +
                   ", outgoing=" +
                   Outgoing.RenderAny() +
                   '}';
        }
    }
}