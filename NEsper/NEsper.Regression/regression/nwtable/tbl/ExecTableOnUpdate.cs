///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableOnUpdate : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            foreach (var clazz in new[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(MyUpdateEvent)})
            {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }

            var listenerUpdate = new SupportUpdateListener();

            var fields = "keyOne,keyTwo,p0".Split(',');
            epService.EPAdministrator.CreateEPL(
                "create table varagg as (" +
                "keyOne string primary key, keyTwo int primary key, p0 long)");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean merge varagg where TheString = keyOne and " +
                "IntPrimitive = keyTwo when not matched then insert select TheString as keyOne, IntPrimitive as keyTwo, 1 as p0");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select varagg[p00, id].p0 as value from SupportBean_S0")
                .Events += listener.Update;
            var stmtUpdate = epService.EPAdministrator.CreateEPL(
                "on MyUpdateEvent update varagg set p0 = newValue " +
                "where k1 = keyOne and k2 = keyTwo");
            stmtUpdate.Events += listenerUpdate.Update;

            var expectedType = new[]
            {
                new object[] {"keyOne", typeof(string)},
                new object[] {"keyTwo", typeof(int)},
                new object[] {"p0", typeof(long)}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, stmtUpdate.EventType, SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            AssertValues(epService, listener, new[] {new object[] {"G1", 10}}, new[] {1L});

            epService.EPRuntime.SendEvent(new MyUpdateEvent("G1", 10, 2));
            AssertValues(epService, listener, new[] {new object[] {"G1", 10}}, new[] {2L});
            EPAssertionUtil.AssertProps(listenerUpdate.LastNewData[0], fields, new object[] {"G1", 10, 2L});
            EPAssertionUtil.AssertProps(
                listenerUpdate.GetAndResetLastOldData()[0], fields, new object[] {"G1", 10, 1L});

            // try property method invocation
            epService.EPAdministrator.CreateEPL("create table MyTableSuppBean as (sb SupportBean)");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 update MyTableSuppBean sb set sb.set_LongPrimitive(10)");
        }

        private void AssertValues(
            EPServiceProvider epService, SupportUpdateListener listener, object[][] keys, long[] values)
        {
            Assert.AreEqual(keys.Length, values.Length);
            for (var i = 0; i < keys.Length; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean_S0(
                    (int) keys[i][1], (string) keys[i][0]));
                var @event = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"),
                    "Failed for key '" + CompatExtensions.Render(keys[i]) + "'");
            }
        }

        public class MyUpdateEvent
        {
            public MyUpdateEvent(string k1, int k2, int newValue)
            {
                K1 = k1;
                K2 = k2;
                NewValue = newValue;
            }

            [PropertyName("k1")]
            public string K1 { get; }
            [PropertyName("k2")]
            public int K2 { get; }
            [PropertyName("newValue")]
            public int NewValue { get; }
        }
    }
} // end of namespace