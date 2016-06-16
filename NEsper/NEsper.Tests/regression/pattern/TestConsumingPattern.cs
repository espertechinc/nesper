///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestConsumingPattern
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof (AEvent));
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof (BEvent));
            _epService.EPAdministrator.Configuration.AddEventType("C", typeof (CEvent));
            _epService.EPAdministrator.Configuration.AddEventType("D", typeof (DEvent));
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestInvalid()
        {
            TryInvalid(
                "select * from pattern @XX [A]",
                "Error in expression: Unrecognized pattern-level annotation 'XX' [select * from pattern @XX [A]]");

            const string expected =
                "Discard-partials and suppress-matches is not supported in a joins, context declaration and on-action ";
            TryInvalid(
                "select * from pattern " + TargetEnum.DISCARD_AND_SUPPRESS.GetText() +
                "[A].win:keepall(), A.win:keepall()",
                expected +
                "[select * from pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [A].win:keepall(), A.win:keepall()]");

            _epService.EPAdministrator.CreateEPL("create window AWindow.win:keepall() as A");
            TryInvalid(
                "on pattern " + TargetEnum.DISCARD_AND_SUPPRESS.GetText() + "[A] select * from AWindow",
                expected + "[on pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [A] select * from AWindow]");
        }

        [Test]
        public void TestCombination()
        {
            foreach (var testsoda in new bool[]{ false, true })
            {
                foreach (var target in EnumHelper.GetValues<TargetEnum>())
                {
                    RunAssertionTargetCurrentMatch(testsoda, target);
                    RunAssertionTargetNextMatch(testsoda, target);
                }
            }

            // test order-by
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern @DiscardPartialsOnMatch [every a=A -> B] order by a.id desc").Events +=
                _listener.Update;
            _epService.EPRuntime.SendEvent(new AEvent("A1", null, null));
            _epService.EPRuntime.SendEvent(new AEvent("A2", null, null));
            _epService.EPRuntime.SendEvent(new BEvent("B1", null));

            var events = _listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRow(
                events, "a.id".Split(','), new Object[][]
                {
                    new Object[] { "A2" },
                    new Object[] { "A1" }
                });
        }

        [Test]
        public void TestFollowedByOp()
        {
            RunFollowedByOp("every a1=A -> a2=A", false);
            RunFollowedByOp("every a1=A -> a2=A", true);
            RunFollowedByOp("every a1=A -[10]> a2=A", false);
            RunFollowedByOp("every a1=A -[10]> a2=A", true);
        }

        [Test]
        public void TestMatchUntilOp()
        {
            RunAssertionMatchUntilBoundOp(true);
            RunAssertionMatchUntilBoundOp(false);
            RunAssertionMatchUntilWChildMatcher(true);
            RunAssertionMatchUntilWChildMatcher(false);
            RunAssertionMatchUntilRangeOpWTime(); // with time
        }

        [Test]
        public void TestObserverOp()
        {
            var fields = "a.id,b.id".Split(',');
            SendTime(0);
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> b=B -> timer:interval(a.mysec)]").Events += _listener.Update;
            SendAEvent("A1", 5); // 5 seconds for this one
            SendAEvent("A2", 1); // 1 seconds for this one
            SendBEvent("B1");
            SendTime(1000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1"
                });

            SendTime(5000);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestAndOp()
        {
            RunAndWAndState(true);
            RunAndWAndState(false);
            RunAndWChild(true);
            RunAndWChild(false);
        }

        [Test]
        public void TestNotOpNotImpacted()
        {
            var fields = "a.id".Split(',');
            SendTime(0);
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> timer:interval(a.mysec) and not (B -> C)]").Events += _listener.Update;
            SendAEvent("A1", 5); // 5 sec
            SendAEvent("A2", 1); // 1 sec
            SendBEvent("B1");
            SendTime(1000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2"
                });

            SendCEvent("C1", null);
            SendTime(5000);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestGuardOp()
        {
            RunGuardOpBeginState(true);
            RunGuardOpBeginState(false);
            RunGuardOpChildState(true);
            RunGuardOpChildState(false);
        }

        [Test]
        public void TestOrOp()
        {
            var fields = "a.id,b.id,c.id".Split(',');
            SendTime(0);
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> (b=B -> c=C(pc=a.pa)) or timer:interval(1000)]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C1", "x");
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestEveryOp()
        {
            RunAssertionEveryBeginState("");
            RunAssertionEveryBeginState("-distinct(id)");
            RunAssertionEveryBeginState("-distinct(id, 10 seconds)");

            RunAssertionEveryChildState("", true);
            RunAssertionEveryChildState("", false);
            RunAssertionEveryChildState("-distinct(id)", true);
            RunAssertionEveryChildState("-distinct(id)", false);
            RunAssertionEveryChildState("-distinct(id, 10 seconds)", true);
            RunAssertionEveryChildState("-distinct(id, 10 seconds)", false);
        }

        private void RunAssertionEveryChildState(String everySuffix, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> every" + everySuffix + " (b=B -> c=C(pc=a.pa))]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionEveryBeginState(String distinct)
        {
            var fields = "a.id,b.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + "[" +
                "every a=A -> every" + distinct + " b=B]").Events += _listener.Update;
            SendAEvent("A1");
            SendBEvent("B1");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A1",
                    "B1"
                });

            SendBEvent("B2");
            Assert.IsFalse(_listener.IsInvoked);

            SendAEvent("A2");
            SendBEvent("B3");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B3"
                });

            SendBEvent("B4");
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void SendTime(long msec)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
        }

        private void SendAEvent(String id)
        {
            SendAEvent(id, null, null);
        }

        private void SendAEvent(String id, String pa)
        {
            SendAEvent(id, pa, null);
        }

        private void SendDEvent(String id)
        {
            _epService.EPRuntime.SendEvent(new DEvent(id));
        }

        private void SendAEvent(String id, int mysec)
        {
            SendAEvent(id, null, mysec);
        }

        private void SendAEvent(String id, String pa, int? mysec)
        {
            _epService.EPRuntime.SendEvent(new AEvent(id, pa, mysec));
        }

        private void SendBEvent(String id)
        {
            SendBEvent(id, null);
        }

        private void SendBEvent(String id, String pb)
        {
            _epService.EPRuntime.SendEvent(new BEvent(id, pb));
        }

        private void SendCEvent(String id, String pc)
        {
            _epService.EPRuntime.SendEvent(new CEvent(id, pc));
        }

        private void RunFollowedByOp(String pattern, bool matchDiscard)
        {
            var fields = "a1.id,a2.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern "
                + (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" + pattern + "]").Events += _listener.Update;

            SendAEvent("E1");
            SendAEvent("E2");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "E1",
                    "E2"
                });

            SendAEvent("E3");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "E2",
                        "E3"
                    });
            }
            SendAEvent("E4");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "E3",
                    "E4"
                });

            SendAEvent("E5");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "E4",
                        "E5"
                    });
            }
            SendAEvent("E6");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "E5",
                    "E6"
                });

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionTargetNextMatch(bool testSoda, TargetEnum target)
        {

            var fields = "a.id,b.id,c.id".Split(',');
            var epl = "select * from pattern " + target.GetText() + "[every a=A -> b=B -> c=C(pc=a.pa)]";
            if (testSoda)
            {
                var model = _epService.EPAdministrator.CompileEPL(epl);
                Assert.AreEqual(epl, model.ToEPL());
                _epService.EPAdministrator.Create(model).Events += _listener.Update;
            }
            else
            {
                _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;
            }

            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.NONE)
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            else
            {
                Assert.IsFalse(_listener.IsInvoked);
            }

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionMatchUntilBoundOp(bool matchDiscard)
        {
            var fields = "a.id,b[0].id,b[1].id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" +
                "every a=A -> [2] b=B(pb in (a.pa, '-'))]").Events += _listener.Update;

            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1", "-"); // applies to both matches
            SendBEvent("B2", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "B2"
                });

            SendBEvent("B3", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "B3"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionMatchUntilWChildMatcher(bool matchDiscard)
        {
            var fields = "a.id,b[0].id,c[0].id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> [1] (b=B -> c=C(pc=a.pa))]").Events += _listener.Update;

            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionMatchUntilRangeOpWTime()
        {
            var fields = "a1.id,aarr[0].id".Split(',');
            SendTime(0);
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + "[" +
                "every a1=A -> ([:100] aarr=A until (timer:interval(10 sec) and not b=B))]").Events += _listener.Update;

            SendAEvent("A1");
            SendTime(1000);
            SendAEvent("A2");
            SendTime(10000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A1",
                    "A2"
                });

            SendTime(11000);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionTargetCurrentMatch(bool testSoda, TargetEnum target)
        {
            var listener = new SupportUpdateListener();
            var fields = "a1.id,aarr[0].id,b.id".Split(',');
            var epl = "select * from pattern " + target.GetText() + "[every a1=A -> [:10] aarr=A until b=B]";
            if (testSoda)
            {
                var model = _epService.EPAdministrator.CompileEPL(epl);
                Assert.AreEqual(epl, model.ToEPL());
                _epService.EPAdministrator.Create(model).Events += listener.Update;
            }
            else
            {
                _epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            }

            SendAEvent("A1");
            SendAEvent("A2");
            SendBEvent("B1");

            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.DISCARD_AND_SUPPRESS)
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "A2",
                        "B1"
                    });
            }
            else
            {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetLastNewData(), fields,
                    new Object[][]
                    {
                        new Object[]
                        {
                            "A1",
                            "A2",
                            "B1"
                        },
                        new Object[]
                        {
                            "A2",
                            null,
                            "B1"
                        }
                    });
            }

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAndWAndState(bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> b=B and c=C(pc=a.pa)]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAndWChild(bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> D and (b=B -> c=C(pc=a.pa))]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendDEvent("D1");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunGuardOpBeginState(bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" +
                "every a=A -> b=B -> c=C(pc=a.pa) where timer:within(1)]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunGuardOpChildState(bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            _epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> (b=B -> c=C(pc=a.pa)) where timer:within(1)]").Events += _listener.Update;
            SendAEvent("A1", "x");
            SendAEvent("A2", "y");
            SendBEvent("B1");
            SendCEvent("C1", "y");
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields, new Object[]
                {
                    "A2",
                    "B1",
                    "C1"
                });

            SendCEvent("C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(_listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    _listener.AssertOneGetNewAndReset(), fields, new Object[]
                    {
                        "A1",
                        "B1",
                        "C2"
                    });
            }
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch(EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
            try
            {
                _epService.EPAdministrator.Create(_epService.EPAdministrator.CompileEPL(epl));
                Assert.Fail();
            }
            catch(EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        [Serializable]
        public sealed class AEvent
        {
            public AEvent(String id, String pa, int? mysec)
            {
                Id = id;
                Pa = pa;
                Mysec = mysec;
            }

            public string Id { get; private set; }

            public string Pa { get; private set; }

            public int? Mysec { get; private set; }
        }

        [Serializable]
        public sealed class BEvent
        {
            public BEvent(String id, String pb)
            {
                Id = id;
                Pb = pb;
            }

            public string Id { get; private set; }

            public string Pb { get; private set; }
        }

        [Serializable]
        public sealed class CEvent
        {
            public CEvent(String id, String pc)
            {
                Id = id;
                Pc = pc;
            }

            public string Id { get; private set; }

            public string Pc { get; private set; }
        }

        [Serializable]
        public sealed class DEvent
        {
            public DEvent(String id)
            {
                Id = id;
            }

            public string Id { get; private set; }
        }
    }

    public enum TargetEnum
    {
        DISCARD_ONLY,
        DISCARD_AND_SUPPRESS,
        SUPPRESS_ONLY,
        NONE
    }

    public static class TargetEnumExtensions
    {
        public static string GetText(this TargetEnum value)
        {
            switch (value)
            {
                case TargetEnum.DISCARD_ONLY:
                    return ("@DiscardPartialsOnMatch ");
                case TargetEnum.DISCARD_AND_SUPPRESS:
                    return ("@DiscardPartialsOnMatch @SuppressOverlappingMatches ");
                case TargetEnum.SUPPRESS_ONLY:
                    return ("@SuppressOverlappingMatches ");
                case TargetEnum.NONE:
                    return ("");
            }

            throw new ArgumentException();
        }
    }
}
