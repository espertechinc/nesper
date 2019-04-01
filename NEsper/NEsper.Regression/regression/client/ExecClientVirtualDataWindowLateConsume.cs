///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.virtualdw;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientVirtualDataWindowLateConsume : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddPlugInVirtualDataWindow(
                "test", "vdw", typeof(SupportVirtualDWFactory).FullName,
                SupportVirtualDW.ITERATE); // configure with iteration
            configuration.AddEventType<SupportBean>();
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            var window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonList<object>(supportBean);
            epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            var fields = "val0".Split(',');
            var stmtAggregate =
                epService.EPAdministrator.CreateEPL("@Name('ABC') select sum(IntPrimitive) as val0 from MyVDW");
            var listener = new SupportUpdateListener();
            stmtAggregate.Events += listener.Update;
            EPAssertionUtil.AssertProps(stmtAggregate.First(), fields, new object[] {100});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {110});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {130});

            // assert events received for add-consumer and remove-consumer
            stmtAggregate.Dispose();
            var addConsumerEvent = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            var removeConsumerEvent = (VirtualDataWindowEventConsumerRemove) window.Events[1];

            var baseConsumers = new VirtualDataWindowEventConsumerBase[] {addConsumerEvent, removeConsumerEvent};
            foreach (var @base in baseConsumers)
            {
                Assert.AreEqual(-1, @base.AgentInstanceId);
                Assert.AreEqual("MyVDW", @base.NamedWindowName);
                Assert.AreEqual("ABC", @base.StatementName);
            }

            Assert.AreSame(removeConsumerEvent.ConsumerObject, addConsumerEvent.ConsumerObject);
            window.Events.Clear();

            // test filter criteria passed to event
            var stmtAggregateWFilter = epService.EPAdministrator.CreateEPL(
                "@Name('ABC') select sum(IntPrimitive) as val0 from MyVDW(TheString = 'A')");
            var eventWithFilter = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            Assert.AreEqual(1, eventWithFilter.FilterExpressions.Length);
            Assert.IsNotNull(eventWithFilter.ExprEvaluatorContext);
            stmtAggregateWFilter.Dispose();
        }

        private VirtualDataWindow GetFromContext(EPServiceProvider epService, string name)
        {
            return (VirtualDataWindow) epService.Directory.Lookup(name);
            //try
            //{
            //    return (VirtualDataWindow) epService.Directory.Lookup(name);
            //} catch (NamingException e) {
            //    throw new EPRuntimeException("Name '" + name + "' could not be looked up");
            //}
        }
    }
} // end of namespace