///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class DefaultSupportCaptureOpStatic<T> : EPDataFlowSignalHandler,
        DataFlowOperator
    {
        private static readonly IList<DefaultSupportCaptureOpStatic<T>> instances =
            new List<DefaultSupportCaptureOpStatic<T>>();

        public DefaultSupportCaptureOpStatic()
        {
            instances.Add(this);
        }

        public IList<object> Current { get; } = new List<object>();

        public void OnSignal(EPDataFlowSignal signal)
        {
            Current.Add(signal);
        }

        public void OnInput(T @event)
        {
            lock (this) {
                Current.Add(@event);
            }
        }

        public static IList<DefaultSupportCaptureOpStatic<T>> GetInstances()
        {
            return instances;
        }
    }
} // end of namespace