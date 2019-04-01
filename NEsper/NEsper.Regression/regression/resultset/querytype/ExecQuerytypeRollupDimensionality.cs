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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRollupDimensionality : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionOutputWhenTerminated(epService);
            RunAssertionGroupByWithComputation(epService);
            RunAssertionContextPartitionAlsoRollup(epService);
            RunAssertionOnSelect(epService);
            RunAssertionUnboundRollupUnenclosed(epService);
            RunAssertionUnboundCubeUnenclosed(epService);
            RunAssertionUnboundGroupingSet2LevelUnenclosed(epService);
            RunAssertionBoundCube3Dim(epService);
            RunAssertionBoundGroupingSet2LevelNoTopNoDetail(epService);
            RunAssertionBoundGroupingSet2LevelTopAndDetail(epService);
            RunAssertionUnboundCube4Dim(epService);
            RunAssertionNamedWindowCube2Dim(epService);
            RunAssertionNamedWindowDeleteAndRStream2Dim(epService);
            RunAssertionBoundRollup2Dim(epService);
            RunAssertionUnboundRollup2Dim(epService);
            RunAssertionUnboundRollup1Dim(epService);
            RunAssertionUnboundRollup2DimBatchWindow(epService);
            RunAssertionUnboundRollup3Dim(epService);
            RunAssertionMixedAccessAggregation(epService);
            RunAssertionNonBoxedTypeWithRollup(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionOutputWhenTerminated(EPServiceProvider epService) {
            TryAssertionOutputWhenTerminated(epService, "last", false);
            TryAssertionOutputWhenTerminated(epService, "last", true);
            TryAssertionOutputWhenTerminated(epService, "all", false);
            TryAssertionOutputWhenTerminated(epService, "snapshot", false);
        }
    
        private void TryAssertionOutputWhenTerminated(EPServiceProvider epService, string outputLimit, bool hinted) {
            string hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            epService.EPAdministrator.CreateEPL("@Name('s0') create context MyContext start SupportBean_S0(id=1) end SupportBean_S0(id=0)");
            epService.EPAdministrator.CreateEPL(hint + "@Name('s1') context MyContext select TheString as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean group by Rollup(TheString) output " + outputLimit + " when terminated");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("s1").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new[] {new object[] {"E1", 4}, new object[] {"E2", 2}, new object[] {null, 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.GetAndResetLastNewData(), "c0,c1".Split(','),
                    new[] {new object[] {"E2", 4}, new object[] {"E1", 11}, new object[] {null, 15}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupByWithComputation(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select LongPrimitive as c0, sum(IntPrimitive) as c1 " +
                    "from SupportBean group by Rollup(case when LongPrimitive > 0 then 1 else 0 end)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c0").GetBoxedType());
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {10L, 1}, new object[] {null, 1}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 11));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {11L, 3}, new object[] {null, 3}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E3", 5, -10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {-10L, 5}, new object[] {null, 8}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E4", 6, -11));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {-11L, 11}, new object[] {null, 14}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E5", 3, 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {12L, 6}, new object[] {null, 17}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionContextPartitionAlsoRollup(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("context SegmentedByString select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean group by Rollup(TheString, IntPrimitive)").Events += listener.Update;
            string[] fields = "c0,c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 2, 20L}, new object[] {"E1", null, 30L}, new object[] {null, null, 30L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 25));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 1, 25L}, new object[] {"E2", null, 25L}, new object[] {null, null, 25L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOnSelect(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as s0 select mw.TheString as c0, sum(mw.IntPrimitive) as c1, count(*) as c2 from MyWindow mw group by Rollup(mw.TheString)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1,c2".Split(',');
    
            // {E0, 0}, {E1, 1}, {E2, 2}, {E0, 3}, {E1, 4}, {E2, 5}, {E0, 6}, {E1, 7}, {E2, 8}, {E0, 9}
            for (int i = 0; i < 10; i++) {
                string theString = "E" + i % 3;
                epService.EPRuntime.SendEvent(new SupportBean(theString, i));
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E0", 18, 4L}, new object[] {"E1", 12, 3L}, new object[] {"E2", 15, 3L}, new object[] {null, 18 + 12 + 15, 10L}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E0", 18, 4L}, new object[] {"E1", 12 + 6, 4L}, new object[] {"E2", 15, 3L}, new object[] {null, 18 + 12 + 15 + 6, 11L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnboundRollupUnenclosed(EPServiceProvider epService) {
            TryAssertionUnboundRollupUnenclosed(epService, "TheString, Rollup(IntPrimitive, LongPrimitive)");
            TryAssertionUnboundRollupUnenclosed(epService, "grouping Sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "TheString)");
            TryAssertionUnboundRollupUnenclosed(epService, "TheString, grouping Sets(" +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive), ())");
        }
    
        private void RunAssertionUnboundCubeUnenclosed(EPServiceProvider epService) {
            TryAssertionUnboundCubeUnenclosed(epService, "TheString, Cube(IntPrimitive, LongPrimitive)");
            TryAssertionUnboundCubeUnenclosed(epService, "grouping Sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "(TheString, LongPrimitive)," +
                    "TheString)");
            TryAssertionUnboundCubeUnenclosed(epService, "TheString, grouping Sets(" +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive)," +
                    "(LongPrimitive)," +
                    "())");
        }
    
        private void TryAssertionUnboundCubeUnenclosed(EPServiceProvider epService, string groupBy) {
    
            string[] fields = "c0,c1,c2,c3".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                    "group by " + groupBy).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, 100L, 1000d}, new object[] {"E1", null, null, 1000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d}, new object[] {"E1", null, 200L, 2000d}, new object[] {"E1", null, null, 3000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 20, 100L, 4000d}, new object[] {"E1", 20, null, 4000d}, new object[] {"E1", null, 100L, 5000d}, new object[] {"E1", null, null, 7000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100, 5000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 10, 100L, 5000d}, new object[] {"E2", 10, null, 5000d}, new object[] {"E2", null, 100L, 5000d}, new object[] {"E2", null, null, 5000d}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionUnboundRollupUnenclosed(EPServiceProvider epService, string groupBy) {
    
            string[] fields = "c0,c1,c2,c3".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                    "group by " + groupBy);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1").GetBoxedType());
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2").GetBoxedType());
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, null, 1000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d}, new object[] {"E1", null, null, 3000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 20, 100L, 3000d}, new object[] {"E1", 20, null, 3000d}, new object[] {"E1", null, null, 6000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 100L, 5000d}, new object[] {"E1", 10, null, 7000d}, new object[] {"E1", null, null, 10000d}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnboundGroupingSet2LevelUnenclosed(EPServiceProvider epService) {
            TryAssertionUnboundGroupingSet2LevelUnenclosed(epService, "TheString, grouping Sets(IntPrimitive, LongPrimitive)");
            TryAssertionUnboundGroupingSet2LevelUnenclosed(epService, "grouping Sets((TheString, IntPrimitive), (TheString, LongPrimitive))");
        }
    
        private void TryAssertionUnboundGroupingSet2LevelUnenclosed(EPServiceProvider epService, string groupBy) {
    
            string[] fields = "c0,c1,c2,c3".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                    "group by " + groupBy);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1").GetBoxedType());
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2").GetBoxedType());
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, 100L, 1000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 200, 2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 20, null, 2000d}, new object[] {"E1", null, 200L, 2000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 3000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, null, 4000d}, new object[] {"E1", null, 200L, 5000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 20, null, 6000d}, new object[] {"E1", null, 100L, 5000d}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionBoundGroupingSet2LevelNoTopNoDetail(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length(4) " +
                    "group by grouping Sets(TheString, IntPrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", null, 100L}, new object[] {null, 10, 100L}},
                    new[] {new object[] {"E1", null, null}, new object[] {null, 10, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", null, 200L}, new object[] {null, 20, 200L}},
                    new[] {new object[] {"E2", null, null}, new object[] {null, 20, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", null, 400L}, new object[] {null, 20, 500L}},
                    new[] {new object[] {"E1", null, 100L}, new object[] {null, 20, 200L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", null, 600L}, new object[] {null, 10, 500L}},
                    new[] {new object[] {"E2", null, 200L}, new object[] {null, 10, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 500));   // removes E1/10/100
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", null, 1100L}, new object[] {"E1", null, 300L}, new object[] {null, 20, 1000L}, new object[] {null, 10, 400L}},
                    new[] {new object[] {"E2", null, 600L}, new object[] {"E1", null, 400L}, new object[] {null, 20, 500L}, new object[] {null, 10, 500L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionBoundGroupingSet2LevelTopAndDetail(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length(4) " +
                    "group by grouping Sets((), (TheString, IntPrimitive))");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}},
                    new[] {new object[] {null, null, null}, new object[] {"E1", 10, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {null, null, 300L}, new object[] {"E1", 10, 300L}},
                    new[] {new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {null, null, 600L}, new object[] {"E2", 20, 300L}},
                    new[] {new object[] {null, null, 300L}, new object[] {"E2", 20, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {null, null, 1000L}, new object[] {"E1", 10, 700L}},
                    new[] {new object[] {null, null, 600L}, new object[] {"E1", 10, 300L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnboundCube4Dim(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2,c3,c4".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, DoublePrimitive as c3, sum(IntBoxed) as c4 from SupportBean " +
                    "group by Cube(TheString, IntPrimitive, LongPrimitive, DoublePrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1").GetBoxedType());
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("c3").GetBoxedType());
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100, 1000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 1, 10L, 100d, 1000},  // {0, 1, 2, 3}
                        new object[]{"E1", 1, 10L, null, 1000},  // {0, 1, 2}
                        new object[]{"E1", 1, null, 100d, 1000},  // {0, 1, 3}
                        new object[]{"E1", 1, null, null, 1000},  // {0, 1}
                        new object[]{"E1", null, 10L, 100d, 1000},  // {0, 2, 3}
                        new object[]{"E1", null, 10L, null, 1000},  // {0, 2}
                        new object[]{"E1", null, null, 100d, 1000},  // {0, 3}
                        new object[]{"E1", null, null, null, 1000},  // {0}
                        new object[]{null, 1, 10L, 100d, 1000},  // {1, 2, 3}
                        new object[]{null, 1, 10L, null, 1000},  // {1, 2}
                        new object[]{null, 1, null, 100d, 1000},  // {1, 3}
                        new object[]{null, 1, null, null, 1000},  // {1}
                        new object[]{null, null, 10L, 100d, 1000},  // {2, 3}
                        new object[]{null, null, 10L, null, 1000},  // {2}
                        new object[]{null, null, null, 100d, 1000},  // {3}
                        new object[]{null, null, null, null, 1000}   // {}
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 20, 100, 2000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 1, 20L, 100d, 2000},  // {0, 1, 2, 3}
                        new object[]{"E2", 1, 20L, null, 2000},  // {0, 1, 2}
                        new object[]{"E2", 1, null, 100d, 2000},  // {0, 1, 3}
                        new object[]{"E2", 1, null, null, 2000},  // {0, 1}
                        new object[]{"E2", null, 20L, 100d, 2000},  // {0, 2, 3}
                        new object[]{"E2", null, 20L, null, 2000},  // {0, 2}
                        new object[]{"E2", null, null, 100d, 2000},  // {0, 3}
                        new object[]{"E2", null, null, null, 2000},  // {0}
                        new object[]{null, 1, 20L, 100d, 2000},  // {1, 2, 3}
                        new object[]{null, 1, 20L, null, 2000},  // {1, 2}
                        new object[]{null, 1, null, 100d, 3000},  // {1, 3}
                        new object[]{null, 1, null, null, 3000},  // {1}
                        new object[]{null, null, 20L, 100d, 2000},  // {2, 3}
                        new object[]{null, null, 20L, null, 2000},  // {2}
                        new object[]{null, null, null, 100d, 3000},  // {3}
                        new object[]{null, null, null, null, 3000}   // {}
                    });
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 100, 4000));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 2, 10L, 100d, 4000},  // {0, 1, 2, 3}
                        new object[]{"E1", 2, 10L, null, 4000},  // {0, 1, 2}
                        new object[]{"E1", 2, null, 100d, 4000},  // {0, 1, 3}
                        new object[]{"E1", 2, null, null, 4000},  // {0, 1}
                        new object[]{"E1", null, 10L, 100d, 5000},  // {0, 2, 3}
                        new object[]{"E1", null, 10L, null, 5000},  // {0, 2}
                        new object[]{"E1", null, null, 100d, 5000},  // {0, 3}
                        new object[]{"E1", null, null, null, 5000},  // {0}
                        new object[]{null, 2, 10L, 100d, 4000},  // {1, 2, 3}
                        new object[]{null, 2, 10L, null, 4000},  // {1, 2}
                        new object[]{null, 2, null, 100d, 4000},  // {1, 3}
                        new object[]{null, 2, null, null, 4000},  // {1}
                        new object[]{null, null, 10L, 100d, 5000},  // {2, 3}
                        new object[]{null, null, 10L, null, 5000},  // {2}
                        new object[]{null, null, null, 100d, 7000},  // {3}
                        new object[]{null, null, null, null, 7000}   // {}
                    });
    
            stmt.Dispose();
        }
    
        private void RunAssertionBoundCube3Dim(EPServiceProvider epService) {
            TryAssertionBoundCube(epService, "Cube(TheString, IntPrimitive, LongPrimitive)");
            TryAssertionBoundCube(epService, "grouping Sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "(TheString, LongPrimitive)," +
                    "(TheString)," +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive)," +
                    "(LongPrimitive)," +
                    "()" +
                    ")");
        }
    
        private void TryAssertionBoundCube(EPServiceProvider epService, string groupBy) {
    
            string[] fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, " +
                    "IntPrimitive as c1, " +
                    "LongPrimitive as c2, " +
                    "count(*) as c3, " +
                    "sum(DoublePrimitive) as c4," +
                    "Grouping(TheString) as c5," +
                    "Grouping(IntPrimitive) as c6," +
                    "Grouping(LongPrimitive) as c7," +
                    "Grouping_id(TheString, IntPrimitive, LongPrimitive) as c8 " +
                    "from SupportBean#length(4) " +
                    "group by " + groupBy).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 1, 10L, 1L, 100d, 0, 0, 0, 0},  // {0, 1, 2}
                        new object[]{"E1", 1, null, 1L, 100d, 0, 0, 1, 1},  // {0, 1}
                        new object[]{"E1", null, 10L, 1L, 100d, 0, 1, 0, 2},  // {0, 2}
                        new object[]{"E1", null, null, 1L, 100d, 0, 1, 1, 3},  // {0}
                        new object[]{null, 1, 10L, 1L, 100d, 1, 0, 0, 4},  // {1, 2}
                        new object[]{null, 1, null, 1L, 100d, 1, 0, 1, 5},  // {1}
                        new object[]{null, null, 10L, 1L, 100d, 1, 1, 0, 6},  // {2}
                        new object[]{null, null, null, 1L, 100d, 1, 1, 1, 7}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 20, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 1, 20L, 1L, 200d, 0, 0, 0, 0},
                        new object[]{"E2", 1, null, 1L, 200d, 0, 0, 1, 1},
                        new object[]{"E2", null, 20L, 1L, 200d, 0, 1, 0, 2},
                        new object[]{"E2", null, null, 1L, 200d, 0, 1, 1, 3},
                        new object[]{null, 1, 20L, 1L, 200d, 1, 0, 0, 4},
                        new object[]{null, 1, null, 2L, 300d, 1, 0, 1, 5},
                        new object[]{null, null, 20L, 1L, 200d, 1, 1, 0, 6},
                        new object[]{null, null, null, 2L, 300d, 1, 1, 1, 7}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 2, 10L, 1L, 300d, 0, 0, 0, 0},
                        new object[]{"E1", 2, null, 1L, 300d, 0, 0, 1, 1},
                        new object[]{"E1", null, 10L, 2L, 400d, 0, 1, 0, 2},
                        new object[]{"E1", null, null, 2L, 400d, 0, 1, 1, 3},
                        new object[]{null, 2, 10L, 1L, 300d, 1, 0, 0, 4},
                        new object[]{null, 2, null, 1L, 300d, 1, 0, 1, 5},
                        new object[]{null, null, 10L, 2L, 400d, 1, 1, 0, 6},
                        new object[]{null, null, null, 3L, 600d, 1, 1, 1, 7}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 20, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 2, 20L, 1L, 400d, 0, 0, 0, 0},
                        new object[]{"E2", 2, null, 1L, 400d, 0, 0, 1, 1},
                        new object[]{"E2", null, 20L, 2L, 600d, 0, 1, 0, 2},
                        new object[]{"E2", null, null, 2L, 600d, 0, 1, 1, 3},
                        new object[]{null, 2, 20L, 1L, 400d, 1, 0, 0, 4},
                        new object[]{null, 2, null, 2L, 700d, 1, 0, 1, 5},
                        new object[]{null, null, 20L, 2L, 600d, 1, 1, 0, 6},
                        new object[]{null, null, null, 4L, 1000d, 1, 1, 1, 7}});
    
            // expiring/removing ("E1", 1, 10, 100)
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 10, 500));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 1, 10L, 1L, 500d, 0, 0, 0, 0},
                        new object[]{"E1", 1, 10L, 0L, null, 0, 0, 0, 0},
                        new object[]{"E2", 1, null, 2L, 700d, 0, 0, 1, 1},
                        new object[]{"E1", 1, null, 0L, null, 0, 0, 1, 1},
                        new object[]{"E2", null, 10L, 1L, 500d, 0, 1, 0, 2},
                        new object[]{"E1", null, 10L, 1L, 300d, 0, 1, 0, 2},
                        new object[]{"E2", null, null, 3L, 1100d, 0, 1, 1, 3},
                        new object[]{"E1", null, null, 1L, 300d, 0, 1, 1, 3},
                        new object[]{null, 1, 10L, 1L, 500d, 1, 0, 0, 4},
                        new object[]{null, 1, null, 2L, 700d, 1, 0, 1, 5},
                        new object[]{null, null, 10L, 2L, 800d, 1, 1, 0, 6},
                        new object[]{null, null, null, 4L, 1400d, 1, 1, 1, 7}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowCube2Dim(EPServiceProvider epService) {
            TryAssertionNamedWindowCube2Dim(epService, "Cube(TheString, IntPrimitive)");
            TryAssertionNamedWindowCube2Dim(epService, "grouping Sets(" +
                    "(TheString, IntPrimitive)," +
                    "(TheString)," +
                    "(IntPrimitive)," +
                    "()" +
                    ")");
        }
    
        private void TryAssertionNamedWindowCube2Dim(EPServiceProvider epService, string groupBy) {
    
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(IntBoxed = 0)");
            epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 3) delete from MyWindow");
    
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
                    "group by " + groupBy).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));    // insert event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, 10, 100L}, new object[] {null, null, 100L}},
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, 10, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 11, 200));    // insert event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 11, 200L}, new object[] {"E1", null, 300L}, new object[] {null, 11, 200L}, new object[] {null, null, 300L}},
                    new[] {new object[] {"E1", 11, null}, new object[] {"E1", null, 100L}, new object[] {null, 11, null}, new object[] {null, null, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));    // insert event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 600L}, new object[] {null, 10, 400L}, new object[] {null, null, 600L}},
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 300L}, new object[] {null, 10, 100L}, new object[] {null, null, 300L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 11, 400));    // insert event
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", 11, 400L}, new object[] {"E2", null, 400L}, new object[] {null, 11, 600L}, new object[] {null, null, 1000L}},
                    new[] {new object[] {"E2", 11, null}, new object[] {"E2", null, null}, new object[] {null, 11, 200L}, new object[] {null, null, 600L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(3, null, -1, -1));    // delete-all
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[]
                    {new object[] {"E1", 10, null}, new object[] {"E1", 11, null}, new object[] {"E2", 11, null},
                        new object[]{"E1", null, null}, new object[]{"E2", null, null}, new object[]{null, 10, null}, new object[]{null, 11, null}, new object[]{null, null, null}},
                    new[]
                    {new object[] {"E1", 10, 400L}, new object[] {"E1", 11, 200L}, new object[] {"E2", 11, 400L},
                        new object[]{"E1", null, 600L}, new object[]{"E2", null, 400L}, new object[]{null, 10, 400L}, new object[]{null, 11, 600L}, new object[]{null, null, 1000L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowDeleteAndRStream2Dim(EPServiceProvider epService) {
            TryAssertionNamedWindowDeleteAndRStream2Dim(epService, "Rollup(TheString, IntPrimitive)");
            TryAssertionNamedWindowDeleteAndRStream2Dim(epService, "grouping Sets(" +
                    "(TheString, IntPrimitive)," +
                    "(TheString)," +
                    "())");
        }
    
        private void TryAssertionNamedWindowDeleteAndRStream2Dim(EPServiceProvider epService, string groupBy) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(IntBoxed = 0)");
            epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 1) as sb " +
                    "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive");
            epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 2) as sb " +
                    "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive and sb.LongPrimitive = mw.LongPrimitive");
            epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 3) delete from MyWindow");
    
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
                    "group by " + groupBy).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));    // insert event IntBoxed=0
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}},
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 10, 100));   // delete (IntBoxed = 1)
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 200));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 200L}, new object[] {"E1", null, 200L}, new object[] {null, null, 200L}},
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 300));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", 20, 300L}, new object[] {"E2", null, 300L}, new object[] {null, null, 500L}},
                    new[] {new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 200L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(3, null, 0, 0));   // delete all
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[]
                    {
                        new object[]{"E1", 10, null}, new object[]{"E2", 20, null},
                        new object[]{"E1", null, null}, new object[]{"E2", null, null},
                        new object[]{null, null, null}},
                    new[]
                    {
                        new object[]{"E1", 10, 200L}, new object[]{"E2", 20, 300L},
                        new object[]{"E1", null, 200L}, new object[]{"E2", null, 300L},
                        new object[]{null, null, 500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 400));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}},
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 500));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 20, 500L}, new object[] {"E1", null, 900L}, new object[] {null, null, 900L}},
                    new[] {new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 600));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1500L}},
                    new[] {new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 900L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 700));   // insert
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 1100L}, new object[] {"E1", null, 1600L}, new object[] {null, null, 2200L}},
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(3, null, 0, 0));   // delete all
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[]
                    {
                        new object[]{"E1", 10, null}, new object[]{"E1", 20, null}, new object[]{"E2", 20, null},
                        new object[]{"E1", null, null}, new object[]{"E2", null, null},
                        new object[]{null, null, null}},
                    new[]
                    {
                        new object[]{"E1", 10, 1100L}, new object[]{"E1", 20, 500L}, new object[]{"E2", 20, 600L},
                        new object[]{"E1", null, 1600L}, new object[]{"E2", null, 600L},
                        new object[]{null, null, 2200L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 200));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 400));   // insert
            listener.Reset();
    
            epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 20, -1));   // delete (IntBoxed = 1)
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}},
                    new[] {new object[] {"E1", 20, 600L}, new object[] {"E1", null, 1000L}, new object[] {null, null, 1000L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 10, -1));   // delete (IntBoxed = 1)
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 200));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 400));   // insert
            epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 500));   // insert
            listener.Reset();
    
            epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 200));   // delete specific (IntBoxed = 2)
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}},
                    new[] {new object[] {"E1", 10, 600L}, new object[] {"E1", null, 1000L}, new object[] {null, null, 1500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 300));   // delete specific
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}},
                    new[] {new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 20, 400));   // delete specific
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 20, null}, new object[] {"E1", null, 100L}, new object[] {null, null, 600L}},
                    new[] {new object[] {"E1", 20, 400L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(2, "E2", 20, 500));   // delete specific
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 100L}},
                    new[] {new object[] {"E2", 20, 500L}, new object[] {"E2", null, 500L}, new object[] {null, null, 600L}});
    
            epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 100));   // delete specific
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[] {new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionBoundRollup2Dim(EPServiceProvider epService) {
            TryAssertionBoundRollup2Dim(epService, false);
            TryAssertionBoundRollup2Dim(epService, true);
        }
    
        private void TryAssertionBoundRollup2Dim(EPServiceProvider epService, bool join) {
    
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                    "from SupportBean#length(3) " + (join ? ", SupportBean_S0#lastevent " : "") +
                    "group by Rollup(TheString, IntPrimitive)").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));   // expires {TheString="E1", IntPrimitive=10, LongPrimitive=100}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 20, 600L}, new object[]{"E1", 10, null},
                        new object[]{"E2", null, 600L}, new object[]{"E1", null, 300L},
                        new object[]{null, null, 900L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 500));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=200}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 20, 900L},
                        new object[]{"E2", null, 900L},
                        new object[]{null, null, 1200L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 600));   // expires {TheString="E1", IntPrimitive=11, LongPrimitive=300}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 21, 600L}, new object[]{"E1", 11, null},
                        new object[]{"E2", null, 1500L}, new object[]{"E1", null, null},
                        new object[]{null, null, 1500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 700));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=400}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 21, 1300L}, new object[]{"E2", 20, 500L},
                        new object[]{"E2", null, 1800L},
                        new object[]{null, null, 1800L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 800));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=500}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 21, 2100L}, new object[]{"E2", 20, null},
                        new object[]{"E2", null, 2100L},
                        new object[]{null, null, 2100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 900));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=600}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 10, 900L}, new object[]{"E2", 21, 1500L},
                        new object[]{"E1", null, 900L}, new object[]{"E2", null, 1500L},
                        new object[]{null, null, 2400L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 1000));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=700}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E1", 11, 1000L}, new object[]{"E2", 21, 800L},
                        new object[]{"E1", null, 1900L}, new object[]{"E2", null, 800L},
                        new object[]{null, null, 2700L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 1100));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=800}
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[]
                    {
                        new object[]{"E2", 20, 1100L}, new object[]{"E2", 21, null},
                        new object[]{"E2", null, 1100L},
                        new object[]{null, null, 3000L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnboundRollup2Dim(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean " +
                    "group by Rollup(TheString, IntPrimitive)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1000L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 11, 800L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10, 700L}, new object[] {"E1", null, 1500L}, new object[] {null, null, 2100L}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnboundRollup1Dim(EPServiceProvider epService) {
            TryAssertionUnboundRollup1Dim(epService, "Rollup(TheString)");
            TryAssertionUnboundRollup1Dim(epService, "Cube(TheString)");
        }
    
        private void RunAssertionUnboundRollup2DimBatchWindow(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length_batch(4) " +
                    "group by Rollup(TheString, IntPrimitive)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetDataListsFlattened(), fields,
                    new[]
                    {new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 11, 300L},
                        new object[]{"E1", null, 400L}, new object[]{"E2", null, 600L},
                        new object[]{null, null, 1000L}},
                    new[]
                    {new object[] {"E1", 10, null}, new object[] {"E2", 20, null}, new object[] {"E1", 11, null},
                        new object[]{"E1", null, null}, new object[]{"E2", null, null},
                        new object[]{null, null, null}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 600));
            epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 700));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 800));
            EPAssertionUtil.AssertPropsPerRow(listener.GetDataListsFlattened(), fields,
                    new[]
                    {new object[] {"E1", 11, 1200L}, new object[] {"E2", 20, 1400L}, new object[] {"E1", 10, null},
                        new object[]{"E1", null, 1200L}, new object[]{"E2", null, 1400L},
                        new object[]{null, null, 2600L}},
                    new[]
                    {new object[] {"E1", 11, 300L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 10, 100L},
                        new object[]{"E1", null, 400L}, new object[]{"E2", null, 600L},
                        new object[]{null, null, 1000L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionUnboundRollup1Dim(EPServiceProvider epService, string rollup) {
    
            string[] fields = "c0,c1".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, sum(IntPrimitive) as c1 from SupportBean " +
                    "group by " + rollup).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 10}, new object[] {null, 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 20}, new object[] {null, 30}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 40}, new object[] {null, 60}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 40));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 60}, new object[] {null, 100}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnboundRollup3Dim(EPServiceProvider epService) {
            string rollupEpl = "Rollup(TheString, IntPrimitive, LongPrimitive)";
            TryAssertionUnboundRollup3Dim(epService, rollupEpl, false);
            TryAssertionUnboundRollup3Dim(epService, rollupEpl, true);
    
            string gsEpl = "grouping Sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "(TheString)," +
                    "()" +
                    ")";
            TryAssertionUnboundRollup3Dim(epService, gsEpl, false);
            TryAssertionUnboundRollup3Dim(epService, gsEpl, true);
        }
    
        private void TryAssertionUnboundRollup3Dim(EPServiceProvider epService, string groupByClause, bool isJoin) {
    
            string[] fields = "c0,c1,c2,c3,c4".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, count(*) as c3, sum(DoublePrimitive) as c4 " +
                    "from SupportBean#keepall " + (isJoin ? ", SupportBean_S0#lastevent " : "") +
                    "group by " + groupByClause).Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 1, 10L, 1L, 100d}, new object[] {"E1", 1, null, 1L, 100d}, new object[] {"E1", null, null, 1L, 100d}, new object[] {null, null, null, 1L, 100d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 11, 200));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 1, 11L, 1L, 200d}, new object[] {"E1", 1, null, 2L, 300d}, new object[] {"E1", null, null, 2L, 300d}, new object[] {null, null, null, 2L, 300d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 300));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 2, 10L, 1L, 300d}, new object[] {"E1", 2, null, 1L, 300d}, new object[] {"E1", null, null, 3L, 600d}, new object[] {null, null, null, 3L, 600d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 10, 400));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E2", 1, 10L, 1L, 400d}, new object[] {"E2", 1, null, 1L, 400d}, new object[] {"E2", null, null, 1L, 400d}, new object[] {null, null, null, 4L, 1000d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 500));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 1, 10L, 2L, 600d}, new object[] {"E1", 1, null, 3L, 800d}, new object[] {"E1", null, null, 4L, 1100d}, new object[] {null, null, null, 5L, 1500d}});
    
            epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 11, 600));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {"E1", 1, 11L, 2L, 800d}, new object[] {"E1", 1, null, 4L, 1400d}, new object[] {"E1", null, null, 5L, 1700d}, new object[] {null, null, null, 6L, 2100d}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMixedAccessAggregation(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            var listener = new SupportUpdateListener();
            string epl = "select sum(IntPrimitive) as c0, TheString as c1, window(*) as c2 " +
                    "from SupportBean#length(2) sb group by Rollup(TheString) order by TheString";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            var eventOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {1, null, new object[] {eventOne}}, new object[] {1, "E1", new object[] {eventOne}}});
    
            var eventTwo = new SupportBean("E1", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {3, null, new object[] {eventOne, eventTwo}}, new object[] {3, "E1", new object[] {eventOne, eventTwo}}});
    
            var eventThree = new SupportBean("E2", 3);
            epService.EPRuntime.SendEvent(eventThree);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {5, null, new object[] {eventTwo, eventThree}}, new object[] {2, "E1", new object[] {eventTwo}}, new object[] {3, "E2", new object[] {eventThree}}});
    
            var eventFour = new SupportBean("E1", 4);
            epService.EPRuntime.SendEvent(eventFour);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new[] {new object[] {7, null, new object[] {eventThree, eventFour}}, new object[] {4, "E1", new object[] {eventFour}}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionNonBoxedTypeWithRollup(EPServiceProvider epService) {
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                    "from SupportBean group by IntPrimitive, Rollup(DoublePrimitive, LongPrimitive)");
            AssertTypesC0C1C2(stmtOne, typeof(int), typeof(double?), typeof(long));
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                    "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive))");
            AssertTypesC0C1C2(stmtTwo, typeof(int), typeof(double), typeof(long));
    
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                    "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive), (IntPrimitive, DoublePrimitive))");
            AssertTypesC0C1C2(stmtThree, typeof(int), typeof(double), typeof(long));
    
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                    "from SupportBean group by grouping sets ((DoublePrimitive, IntPrimitive), (LongPrimitive, IntPrimitive))");
            AssertTypesC0C1C2(stmtFour, typeof(int), typeof(double?), typeof(long));
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string prefix = "select TheString, sum(IntPrimitive) from SupportBean group by ";
    
            // invalid rollup expressions
            TryInvalid(epService, prefix + "Rollup()",
                    "Incorrect syntax near ')' at line 1 column 69, please check the group-by clause [select TheString, sum(IntPrimitive) from SupportBean group by Rollup()]");
            TryInvalid(epService, prefix + "Rollup(TheString, TheString)",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by Rollup(TheString, TheString)]");
            TryInvalid(epService, prefix + "Rollup(x)",
                    "Error starting statement: Failed to validate group-by-clause expression 'x': Property named 'x' is not valid in any stream [select TheString, sum(IntPrimitive) from SupportBean group by Rollup(x)]");
            TryInvalid(epService, prefix + "Rollup(LongPrimitive)",
                    "Error starting statement: Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'TheString' [select TheString, sum(IntPrimitive) from SupportBean group by Rollup(LongPrimitive)]");
            TryInvalid(epService, prefix + "Rollup((TheString, LongPrimitive), (TheString, LongPrimitive))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by Rollup((TheString, LongPrimitive), (TheString, LongPrimitive))]");
            TryInvalid(epService, prefix + "Rollup((TheString, LongPrimitive), (LongPrimitive, TheString))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by Rollup((TheString, LongPrimitive), (LongPrimitive, TheString))]");
            TryInvalid(epService, prefix + "grouping Sets((TheString, TheString))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping Sets((TheString, TheString))]");
            TryInvalid(epService, prefix + "grouping Sets(TheString, TheString)",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping Sets(TheString, TheString)]");
            TryInvalid(epService, prefix + "grouping Sets((), ())",
                    "Failed to validate the group-by clause, found duplicate specification of the overall grouping '()' [select TheString, sum(IntPrimitive) from SupportBean group by grouping Sets((), ())]");
            TryInvalid(epService, prefix + "grouping Sets(())",
                    "Failed to validate the group-by clause, the overall grouping '()' cannot be the only grouping [select TheString, sum(IntPrimitive) from SupportBean group by grouping Sets(())]");
    
            // invalid select clause for this type of query
            TryInvalid(epService, "select * from SupportBean group by grouping Sets(TheString)",
                    "Group-by with rollup requires that the select-clause does not use wildcard [select * from SupportBean group by grouping Sets(TheString)]");
            TryInvalid(epService, "select sb.* from SupportBean sb group by grouping Sets(TheString)",
                    "Group-by with rollup requires that the select-clause does not use wildcard [select sb.* from SupportBean sb group by grouping Sets(TheString)]");
    
            TryInvalid(epService, "@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping Sets(TheString)",
                    "Error starting statement: Reclaim hints are not available with rollup [@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping Sets(TheString)]");
        }
    
        private SupportBean MakeEvent(int intBoxed, string theString, int intPrimitive, long longPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.IntBoxed = intBoxed;
            return sb;
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            return sb;
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive, int intBoxed) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            sb.IntBoxed = intBoxed;
            return sb;
        }
    
        private void AssertTypesC0C1C2(EPStatement stmtOne, Type expectedC0, Type expectedC1, Type expectedC2) {
            Assert.AreEqual(expectedC0.GetBoxedType(), stmtOne.EventType.GetPropertyType("c0").GetBoxedType());
            Assert.AreEqual(expectedC1.GetBoxedType(), stmtOne.EventType.GetPropertyType("c1").GetBoxedType());
            Assert.AreEqual(expectedC2.GetBoxedType(), stmtOne.EventType.GetPropertyType("c2").GetBoxedType());
        }
    }
} // end of namespace
