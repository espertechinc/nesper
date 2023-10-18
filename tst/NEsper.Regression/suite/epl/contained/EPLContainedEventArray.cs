///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

// assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.contained
{
	public class EPLContainedEventArray {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLContainedEventDocSample());
	        execs.Add(new EPLContainedEventIntArray());
	        execs.Add(new EPLContainedStringArrayWithWhere());
	        return execs;
	    }

	    private class EPLContainedStringArrayWithWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create schema MyRow(id String);\n" +
	                      "@public @buseventtype create schema MyEvent(idsBefore string[], idsAfter string[]);\n" +
	                      "@name('s0') select id from MyEvent[select idsBefore, * from idsAfter@type(MyRow)] where id not in (idsBefore);\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            AssertSend(env, "A,B,C", "D,E", new object[][] {new object[] {"D"}, new object[] {"E"}});
	            AssertSend(env, "A,C", "C,A", null);
	            AssertSend(env, "A", "B", new object[][] {new object[] {"B"}});
	            AssertSend(env, "A,B", "F,B,A", new object[][] {new object[] {"F"}});

	            env.UndeployAll();
	        }

	        private void AssertSend(RegressionEnvironment env, string idsBeforeCSV, string idsAfterCSV, object[][] expected) {
	            var data = CollectionUtil.BuildMap("idsBefore", idsBeforeCSV.SplitCsv(), "idsAfter", idsAfterCSV.SplitCsv());
	            env.SendEventMap(data, "MyEvent");
	            if (expected == null) {
	                env.AssertListenerNotInvoked("s0");
	            } else {
	                env.AssertPropsPerRowLastNew("s0", "id".SplitCsv(), expected);
	            }
	        }
	    }

	    private class EPLContainedEventDocSample : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy(
	                "@public create schema IdContainer(id int);" +
	                    "@public create schema MyEvent(ids int[]);" +
	                    "select * from MyEvent[ids@type(IdContainer)];", path);

	            env.CompileDeploy(
	                    "@public create window MyWindow#keepall (id int);" +
	                    "on MyEvent[ids@type(IdContainer)] as my_ids \n" +
	                    "delete from MyWindow my_window \n" +
	                    "where my_ids.id = my_window.id;", path);

	            env.UndeployAll();
	        }
	    }

	    private class EPLContainedEventIntArray : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var path = new RegressionPath();
	            var epl = "create objectarray schema DeleteId(id int);" +
	                      "@public create window MyWindow#keepall as SupportBean;" +
	                      "insert into MyWindow select * from SupportBean;" +
	                      "on SupportBeanArrayCollMap[intArr@type(DeleteId)] delete from MyWindow where intPrimitive = id";
	            env.CompileDeploy(epl, path);

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", 2));

	            AssertCount(env, path, 2);
	            env.SendEventBean(new SupportBeanArrayCollMap(new int[]{1, 2}));
	            AssertCount(env, path, 0);

	            env.UndeployAll();
	        }
	    }

	    private static void AssertCount(RegressionEnvironment env, RegressionPath path, long i) {
	        env.AssertThat(() => Assert.AreEqual(i, env.CompileExecuteFAF("select count(*) as c0 from MyWindow", path).Array[0].Get("c0")));
	    }
	}
} // end of namespace
