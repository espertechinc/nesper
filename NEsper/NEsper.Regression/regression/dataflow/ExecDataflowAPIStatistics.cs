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
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIStatistics : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp).Name);
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).Name);
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {} " +
                    "DefaultSupportCaptureOp(outstream) {}");
    
            var source = new DefaultSupportSourceOp(new object[]{new SupportBean("E1", 1), new SupportBean("E2", 2)});
            var capture = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(source, capture))
                    .OperatorStatistics(true)
                    .CpuStatistics(true);
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph", options);
    
            instance.Run();
    
            IList<EPDataFlowInstanceOperatorStat> stats = instance.Statistics.OperatorStatistics;
            Assert.AreEqual(2, stats.Count);
    
            EPDataFlowInstanceOperatorStat sourceStat = stats[0];
            Assert.AreEqual("DefaultSupportSourceOp", sourceStat.OperatorName);
            Assert.AreEqual(0, sourceStat.OperatorNumber);
            Assert.AreEqual("DefaultSupportSourceOp#0() -> outstream<SupportBean>", sourceStat.OperatorPrettyPrint);
            Assert.AreEqual(2, sourceStat.SubmittedOverallCount);
            EPAssertionUtil.AssertEqualsExactOrder(new long[]{2L}, sourceStat.SubmittedPerPortCount);
            Assert.IsTrue(sourceStat.TimeOverall > 0);
            Assert.AreEqual(sourceStat.TimeOverall, sourceStat.TimePerPort[0]);
    
            EPDataFlowInstanceOperatorStat destStat = stats[1];
            Assert.AreEqual("DefaultSupportCaptureOp", destStat.OperatorName);
            Assert.AreEqual(1, destStat.OperatorNumber);
            Assert.AreEqual("DefaultSupportCaptureOp#1(outstream)", destStat.OperatorPrettyPrint);
            Assert.AreEqual(0, destStat.SubmittedOverallCount);
            Assert.AreEqual(0, destStat.SubmittedPerPortCount.Length);
            Assert.AreEqual(0, destStat.TimeOverall);
            Assert.AreEqual(0, destStat.TimePerPort.Length);
        }
    }
} // end of namespace
