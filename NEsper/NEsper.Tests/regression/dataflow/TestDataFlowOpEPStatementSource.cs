///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDataFlowOpEPStatementSource
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportCaptureOp));
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestStmtNameDynamic() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create map schema SingleProp (id string), " +
                    "EPStatementSource -> thedata<SingleProp> {" +
                    "  statementName : 'MyStatement'," +
                    "} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
    
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.IsNull(df.UserObject);
            Assert.IsNull(df.InstanceId);
            df.Start();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, captureOp.GetCurrent().Length);
    
            var stmt = _epService.EPAdministrator.CreateEPL("@Name('MyStatement') select TheString as id from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"E2"});
    
            stmt.Stop();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(0, captureOp.GetCurrent().Length);
    
            stmt.Start();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"E4"});
    
            stmt.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.AreEqual(0, captureOp.GetCurrent().Length);
    
            _epService.EPAdministrator.CreateEPL("@Name('MyStatement') select 'X'||TheString||'X' as id from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"XE6X"});
    
            df.Cancel();
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestAllTypes()
        {
            DefaultSupportGraphEventUtil.AddTypeConfiguration(_epService);
    
            RunAssertionStatementNameExists("MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionStatementNameExists("MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionStatementNameExists("MyEvent", DefaultSupportGraphEventUtil.PONOEvents);
            RunAssertionStatementNameExists("MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
    
            // test doc samples
            var epl = "create dataflow MyDataFlow\n" +
                    "  create schema SampleSchema(tagId string, locX double),\t// sample type\t\t\t\n" +
                    "\t\t\t\n" +
                    "  // Consider only the statement named MySelectStatement when it exists.\n" +
                    "  EPStatementSource -> stream.one<eventbean<?>> {\n" +
                    "    statementName : 'MySelectStatement'\n" +
                    "  }\n" +
                    "  \n" +
                    "  // Consider all statements that match the filter object provided.\n" +
                    "  EPStatementSource -> stream.two<eventbean<?>> {\n" +
                    "    statementFilter : {\n" +
                    "      class : '" + typeof(MyFilter).FullName + "'\n" +
                    "    }\n" +
                    "  }\n" +
                    "  \n" +
                    "  // Consider all statements that match the filter object provided.\n" +
                    "  // With collector that performs transformation.\n" +
                    "  EPStatementSource -> stream.two<SampleSchema> {\n" +
                    "    collector : {\n" +
                    "      class : '" + typeof(MyCollector).FullName + "'\n" +
                    "    },\n" +
                    "    statementFilter : {\n" +
                    "      class : '" + typeof(MyFilter).FullName + "'\n" +
                    "    }\n" +
                    "  }";
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
        }
    
        [Test]
        public void TestInvalid() {
            // test no statement name or statement filter provided
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 EPStatementSource -> abc {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EPStatementSource': Failed to find required 'StatementName' or 'statementFilter' parameter");
    
            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(_epService, "DF1", "create dataflow DF1 EPStatementSource { statementName : 'abc' }",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EPStatementSource': EPStatementSource operator requires one output stream but produces 0 streams");
        }
    
        [Test]
        public void TestStatementFilter() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_B));
    
            // one statement exists before the data flow
            var stmt = _epService.EPAdministrator.CreateEPL("select id from SupportBean_B");
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create schema AllObjects Object," +
                    "EPStatementSource -> thedata<AllObjects> {} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp();
            var options = new EPDataFlowInstantiationOptions();
            var myFilter = new MyFilter();
            options.ParameterProvider(new DefaultSupportGraphParamProvider(Collections.SingletonDataMap("statementFilter", myFilter)));
            options.OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"B1"});
    
            _epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "TheString,IntPrimitive".Split(','), new Object[]{"E1", 1});
    
            var stmtTwo = _epService.EPAdministrator.CreateEPL("select id from SupportBean_A");
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"A1"});
    
            stmtTwo.Stop();
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            Thread.Sleep(50);
            Assert.AreEqual(0, captureOp.GetCurrent().Length);
    
            stmtTwo.Start();
            
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"A3"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(captureOp.GetCurrentAndReset()[0], "id".Split(','), new Object[]{"B2"});
    
            df.Cancel();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            Assert.AreEqual(0, captureOp.GetCurrent().Length);
        }
    
        private void RunAssertionStatementNameExists(String typeName, Object[] events)
        {
            _epService.EPAdministrator.CreateEPL("@Name('MyStatement') select * from " + typeName);
    
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create schema AllObject System.Object," +
                    "EPStatementSource -> thedata<AllObject> {" +
                    "  statementName : 'MyStatement'," +
                    "} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp(2);
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
    
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
    
            var sender = _epService.EPRuntime.GetEventSender(typeName);
            foreach (var theEvent in events) {
                sender.SendEvent(theEvent);
            }

            captureOp.GetValue(TimeSpan.FromSeconds(1));
            EPAssertionUtil.AssertEqualsExactOrder(events, captureOp.GetCurrent());
    
            df.Cancel();
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class MyFilter : EPDataFlowEPStatementFilter {
            public bool Pass(EPStatement statement) {
                return true;
            }
        }
    
        public class MyCollector : EPDataFlowIRStreamCollector {
            public void Collect(EPDataFlowIRStreamCollectorContext data) {
            }
        }
    }
}
