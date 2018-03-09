///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinSelectClause : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string eventA = typeof(SupportBean).FullName;
            string eventB = typeof(SupportBean).FullName;
    
            string joinStatement = "select s0.DoubleBoxed, s1.IntPrimitive*s1.IntBoxed/2.0 as div from " +
                    eventA + "(TheString='s0')#length(3) as s0," +
                    eventB + "(TheString='s1')#length(3) as s1" +
                    " where s0.DoubleBoxed = s1.DoubleBoxed";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            var updateListener = new SupportUpdateListener();
            joinView.Events += updateListener.Update;
    
            EventType result = joinView.EventType;
            Assert.AreEqual(typeof(double?), result.GetPropertyType("s0.DoubleBoxed"));
            Assert.AreEqual(typeof(double?), result.GetPropertyType("div"));
            Assert.AreEqual(2, joinView.EventType.PropertyNames.Length);
    
            Assert.IsNull(updateListener.LastNewData);
    
            SendEvent(epService, "s0", 1, 4, 5);
            SendEvent(epService, "s1", 1, 3, 2);
    
            EventBean[] newEvents = updateListener.LastNewData;
            Assert.AreEqual(1d, newEvents[0].Get("s0.DoubleBoxed"));
            Assert.AreEqual(3d, newEvents[0].Get("div"));
    
            IEnumerator<EventBean> iterator = joinView.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            EventBean theEvent = iterator.Current;
            Assert.AreEqual(1d, theEvent.Get("s0.DoubleBoxed"));
            Assert.AreEqual(3d, theEvent.Get("div"));
        }
    
        private void SendEvent(EPServiceProvider epService, string s, double doubleBoxed, int intPrimitive, int intBoxed) {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = doubleBoxed;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
