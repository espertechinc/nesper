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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOnUpdateWMultiDispatch : RegressionExecution {
        private readonly bool useDefault;
        private readonly bool preserve;
        private readonly ConfigurationEngineDefaults.ThreadingConfig.Locking locking;
    
        public ExecNamedWindowOnUpdateWMultiDispatch(bool useDefault, bool? preserve, ConfigurationEngineDefaults.ThreadingConfig.Locking? locking) {
            this.useDefault = useDefault;
            this.preserve = preserve.GetValueOrDefault();
            this.locking = locking.GetValueOrDefault();
        }
    
        public override void Configure(Configuration configuration) {
            if (!useDefault) {
                configuration.EngineDefaults.Threading.IsNamedWindowConsumerDispatchPreserveOrder = preserve;
                configuration.EngineDefaults.Threading.NamedWindowConsumerDispatchLocking = locking;
            }
        }
    
        public override void Run(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string[] fields = "company,value,total".Split(',');
    
            // ESPER-568
            epService.EPAdministrator.CreateEPL("create schema S2 ( company string, value double, total double)");
            EPStatement stmtWin = epService.EPAdministrator.CreateEPL("create window S2Win#time(25 hour)#firstunique(company) as S2");
            epService.EPAdministrator.CreateEPL("insert into S2Win select * from S2#firstunique(company)");
            epService.EPAdministrator.CreateEPL("on S2 as a update S2Win as b set total = b.value + a.value");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select count(*) as cnt from S2Win");
            stmt.Events += listener.Update;
    
            CreateSendEvent(epService, "S2", "AComp", 3.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{new object[] {"AComp", 3.0, 0.0}});
    
            CreateSendEvent(epService, "S2", "AComp", 6.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{new object[] {"AComp", 3.0, 9.0}});
    
            CreateSendEvent(epService, "S2", "AComp", 5.0, 0.0);
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("cnt"));
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{new object[] {"AComp", 3.0, 8.0}});
    
            CreateSendEvent(epService, "S2", "BComp", 4.0, 0.0);
            // this example does not have @priority thereby it is undefined whether there are two counts delivered or one
            if (listener.LastNewData.Length == 2) {
                Assert.AreEqual(1L, listener.LastNewData[0].Get("cnt"));
                Assert.AreEqual(2L, listener.LastNewData[1].Get("cnt"));
            } else {
                Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("cnt"));
            }
            EPAssertionUtil.AssertPropsPerRow(stmtWin.GetEnumerator(), fields, new object[][]{new object[] {"AComp", 3.0, 7.0}, new object[] {"BComp", 4.0, 0.0}});
        }
    
        private void CreateSendEvent(EPServiceProvider engine, string typeName, string company, double value, double total) {
            var map = new LinkedHashMap<string, Object>();
            map.Put("company", company);
            map.Put("value", value);
            map.Put("total", total);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(engine).IsObjectArrayEvent()) {
                engine.EPRuntime.SendEvent(map.Values.ToArray(), typeName);
            } else {
                engine.EPRuntime.SendEvent(map, typeName);
            }
        }
    }
} // end of namespace
