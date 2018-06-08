///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;

namespace com.espertech.esper.supportregression.graph
{
    public class SupportGenericOutputOpWPort : SupportGenericOutputOpWPort<object>
    {
    }

    public class SupportGenericOutputOpWPort<T>
    {
        private List<T> _received = new List<T>();
        private List<int> _receivedPorts = new List<int>();
    
        public void OnInput(int port, T theEvent)
        {
            lock(this)
            {
                _received.Add(theEvent);
                _receivedPorts.Add(port);
            }
        }

        public Pair<IList<T>, IList<int>> GetAndReset()
        {
            lock (this)
            {
                List<T> resultEvents = _received;
                List<int> resultPorts = _receivedPorts;
                _received = new List<T>();
                _receivedPorts = new List<int>();
                return new Pair<IList<T>, IList<int>>(resultEvents, resultPorts);
            }
        }
    }
}
