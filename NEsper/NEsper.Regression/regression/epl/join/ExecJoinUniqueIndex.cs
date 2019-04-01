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
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinUniqueIndex : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            epService.EPAdministrator.Configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var listener = new SupportUpdateListener();
    
            // test no where clause with unique on multiple props, exact specification of where-clause
            var assertSendEvents = new IndexAssertionEventSend(() => {
                string[] fields = "ssb1.s1,ssb2.s2".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("EX", 1, 3, 9));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"EX", "E3"});
            });

            var testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (CaseEnum caseEnum in testCases) {
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1", false, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1", false, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000", false, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1", false, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "i2,d2,l2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "d2,l2,i2", "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "l2", "where ssb2.l2 = ssb1.l1", true, assertSendEvents);
                RunAssertion(epService, listener, caseEnum, "l2", "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20", true, assertSendEvents);
            }
        }
    
        private void RunAssertion(EPServiceProvider epService, SupportUpdateListener listener, CaseEnum caseEnum, string uniqueFields, string whereClause, bool unique, IndexAssertionEventSend assertion) {
            SupportQueryPlanIndexHook.Reset();
            string eplUnique = INDEX_CALLBACK_HOOK +
                    "select * from ";
    
            if (caseEnum == CaseEnum.UNIDIRECTIONAL || caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM) {
                eplUnique += "SSB1 as ssb1 unidirectional ";
            } else {
                eplUnique += "SSB1#lastevent as ssb1 ";
            }
            eplUnique += ", SSB2#unique(" + uniqueFields + ") as ssb2 ";
            if (caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM || caseEnum == CaseEnum.MULTIDIRECTIONAL_3STREAM) {
                eplUnique += ", SupportBean#lastevent ";
            }
            eplUnique += whereClause;
    
            EPStatement stmtUnique = epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += listener.Update;
    
            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);
    
            epService.EPRuntime.SendEvent(new SupportBean("JOINEVENT", 1));
            assertion.Invoke();
    
            stmtUnique.Dispose();
        }
    
        private enum CaseEnum {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
            UNIDIRECTIONAL_3STREAM,
            MULTIDIRECTIONAL_3STREAM
        }
    
    }
} // end of namespace
