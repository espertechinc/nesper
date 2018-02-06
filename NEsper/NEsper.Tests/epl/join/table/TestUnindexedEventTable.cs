///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.table
{
    [TestFixture]
    public class TestUnindexedEventTable 
    {
        [Test]
        public void TestFlow()
        {
            UnindexedEventTable rep = new UnindexedEventTableImpl(1);
    
            EventBean[] addOne = SupportEventBeanFactory.MakeEvents(new String[] {"a", "b"});
            rep.Add(addOne, null);
            rep.Remove(new EventBean[] {addOne[0]}, null);
        }
    }
}
