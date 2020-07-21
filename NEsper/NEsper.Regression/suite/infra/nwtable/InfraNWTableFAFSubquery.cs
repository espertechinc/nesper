///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
	public class InfraNWTableFAFSubquery : IndexBackingTableInfo {

	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new InfraFAFSubquerySimple(true));
	        execs.Add(new InfraFAFSubquerySimple(false));
	        execs.Add(new InfraFAFSubquerySimpleJoin());
	        execs.Add(new InfraFAFSubqueryInsert(true));
	        execs.Add(new InfraFAFSubqueryInsert(false));
	        execs.Add(new InfraFAFSubqueryUpdateUncorrelated());
	        execs.Add(new InfraFAFSubqueryDeleteUncorrelated());
	        execs.Add(new InfraFAFSubquerySelectCorrelated());
	        execs.Add(new InfraFAFSubqueryUpdateCorrelatedSet());
	        execs.Add(new InfraFAFSubqueryUpdateCorrelatedWhere());
	        execs.Add(new InfraFAFSubqueryDeleteCorrelatedWhere());
	        execs.Add(new InfraFAFSubqueryContextBothWindows());
	        execs.Add(new InfraFAFSubqueryContextSelect());
	        execs.Add(new InfraFAFSubquerySelectWhere());
	        execs.Add(new InfraFAFSubquerySelectGroupBy());
	        execs.Add(new InfraFAFSubquerySelectIndexPerfWSubstitution(true));
	        execs.Add(new InfraFAFSubquerySelectIndexPerfWSubstitution(false));
	        execs.Add(new InfraFAFSubquerySelectIndexPerfCorrelated(true));
	        execs.Add(new InfraFAFSubquerySelectIndexPerfCorrelated(false));
	        execs.Add(new InfraFAFSubqueryInvalid());
	        return execs;
	    }

	    public class InfraFAFSubqueryInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = "@public create window WinSB#keepall as SupportBean;\n" +
	                      "create context MyContext partition by id from SupportBean_S0;\n" +
	                      "context MyContext create window PartitionedWinS0#keepall as SupportBean_S0;\n";
	            env.Compile(epl, path);

	            TryInvalidFAFCompile(env, path, "select (select * from SupportBean#lastevent) from WinSB",
	                "Fire-and-forget queries only allow subqueries against named windows and tables");

	            TryInvalidFAFCompile(env, path, "select (select * from WinSB(theString='x')) from WinSB",
	                "Failed to plan subquery number 1 querying WinSB: Subqueries in fire-and-forget queries do not allow filter expressions");

	            TryInvalidFAFCompile(env, path, "select (select * from PartitionedWinS0) from WinSB",
	                "Failed to plan subquery number 1 querying PartitionedWinS0: Mismatch in context specification, the context for the named window 'PartitionedWinS0' is 'MyContext' and the query specifies no context");
	        }
	    }

	    private class InfraFAFSubquerySelectIndexPerfCorrelated : RegressionExecution {
	        private bool namedWindow;

	        public InfraFAFSubquerySelectIndexPerfCorrelated(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinSB#keepall as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            if (namedWindow) {
	                epl += "@public create window Infra#unique(id) as (id int, value string);\n";
	            } else {
	                epl += "@public create table Infra(id int primary key, value string);\n";
	            }
	            epl += "@public create index InfraIndex on Infra(value);\n" +
	                "insert into Infra select id, p00 as value from SupportBean_S0;\n";
	            env.CompileDeploy(epl, path);

	            var numRows = 10000;  // less than 1M
	            for (var i = 0; i < numRows; i++) {
	                SendSB(env, "v" + i, 0);
	                SendS0(env, -1 * i, "v" + i);
	            }

	            EPFireAndForgetQueryResult result = null;
	            
	            var delta = PerformanceObserver.TimeMillis(
		            () => {
			            var query = "select (select id from Infra as i where i.value = wsb.theString) as c0 from WinSB as wsb";
			            result = CompileExecute(env, path, query);
		            });
	            
	            Assert.That(delta, Is.LessThan(1000), "delta is " + delta);
	            Assert.AreEqual(numRows, result.Array.Length);
	            for (var i = 0; i < numRows; i++) {
	                Assert.AreEqual(-1 * i, result.Array[i].Get("c0"));
	            }

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubquerySelectIndexPerfWSubstitution : RegressionExecution {
	        private bool namedWindow;

	        public InfraFAFSubquerySelectIndexPerfWSubstitution(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            if (namedWindow) {
	                epl += "@public create window Infra#unique(id) as (id int, value string);\n";
	            } else {
	                epl += "@public create table Infra(id int primary key, value string);\n";
	            }
	            epl += "@public create index InfraIndex on Infra(value);\n" +
	                "insert into Infra select id, p00 as value from SupportBean_S0;\n";
	            env.CompileDeploy(epl, path);

	            SendSB(env, "E1", -1);
	            for (var i = 0; i < 10000; i++) {
	                SendS0(env, i, "v" + i);
	            }

	            var query = "select (select id from Infra as i where i.value = ?:p0:string) as c0 from WinSB";
	            var compiled = env.CompileFAF(query, path);
	            var prepared = env.Runtime.FireAndForgetService.PrepareQueryWithParameters(compiled);

	            var delta = PerformanceObserver.TimeMillis(
		            () => {
			            for (var i = 5000; i < 6000; i++) {
				            prepared.SetObject("p0", "v" + i);
				            var result = env.Runtime.FireAndForgetService.ExecuteQuery(prepared);
				            Assert.AreEqual(1, result.Array.Length);
				            Assert.AreEqual(i, result.Array[0].Get("c0"));
			            }
		            });

	            Assert.That(delta, Is.LessThan(1000), "delta is " + delta);

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubquerySelectWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#keepall as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            var query = "select (select intPrimitive from WinSB where theString = 'x') as c0 from WinS0";
	            SendS0(env, 0, null);
	            AssertQuerySingle(env, path, query, null);

	            SendSB(env, "E1", 1);
	            AssertQuerySingle(env, path, query, null);

	            SendSB(env, "x", 2);
	            AssertQuerySingle(env, path, query, 2);

	            SendSB(env, "x", 3);
	            AssertQuerySingle(env, path, query, null);

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubquerySelectGroupBy : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#keepall as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            var query = "select (select theString, sum(intPrimitive) as thesum from WinSB group by theString) as c0 from WinS0";
	            SendS0(env, 0, null);

	            SendSB(env, "E1", 10);
	            SendSB(env, "E1", 11);
	            var result = (IDictionary<string, object>) RunQuerySingle(env, path, query);
	            Assert.AreEqual("E1", result.Get("theString"));
	            Assert.AreEqual(21, result.Get("thesum"));

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryContextSelect : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "create context MyContext partition by id from SupportBean_S0;\n" +
	                    "@public context MyContext create window WinS0#keepall as SupportBean_S0;\n" +
	                    "context MyContext on SupportBean_S0 as s0 merge WinS0 insert select *;\n" +
	                    "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendSB(env, "E1", 1);

	            var query = "context MyContext select p00, (select theString from WinSB) as theString from WinS0";
	            AssertQueryMultirowAnyOrder(env, path, query, "p00,theString", new object[][]{
		            new object[] {"a", "E1"}, 
		            new object[] {"b", "E1"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryContextBothWindows : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "create context MyContext partition by id from SupportBean_S0, id from SupportBean_S1;\n" +
	                    "@public context MyContext create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public context MyContext create window WinS1#keepall as SupportBean_S1;\n" +
	                    "context MyContext on SupportBean_S0 as s0 merge WinS0 insert select *;\n" +
	                    "context MyContext on SupportBean_S1 as s1 merge WinS1 insert select *;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendS0(env, 3, "c");
	            SendS1(env, 1, "X");
	            SendS1(env, 2, "Y");
	            SendS1(env, 3, "Z");

	            var query = "context MyContext select p00, (select p10 from WinS1) as p10 from WinS0";
	            AssertQueryMultirowAnyOrder(env, path, query, "p00,p10", new object[][] {
		            new object[]{"a", "X"}, 
		            new object[]{"b", "Y"}, 
		            new object[]{"c", "Z"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryDeleteCorrelatedWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#unique(intPrimitive) as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendS0(env, 3, "c");

	            SendSB(env, "a", 0);
	            SendSB(env, "b", 2);

	            var update = "delete from WinS0 as wins0 where id = (select intPrimitive from WinSB winsb where winsb.theString = wins0.p00)";
	            CompileExecute(env, path, update);

	            var query = "select * from WinS0";
	            AssertQueryMultirowAnyOrder(env, path, query, "id,p00", new object[][] {
		            new object[]{1, "a"}, 
		            new object[]{3, "c"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryUpdateCorrelatedWhere : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#unique(intPrimitive) as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendS0(env, 3, "c");

	            SendSB(env, "a", 0);
	            SendSB(env, "b", 2);

	            var update = "update WinS0 as wins0 set p00 = 'x' where id = (select intPrimitive from WinSB winsb where winsb.theString = wins0.p00)";
	            CompileExecute(env, path, update);

	            var query = "select * from WinS0";
	            AssertQueryMultirowAnyOrder(env, path, query, "id,p00", new object[][] {
		            new object[]{1, "a"}, 
		            new object[]{2, "x"}, 
		            new object[]{3, "c"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryUpdateCorrelatedSet : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#unique(intPrimitive) as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendS0(env, 3, "c");

	            SendSB(env, "X", 2);
	            SendSB(env, "Y", 1);
	            SendSB(env, "Z", 3);

	            var update = "update WinS0 as wins0 set p00 = (select theString from WinSB winsb where winsb.intPrimitive = wins0.id)";
	            CompileExecute(env, path, update);

	            var query = "select * from WinS0";
	            AssertQueryMultirowAnyOrder(env, path, query, "id,p00", new object[][] {
		            new object[]{1, "Y"}, 
		            new object[]{2, "X"}, 
		            new object[]{3, "Z"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubquerySelectCorrelated : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinSB#unique(intPrimitive) as SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            SendS0(env, 1, "a");
	            SendS0(env, 2, "b");
	            SendS0(env, 3, "c");

	            SendSB(env, "X", 2);
	            SendSB(env, "Y", 1);
	            SendSB(env, "Z", 3);

	            var query = "select id, (select theString from WinSB winsb where winsb.intPrimitive = wins0.id) as theString from WinS0 as wins0";
	            AssertQueryMultirowAnyOrder(env, path, query, "id,theString", new object[][] {
		            new object[]{1, "Y"}, 
		            new object[]{2, "X"}, 
		            new object[]{3, "Z"}
	            });

	            SendSB(env, "Q", 1);
	            SendSB(env, "R", 3);
	            SendSB(env, "S", 2);
	            AssertQueryMultirowAnyOrder(env, path, query, "id,theString", new object[][] {
		            new object[]{1, "Q"}, 
		            new object[]{2, "S"}, 
		            new object[]{3, "R"}
	            });

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryDeleteUncorrelated : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window Win#keepall as (key string, value int);\n" +
	                    "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);
	            CompileExecute(env, path, "insert into Win select 'k1' as key, 1 as value");
	            CompileExecute(env, path, "insert into Win select 'k2' as key, 2 as value");
	            CompileExecute(env, path, "insert into Win select 'k3' as key, 3 as value");

	            var delete = "delete from Win where value = (select intPrimitive from WinSB)";
	            var query = "select * from Win";

	            AssertQueryMultirowAnyOrder(env, path, query, "key,value", new object[][] {
		            new object[]{"k1", 1}, 
		            new object[]{"k2", 2}, 
		            new object[]{"k3", 3}
	            });

	            CompileExecute(env, path, delete);
	            AssertQueryMultirowAnyOrder(env, path, query, "key,value", new object[][]{ 
		            new object[]{"k1", 1},  
		            new object[]{"k2", 2},  
		            new object[]{"k3", 3}});

	            SendSB(env, "E1", 2);
	            CompileExecute(env, path, delete);
	            AssertQueryMultirowAnyOrder(env, path, query, "key,value", new object[][]{ 
		            new object[]{"k1", 1},  
		            new object[]{"k3", 3}});

	            SendSB(env, "E1", 1);
	            CompileExecute(env, path, delete);
	            AssertQueryMultirowAnyOrder(env, path, query, "key,value", new object[][]{ 
		            new object[]{"k3", 3}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryUpdateUncorrelated : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window Win#lastevent as (value int);\n" +
	                    "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            env.CompileDeploy(epl, path);
	            CompileExecute(env, path, "insert into Win select 1 as value");

	            var update = "update Win set value = (select intPrimitive from WinSB)";
	            var query = "select value as c0 from Win";

	            AssertQuerySingle(env, path, query, 1);

	            CompileExecute(env, path, update);
	            AssertQuerySingle(env, path, query, null);

	            SendSB(env, "E1", 10);
	            CompileExecute(env, path, update);
	            AssertQuerySingle(env, path, query, 10);

	            SendSB(env, "E2", 20);
	            CompileExecute(env, path, update);
	            AssertQuerySingle(env, path, query, 20);

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubqueryInsert : RegressionExecution {
	        private bool namedWindow;

	        public InfraFAFSubqueryInsert(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = "@public create window Win#keepall as (value string);\n";
	            if (namedWindow) {
	                epl +=
	                    "@public create window InfraSB#lastevent as SupportBean;\n" +
	                        "insert into InfraSB select * from SupportBean;\n";
	            } else {
	                epl +=
	                    "@public create table InfraSB(theString string);\n" +
	                        "on SupportBean as sb merge InfraSB as issb" +
	                        "  when not matched then insert select theString when matched then update set issb.theString=sb.theString;\n";

	            }
	            env.CompileDeploy(epl, path);

	            var insert = "insert into Win(value) select (select theString from InfraSB)";
	            var query = "select * from Win";

	            CompileExecute(env, path, insert);
	            AssertQueryMultirowAnyOrder(env, path, query, "value", new object[][]{ 
		            new object[]{null}});

	            SendSB(env, "E1", 0);
	            CompileExecute(env, path, insert);
	            AssertQueryMultirowAnyOrder(env, path, query, "value", new object[][]{ 
		            new object[]{null},  
		            new object[]{"E1"}});

	            SendSB(env, "E2", 0);
	            CompileExecute(env, path, insert);
	            AssertQueryMultirowAnyOrder(env, path, query, "value", new object[][]{ 
		            new object[]{null},  
		            new object[]{"E1"},  
		            new object[]{"E2"}});

	            env.UndeployAll();
	        }
	    }

	    private class InfraFAFSubquerySimpleJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "@public create window WinS0#keepall as SupportBean_S0;\n" +
	                    "@public create window WinS1#keepall as SupportBean_S1;\n" +
	                    "insert into WinSB select * from SupportBean;\n" +
	                    "insert into WinS0 select * from SupportBean_S0;\n" +
	                    "insert into WinS1 select * from SupportBean_S1;\n";
	            var query = "select (select theString from WinSB) as c0, p00, p10 from WinS0, WinS1";
	            env.CompileDeploy(epl, path);

	            AssertQueryNoRows(env, path, query, typeof(string));

	            SendS0(env, 1, "S0_0");
	            SendS1(env, 2, "S1_0");
	            AssertQuerySingle(env, path, query, null);

	            SendSB(env, "SB_0", 0);
	            AssertQuerySingle(env, path, query, "SB_0");

	            SendS0(env, 3, "S0_1");
	            AssertQueryMultirowAnyOrder(env, path, query, "c0,p00,p10", new object[][]{ 
		            new object[]{"SB_0", "S0_0", "S1_0"},  
		            new object[]{"SB_0", "S0_1", "S1_0"}});

	            env.UndeployAll();

	        }
	    }

	    private class InfraFAFSubquerySimple : RegressionExecution {
	        bool namedWindow;

	        public InfraFAFSubquerySimple(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl =
	                "@public create window WinSB#lastevent as SupportBean;\n" +
	                    "insert into WinSB select * from SupportBean;\n";
	            if (namedWindow) {
	                epl +=
	                    "@public create window InfraS0#lastevent as SupportBean_S0;\n" +
	                        "insert into InfraS0 select * from SupportBean_S0;\n";
	            } else {
	                epl +=
	                    "@public create table InfraS0(id int primary key, p00 string);\n" +
	                        "on SupportBean_S0 as s0 merge InfraS0 as is0 where s0.id = is0.id" +
	                        "  when not matched then insert select id, p00 when matched then update set is0.p00=s0.p00;\n";
	            }
	            var query = "select (select p00 from InfraS0) as c0 from WinSB";
	            env.CompileDeploy(epl, path);

	            AssertQueryNoRows(env, path, query, typeof(string));

	            SendSB(env, "E1", 1);
	            AssertQuerySingle(env, path, query, null);

	            SendS0(env, 1, "a");
	            AssertQuerySingle(env, path, query, "a");

	            SendS0(env, 1, "b");
	            AssertQuerySingle(env, path, query, "b");

	            env.UndeployAll();
	        }
	    }

	    private static void AssertQueryNoRows(RegressionEnvironment env, RegressionPath path, string query, Type resultType) {
	        var compiled = env.CompileFAF(query, path);
	        var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
	        Assert.AreEqual(0, result.Array == null ? 0 : result.Array.Length);
	        Assert.AreEqual(result.EventType.GetPropertyType("c0"), resultType);
	    }

	    private static void AssertQuerySingle(RegressionEnvironment env, RegressionPath path, string query, object c0Expected) {
	        var result = RunQuerySingle(env, path, query);
	        Assert.AreEqual(c0Expected, result);
	    }

	    private static object RunQuerySingle(RegressionEnvironment env, RegressionPath path, string query) {
	        var result = CompileExecute(env, path, query);
	        Assert.AreEqual(1, result.Array.Length);
	        return result.Array[0].Get("c0");
	    }

	    private static void AssertQueryMultirowAnyOrder(RegressionEnvironment env, RegressionPath path, string query, string fieldCSV, object[][] expected) {
	        var result = CompileExecute(env, path, query);
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fieldCSV.SplitCsv(), expected);
	    }

	    private static EPFireAndForgetQueryResult CompileExecute(RegressionEnvironment env, RegressionPath path, string query) {
	        var compiled = env.CompileFAF(query, path);
	        return env.Runtime.FireAndForgetService.ExecuteQuery(compiled);
	    }

	    private static void SendS0(RegressionEnvironment env, int id, string p00) {
	        env.SendEventBean(new SupportBean_S0(id, p00));
	    }

	    private static void SendS1(RegressionEnvironment env, int id, string p10) {
	        env.SendEventBean(new SupportBean_S1(id, p10));
	    }

	    private static void SendSB(RegressionEnvironment env, string theString, int intPrimitive) {
	        env.SendEventBean(new SupportBean(theString, intPrimitive));
	    }
	}
} // end of namespace
