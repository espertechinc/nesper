///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.events;
using NUnit.Framework;

namespace com.espertech.esper.epl.join.@base
{
    [TestFixture]
    public class TestJoinSetFilter 
    {
        [Test]
        public void TestFilter()
        {
            ExprNode topNode = SupportExprNodeFactory.Make2SubNodeAnd();
    
            var pairOne = new EventBean[2];
            pairOne[0] = MakeEvent(1, 2, "a");
            pairOne[1] = MakeEvent(2, 1, "a");
    
            var pairTwo = new EventBean[2];
            pairTwo[0] = MakeEvent(1, 2, "a");
            pairTwo[1] = MakeEvent(2, 999, "a");

            var eventSet = new HashSet<MultiKey<EventBean>>();
            eventSet.Add(new MultiKey<EventBean>(pairOne));
            eventSet.Add(new MultiKey<EventBean>(pairTwo));
    
            JoinSetFilter.Filter(topNode.ExprEvaluator, eventSet, true, null);
    
            Assert.AreEqual(1, eventSet.Count);
            Assert.AreSame(pairOne, eventSet.FirstOrDefault().Array);
        }
    
        private EventBean MakeEvent(int intPrimitive, int intBoxed, String TheString)
        {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.IntBoxed = intBoxed;
            theEvent.TheString = TheString;
            return SupportEventBeanFactory.CreateObject(theEvent);
        }
    }
}
