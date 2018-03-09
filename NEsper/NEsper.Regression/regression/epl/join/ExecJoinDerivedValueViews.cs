///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinDerivedValueViews : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select\n" +
                    "Math.Sign(stream1.slope) as s1,\n" +
                    "Math.Sign(stream2.slope) as s2\n" +
                    "from\n" +
                    "SupportBean#length_batch(3)#linest(IntPrimitive, LongPrimitive) as stream1,\n" +
                    "SupportBean#length_batch(2)#linest(IntPrimitive, LongPrimitive) as stream2").Events += listener.Update;
            epService.EPRuntime.SendEvent(MakeEvent("E3", 1, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E4", 1, 100));
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private SupportBean MakeEvent(string id, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(id, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    }
} // end of namespace
