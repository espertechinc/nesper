///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinUniqueIndex : RegressionExecution
    {
        private readonly CaseEnum _caseEnum;

        public EPLJoinUniqueIndex(CaseEnum caseEnum)
        {
            _caseEnum = caseEnum;
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.STATICHOOK);
        }

        public void Run(RegressionEnvironment env)
        {
            var milestone = new AtomicLong();

            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () => {
                var fields = new[] { "ssb1.S1", "ssb2.S2" };
                env.SendEventBean(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                env.SendEventBean(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                env.SendEventBean(new SupportSimpleBeanTwo("E3", 1, 3, 9));
                env.SendEventBean(new SupportSimpleBeanOne("EX", 1, 3, 9));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "EX", "E3" });
            };

            RunCase(env, milestone, _caseEnum, assertSendEvents);
        }

        private void RunCase(
            RegressionEnvironment env,
            AtomicLong milestone,
            CaseEnum caseEnum,
            IndexAssertionEventSend assertSendEvents)
        {
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.I2 = ssb1.I1 and ssb2.D2 = ssb1.D1",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.D2 = ssb1.D1 and ssb2.I2 = ssb1.I1",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.L2 = ssb1.L1 and ssb2.D2 = ssb1.D1 and ssb2.I2 = ssb1.I1",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.L2 = ssb1.L1 and ssb2.I2 = ssb1.I1",
                false,
                assertSendEvents);
            RunAssertion(env, milestone, caseEnum, "D2,I2", "where ssb2.D2 = ssb1.D1", false, assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.I2 = ssb1.I1 and ssb2.D2 = ssb1.D1 and ssb2.L2 between 1 and 1000",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,I2",
                "where ssb2.D2 = ssb1.D1 and ssb2.L2 between 1 and 1000",
                false,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "I2,D2,L2",
                "where ssb2.L2 = ssb1.L1 and ssb2.D2 = ssb1.D1",
                false,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "I2,D2,L2",
                "where ssb2.L2 = ssb1.L1 and ssb2.I2 = ssb1.I1 and ssb2.D2 = ssb1.D1",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,L2,I2",
                "where ssb2.L2 = ssb1.L1 and ssb2.I2 = ssb1.I1 and ssb2.D2 = ssb1.D1",
                true,
                assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "D2,L2,I2",
                "where ssb2.L2 = ssb1.L1 and ssb2.I2 = ssb1.I1 and ssb2.D2 = ssb1.D1 and ssb2.S2 between 'E3' and 'E4'",
                true,
                assertSendEvents);
            RunAssertion(env, milestone, caseEnum, "L2", "where ssb2.L2 = ssb1.L1", true, assertSendEvents);
            RunAssertion(
                env,
                milestone,
                caseEnum,
                "L2",
                "where ssb2.L2 = ssb1.L1 and ssb1.I1 between 1 and 20",
                true,
                assertSendEvents);
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
            var eplUnique = $"{IndexBackingTableInfo.INDEX_CALLBACK_HOOK}@name('s0') select * from ";

            if (caseEnum == CaseEnum.UNIDIRECTIONAL || caseEnum == CaseEnum.UNIDIRECTIONAL_3STREAM) {
                eplUnique += "SupportSimpleBeanOne as ssb1 unidirectional ";
            }
            else {
                eplUnique += "SupportSimpleBeanOne#lastevent as ssb1 ";
            }

            eplUnique += $", SupportSimpleBeanTwo#unique({uniqueFields}) as ssb2 ";
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

        public enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
            UNIDIRECTIONAL_3STREAM,
            MULTIDIRECTIONAL_3STREAM
        }
    }
} // end of namespace