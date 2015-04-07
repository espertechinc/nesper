///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.graph;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestTypes  {
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestBeanType() {
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean));
            _epService.EPAdministrator.CreateEPL("create schema SupportBean SupportBean");
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {}" +
                    "MySupportBeanOutputOp(outstream) {}" +
                    "SupportGenericOutputOpWPort(outstream) {}");
    
            var source = new DefaultSupportSourceOp(new Object[] {new SupportBean("E1", 1)});
            var outputOne = new MySupportBeanOutputOp();
            var outputTwo = new SupportGenericOutputOpWPort();
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
            var dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            dfOne.Run();
    
            EPAssertionUtil.AssertPropsPerRow(outputOne.GetAndReset().ToArray(), "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] {"E1", 1}});
            var received = outputTwo.GetAndReset();
            EPAssertionUtil.AssertPropsPerRow(received.First.ToArray(), "TheString,IntPrimitive".Split(','), new Object[][] { new Object[] {"E1", 1}});
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {0}, received.Second.ToArray());
        }
    
        [Test]
        public void TestMapType() {
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportBean));
            _epService.EPAdministrator.CreateEPL("create map schema MyMap (p0 String, p1 int)");
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "DefaultSupportSourceOp -> outstream<MyMap> {}" +
                    "MyMapOutputOp(outstream) {}" +
                    "DefaultSupportCaptureOp(outstream) {}");
    
            var source = new DefaultSupportSourceOp(new Object[] {MakeMap("E1", 1)});
            var outputOne = new MyMapOutputOp();
            var outputTwo = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions().OperatorProvider(new DefaultSupportGraphOpProvider(source, outputOne, outputTwo));
            var dfOne = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            dfOne.Run();
    
            EPAssertionUtil.AssertPropsPerRow(outputOne.GetAndReset().ToArray(), "p0,p1".Split(','), new Object[][] { new Object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(outputTwo.GetAndReset()[0].ToArray(), "p0,p1".Split(','), new Object[][] { new Object[] {"E1", 1}});
        }
    
        private static IDictionary<String, Object> MakeMap(String p0, int p1) {
            IDictionary<String, Object> map = new Dictionary<String, Object>();
            map["p0"] = p0;
            map["p1"] = p1;
            return map;
        }
    
        public class MySupportBeanOutputOp {
            private List<SupportBean> _received = new List<SupportBean>();
    
            public void OnInput(SupportBean theEvent) {
                lock(this)
                {
                    _received.Add(theEvent);
                }
            }
    
            public List<SupportBean> GetAndReset() {
                lock(this)
                {
                    List<SupportBean> result = _received;
                    _received = new List<SupportBean>();
                    return result;
                }
            }
        }
    
        public class MyMapOutputOp {
            private List<IDictionary<String, Object>> _received = new List<IDictionary<String, Object>>();
    
            public void OnInput(IDictionary<String, Object> theEvent) {
                lock(this)
                {
                    _received.Add(theEvent);
                }
            }
    
            public List<IDictionary<String, Object>> GetAndReset()
            {
                lock (this)
                {
                    List<IDictionary<String, Object>> result = _received;
                    _received = new List<IDictionary<String, Object>>();
                    return result;
                }
            }
        }
    }
}
