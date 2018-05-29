///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.dataflow;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowOpEPStatementSource : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionStmtNameDynamic(epService);
            RunAssertionAllTypes(epService);
            RunAssertionInvalid(epService);
            RunAssertionStatementFilter(epService);
        }
    
        private void RunAssertionStmtNameDynamic(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create map schema SingleProp (id string), " +
                    "EPStatementSource -> thedata<SingleProp> {" +
                    "  statementName : 'MyStatement'," +
                    "} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
    
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            Assert.IsNull(df.UserObject);
            Assert.IsNull(df.InstanceId);
            df.Start();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(0, captureOp.Current.Length);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('MyStatement') select TheString as id from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"E2"});
    
            stmt.Stop();
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.AreEqual(0, captureOp.Current.Length);
    
            stmt.Start();
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"E4"});
    
            stmt.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.AreEqual(0, captureOp.Current.Length);
    
            epService.EPAdministrator.CreateEPL("@Name('MyStatement') select 'X'||TheString||'X' as id from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            captureOp.WaitForInvocation(100, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"XE6X"});
    
            df.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAllTypes(EPServiceProvider epService) {
            DefaultSupportGraphEventUtil.AddTypeConfiguration((EPServiceProviderSPI) epService);
    
            RunAssertionStatementNameExists(epService, "MyMapEvent", DefaultSupportGraphEventUtil.MapEvents);
            RunAssertionStatementNameExists(epService, "MyOAEvent", DefaultSupportGraphEventUtil.OAEvents);
            RunAssertionStatementNameExists(epService, "MyEvent", DefaultSupportGraphEventUtil.PonoEvents);
            RunAssertionStatementNameExists(epService, "MyXMLEvent", DefaultSupportGraphEventUtil.XMLEvents);
    
            // test doc samples
            string epl = "create dataflow MyDataFlow\n" +
                    "  create schema SampleSchema(tagId string, locX double),\t// sample type\t\t\t\n" +
                    "\t\t\t\n" +
                    "  // Consider only the statement named MySelectStatement when it Exists.\n" +
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
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // test no statement name or statement filter provided
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 EPStatementSource -> abc {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EPStatementSource': Failed to find required 'StatementName' or 'StatementFilter' parameter");
    
            // invalid: no output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 EPStatementSource { statementName : 'abc' }",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'EPStatementSource': EPStatementSource operator requires one output stream but produces 0 streams");
        }
    
        private void RunAssertionStatementFilter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_B));
    
            // one statement Exists before the data flow
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select id from SupportBean_B");
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create schema AllObjects Object," +
                    "EPStatementSource -> thedata<AllObjects> {} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp(SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            var myFilter = new MyFilter();
            options.ParameterProvider(new DefaultSupportGraphParamProvider(Collections.SingletonDataMap("statementFilter", myFilter)));
            options.OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"B1"});
    
            epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "TheString,IntPrimitive".Split(','), new object[]{"E1", 1});
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select id from SupportBean_A");
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"A1"});
    
            stmtTwo.Stop();
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            Thread.Sleep(50);
            Assert.AreEqual(0, captureOp.Current.Length);
    
            stmtTwo.Start();
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"A3"});
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            captureOp.WaitForInvocation(200, 1);
            EPAssertionUtil.AssertProps(
                epService.Container, captureOp.GetCurrentAndReset()[0], "id".Split(','), new object[]{"B2"});
    
            df.Cancel();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            Assert.AreEqual(0, captureOp.Current.Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStatementNameExists(EPServiceProvider epService, string typeName, object[] events) {
            epService.EPAdministrator.CreateEPL("@Name('MyStatement') select * from " + typeName);
    
            epService.EPAdministrator.CreateEPL("create dataflow MyDataFlowOne " +
                    "create schema AllObject System.Object," +
                    "EPStatementSource -> thedata<AllObject> {" +
                    "  statementName : 'MyStatement'," +
                    "} " +
                    "DefaultSupportCaptureOp(thedata) {}");
    
            var captureOp = new DefaultSupportCaptureOp(2, SupportContainer.Instance.LockManager());
            var options = new EPDataFlowInstantiationOptions()
                    .OperatorProvider(new DefaultSupportGraphOpProvider(captureOp));
    
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlowOne", options);
            df.Start();
    
            EventSender sender = epService.EPRuntime.GetEventSender(typeName);
            foreach (Object @event in events) {
                sender.SendEvent(@event);
            }
    
            captureOp.GetValue(1, TimeUnit.SECONDS);
            EPAssertionUtil.AssertEqualsExactOrder(events, captureOp.Current);
    
            df.Cancel();
            epService.EPAdministrator.DestroyAllStatements();
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
} // end of namespace
