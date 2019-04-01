///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.dataflow;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;

namespace com.espertech.esper.dataflow.util
{
    [DataFlowOperator]
    public class DefaultSupportCaptureOpStatic
        : EPDataFlowSignalHandler
    {
        private readonly List<object> _current = new List<object>();

        public DefaultSupportCaptureOpStatic()
        {
            Instances.Add(this);
        }

        static DefaultSupportCaptureOpStatic()
        {
            Instances = new List<DefaultSupportCaptureOpStatic>();
        }

        public void OnInput(object theEvent)
        {
            lock (this)
            {
                _current.Add(theEvent);
            }
        }

        public void OnSignal(EPDataFlowSignal signal)
        {
            _current.Add(signal);
        }

        public static List<DefaultSupportCaptureOpStatic> Instances { get; private set; }

        public IList<object> Current {
            get { return _current; }
        }
    }
}
