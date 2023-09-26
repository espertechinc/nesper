///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///  General purpose callback to Stop a resource and free it's underlying resources.
    /// </summary>
    public interface StopCallback
    {
        void Stop();
    }

    public class ProxyStopCallback : StopCallback
    {
        public Action ProcStop;

        public ProxyStopCallback()
        {
        }

        public ProxyStopCallback(Action procStop)
        {
            ProcStop = procStop;
        }

        public void Stop()
        {
            ProcStop.Invoke();
        }

        private bool Equals(ProxyStopCallback other)
        {
            return Equals(ProcStop, other.ProcStop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is ProxyStopCallback callback && Equals(callback);
        }

        public override int GetHashCode()
        {
            return ProcStop != null ? ProcStop.GetHashCode() : 0;
        }
    }
}