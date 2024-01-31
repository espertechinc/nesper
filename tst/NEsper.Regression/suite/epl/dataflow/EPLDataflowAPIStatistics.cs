///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIStatistics : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@name('flow') create dataflow MyGraph " +
                "DefaultSupportSourceOp -> outstream<SupportBean> {} " +
                "DefaultSupportCaptureOp(outstream) {}");
            ClassicAssert.AreEqual(
                StatementType.CREATE_DATAFLOW,
                env.Statement("flow").GetProperty(StatementProperty.STATEMENTTYPE));
            ClassicAssert.AreEqual("MyGraph", env.Statement("flow").GetProperty(StatementProperty.CREATEOBJECTNAME));


            var source =
                new DefaultSupportSourceOp(new object[] { new SupportBean("E1", 1), new SupportBean("E2", 2) });
            var capture = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions()
                .WithOperatorProvider(new DefaultSupportGraphOpProvider(source, capture))
                .WithOperatorStatistics(true)
                .WithCpuStatistics(true);

            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyGraph", options);

            instance.Run();

            var stats = instance.Statistics.OperatorStatistics;
            ClassicAssert.AreEqual(2, stats.Count);

            var sourceStat = stats[0];
            ClassicAssert.AreEqual("DefaultSupportSourceOp", sourceStat.OperatorName);
            ClassicAssert.AreEqual(0, sourceStat.OperatorNumber);
            ClassicAssert.AreEqual("DefaultSupportSourceOp#0() -> outstream<SupportBean>", sourceStat.OperatorPrettyPrint);
            ClassicAssert.AreEqual(2, sourceStat.SubmittedOverallCount);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { 2L }, sourceStat.SubmittedPerPortCount);
            ClassicAssert.IsTrue(sourceStat.TimeOverall > 0);
            ClassicAssert.AreEqual(sourceStat.TimeOverall, sourceStat.TimePerPort[0]);

            var destStat = stats[1];
            ClassicAssert.AreEqual("DefaultSupportCaptureOp", destStat.OperatorName);
            ClassicAssert.AreEqual(1, destStat.OperatorNumber);
            ClassicAssert.AreEqual("DefaultSupportCaptureOp#1(outstream)", destStat.OperatorPrettyPrint);
            ClassicAssert.AreEqual(0, destStat.SubmittedOverallCount);
            ClassicAssert.AreEqual(0, destStat.SubmittedPerPortCount.Length);
            ClassicAssert.AreEqual(0, destStat.TimeOverall);
            ClassicAssert.AreEqual(0, destStat.TimePerPort.Length);

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.DATAFLOW);
        }
    }
} // end of namespace