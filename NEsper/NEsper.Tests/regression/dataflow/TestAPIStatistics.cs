///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.dataflow.util;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPIStatistics
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp).FullName);
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).FullName);
        }
    
        [Test]
        public void TestOperatorStatistics() {
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("create dataflow MyGraph " +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {} " +
                    "DefaultSupportCaptureOp(outstream) {}");
    
            DefaultSupportSourceOp source = new DefaultSupportSourceOp(new Object[] {new SupportBean("E1", 1), new SupportBean("E2", 2) });
            DefaultSupportCaptureOp capture = new DefaultSupportCaptureOp();
            EPDataFlowInstantiationOptions options = new EPDataFlowInstantiationOptions()
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
}
