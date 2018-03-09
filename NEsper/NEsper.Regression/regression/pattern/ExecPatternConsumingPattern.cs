///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternConsumingPattern : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(AEvent));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(BEvent));
            epService.EPAdministrator.Configuration.AddEventType("C", typeof(CEvent));
            epService.EPAdministrator.Configuration.AddEventType("D", typeof(DEvent));

            RunAssertionInvalid(epService);
            RunAssertionCombination(epService);
            RunAssertionFollowedByOp(epService);
            RunAssertionMatchUntilOp(epService);
            RunAssertionObserverOp(epService);
            RunAssertionAndOp(epService);
            RunAssertionNotOpNotImpacted(epService);
            RunAssertionGuardOp(epService);
            RunAssertionOrOp(epService);
            RunAssertionEveryOp(epService);
        }

        private void RunAssertionInvalid(EPServiceProvider epService)
        {
            TryInvalid(
                epService, "select * from pattern @XX [A]",
                "Error in expression: Unrecognized pattern-level annotation 'XX' [select * from pattern @XX [A]]");

            var expected =
                "Discard-partials and suppress-matches is not supported in a joins, context declaration and on-action ";
            TryInvalid(
                epService, "select * from pattern " + TargetEnum.DISCARD_AND_SUPPRESS.GetText() + "[A]#keepall, A#keepall",
                expected +
                "[select * from pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [A]#keepall, A#keepall]");

            epService.EPAdministrator.CreateEPL("create window AWindow#keepall as A");
            TryInvalid(
                epService, "on pattern " + TargetEnum.DISCARD_AND_SUPPRESS.GetText() + "[A] select * from AWindow",
                expected +
                "[on pattern @DiscardPartialsOnMatch @SuppressOverlappingMatches [A] select * from AWindow]");
        }

        private void RunAssertionCombination(EPServiceProvider epService)
        {
            foreach (var testsoda in new bool[] {false, true})
            {
                foreach (var target in EnumHelper.GetValues<TargetEnum>())
                {
                    TryAssertionTargetCurrentMatch(epService, testsoda, target);
                    TryAssertionTargetNextMatch(epService, testsoda, target);
                }
            }

            // test order-by
            var listener = new SupportUpdateListener();
            epService.EPAdministrator
                .CreateEPL("select * from pattern @DiscardPartialsOnMatch [every a=A -> B] order by a.id desc")
                .Events += listener.Update;
            epService.EPRuntime.SendEvent(new AEvent("A1", null, null));
            epService.EPRuntime.SendEvent(new AEvent("A2", null, null));
            epService.EPRuntime.SendEvent(new BEvent("B1", null));
            var events = listener.GetAndResetLastNewData();
            EPAssertionUtil.AssertPropsPerRow(
                events, "a.id".Split(','), new object[][] {new object[] {"A2"}, new object[] {"A1"}});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionFollowedByOp(EPServiceProvider epService)
        {
            RunFollowedByOp(epService, "every a1=A -> a2=A", false);
            RunFollowedByOp(epService, "every a1=A -> a2=A", true);
            RunFollowedByOp(epService, "every a1=A -[10]> a2=A", false);
            RunFollowedByOp(epService, "every a1=A -[10]> a2=A", true);
        }

        private void RunAssertionMatchUntilOp(EPServiceProvider epService)
        {
            TryAssertionMatchUntilBoundOp(epService, true);
            TryAssertionMatchUntilBoundOp(epService, false);
            TryAssertionMatchUntilWChildMatcher(epService, true);
            TryAssertionMatchUntilWChildMatcher(epService, false);
            TryAssertionMatchUntilRangeOpWTime(epService); // with time
        }

        private void RunAssertionObserverOp(EPServiceProvider epService)
        {
            var fields = "a.id,b.id".Split(',');
            SendTime(epService, 0);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> b=B -> timer:interval(a.mysec)]").Events += listener.Update;
            SendAEvent(epService, "A1", 5); // 5 seconds for this one
            SendAEvent(epService, "A2", 1); // 1 seconds for this one
            SendBEvent(epService, "B1");
            SendTime(epService, 1000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1"});

            SendTime(epService, 5000);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionAndOp(EPServiceProvider epService)
        {
            RunAndWAndState(epService, true);
            RunAndWAndState(epService, false);
            RunAndWChild(epService, true);
            RunAndWChild(epService, false);
        }

        private void RunAssertionNotOpNotImpacted(EPServiceProvider epService)
        {
            var fields = "a.id".Split(',');
            SendTime(epService, 0);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> timer:interval(a.mysec) and not (B -> C)]").Events += listener.Update;
            SendAEvent(epService, "A1", 5); // 5 sec
            SendAEvent(epService, "A2", 1); // 1 sec
            SendBEvent(epService, "B1");
            SendTime(epService, 1000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2"});

            SendCEvent(epService, "C1", null);
            SendTime(epService, 5000);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionGuardOp(EPServiceProvider epService)
        {
            RunGuardOpBeginState(epService, true);
            RunGuardOpBeginState(epService, false);
            RunGuardOpChildState(epService, true);
            RunGuardOpChildState(epService, false);
        }

        private void RunAssertionOrOp(EPServiceProvider epService)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            SendTime(epService, 0);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + " [" +
                "every a=A -> (b=B -> c=C(pc=a.pa)) or timer:interval(1000)]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C1", "x");
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionEveryOp(EPServiceProvider epService)
        {
            TryAssertionEveryBeginState(epService, "");
            TryAssertionEveryBeginState(epService, "-distinct(id)");
            TryAssertionEveryBeginState(epService, "-distinct(id, 10 seconds)");

            TryAssertionEveryChildState(epService, "", true);
            TryAssertionEveryChildState(epService, "", false);
            TryAssertionEveryChildState(epService, "-distinct(id)", true);
            TryAssertionEveryChildState(epService, "-distinct(id)", false);
            TryAssertionEveryChildState(epService, "-distinct(id, 10 seconds)", true);
            TryAssertionEveryChildState(epService, "-distinct(id, 10 seconds)", false);
        }

        private void TryAssertionEveryChildState(EPServiceProvider epService, string everySuffix, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> every" + everySuffix + " (b=B -> c=C(pc=a.pa))]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionEveryBeginState(EPServiceProvider epService, string distinct)
        {
            var fields = "a.id,b.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + "[" +
                "every a=A -> every" + distinct + " b=B]").Events += listener.Update;
            SendAEvent(epService, "A1");
            SendBEvent(epService, "B1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1"});

            SendBEvent(epService, "B2");
            Assert.IsFalse(listener.IsInvoked);

            SendAEvent(epService, "A2");
            SendBEvent(epService, "B3");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B3"});

            SendBEvent(epService, "B4");
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunFollowedByOp(EPServiceProvider epService, string pattern, bool matchDiscard)
        {
            var fields = "a1.id,a2.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern "
                + (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" + pattern + "]").Events += listener.Update;

            SendAEvent(epService, "E1");
            SendAEvent(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", "E2"});

            SendAEvent(epService, "E3");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", "E3"});
            }

            SendAEvent(epService, "E4");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E3", "E4"});

            SendAEvent(epService, "E5");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E4", "E5"});
            }

            SendAEvent(epService, "E6");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E5", "E6"});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionTargetNextMatch(EPServiceProvider epService, bool testSoda, TargetEnum target)
        {

            var fields = "a.id,b.id,c.id".Split(',');
            var epl = "select * from pattern " + target.GetText() + "[every a=A -> b=B -> c=C(pc=a.pa)]";
            var listener = new SupportUpdateListener();
            if (testSoda)
            {
                var model = epService.EPAdministrator.CompileEPL(epl);
                Assert.AreEqual(epl, model.ToEPL());
                epService.EPAdministrator.Create(model).Events += listener.Update;
            }
            else
            {
                epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            }

            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.NONE)
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }
            else
            {
                Assert.IsFalse(listener.IsInvoked);
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionMatchUntilBoundOp(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b[0].id,b[1].id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" +
                "every a=A -> [2] b=B(pb in (a.pa, '-'))]").Events += listener.Update;

            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1", "-"); // applies to both matches
            SendBEvent(epService, "B2", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "B2"});

            SendBEvent(epService, "B3", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "B3"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionMatchUntilWChildMatcher(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b[0].id,c[0].id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> [1] (b=B -> c=C(pc=a.pa))]").Events += listener.Update;

            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionMatchUntilRangeOpWTime(EPServiceProvider epService)
        {
            var fields = "a1.id,aarr[0].id".Split(',');
            SendTime(epService, 0);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " + TargetEnum.DISCARD_ONLY.GetText() + "[" +
                "every a1=A -> ([:100] aarr=A until (timer:interval(10 sec) and not b=B))]").Events += listener.Update;

            SendAEvent(epService, "A1");
            SendTime(epService, 1000);
            SendAEvent(epService, "A2");
            SendTime(epService, 10000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "A2"});

            SendTime(epService, 11000);
            Assert.IsFalse(listener.IsInvoked);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryAssertionTargetCurrentMatch(EPServiceProvider epService, bool testSoda, TargetEnum target)
        {

            var listener = new SupportUpdateListener();
            var fields = "a1.id,aarr[0].id,b.id".Split(',');
            var epl = "select * from pattern " + target.GetText() + "[every a1=A -> [:10] aarr=A until b=B]";
            if (testSoda)
            {
                var model = epService.EPAdministrator.CompileEPL(epl);
                Assert.AreEqual(epl, model.ToEPL());
                epService.EPAdministrator.Create(model).Events += listener.Update;
            }
            else
            {
                epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            }

            SendAEvent(epService, "A1");
            SendAEvent(epService, "A2");
            SendBEvent(epService, "B1");

            if (target == TargetEnum.SUPPRESS_ONLY || target == TargetEnum.DISCARD_AND_SUPPRESS)
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "A2", "B1"});
            }
            else
            {
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    listener.GetAndResetLastNewData(), fields,
                    new object[][] {new object[] {"A1", "A2", "B1"}, new object[] {"A2", null, "B1"}});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAndWAndState(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> b=B and c=C(pc=a.pa)]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAndWChild(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> D and (b=B -> c=C(pc=a.pa))]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendDEvent(epService, "D1");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunGuardOpBeginState(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + "[" +
                "every a=A -> b=B -> c=C(pc=a.pa) where timer:within(1)]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunGuardOpChildState(EPServiceProvider epService, bool matchDiscard)
        {
            var fields = "a.id,b.id,c.id".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                "select * from pattern " +
                (matchDiscard ? TargetEnum.DISCARD_ONLY.GetText() : "") + " [" +
                "every a=A -> (b=B -> c=C(pc=a.pa)) where timer:within(1)]").Events += listener.Update;
            SendAEvent(epService, "A1", "x");
            SendAEvent(epService, "A2", "y");
            SendBEvent(epService, "B1");
            SendCEvent(epService, "C1", "y");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"A2", "B1", "C1"});

            SendCEvent(epService, "C2", "x");
            if (matchDiscard)
            {
                Assert.IsFalse(listener.IsInvoked);
            }
            else
            {
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(), fields, new object[] {"A1", "B1", "C2"});
            }

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryInvalid(EPServiceProvider epService, string epl, string message)
        {
            try
            {
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }

            try
            {
                epService.EPAdministrator.Create(epService.EPAdministrator.CompileEPL(epl));
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void SendTime(EPServiceProvider epService, long msec)
        {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
        }

        private void SendAEvent(EPServiceProvider epService, string id)
        {
            SendAEvent(id, null, null, epService);
        }

        private void SendAEvent(EPServiceProvider epService, string id, string pa)
        {
            SendAEvent(id, pa, null, epService);
        }

        private void SendDEvent(EPServiceProvider epService, string id)
        {
            epService.EPRuntime.SendEvent(new DEvent(id));
        }

        private void SendAEvent(EPServiceProvider epService, string id, int mysec)
        {
            SendAEvent(id, null, mysec, epService);
        }

        private void SendAEvent(string id, string pa, int? mysec, EPServiceProvider epService)
        {
            epService.EPRuntime.SendEvent(new AEvent(id, pa, mysec));
        }

        private void SendBEvent(EPServiceProvider epService, string id)
        {
            SendBEvent(epService, id, null);
        }

        private void SendBEvent(EPServiceProvider epService, string id, string pb)
        {
            epService.EPRuntime.SendEvent(new BEvent(id, pb));
        }

        private void SendCEvent(EPServiceProvider epService, string id, string pc)
        {
            epService.EPRuntime.SendEvent(new CEvent(id, pc));
        }

        [Serializable]
        public class AEvent
        {
            private readonly string id;
            private readonly string pa;
            private readonly int? mysec;

            public AEvent(string id, string pa, int? mysec)
            {
                this.id = id;
                this.pa = pa;
                this.mysec = mysec;
            }

            public string Id => id;
            public string Pa => pa;
            public int? Mysec => mysec;
        }

        [Serializable]
        public class BEvent
        {
            public BEvent(string id, string pb)
            {
                this.Id = id;
                this.Pb = pb;
            }

            public string Id { get; }
            public string Pb { get; }
        }

        [Serializable]
        public class CEvent
        {
            public CEvent(string id, string pc)
            {
                this.Id = id;
                this.Pc = pc;
            }

            public string Id { get; }
            public string Pc { get; }
        }

        [Serializable]
        public class DEvent
        {
            public DEvent(string id)
            {
                this.Id = id;
            }

            public string Id { get; }
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
} // end of namespace
