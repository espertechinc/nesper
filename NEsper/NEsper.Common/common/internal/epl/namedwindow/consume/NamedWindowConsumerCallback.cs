///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
	public interface NamedWindowConsumerCallback : IEnumerable<EventBean>
	{
	    void Stopped(NamedWindowConsumerView namedWindowConsumerView);
	    bool IsParentBatchWindow { get; }
	    ICollection<EventBean> Snapshot(QueryGraph queryGraph, Attribute[] annotations);
	}

    public class ProxyNamedWindowConsumerCallback : NamedWindowConsumerCallback
    {
        public Func<IEnumerator<EventBean>> ProcGetEnumerator;
        public Action<NamedWindowConsumerView> ProcStopped;
        public Func<bool> ProcIsParentBatchWindow;
        public Func<QueryGraph, Attribute[], ICollection<EventBean>> ProcSnapshot;

        public IEnumerator<EventBean> GetEnumerator()
            => ProcGetEnumerator.Invoke();
        public void Stopped(NamedWindowConsumerView namedWindowConsumerView)
            => ProcStopped.Invoke(namedWindowConsumerView);
        public bool IsParentBatchWindow 
            => ProcIsParentBatchWindow.Invoke();
        public ICollection<EventBean> Snapshot(QueryGraph queryGraph, Attribute[] annotations)
            => ProcSnapshot(queryGraph, annotations);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
} // end of namespace