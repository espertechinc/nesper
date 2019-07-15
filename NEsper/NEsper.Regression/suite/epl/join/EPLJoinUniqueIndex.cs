///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinUniqueIndex : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();

            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () => {
                var fields = "ssb1.s1,ssb2.s2".SplitCsv();
                env.SendEventBean(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                env.SendEventBean(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                env.SendEventBean(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                env.SendEventBean(new SupportSimpleBeanOne("EX", 1, 3, 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"EX", "E3"});
            };

            var testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (var caseEnum in testCases) {
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1",
                    false,
                    assertSendEvents);
                RunAssertion(env, milestone, caseEnum, "d2,i2", "where ssb2.d2 = ssb1.d1", false, assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,i2",
                    "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000",
                    false,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "i2,d2,l2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1",
                    false,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "i2,d2,l2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,l2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    true,
                    assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "d2,l2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'",
                    true,
                    assertSendEvents);
                RunAssertion(env, milestone, caseEnum, "l2", "where ssb2.l2 = ssb1.l1", true, assertSendEvents);
                RunAssertion(
                    env,
                    milestone,
                    caseEnum,
                    "l2",
                    "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20",
                    true,
                    assertSendEvents);
            }
        }

        private void RunAssertion(
            RegressionEnvironment env,
            AtomicLong milestone,
            CaseEnum caseEnum,
            string uniqueFields,
            string whereClause,
            bool unique,
            IndexAssertionEventSend assertion)
        {
            SupportQueryPlanIndexHook.Reset();
            var eplUnique = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                            "@Name('s0') select * from ";

            if (caseEnum == CaseEnum.UNIDIRECTIONAL || caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM) {
                eplUnique += "SupportSimpleBeanOne as ssb1 unIdirectional ";
            }
            else {
                eplUnique += "SupportSimpleBeanOne#lastevent as ssb1 ";
            }

            eplUnique += ", SupportSimpleBeanTwo#unique(" + uniqueFields + ") as ssb2 ";
            if (caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM || caseEnum == CaseEnum.MULTIDIRECTIONAL_3STREAM) {
                eplUnique += ", SupportBean#lastevent ";
            }

            eplUnique += whereClause;

            env.CompileDeployAddListenerMile(eplUnique, "s0", milestone.GetAndIncrement());

            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);

            env.SendEventBean(new SupportBean("JOINEVENT", 1));
            assertion.Invoke();

            env.UndeployAll();
        }

        private enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
            UNIDIRECTIONAL_3STREAM,
            MULTIDIRECTIONAL_3STREAM
        }
    }
} // end of namespace