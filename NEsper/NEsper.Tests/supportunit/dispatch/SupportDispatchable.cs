///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.dispatch;


namespace com.espertech.esper.supportunit.dispatch
{
    public class SupportDispatchable : Dispatchable
    {
        private static IList<SupportDispatchable> instanceList = new List<SupportDispatchable>();
        private int numExecuted;
    
        public void Execute()
        {
            numExecuted++;
            instanceList.Add(this);
        }
    
        public int GetAndResetNumExecuted()
        {
            int val = numExecuted;
            numExecuted = 0;
            return val;
        }
    
        public static IList<SupportDispatchable> GetAndResetInstanceList()
        {
            IList<SupportDispatchable> instances = instanceList;
            instanceList = new List<SupportDispatchable>();
            return instances;
        }
    }
}
