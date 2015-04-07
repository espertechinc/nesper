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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestRevisionWindowed
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();

            // first revision event type
            config.AddEventType("SupportBean", typeof (SupportBean));
            config.AddEventType("FullEvent", typeof (SupportRevisionFull));
            config.AddEventType("D1", typeof (SupportDeltaOne));
            config.AddEventType("D5", typeof (SupportDeltaFive));

            var configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = (new String[] {"K0"});
            configRev.AddNameBaseEventType("FullEvent");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D5");
            config.AddRevisionEventType("RevisableQuote", configRev);

            // second revision event type
            config.AddEventType("MyMap", MakeMap(
                new Object[][]
                {
                    new Object[] {"P5", typeof (string)}, new Object[] {"P1", typeof (string)},
                    new Object[] {"K0", typeof (string)}, new Object[] {"m0", typeof (string)}
                }));
            configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = (new String[] {"P5", "P1"});
            configRev.AddNameBaseEventType("MyMap");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D5");
            config.AddRevisionEventType("RevisableMap", configRev);

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listenerOne = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listenerOne = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private EPStatement _stmtCreateWin;
        private SupportUpdateListener _listenerOne;

        private readonly String[] _fields = "K0,P1,P5".Split(',');

        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            Map result = new Dictionary<String, Object>();
            for (int i = 0; i < entries.Length; i++)
            {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }

        [Test]
        public void TestGroupLength()
        {
            _stmtCreateWin =
                _epService.EPAdministrator.CreateEPL(
                    "create window RevQuote.std:groupwin(P1).win:length(2) as select * from RevisableQuote");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");

            EPStatement consumerOne =
                _epService.EPAdministrator.CreateEPL("select irstream * from RevQuote order by K0 asc");
            consumerOne.Events += _listenerOne.Update;

            _epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "P1", "a50"));
            _epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "P1", "a51"));
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "P2", "b50"));
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "P3", "c50"));
            _epService.EPRuntime.SendEvent(new SupportDeltaFive("d", "P3", "d50"));

            _listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                                              new Object[][]
                                              {
                                                  new Object[] {"a", "P1", "a51"}, new Object[] {"b", "P2", "b50"},
                                                  new Object[] {"c", "P3", "c50"}
                                              });

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "P1", "b51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[] {"b", "P1", "b51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[] {"b", "P2", "b50"});
            _listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                                              new Object[][]
                                              {
                                                  new Object[] {"a", "P1", "a51"}, new Object[] {"b", "P1", "b51"},
                                                  new Object[] {"c", "P3", "c50"}
                                              });

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("c", "P1", "c51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[] {"c", "P1", "c51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[1], _fields, new Object[] {"c", "P3", "c50"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[] {"a", "P1", "a51"});
            _listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                                              new Object[][]
                                              {new Object[] {"b", "P1", "b51"}, new Object[] {"c", "P1", "c51"}});
        }

        [Test]
        public void TestMultiPropertyMapMixin()
        {
            String[] fields = "K0,P1,P5,m0".Split(',');
            _stmtCreateWin =
                _epService.EPAdministrator.CreateEPL("create window RevMap.win:length(3) as select * from RevisableMap");
            _epService.EPAdministrator.CreateEPL("insert into RevMap select * from MyMap");
            _epService.EPAdministrator.CreateEPL("insert into RevMap select * from D1");
            _epService.EPAdministrator.CreateEPL("insert into RevMap select * from D5");

            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from RevMap order by K0");
            consumerOne.Events += _listenerOne.Update;

            _epService.EPRuntime.SendEvent(
                MakeMap(new Object[][]
                {
                    new Object[] {"P5", "P5_1"}, new Object[] {"P1", "P1_1"}, new Object[] {"K0", "E1"},
                    new Object[] {"m0", "M0"}
                }), "MyMap");
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"E1", "P1_1", "P5_1", "M0"});

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("E2", "P1_1", "P5_1"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields,
                                        new Object[] {"E2", "P1_1", "P5_1", "M0"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields,
                                        new Object[] {"E1", "P1_1", "P5_1", "M0"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), fields, new Object[] {"E2", "P1_1", "P5_1", "M0"});
            _listenerOne.Reset();

            _epService.EPRuntime.SendEvent(
                MakeMap(new Object[][]
                {
                    new Object[] {"P5", "P5_1"}, new Object[] {"P1", "P1_2"}, new Object[] {"K0", "E3"},
                    new Object[] {"m0", "M1"}
                }), "MyMap");
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), fields,
                                              new Object[][]
                                              {
                                                  new Object[] {"E2", "P1_1", "P5_1", "M0"},
                                                  new Object[] {"E3", "P1_2", "P5_1", "M1"}
                                              });

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("E4", "P1_1", "P5_1"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields,
                                        new Object[] {"E4", "P1_1", "P5_1", "M0"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields,
                                        new Object[] {"E2", "P1_1", "P5_1", "M0"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), fields,
                                              new Object[][]
                                              {
                                                  new Object[] {"E3", "P1_2", "P5_1", "M1"},
                                                  new Object[] {"E4", "P1_1", "P5_1", "M0"}
                                              });
            _listenerOne.Reset();

            _epService.EPRuntime.SendEvent(
                MakeMap(new Object[][]
                {
                    new Object[] {"P5", "P5_2"}, new Object[] {"P1", "P1_1"}, new Object[] {"K0", "E5"},
                    new Object[] {"m0", "M2"}
                }), "MyMap");
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields,
                                        new Object[] {"E5", "P1_1", "P5_2", "M2"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), fields,
                                              new Object[][]
                                              {
                                                  new Object[] {"E3", "P1_2", "P5_1", "M1"},
                                                  new Object[] {"E4", "P1_1", "P5_1", "M0"},
                                                  new Object[] {"E5", "P1_1", "P5_2", "M2"}
                                              });

            _epService.EPRuntime.SendEvent(new SupportDeltaOne("E6", "P1_1", "P5_2"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertPairGetIRAndReset(), fields,
                                        new Object[] { "E6", "P1_1", "P5_2", "M2" },
                                        new Object[] { "E5", "P1_1", "P5_2", "M2" });
        }

        [Test]
        public void TestSubclassInterface()
        {
            _epService.EPAdministrator.Configuration.AddEventType("ISupportRevisionFull", typeof (ISupportRevisionFull));
            _epService.EPAdministrator.Configuration.AddEventType("ISupportDeltaFive", typeof (ISupportDeltaFive));

            var config = new ConfigurationRevisionEventType();
            config.AddNameBaseEventType("ISupportRevisionFull");
            config.KeyPropertyNames = (new String[] {"K0"});
            config.AddNameDeltaEventType("ISupportDeltaFive");
            _epService.EPAdministrator.Configuration.AddRevisionEventType("MyInterface", config);

            _stmtCreateWin =
                _epService.EPAdministrator.CreateEPL(
                    "create window MyInterfaceWindow.win:keepall() as select * from MyInterface");
            _epService.EPAdministrator.CreateEPL("insert into MyInterfaceWindow select * from ISupportRevisionFull");
            _epService.EPAdministrator.CreateEPL("insert into MyInterfaceWindow select * from ISupportDeltaFive");

            EPStatement consumerOne =
                _epService.EPAdministrator.CreateEPL("@Audit select irstream K0,P0,P1 from MyInterfaceWindow");
            consumerOne.Events += _listenerOne.Update;
            String[] fields = "K0,P0,P1".Split(',');
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);

            _epService.EPRuntime.SendEvent(new SupportRevisionFull(null, "00", "10", "20", "30", "40", "50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[] {null, "00", "10"});

            _epService.EPRuntime.SendEvent(new SupportDeltaFive(null, "999", null));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[] {null, "00", "999"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[] {null, "00", "10"});
            _listenerOne.Reset();

            _stmtCreateWin.Stop();
            _stmtCreateWin.Start();
            consumerOne.Stop();
            consumerOne.Start();

            _epService.EPRuntime.SendEvent(new SupportRevisionFull("zz", "xx", "yy", "20", "30", "40", "50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[] {"zz", "xx", "yy"});
        }

        [Test]
        public void TestTimeWindow()
        {
            SendTimer(0);
            _stmtCreateWin =
                _epService.EPAdministrator.CreateEPL(
                    "create window RevQuote.win:time(10 sec) as select * from RevisableQuote");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");

            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from RevQuote");
            consumerOne.Events += _listenerOne.Update;

            _epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a10", "a50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields,
                                        new Object[] {"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[] {"a", "a10", "a50"});

            SendTimer(1000);

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "a11", "a51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[] {"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[] {"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[] {"a", "a11", "a51"});

            SendTimer(2000);

            _epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b10", "b50"));
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "c10", "c50"));

            SendTimer(3000);
            _epService.EPRuntime.SendEvent(new SupportDeltaOne("c", "c11", "c51"));

            SendTimer(8000);
            _epService.EPRuntime.SendEvent(new SupportDeltaOne("c", "c12", "c52"));
            _listenerOne.Reset();

            SendTimer(10000);
            Assert.IsFalse(_listenerOne.IsInvoked);

            SendTimer(11000);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields,
                                        new Object[] {"a", "a11", "a51"});

            SendTimer(12000);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields,
                                        new Object[] {"b", "b10", "b50"});

            SendTimer(13000);
            Assert.IsFalse(_listenerOne.IsInvoked);

            SendTimer(18000);
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields,
                                        new Object[] {"c", "c12", "c52"});
        }

        [Test]
        public void TestUnique()
        {
            _stmtCreateWin =
                _epService.EPAdministrator.CreateEPL(
                    "create window RevQuote.std:unique(P1) as select * from RevisableQuote");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");

            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from RevQuote");
            consumerOne.Events += _listenerOne.Update;

            _epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a10", "a50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields,
                                        new Object[] {"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[] {"a", "a10", "a50"});

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "a11", "a51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[] {"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[] {"a", "a10", "a50"});
            _listenerOne.Reset();
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[] {"a", "a11", "a51"});

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "b10", "b50"));
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b10", "b50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields,
                                        new Object[] {"b", "b10", "b50"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                                              new Object[][]
                                              {new Object[] {"a", "a11", "a51"}, new Object[] {"b", "b10", "b50"}});

            _epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "a11", "b51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[] {"b", "a11", "b51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[] {"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[] {"b", "a11", "b51"});
        }
    }
}