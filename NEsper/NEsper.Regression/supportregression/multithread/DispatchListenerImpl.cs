///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.multithread
{
    public class DispatchListenerImpl : DispatchListener
    {
        private readonly IList<int[][]> received = new List<int[][]>();
    
        public void Dispatched(int[][] objects)
        {
            lock(this) {
                received.Add(objects);
            }
        }

        public IList<int[][]> Received
        {
            get { return received; }
        }
    }
}
