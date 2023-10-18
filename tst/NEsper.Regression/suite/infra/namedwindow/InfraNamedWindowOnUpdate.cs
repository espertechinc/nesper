///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.common.@internal.support.SupportBean_A; // assertEquals

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
	/// <summary>
	/// NOTE: More namedwindow-related tests in "nwtable"
	/// </summary>
	public class InfraNamedWindowOnUpdate {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new InfraUpdateNonPropertySet());
	        execs.Add(new InfraMultipleDataWindowIntersect());
	        execs.Add(new InfraMultipleDataWindowUnion());
	        execs.Add(new InfraSubclass());
	        execs.Add(new InfraUpdateCopyMethodBean());
	        execs.Add(new InfraUpdateWrapper());
	        execs.Add(new InfraUpdateMultikeyWArrayPrimitiveArray());
	        execs.Add(new InfraUpdateMultikeyWArrayTwoFields());
	        return execs;
	    }

	    private class InfraUpdateMultikeyWArrayTwoFields : RegressionExecution {

	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('create') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
	                      "insert into MyWindow select * from SupportEventWithManyArray;\n" +
	                      "on SupportEventWithIntArray as sewia " +
	                      "update MyWindow as mw set value = sewia.value " +
	                      "where mw.id = sewia.id and mw.intOne = sewia.array;\n";
	            env.CompileDeploy(epl);

	            env.SendEventBean(new SupportEventWithManyArray("ID1").WithIntOne(new int[] {1, 2}));
	            env.SendEventBean(new SupportEventWithManyArray("ID2").WithIntOne(new int[] {3, 4}));
	            env.SendEventBean(new SupportEventWithManyArray("ID3").WithIntOne(new int[] {1}));

	            env.Milestone(0);

	            env.SendEventBean(new SupportEventWithIntArray("ID2", new int[] {3, 4}, 10));
	            env.SendEventBean(new SupportEventWithIntArray("ID3", new int[] {1}, 11));
	            env.SendEventBean(new SupportEventWithIntArray("ID1", new int[] {1, 2}, 12));
	            env.SendEventBean(new SupportEventWithIntArray("IDX", new int[] {1}, 14));
	            env.SendEventBean(new SupportEventWithIntArray("ID1", new int[] {1, 2, 3}, 15));

	            env.AssertPropsPerRowIteratorAnyOrder("create", "id,value".SplitCsv(),
	                new object[][] {new object[] {"ID1", 12}, new object[] {"ID2", 10}, new object[] {"ID3", 11}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraUpdateMultikeyWArrayPrimitiveArray : RegressionExecution {

	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('create') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
	                      "insert into MyWindow select * from SupportEventWithManyArray;\n" +
	                      "on SupportEventWithIntArray as sewia " +
	                      "update MyWindow as mw set value = sewia.value " +
	                      "where mw.intOne = sewia.array;\n";
	            env.CompileDeploy(epl);

	            env.SendEventBean(new SupportEventWithManyArray("E1").WithIntOne(new int[] {1, 2}));
	            env.SendEventBean(new SupportEventWithManyArray("E2").WithIntOne(new int[] {3, 4}));
	            env.SendEventBean(new SupportEventWithManyArray("E3").WithIntOne(new int[] {1}));
	            env.SendEventBean(new SupportEventWithManyArray("E4").WithIntOne(new int[] {}));

	            env.Milestone(0);

	            env.SendEventBean(new SupportEventWithIntArray("U1", new int[] {3, 4}, 10));
	            env.SendEventBean(new SupportEventWithIntArray("U2", new int[] {1}, 11));
	            env.SendEventBean(new SupportEventWithIntArray("U3", new int[] {}, 12));
	            env.SendEventBean(new SupportEventWithIntArray("U4", new int[] {1, 2}, 13));

	            env.AssertPropsPerRowIteratorAnyOrder("create", "id,value".SplitCsv(),
	                new object[][] {new object[] {"E1", 13}, new object[] {"E2", 10}, new object[] {"E3", 11}, new object[] {"E4", 12}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraUpdateWrapper : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('window') create window MyWindow#keepall as select *, 1 as p0 from SupportBean;\n" +
	                      "insert into MyWindow select *, 2 as p0 from SupportBean;\n" +
	                      "on SupportBean_S0 update MyWindow set theString = 'x', p0 = 2;\n";
	            env.CompileDeploy(epl);
	            env.SendEventBean(new SupportBean("E1", 100));
	            env.SendEventBean(new SupportBean_S0(-1));
	            env.AssertPropsPerRowIterator("window", new string[]{"theString", "p0"}, new object[][]{new object[] {"x", 2}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraUpdateCopyMethodBean : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('window') create window MyWindowBeanCopyMethod#keepall as SupportBeanCopyMethod;\n" +
	                      "insert into MyWindowBeanCopyMethod select * from SupportBeanCopyMethod;\n" +
	                      "on SupportBean update MyWindowBeanCopyMethod set valOne = 'x';\n";
	            env.CompileDeploy(epl);
	            env.SendEventBean(new SupportBeanCopyMethod("a", "b"));
	            env.SendEventBean(new SupportBean());
	            env.AssertIterator("window", iterator => Assert.AreEqual("x", iterator.Advance().Get("valOne")));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.SERDEREQUIRED);
	        }
	    }

	    private class InfraUpdateNonPropertySet : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create window MyWindowUNP#keepall as SupportBean;\n" +
	                      "insert into MyWindowUNP select * from SupportBean;\n" +
	                      "@name('update') on SupportBean_S0 as sb " +
	                      "update MyWindowUNP as mywin" +
	                      " set mywin.setIntPrimitive(10)," +
	                      "     setBeanLongPrimitive999(mywin);\n";
	            env.CompileDeploy(epl).AddListener("update");

	            var fields = "intPrimitive,longPrimitive".SplitCsv();
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertPropsPerRowLastNew("update", fields, new object[][]{new object[] {10, 999L}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraMultipleDataWindowIntersect : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('create') create window MyWindowMDW#unique(theString)#length(2) as select * from SupportBean;\n" +
	                      "insert into MyWindowMDW select * from SupportBean;\n" +
	                      "on SupportBean_A update MyWindowMDW set intPrimitive=intPrimitive*100 where theString=id;\n";
	            env.CompileDeploy(epl).AddListener("create");

	            env.SendEventBean(new SupportBean("E1", 2));
	            env.SendEventBean(new SupportBean("E2", 3));
	            env.SendEventBean(new SupportBean_A("E2"));
	            env.AssertListener("create", listener => {
	                var newevents = listener.LastNewData;
	                var oldevents = listener.LastOldData;

	                Assert.AreEqual(1, newevents.Length);
	                EPAssertionUtil.AssertProps(newevents[0], "intPrimitive".SplitCsv(), new object[]{300});
	                Assert.AreEqual(1, oldevents.Length);
	                oldevents = EPAssertionUtil.Sort(oldevents, "theString");
	                EPAssertionUtil.AssertPropsPerRow(oldevents, "theString,intPrimitive".SplitCsv(), new object[][]{new object[] {"E2", 3}});
	            });

	            env.AssertPropsPerRowIteratorAnyOrder("create", "theString,intPrimitive".SplitCsv(), new object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraMultipleDataWindowUnion : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('create') create window MyWindowMU#unique(theString)#length(2) retain-union as select * from SupportBean;\n" +
	                      "insert into MyWindowMU select * from SupportBean;\n" +
	                      "on SupportBean_A update MyWindowMU mw set mw.intPrimitive=intPrimitive*100 where theString=id;\n";
	            env.CompileDeploy(epl).AddListener("create");

	            env.SendEventBean(new SupportBean("E1", 2));
	            env.SendEventBean(new SupportBean("E2", 3));
	            env.SendEventBean(new SupportBean_A("E2"));
	            env.AssertListener("create", listener => {
	                var newevents = listener.LastNewData;
	                var oldevents = listener.LastOldData;

	                Assert.AreEqual(1, newevents.Length);
	                EPAssertionUtil.AssertProps(newevents[0], "intPrimitive".SplitCsv(), new object[]{300});
	                Assert.AreEqual(1, oldevents.Length);
	                EPAssertionUtil.AssertPropsPerRow(oldevents, "theString,intPrimitive".SplitCsv(), new object[][]{new object[] {"E2", 3}});
	            });
	            env.AssertIterator("create", iterator => {
	                var events = EPAssertionUtil.Sort(iterator, "theString");
	                EPAssertionUtil.AssertPropsPerRow(events, "theString,intPrimitive".SplitCsv(), new object[][]{new object[] {"E1", 2}, new object[] {"E2", 300}});
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraSubclass : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('create') create window MyWindowSC#keepall as select * from SupportBeanAbstractSub;\n" +
	                      "insert into MyWindowSC select * from SupportBeanAbstractSub;\n" +
	                      "on SupportBean update MyWindowSC set v1=theString, v2=theString;\n";
	            env.CompileDeploy(epl).AddListener("create");

	            env.SendEventBean(new SupportBeanAbstractSub("value2"));
	            env.ListenerReset("create");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsPerRowLastNew("create", new string[]{"v1", "v2"}, new object[][]{new object[] {"E1", "E1"}});

	            env.UndeployAll();
	        }
	    }

	    // Don't delete me, dynamically-invoked
	    public static void SetBeanLongPrimitive999(SupportBean @event) {
	        @event.LongPrimitive = 999;
	    }
	}
} // end of namespace
