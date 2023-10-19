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

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendVirtualDataWindow : RegressionExecution,
        IndexBackingTableInfo
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

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.STATICHOOK);
        }

        private void RunAssertionLateConsume(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdwwithparam() as SupportBean", path);
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            var fields = "val0".SplitCsv();
            env.CompileDeploy("@name('s0') select sum(intPrimitive) as val0 from MyVDW", path).AddListener("s0");
            env.AssertIterator("s0", en => EPAssertionUtil.AssertProps(en.Advance(), fields, new object[] { 100 }));

            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsNew("s0", fields, new object[] { 110 });

            env.SendEventBean(new SupportBean("E1", 20));
            env.AssertPropsNew("s0", fields, new object[] { 130 });

            // assert events received for add-consumer and remove-consumer
            env.UndeployModuleContaining("s0");

            if (env.IsHA) {
                env.UndeployAll();
                return;
            }

            var addConsumerEvent = (VirtualDataWindowEventConsumerAdd)window.Events[0];
            var removeConsumerEvent = (VirtualDataWindowEventConsumerRemove)window.Events[1];

            foreach (var @base in new VirtualDataWindowEventConsumerBase[] { addConsumerEvent, removeConsumerEvent }) {
                Assert.AreEqual(-1, @base.AgentInstanceId);
                Assert.AreEqual("MyVDW", @base.NamedWindowName);
                Assert.AreEqual("s0", @base.StatementName);
            }

            Assert.AreSame(removeConsumerEvent.ConsumerObject, addConsumerEvent.ConsumerObject);
            window.Events.Clear();

            // test filter criteria passed to event
            env.CompileDeploy("@name('ABC') select sum(intPrimitive) as val0 from MyVDW(theString = 'A')", path);
            var eventWithFilter = (VirtualDataWindowEventConsumerAdd)window.Events[0];
            Assert.IsNotNull(eventWithFilter.Filter);
            Assert.IsNotNull(eventWithFilter.ExprEvaluatorContext);

            env.UndeployAll();
        }

        private void RunAssertionLookupSPI(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdwnoparam() as SupportBean", path);

            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("E1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);

            env.CompileDeploy(
                "@name('s0') select (select sum(intPrimitive) from MyVDW vdw where vdw.theString = s0.p00) from SupportBean_S0 s0",
                path);
            env.AddListener("s0");
            var spiContext = (VirtualDataWindowLookupContextSPI)window.LastRequestedLookup;
            Assert.IsNotNull(spiContext);

            env.UndeployAll();
        }

        private void RunAssertionInsertConsume(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // test straight consume
            env.CompileDeploy("@name('s0') select irstream * from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 200));
            env.AssertListener(
                "s0",
                listener => {
                    Assert.IsNull(listener.LastOldData);
                    var fieldsOne = "theString,intPrimitive".SplitCsv();
                    EPAssertionUtil.AssertProps(
                        listener.GetAndResetLastNewData()[0],
                        fieldsOne,
                        new object[] { "E1", 200 });
                });

            env.UndeployModuleContaining("s0");

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            var fieldsTwo = "val0".SplitCsv();
            env.CompileDeploy("@name('s0') select sum(intPrimitive) as val0 from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 100));
            env.AssertPropsNew("s0", fieldsTwo, new object[] { 200 });

            env.SendEventBean(new SupportBean("E1", 50));
            env.AssertPropsNew("s0", fieldsTwo, new object[] { 250 });

            env.UndeployAll();
        }

        private void RunAssertionOnMerge(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdw() as MapType", path);

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            IDictionary<string, object> mapData = new Dictionary<string, object>();
            mapData.Put("col1", "key1");
            mapData.Put("col2", "key2");
            window.Data = Collections.SingletonSet<object>(mapData);

            var fieldsMerge = "col1,col2".SplitCsv();
            env.CompileDeploy(
                    "@name('s0') on SupportBean sb merge MyVDW vdw " +
                    "where col1 = theString " +
                    "when matched then update set col2 = 'xxx'" +
                    "when not matched then insert select theString as col1, 'abc' as col2, 1 as col3",
                    path)
                .AddListener("s0");
            env.CompileDeploy("@name('consume') select * from MyVDW", path).AddListener("consume");

            // try yes-matched case
            env.SendEventBean(new SupportBean("key1", 2));
            env.AssertListener(
                "s0",
                listener => {
                    EPAssertionUtil.AssertProps(listener.LastOldData[0], fieldsMerge, new object[] { "key1", "key2" });
                    EPAssertionUtil.AssertProps(
                        listener.GetAndResetLastNewData()[0],
                        fieldsMerge,
                        new object[] { "key1", "xxx" });
                });
            EPAssertionUtil.AssertProps(window.LastUpdateOld[0], fieldsMerge, new object[] { "key1", "key2" });
            EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new object[] { "key1", "xxx" });
            EPAssertionUtil.AssertProps(
                env.Listener("consume").AssertOneGetNewAndReset(),
                fieldsMerge,
                new object[] { "key1", "xxx" });

            // try not-matched case
            env.SendEventBean(new SupportBean("key2", 3));
            env.AssertListener(
                "s0",
                listener => {
                    Assert.IsNull(listener.LastOldData);
                    EPAssertionUtil.AssertProps(
                        listener.GetAndResetLastNewData()[0],
                        fieldsMerge,
                        new object[] { "key2", "abc" });
                });
            env.AssertPropsNew("consume", fieldsMerge, new object[] { "key2", "abc" });
            env.AssertThat(
                () => {
                    Assert.IsNull(window.LastUpdateOld);
                    EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new object[] { "key2", "abc" });
                });

            env.UndeployAll();
        }

        private void RunAssertionLimitation(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('window') @public create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
            env.CompileDeploy("insert into MyVDW select * from SupportBean", path);

            // cannot iterate named window
            env.AssertIterator("window", enumerator => Assert.IsFalse(enumerator.MoveNext()));

            // test data window aggregation (rows not included in aggregation)
            env.CompileDeploy("@name('s0') select window(theString) as val0 from MyVDW", path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 100));
            env.AssertListener(
                "s0",
                listener => EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] { "S1", "E1" },
                    (string[])listener.AssertOneGetNewAndReset().Get("val0")));

            env.UndeployAll();
        }

        private void RunAssertionJoinAndLifecyle(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdw(1, 'abc') as SupportBean", path);
            var fields = "st0.id,vdw.theString,vdw.intPrimitive".SplitCsv();

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
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
            env.CompileDeploy("@name('s0') select * from MyVDW vdw, SupportBean_ST0#lastevent st0", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            env.AssertPropsNew("s0", fields, new object[] { "E1", "S1", 100 });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria join
            env.CompileDeploy(
                    "@name('s0') select * from MyVDW vdw, SupportBean_ST0#lastevent st0 where vdw.theString = st0.id",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.RequestedLookups[1], "theString=(String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1" }, window.LastAccessKeys);
            env.AssertListenerNotInvoked("s0");
            env.SendEventBean(new SupportBean_ST0("S1", 0));
            env.AssertPropsNew("s0", fields, new object[] { "S1", "S1", 100 });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "S1" }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multi-criteria join
            env.CompileDeploy(
                "@name('s0') select vdw.theString from MyVDW vdw, SupportBeanRange#lastevent st0 " +
                "where vdw.theString = st0.id and longPrimitive = keyLong and intPrimitive between rangeStart and rangeEnd",
                path);
            env.AddListener("s0");
            AssertIndexSpec(
                window.RequestedLookups[1],
                "theString=(String)|longPrimitive=(Long)",
                "intPrimitive[,](Integer)");

            env.SendEventBean(SupportBeanRange.MakeKeyLong("S1", 50L, 80, 120));
            env.AssertPropsNew("s0", "vdw.theString".SplitCsv(), new object[] { "S1" });
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] { "S1", 50L, new VirtualDataWindowKeyRange(80, 120) },
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
            env.CompileDeploy("@name('s0') select (select col1 from MyVDW vdw) from SupportBean_ST0", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            env.AssertPropsNew("s0", "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria subquery
            env.CompileDeploy(
                    "@name('s0') select (select col1 from MyVDW vdw where col1=st0.id) as val0 from SupportBean_ST0 st0",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { null });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1" }, window.LastAccessKeys);
            env.SendEventBean(new SupportBean_ST0("key1", 0));
            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "key1" }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multi-criteria subquery
            env.CompileDeploy(
                    "@name('s0') select " +
                    "(select col1 from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd) as val0 " +
                    "from SupportBeanRange r",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)|col2=(String)", "col3[,](Integer)");

            env.SendEventBean(new SupportBeanRange("key1", "key2", 5, 10));
            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] { "key1", "key2", new VirtualDataWindowKeyRange(5, 10) },
                window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test aggregation
            env.CompileDeploy("@public create schema SampleEvent as (id string)", path);
            env.CompileDeploy("@public create window MySampleWindow.test:vdw() as SampleEvent", path);
            env.CompileDeploy(
                    "@name('s0') select (select count(*) as cnt from MySampleWindow) as c0 " + "from SupportBean ste",
                    path)
                .AddListener("s0");

            var thewindow = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MySampleWindow");
            var row1 = Collections.SingletonDataMap("id", "V1");
            thewindow.Data = Collections.SingletonSet<object>(row1);

            env.SendEventBean(new SupportBean("E1", 1));
            env.AssertEqualsNew("s0", "c0", 1L);

            var rows = new HashSet<object>();
            rows.Add(row1);
            rows.Add(Collections.SingletonDataMap("id", "V2"));
            thewindow.Data = rows;

            env.SendEventBean(new SupportBean("E2", 2));
            env.AssertEqualsNew("s0", "c0", 2L);

            env.UndeployAll();
        }

        private void RunAssertionContextWJoin(RegressionEnvironment env)
        {
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));
            var path = new RegressionPath();

            // prepare
            env.CompileDeploy(
                "@public create context MyContext coalesce by " +
                "consistent_hash_crc32(theString) from SupportBean, " +
                "consistent_hash_crc32(p00) from SupportBean_S0 " +
                "granularity 4 preallocate",
                path);
            env.CompileDeploy("@public context MyContext create window MyWindow.test:vdw() as SupportBean", path);

            // join
            var eplSubquerySameCtx = "@name('s0') context MyContext " +
                                     "select * from SupportBean_S0 as s0 unidirectional, MyWindow as mw where mw.theString = s0.p00";
            env.CompileDeploy(eplSubquerySameCtx, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private void RunAssertionContextWSubquery(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));

            env.CompileDeploy(
                "@public create context MyContext coalesce by " +
                "consistent_hash_crc32(theString) from SupportBean, " +
                "consistent_hash_crc32(p00) from SupportBean_S0 " +
                "granularity 4 preallocate",
                path);
            env.CompileDeploy("@public context MyContext create window MyWindow.test:vdw() as SupportBean", path);

            // subquery - same context
            var eplSubquerySameCtx = "context MyContext " +
                                     "select (select intPrimitive from MyWindow mw where mw.theString = s0.p00) as c0 " +
                                     "from SupportBean_S0 s0";
            env.CompileDeploy("@name('s0') " + eplSubquerySameCtx, path).AddListener("s0");
            env.CompileDeploy("@Hint('disable_window_subquery_indexshare') @name('s1') " + eplSubquerySameCtx, path);

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            env.AssertEqualsNew("s0", "c0", 1);
            env.UndeployModuleContaining("s0");

            // subquery - no context
            var eplSubqueryNoCtx = "select (select intPrimitive from MyWindow mw where mw.theString = s0.p00) as c0 " +
                                   "from SupportBean_S0 s0";
            env.TryInvalidCompile(
                path,
                eplSubqueryNoCtx,
                "Failed to plan subquery number 1 querying MyWindow: Mismatch in context specification, the context for the named window 'MyWindow' is 'MyContext' and the query specifies no context  [select (select intPrimitive from MyWindow mw where mw.theString = s0.p00) as c0 from SupportBean_S0 s0]");

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
            EPAssertionUtil.AssertProps(result.Array[0], "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(Array.Empty<object>(), window.LastAccessKeys);

            // test single-criteria FAF
            result = env.CompileExecuteFAF("select col1 from MyVDW vdw where col1='key1'", path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)", "");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "key1" }, window.LastAccessKeys);

            // test multi-criteria subquery
            result = env.CompileExecuteFAF(
                "select col1 from MyVDW vdw where col1='key1' and col2='key2' and col3 between 5 and 15",
                path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)|col2=(String)", "col3[,](Double)");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] { "key1", "key2", new VirtualDataWindowKeyRange(5d, 15d) },
                window.LastAccessKeys);

            // test multi-criteria subquery
            result = env.CompileExecuteFAF(
                "select col1 from MyVDW vdw where col1='key1' and col2>'key0' and col3 between 5 and 15",
                path);
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)", "col3[,](Double)|col2>(String)");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsAnyOrder(
                new object[] { "key1", new VirtualDataWindowKeyRange(5d, 15d), "key0" },
                window.LastAccessKeys);

            env.UndeployAll();
        }

        private void RunAssertionOnDelete(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var window = RegisterTypeSetMapData(env, path);

            // test no-criteria on-delete
            env.CompileDeploy("@name('s0') on SupportBean_ST0 delete from MyVDW vdw", path).AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            env.AssertPropsNew("s0", "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test single-criteria on-delete
            env.CompileDeploy("@name('s0') on SupportBean_ST0 st0 delete from MyVDW vdw where col1=st0.id", path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)", "");

            env.SendEventBean(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1" }, window.LastAccessKeys);
            env.AssertListenerNotInvoked("s0");
            env.SendEventBean(new SupportBean_ST0("key1", 0));
            env.AssertPropsNew("s0", "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { "key1" }, window.LastAccessKeys);
            env.UndeployModuleContaining("s0");

            // test multie-criteria on-delete
            env.CompileDeploy(
                    "@name('s0') on SupportBeanRange r delete " +
                    "from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd",
                    path)
                .AddListener("s0");
            AssertIndexSpec(window.LastRequestedLookup, "col1=(String)|col2=(String)", "col3[,](Integer)");
            Assert.AreEqual("MyVDW", window.LastRequestedLookup.NamedWindowName);
            Assert.IsNotNull(window.LastRequestedLookup.StatementId);
            Assert.AreEqual("s0", window.LastRequestedLookup.StatementName);
            Assert.AreEqual(1, window.LastRequestedLookup.StatementAnnotations.Length);
            Assert.IsFalse(window.LastRequestedLookup.IsFireAndForget);

            env.SendEventBean(new SupportBeanRange("key1", "key2", 5, 10));
            env.AssertPropsNew("s0", "col1".SplitCsv(), new object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] { "key1", "key2", new VirtualDataWindowKeyRange(5, 10) },
                window.LastAccessKeys);

            env.UndeployAll();
        }

        private void RunAssertionInvalid(RegressionEnvironment env)
        {
            string epl;

            epl = "create window ABC.invalid:invalid() as SupportBean";
            env.TryInvalidCompile(
                epl,
                "Failed to validate data window declaration: Virtual data window forge class " +
                typeof(SupportBean).FullName +
                " does not implement the interface " +
                typeof(VirtualDataWindowForge).FullName);

            epl = "select * from SupportBean.test:vdw()";
            env.TryInvalidCompile(
                epl,
                "Failed to validate data window declaration: Virtual data window requires use with a named window in the create-window syntax [select * from SupportBean.test:vdw()]");

            env.TryInvalidCompile(
                "create window ABC.test:exceptionvdw() as SupportBean",
                "Failed to validate data window declaration: Validation exception initializing virtual data window 'ABC': This is a test exception [create window ABC.test:exceptionvdw() as SupportBean]");
        }

        private void RunAssertionManagementEvents(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var vdw = RegisterTypeSetMapData(env, path);

            // create-index event
            vdw.Events.Clear();
            env.CompileDeploy("@name('idx') create index IndexOne on MyVDW (col3, col2 btree)", path);
            var startEvent = (VirtualDataWindowEventStartIndex)vdw.Events[0];
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
            var stopEvent = (VirtualDataWindowEventStopIndex)vdw.Events[0];
            Assert.AreEqual("MyVDW", stopEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", stopEvent.IndexName);

            // stop named window
            vdw.Events.Clear();
            env.UndeployAll();
            var stopWindow = (VirtualDataWindowEventStopWindow)vdw.Events[0];
            Assert.AreEqual("MyVDW", stopWindow.NamedWindowName);
        }

        private void RunAssertionIndexChoicesJoinUniqueVirtualDW(RegressionEnvironment env)
        {
            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () => {
                var fields = "vdw.theString,vdw.intPrimitive,ssb1.i1".SplitCsv();
                env.SendEventBean(new SupportSimpleBeanOne("S1", 1, 102, 103));
                env.AssertPropsNew("s0", fields, new object[] { "S1", 101, 1 });
            };

            var testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (var caseEnum in testCases) {
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "theString",
                    "where vdw.theString = ssb1.s1",
                    true,
                    assertSendEvents);
                TryAssertionVirtualDW(env, caseEnum, "i1", "where vdw.theString = ssb1.s1", false, assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "intPrimitive",
                    "where vdw.theString = ssb1.s1",
                    false,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "longPrimitive",
                    "where vdw.longPrimitive = ssb1.l1",
                    true,
                    assertSendEvents);
                TryAssertionVirtualDW(
                    env,
                    caseEnum,
                    "longPrimitive,theString",
                    "where vdw.theString = ssb1.s1 and vdw.longPrimitive = ssb1.l1",
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
            SupportVirtualDWForge.UniqueKeys = new HashSet<string>(uniqueFields.SplitCsv());

            var path = new RegressionPath();
            env.CompileDeploy("@public create window MyVDW.test:vdw() as SupportBean", path);
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 101);
            supportBean.DoublePrimitive = 102;
            supportBean.LongPrimitive = 103;
            window.Data = Collections.SingletonSet<object>(supportBean);

            var eplUnique = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                            "@name('s0') select * from ";

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

        private enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
        }

        private SupportVirtualDW RegisterTypeSetMapData(
            RegressionEnvironment env,
            RegressionPath path)
        {
            SupportVirtualDWForge.Initializations.Clear();
            env.CompileDeploy("@name('create-nw') @public create window MyVDW.test:vdw() as MapType", path);

            Assert.AreEqual(1, SupportVirtualDWForge.Initializations.Count);
            var forgeContext = SupportVirtualDWForge.Initializations[0];
            Assert.AreEqual("MyVDW", forgeContext.EventType.Name);
            Assert.IsNotNull("MyVDW", forgeContext.NamedWindowName);
            Assert.AreEqual(0, forgeContext.Parameters.Length);
            Assert.AreEqual(0, forgeContext.ParameterExpressions.Length);
            Assert.IsNotNull(forgeContext.ViewForgeEnv);

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext(env, "/virtualdw/MyVDW");
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

            var split = hashfields.Split("\\|");
            IList<string> found = new List<string>();
            for (var i = 0; i < split.Length; i++) {
                var field = fields[i];
                var result = field.PropertyName + field.Operator.Value.GetOp() + "(" + field.LookupValueType + ")";
                found.Add(result);
            }

            EPAssertionUtil.AssertEqualsAnyOrder(split, found.ToArray());
        }

        private VirtualDataWindow GetFromContext(
            RegressionEnvironment env,
            string name)
        {
            try {
                return (VirtualDataWindow)env.Runtime.Context.Lookup(name);
            }
            catch (NamingException e) {
                throw new EPRuntimeException("Name '" + name + "' could not be looked up");
            }
        }
    }
} // end of namespace