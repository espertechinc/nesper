///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.directory;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendVirtualDataWindow : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionInsertConsume(env);
            RunAssertionOnMerge(env);
            RunAssertionLimitation(env);
            RunAssertionJoinAndLifecyle(env);
            RunAssertionContextWJoin(env);
            RunAssertionFireAndForget(env);
            RunAssertionOnDelete(env);
            RunAssertionInvalid(env);
            RunAssertionManagementEvents(env);
            RunAssertionIndexChoicesJoinUniqueVirtualDW(env);
            RunAssertionLateConsume(env);
            RunAssertionContextWSubquery(env);
            RunAssertionSubquery(env);
            RunAssertionLookupSPI(env);
        }

        private void RunAssertionLateConsume(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyVDW.test:vdwwithparam() as SupportBean", path);
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            var fields = new [] { "val0" };
            env.CompileDeploy("@Name('s0') select sum(IntPrimitive) as val0 from MyVDW", path).AddListener("s0");
            EPAssertionUtil.AssertProps(
                env.Statement("s0").First(),
                fields,
                new object[] {100});

            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {110});

            env.SendEventBean(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {130});

            // assert events received for add-consumer and remove-consumer
            env.UndeployModuleContaining("s0");

            if (env.IsHA) {
                env.UndeployAll();
                return;
            }

            var addConsumerEvent = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            var removeConsumerEvent = (VirtualDataWindowEventConsumerRemove) window.Events[1];

            foreach (var @base in new VirtualDataWindowEventConsumerBase[] {addConsumerEvent, removeConsumerEvent}) {
                Assert.AreEqual(-1, @base.AgentInstanceId);
                Assert.AreEqual("MyVDW", @base.NamedWindowName);
                Assert.AreEqual("s0", @base.StatementName);
            }

            Assert.AreSame(removeConsumerEvent.ConsumerObject, addConsumerEvent.ConsumerObject);
            window.Events.Clear();

            // test filter criteria passed to event
            env.CompileDeploy("@Name('ABC') select sum(IntPrimitive) as val0 from MyVDW(TheString = 'A')", path);
            var eventWithFilter = (VirtualDataWindowEventConsumerAdd) window.Events[0];
            Assert.IsNotNull(eventWithFilter.Filter);
            Assert.IsNotNull(eventWithFilter.ExprEvaluatorContext);

            env.UndeployAll();
        }

        private void RunAssertionLookupSPI(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyVDW.test:vdwnoparam() as SupportBean", path);

            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("E1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);

            env.CompileDeploy(
                "@Name('s0') select (select sum(IntPrimitive) from MyVDW vdw where vdw.TheString = S0.P00) from SupportBean_S0 S0",
                path);
            env.AddListener("s0");
            var spiContext = (VirtualDataWindowLookupContextSPI) window.LastRequestedLookup;
            Assert.IsNotNull(spiContext);

            env.UndeployAll();
        }

        private void RunAssertionInsertConsume(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            string[] fields;
            env.CompileDeploy("create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // test straight consume
            fields = new [] { "TheString","IntPrimitive" };
            env.CompileDeploy("@Name('s0') select irstream * from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 200));
            Assert.IsNull(env.Listener("s0").LastOldData);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").GetAndResetLastNewData()[0],
                fields,
                new object[] {"E1", 200});
            env.UndeployModuleContaining("s0");

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            fields = new [] { "val0" };
            env.CompileDeploy("@Name('s0') select sum(IntPrimitive) as val0 from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {200});

            env.SendEventBean(new SupportBean("E1", 50));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {250});

            env.UndeployAll();
        }

        private void RunAssertionOnMerge(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyVDW.test:vdw() as MapType", path);

            // define some test data to return, via lookup
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            IDictionary<string, object> mapData = new Dictionary<string, object>();
            mapData.Put("col1", "key1");
            mapData.Put("col2", "key2");
            window.Data = Collections.SingletonSet<object>(mapData);

            var fieldsMerge = new [] { "col1","col2" };
            env.CompileDeploy(
                    "@Name('s0') on SupportBean sb merge MyVDW vdw " +
                    "where col1 = TheString " +
                    "when matched then update set col2 = 'xxx'" +
                    "when not matched then insert select TheString as col1, 'abc' as col2, 1 as col3",
                    path)
                .AddListener("s0");
            env.CompileDeploy("@Name('consume') select * from MyVDW", path).AddListener("consume");

            // try yes-matched case
            env.SendEventBean(new SupportBean("key1", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[0],
                fieldsMerge,
                new object[] {"key1", "key2"});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").GetAndResetLastNewData()[0],
                fieldsMerge,
                new object[] {"key1", "xxx"});
            EPAssertionUtil.AssertProps(
                window.LastUpdateOld[0],
                fieldsMerge,
                new object[] {"key1", "key2"});
            EPAssertionUtil.AssertProps(
                window.LastUpdateNew[0],
                fieldsMerge,
                new object[] {"key1", "xxx"});
            EPAssertionUtil.AssertProps(
                env.Listener("consume").AssertOneGetNewAndReset(),
                fieldsMerge,
                new object[] {"key1", "xxx"});

            // try not-matched case
            env.SendEventBean(new SupportBean("key2", 3));
            Assert.IsNull(env.Listener("s0").LastOldData);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").GetAndResetLastNewData()[0],
                fieldsMerge,
                new object[] {"key2", "abc"});
            EPAssertionUtil.AssertProps(
                env.Listener("consume").AssertOneGetNewAndReset(),
                fieldsMerge,
                new object[] {"key2", "abc"});
            Assert.IsNull(window.LastUpdateOld);
            EPAssertionUtil.AssertProps(
                window.LastUpdateNew[0],
                fieldsMerge,
                new object[] {"key2", "abc"});

            env.UndeployAll();
        }

        private void RunAssertionLimitation(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@Name('window') create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // cannot iterate named window
            Assert.IsFalse(env.GetEnumerator("window").MoveNext());

            // test data window aggregation (rows not included in aggregation)
            env.CompileDeploy("@Name('s0') select window(TheString) as val0 from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {"S1", "E1"},
                (string[]) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

            env.UndeployAll();
        }

        private void RunAssertionJoinAndLifecyle(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyVDW.test:vdw(1, 'abc') as SupportBean", path);
            var fields = new [] { "st0.Id","vdw.TheString","vdw.IntPrimitive" };

            // define some test data to return, via lookup
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            supportBean.LongPrimitive = 50;
            window.Data = Collections.SingletonSet<object>(supportBean);

            Assert.IsNotNull(window.Context.EventFactory);
            Assert.AreEqual("MyVDW", window.Context.EventType.Name);
            Assert.IsNotNull(window.Context.StatementContext);
            Assert.AreEqual(2, window.Context.Parameters.Length);
            Assert.AreEqual(1, window.Context.Parameters[0]);
            Assert.AreEqual("abc", window.Context.Parameters[1]);
            Assert.AreEqual("MyVDW", window.Context.NamedWindowName);

            // test no-criteria join
            env.CompileDeploy("@Name('s0') select * from MyVDW vdw, SupportBean_ST0#lastevent st0", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "S1", 100});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria join
            env.CompileDeploy(
                    "@Name('s0') select * from MyVDW vdw, SupportBean_ST0#lastevent st0 where vdw.TheString = st0.Id",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.RequestedLookups[1], "TheString=(System.String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1"}, window.LastAccessKeys);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean_ST0("S1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"S1", "S1", 100});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"S1"}, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multi-criteria join
            env.CompileDeploy(
                "@Name('s0') select vdw.TheString from MyVDW vdw, SupportBeanRange#lastevent st0 " +
                "where vdw.TheString = st0.Id and LongPrimitive = KeyLong and IntPrimitive between RangeStart and RangeEnd",
                path);
            env.AddListener("s0");
            AssertIndexSpec(
                window.RequestedLookups[1],
                "TheString=(System.String)|LongPrimitive=(System.Nullable<System.Int64>)",
                "IntPrimitive[,](System.Nullable<System.Int32>)");

            env.SendEventBean(SupportBeanRange.MakeKeyLong("S1", 50L, 80, 120));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "vdw.TheString" },
                new object[] {"S1"});
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {"S1", 50L, new VirtualDataWindowKeyRange(80, 120)},
                window.LastAccessKeys);

            // destroy
            env.UndeployAll();
            Assert.IsNull(GetFromContext(env, "/virtualdw/MyVDW"));
            Assert.IsTrue(window.IsDestroyed);

            env.UndeployAll();
        }

        private void RunAssertionSubquery(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var window = RegisterTypeSetMapData(env, path);

            // test no-criteria subquery
            env.CompileDeploy("@Name('s0') select (select col1 from MyVDW vdw) from SupportBean_ST0", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria subquery
            env.CompileDeploy(
                    "@Name('s0') select (select col1 from MyVDW vdw where col1=st0.Id) as val0 from SupportBean_ST0 st0",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0" },
                new object[] {null});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1"}, window.LastAccessKeys);
            env.SendEventBean(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"key1"}, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multi-criteria subquery
            env.CompileDeploy(
                    "@Name('s0') select " +
                    "(select col1 from MyVDW vdw where col1=r.Id and col2=r.Key and col3 between r.RangeStart and r.RangeEnd) as val0 " +
                    "from SupportBeanRange r",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)|col2=(System.String)", "col3[,](System.Nullable<System.Int32>)");

            env.SendEventBean(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "val0" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {"key1", "key2", new VirtualDataWindowKeyRange(5, 10)},
                window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test aggregation
            env.CompileDeploy("create schema SampleEvent as (Id string)", path);
            env.CompileDeploy("create window MySampleWindow.test:vdw() as SampleEvent", path);
            env.CompileDeploy(
                    "@Name('s0') select (select count(*) as cnt from MySampleWindow) as c0 " + "from SupportBean ste",
                    path)
                .AddListener("s0");

            var thewindow = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MySampleWindow");
            var row1 = Collections.SingletonDataMap("Id", "V1");
            thewindow.Data = Collections.SingletonSet<object>(row1);

            env.SendEventBean(new SupportBean("E1", 1));
            Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

            ISet<object> rows = new HashSet<object>();
            rows.Add(row1);
            rows.Add(Collections.SingletonDataMap("Id", "V2"));
            thewindow.Data = rows;

            env.SendEventBean(new SupportBean("E2", 2));
            Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

            env.UndeployAll();
        }

        private void RunAssertionContextWJoin(RegressionEnvironment env)
        {
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));
            var path = new RegressionPath();

            // prepare
            env.CompileDeploy(
                "create context MyContext coalesce by " +
                "consistent_hash_crc32(TheString) from SupportBean, " +
                "consistent_hash_crc32(P00) from SupportBean_S0 " +
                "granularity 4 preallocate",
                path);
            env.CompileDeploy("context MyContext create window MyWindow.test:vdw() as SupportBean", path);

            // join
            var eplSubquerySameCtx = "@Name('s0') context MyContext " +
                                     "select * from SupportBean_S0 as S0 unidirectional, MyWindow as mw where mw.TheString = S0.P00";
            env.CompileDeploy(eplSubquerySameCtx, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            Assert.IsTrue(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private void RunAssertionContextWSubquery(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));

            env.CompileDeploy(
                "create context MyContext coalesce by " +
                "consistent_hash_crc32(TheString) from SupportBean, " +
                "consistent_hash_crc32(P00) from SupportBean_S0 " +
                "granularity 4 preallocate",
                path);
            env.CompileDeploy("context MyContext create window MyWindow.test:vdw() as SupportBean", path);

            // subquery - same context
            var eplSubquerySameCtx = "context MyContext " +
                                     "select (select IntPrimitive from MyWindow mw where mw.TheString = S0.P00) as c0 " +
                                     "from SupportBean_S0 S0";
            env.CompileDeploy("@Name('s0') " + eplSubquerySameCtx, path).AddListener("s0");
            env.CompileDeploy("@Hint('disable_window_subquery_indexshare') @Name('s1') " + eplSubquerySameCtx, path);

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            env.UndeployModuleContaining("s0");

            // subquery - no context
            var eplSubqueryNoCtx = "select (select IntPrimitive from MyWindow mw where mw.TheString = S0.P00) as c0 " +
                                   "from SupportBean_S0 S0";
            TryInvalidCompile(
                env,
                path,
                eplSubqueryNoCtx,
                "Failed to plan subquery number 1 querying MyWindow: Mismatch in context specification, the context for the named window 'MyWindow' is 'MyContext' and the query specifies no context  [select (select IntPrimitive from MyWindow mw where mw.TheString = S0.P00) as c0 from SupportBean_S0 S0]");

            SupportVirtualDW.InitializationData = null;
            env.UndeployAll();
        }

        private void RunAssertionFireAndForget(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var window = RegisterTypeSetMapData(env, path);

            // test no-criteria FAF
            var result = env.CompileExecuteFAF("select col1 from MyVDW vdw", path);
            AssertIndexSpec(window.LastRequestedLookup, "", "");
            Assert.AreEqual("MyVDW", window.LastRequestedLookup.NamedWindowName);
            Assert.AreEqual(-1, window.LastRequestedLookup.StatementId);
            Assert.IsNull(window.LastRequestedLookup.StatementName);
            Assert.IsNotNull(window.LastRequestedLookup.StatementAnnotations);
            Assert.IsTrue(window.LastRequestedLookup.IsFireAndForget);
            EPAssertionUtil.AssertProps(
                result.Array[0],
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[0], window.LastAccessKeys);

            // test single-criteria FAF
            result = env.CompileExecuteFAF("select col1 from MyVDW vdw where col1='key1'", path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)", "");
            EPAssertionUtil.AssertProps(
                result.Array[0],
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"key1"}, window.LastAccessKeys);

            // test multi-criteria subquery
            result = env.CompileExecuteFAF(
                "select col1 from MyVDW vdw where col1='key1' and col2='key2' and col3 between 5 and 15",
                path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)|col2=(System.String)", "col3[,](System.Double)");
            EPAssertionUtil.AssertProps(
                result.Array[0],
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {"key1", "key2", new VirtualDataWindowKeyRange(5d, 15d)},
                window.LastAccessKeys);

            // test multi-criteria subquery
            result = env.CompileExecuteFAF(
                "select col1 from MyVDW vdw where col1='key1' and col2>'Key0' and col3 between 5 and 15",
                path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)", "col3[,](System.Double)|col2>(System.String)");
            EPAssertionUtil.AssertProps(
                result.Array[0],
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] {"key1", new VirtualDataWindowKeyRange(5d, 15d), "Key0"},
                window.LastAccessKeys);

            env.UndeployAll();
        }

        private void RunAssertionOnDelete(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var window = RegisterTypeSetMapData(env, path);

            // test no-criteria on-delete
            env.CompileDeploy("@Name('s0') on SupportBean_ST0 delete from MyVDW vdw", path).AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria on-delete
            env.CompileDeploy("@Name('s0') on SupportBean_ST0 st0 delete from MyVDW vdw where col1=st0.Id", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1"}, window.LastAccessKeys);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.SendEventBean(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"key1"}, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multie-criteria on-delete
            env.CompileDeploy(
                    "@Name('s0') on SupportBeanRange r delete " +
                    "from MyVDW vdw where col1=r.Id and col2=r.Key and col3 between r.RangeStart and r.RangeEnd",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(System.String)|col2=(System.String)", "col3[,](System.Nullable<System.Int32>)");
            Assert.AreEqual("MyVDW", window.LastRequestedLookup.NamedWindowName);
            Assert.IsNotNull(window.LastRequestedLookup.StatementId);
            Assert.AreEqual("s0", window.LastRequestedLookup.StatementName);
            Assert.AreEqual(1, window.LastRequestedLookup.StatementAnnotations.Length);
            Assert.IsFalse(window.LastRequestedLookup.IsFireAndForget);

            env.SendEventBean(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "col1" },
                new object[] {"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {"key1", "key2", new VirtualDataWindowKeyRange(5, 10)},
                window.LastAccessKeys);

            env.UndeployAll();
        }

        private void RunAssertionInvalid(RegressionEnvironment env)
        {
            string epl;

            epl = "create window ABC.invalid:invalid() as SupportBean";
            TryInvalidCompile(
                env,
                epl,
                "Failed to validate data window declaration: Virtual data window forge class " +
                typeof(SupportBean).Name +
                " does not implement the interface " +
                typeof(VirtualDataWindowForge).Name);

            epl = "select * from SupportBean.test:vdw()";
            TryInvalidCompile(
                env,
                epl,
                "Failed to validate data window declaration: Virtual data window requires use with a named window in the create-window syntax [select * from SupportBean.test:vdw()]");

            TryInvalidCompile(
                env,
                "create window ABC.test:exceptionvdw() as SupportBean",
                "Failed to validate data window declaration: Validation exception initializing virtual data window 'ABC': This is a test exception [create window ABC.test:exceptionvdw() as SupportBean]");
        }

        private void RunAssertionManagementEvents(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var vdw = RegisterTypeSetMapData(env, path);

            // create-index event
            vdw.Events.Clear();
            env.CompileDeploy("@Name('idx') create index IndexOne on MyVDW (col3, col2 btree)", path);
            var startEvent = (VirtualDataWindowEventStartIndex) vdw.Events[0];
            Assert.AreEqual("MyVDW", startEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", startEvent.IndexName);
            Assert.AreEqual(2, startEvent.Fields.Count);
            Assert.AreEqual("col3", startEvent.Fields[0].Name);
            Assert.AreEqual("hash", startEvent.Fields[0].Type);
            Assert.AreEqual("col2", startEvent.Fields[1].Name);
            Assert.AreEqual("btree", startEvent.Fields[1].Type);
            Assert.IsFalse(startEvent.IsUnique);

            // stop-index event
            vdw.Events.Clear();
            env.UndeployModuleContaining("idx");
            var stopEvent = (VirtualDataWindowEventStopIndex) vdw.Events[0];
            Assert.AreEqual("MyVDW", stopEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", stopEvent.IndexName);

            // stop named window
            vdw.Events.Clear();
            env.UndeployAll();
            var stopWindow = (VirtualDataWindowEventStopWindow) vdw.Events[0];
            Assert.AreEqual("MyVDW", stopWindow.NamedWindowName);
        }

        private void RunAssertionIndexChoicesJoinUniqueVirtualDW(RegressionEnvironment env)
        {
            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () => {
                var fields = new [] { "vdw.TheString","vdw.IntPrimitive","ssb1.I1" };
                env.SendEventBean(new SupportSimpleBeanOne("S1", 1, 102, 103));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 101, 1});
            };

            foreach (var caseEnum in EnumHelper.GetValues<CaseEnum>()) {
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "TheString",
                    "where vdw.TheString = ssb1.S1",
                    true,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "I1",
                    "where vdw.TheString = ssb1.S1",
                    false,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "IntPrimitive",
                    "where vdw.TheString = ssb1.S1",
                    false,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "LongPrimitive",
                    "where vdw.LongPrimitive = ssb1.L1",
                    true,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "LongPrimitive,TheString",
                    "where vdw.TheString = ssb1.S1 and vdw.LongPrimitive = ssb1.L1",
                    true,
                    assertSendEvents);
            }
        }

        private void TryAssertionVirtualDW(
            RegressionEnvironment env,
            CaseEnum caseEnum,
            string uniqueFields,
            string whereClause,
            bool unique,
            IndexAssertionEventSend assertion)
        {
            SupportQueryPlanIndexHook.Reset();
            SupportVirtualDWForge.UniqueKeys = new HashSet<string>(Arrays.AsList(uniqueFields.SplitCsv()));

            var path = new RegressionPath();
            env.CompileDeploy("create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 101);
            supportBean.DoublePrimitive = 102;
            supportBean.LongPrimitive = 103;
            window.Data = Collections.SingletonSet<object>(supportBean);

            var eplUnique = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                            "@Name('s0') select * from ";

            if (caseEnum == CaseEnum.UNIDIRECTIONAL) {
                eplUnique += "SupportSimpleBeanOne as ssb1 unidirectional ";
            }
            else {
                eplUnique += "SupportSimpleBeanOne#lastevent as ssb1 ";
            }

            eplUnique += ", MyVDW as vdw ";
            eplUnique += whereClause;

            env.CompileDeploy(eplUnique, path).AddListener("s0");

            // assert query plan
            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);

            // run assertion
            assertion.Invoke();

            env.UndeployAll();
            SupportVirtualDWForge.UniqueKeys = null;
        }

        private SupportVirtualDW RegisterTypeSetMapData(
            RegressionEnvironment env,
            RegressionPath path)
        {
            SupportVirtualDWForge.Initializations.Clear();
            env.CompileDeploy("@Name('create-nw') create window MyVDW.test:vdw() as MapType", path);

            Assert.AreEqual(1, SupportVirtualDWForge.Initializations.Count);
            var forgeContext = SupportVirtualDWForge.Initializations[0];
            Assert.AreEqual("MyVDW", forgeContext.EventType.Name);
            Assert.IsNotNull("MyVDW", forgeContext.NamedWindowName);
            Assert.AreEqual(0, forgeContext.Parameters.Length);
            Assert.AreEqual(0, forgeContext.ParameterExpressions.Length);
            Assert.IsNotNull(forgeContext.ViewForgeEnv);

            // define some test data to return, via lookup
            var window = (SupportVirtualDW) GetFromContext(env, "/virtualdw/MyVDW");
            IDictionary<string, object> mapData = new Dictionary<string, object>();
            mapData.Put("col1", "key1");
            mapData.Put("col2", "key2");
            mapData.Put("col3", 10);
            window.Data = Collections.SingletonSet<object>(mapData);

            return window;
        }

        private void AssertIndexSpec(
            VirtualDataWindowLookupContext indexSpec,
            string hashfields,
            string btreefields)
        {
            AssertIndexFields(hashfields, indexSpec.HashFields);
            AssertIndexFields(btreefields, indexSpec.BtreeFields);
        }

        private void AssertIndexFields(
            string hashfields,
            IList<VirtualDataWindowLookupFieldDesc> fields)
        {
            if (string.IsNullOrEmpty(hashfields) && fields.IsEmpty()) {
                return;
            }

            var split = hashfields.Split('|');
            IList<string> found = new List<string>();
            for (var i = 0; i < split.Length; i++) {
                var field = fields[i];
                var result = field.PropertyName + field.Operator.Value.GetOp() + "(" + field.LookupValueType.CleanName() + ")";
                found.Add(result);
            }

            EPAssertionUtil.AssertEqualsAnyOrder(split, found.ToArray());
        }

        private VirtualDataWindow GetFromContext(
            RegressionEnvironment env,
            string name)
        {
            try {
                return (VirtualDataWindow) env.Runtime.Context.Lookup(name);
            }
            catch (NamingException) {
                throw new EPException("Name '" + name + "' could not be looked up");
            }
        }

        private enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL
        }
    }
} // end of namespace