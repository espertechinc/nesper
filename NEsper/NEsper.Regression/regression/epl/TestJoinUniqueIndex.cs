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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinUniqueIndex 
        : IndexBackingTableInfo
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            SupportQueryPlanIndexHook.Reset();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestIndexChoicesJoinUnique() {
            _epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            _epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = 
                () =>
                {
                    String[] fields = "ssb1.s1,ssb2.s2".Split(',');
                    _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                    _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                    _epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                    _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 1, 3, 9));
                    EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"EX", "E3"});
                };

            IEnumerable<CaseEnum> testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (CaseEnum caseEnum in testCases) {
                RunAssertion(caseEnum, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1", false, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1", false, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", false, assertSendEvents);
                RunAssertion(caseEnum, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1", false, assertSendEvents);
                RunAssertion(caseEnum, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(caseEnum, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'", true, assertSendEvents);
                RunAssertion(caseEnum, "l2", "where ssb2.l2 = ssb1.l1", true, assertSendEvents);
                RunAssertion(caseEnum, "l2", "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20", true, assertSendEvents);
            }
        }
    
        private void RunAssertion(CaseEnum caseEnum, String uniqueFields, String whereClause, bool unique, IndexAssertionEventSend assertion) {
            String eplUnique = INDEX_CALLBACK_HOOK +
                    "select * from ";
    
            if (caseEnum == CaseEnum.UNIDIRECTIONAL || caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM) {
                eplUnique += "SSB1 as ssb1 unidirectional ";
            }
            else {
                eplUnique += "SSB1#lastevent as ssb1 ";
            }
            eplUnique += ", SSB2#unique(" + uniqueFields + ") as ssb2 ";
            if (caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM || caseEnum == CaseEnum.MULTIDIRECTIONAL_3STREAM) {
                eplUnique += ", SupportBean#lastevent ";
            }
            eplUnique += whereClause;
    
            EPStatement stmtUnique = _epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += _listener.Update;
    
            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);
    
            _epService.EPRuntime.SendEvent(new SupportBean("JOINEVENT", 1));
            assertion.Invoke();
    
            stmtUnique.Dispose();
        }
    
        private enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
            UNIDIRECTIONAL_3STREAM,
            MULTIDIRECTIONAL_3STREAM
        }
    }
}
