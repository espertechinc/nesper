///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinNoWhereClause : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Threading.IsListenerDispatchPreserveOrder = false;
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionJoinWInnerKeywordWOOnClause(epService);
            RunAssertionJoinNoWhereClause(epService);
        }

        private void RunAssertionJoinWInnerKeywordWOOnClause(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            var fields = "a.TheString,b.TheString".Split(',');
            var epl =
                "select * from SupportBean(TheString like 'A%')#length(3) as a inner join SupportBean(TheString like 'B%')#length(3) as b " +
                "where a.IntPrimitive = b.IntPrimitive";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            SendEvent(epService, "A1", 1);
            SendEvent(epService, "A2", 2);
            SendEvent(epService, "A3", 3);
            SendEvent(epService, "B2", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B2"});

            stmt.Dispose();
        }

        private void RunAssertionJoinNoWhereClause(EPServiceProvider epService)
        {
            var fields = new[] {"stream_0.volume", "stream_1.LongBoxed"};
            var joinStatement = "select * from " +
                                typeof(SupportMarketDataBean).FullName + "#length(3)," +
                                typeof(SupportBean).FullName + "()#length(3)";

            var stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var setOne = new object[5];
            var setTwo = new object[5];
            for (var i = 0; i < setOne.Length; i++)
            {
                setOne[i] = new SupportMarketDataBean("IBM", 0, i, "");

                var theEvent = new SupportBean();
                theEvent.LongBoxed = i;
                setTwo[i] = theEvent;
            }

            // Send 2 events, should join on second one
            SendEvent(epService, setOne[0]);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendEvent(epService, setTwo[0]);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(setOne[0], listener.LastNewData[0].Get("stream_0"));
            Assert.AreEqual(setTwo[0], listener.LastNewData[0].Get("stream_1"));
            listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new[] {new object[] {0L, 0L}});

            SendEvent(epService, setOne[1]);
            SendEvent(epService, setOne[2]);
            SendEvent(epService, setTwo[1]);
            Assert.AreEqual(3, listener.LastNewData.Length);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new[]
                {
                    new object[] {0L, 0L},
                    new object[] {1L, 0L},
                    new object[] {2L, 0L},
                    new object[] {0L, 1L},
                    new object[] {1L, 1L},
                    new object[] {2L, 1L}
                });

            stmt.Dispose();
        }

        private void SendEvent(EPServiceProvider epService, string theString, int intPrimitive)
        {
            SendEvent(epService, new SupportBean(theString, intPrimitive));
        }

        private void SendEvent(EPServiceProvider epService, object theEvent)
        {
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace