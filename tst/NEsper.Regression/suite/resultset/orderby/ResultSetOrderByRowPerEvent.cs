///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
	public class ResultSetOrderByRowPerEvent {
	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ResultSetIteratorAggregateRowPerEvent());
	        execs.Add(new ResultSetAliases());
	        execs.Add(new ResultSetRowPerEventJoinOrderFunction());
	        execs.Add(new ResultSetRowPerEventOrderFunction());
	        execs.Add(new ResultSetRowPerEventSum());
	        execs.Add(new ResultSetRowPerEventMaxSum());
	        execs.Add(new ResultSetRowPerEventSumHaving());
	        execs.Add(new ResultSetAggOrderWithSum());
	        execs.Add(new ResultSetRowPerEventJoin());
	        execs.Add(new ResultSetRowPerEventJoinMax());
	        execs.Add(new ResultSetAggHaving());
	        return execs;
	    }

	    private class ResultSetIteratorAggregateRowPerEvent : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = new string[]{"symbol", "sumPrice"};
	            var epl = "@name('s0') select symbol, sum(price) as sumPrice from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBeanString("CAT"));
	            env.SendEventBean(new SupportBeanString("IBM"));
	            env.SendEventBean(new SupportBeanString("KGB"));

	            SendEvent(env, "CAT", 50);
	            SendEvent(env, "IBM", 49);
	            SendEvent(env, "CAT", 15);
	            SendEvent(env, "IBM", 100);
	            env.AssertPropsPerRowIterator("s0", fields,
	                new object[][]{
	                    new object[] {"CAT", 214d},
	                    new object[] {"CAT", 214d},
	                    new object[] {"IBM", 214d},
	                    new object[] {"IBM", 214d},
	                });

	            SendEvent(env, "KGB", 75);
	            env.AssertPropsPerRowIterator("s0", fields,
	                new object[][]{
	                    new object[] {"CAT", 289d},
	                    new object[] {"CAT", 289d},
	                    new object[] {"IBM", 289d},
	                    new object[] {"IBM", 289d},
	                    new object[] {"KGB", 289d},
	                });

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAliases : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol as mySymbol, sum(price) as mySum from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by mySymbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);

	            env.Milestone(0);

	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);
	            SendEvent(env, "CAT", 5);

	            env.Milestone(1);

	            SendEvent(env, "CAT", 6);

	            var fields = "mySymbol,mySum".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0}, new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventJoinOrderFunction : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by volume*sum(price), symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 2);

	            env.Milestone(0);

	            SendEvent(env, "KGB", 1);
	            SendEvent(env, "CMU", 3);
	            SendEvent(env, "IBM", 6);
	            SendEvent(env, "CAT", 6);

	            env.Milestone(1);

	            SendEvent(env, "CAT", 5);

	            env.SendEventBean(new SupportBeanString("CAT"));
	            env.SendEventBean(new SupportBeanString("IBM"));
	            env.SendEventBean(new SupportBeanString("CMU"));

	            env.Milestone(2);

	            env.SendEventBean(new SupportBeanString("KGB"));
	            env.SendEventBean(new SupportBeanString("DOG"));

	            var fields = "symbol".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT"}, new object[] {"CAT"}, new object[] {"CMU"}, new object[] {"IBM"}, new object[] {"IBM"}, new object[] {"KGB"}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventOrderFunction : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by volume*sum(price), symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 2);
	            SendEvent(env, "KGB", 1);
	            SendEvent(env, "CMU", 3);
	            SendEvent(env, "IBM", 6);

	            env.Milestone(0);

	            SendEvent(env, "CAT", 6);
	            SendEvent(env, "CAT", 5);

	            var fields = "symbol".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT"}, new object[] {"CAT"}, new object[] {"CMU"}, new object[] {"IBM"}, new object[] {"IBM"}, new object[] {"KGB"}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);
	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);

	            env.Milestone(0);

	            SendEvent(env, "CAT", 5);
	            SendEvent(env, "CAT", 6);

	            var fields = "symbol,sum(price)".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0}, new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventMaxSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, max(sum(price)) from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);

	            env.Milestone(0);

	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);
	            SendEvent(env, "CAT", 5);

	            env.Milestone(1);

	            SendEvent(env, "CAT", 6);

	            var fields = "symbol,max(sum(price))".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0}, new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventSumHaving : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "having sum(price) > 0 " +
	                      "output every 6 events " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);
	            SendEvent(env, "CMU", 1);

	            env.Milestone(0);

	            SendEvent(env, "CMU", 2);
	            SendEvent(env, "CAT", 5);
	            SendEvent(env, "CAT", 6);

	            var fields = "symbol,sum(price)".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0}, new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggOrderWithSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) " +
	                      "output every 6 events " +
	                      "order by symbol, sum(price)";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);

	            env.Milestone(0);

	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);
	            SendEvent(env, "CAT", 5);
	            SendEvent(env, "CAT", 6);

	            var fields = "symbol,sum(price)".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 15.0}, new object[] {"CAT", 21.0}, new object[] {"CMU", 8.0}, new object[] {"CMU", 10.0}, new object[] {"IBM", 3.0}, new object[] {"IBM", 7.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by symbol, sum(price)";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);
	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);
	            SendEvent(env, "CAT", 5);

	            env.Milestone(0);

	            SendEvent(env, "CAT", 6);

	            env.SendEventBean(new SupportBeanString("CAT"));
	            env.SendEventBean(new SupportBeanString("IBM"));
	            env.SendEventBean(new SupportBeanString("CMU"));

	            var fields = "symbol,sum(price)".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0}, new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetRowPerEventJoinMax : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, max(sum(price)) from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "output every 6 events " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);
	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);

	            env.Milestone(0);

	            SendEvent(env, "CAT", 5);
	            SendEvent(env, "CAT", 6);

	            env.SendEventBean(new SupportBeanString("CAT"));
	            env.SendEventBean(new SupportBeanString("IBM"));

	            env.Milestone(1);

	            env.SendEventBean(new SupportBeanString("CMU"));

	            var fields = "symbol,max(sum(price))".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0}, new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}});

	            env.UndeployAll();
	        }
	    }

	    private class ResultSetAggHaving : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select symbol, sum(price) from " +
	                      "SupportMarketDataBean#length(10) as one, " +
	                      "SupportBeanString#length(100) as two " +
	                      "where one.symbol = two.theString " +
	                      "having sum(price) > 0 " +
	                      "output every 6 events " +
	                      "order by symbol";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.Milestone(0);

	            SendEvent(env, "IBM", 3);
	            SendEvent(env, "IBM", 4);
	            SendEvent(env, "CMU", 1);
	            SendEvent(env, "CMU", 2);

	            env.Milestone(1);

	            SendEvent(env, "CAT", 5);
	            SendEvent(env, "CAT", 6);

	            env.SendEventBean(new SupportBeanString("CAT"));
	            env.SendEventBean(new SupportBeanString("IBM"));
	            env.SendEventBean(new SupportBeanString("CMU"));

	            var fields = "symbol,sum(price)".SplitCsv();
	            env.AssertPropsPerRowNewOnly("s0", fields, new object[][]{
	                new object[] {"CAT", 11.0}, new object[] {"CAT", 11.0}, new object[] {"CMU", 21.0}, new object[] {"CMU", 21.0}, new object[] {"IBM", 18.0}, new object[] {"IBM", 18.0}});

	            env.UndeployAll();
	        }
	    }

	    private static void SendEvent(RegressionEnvironment env, string symbol, double price) {
	        var bean = new SupportMarketDataBean(symbol, price, 0L, null);
	        env.SendEventBean(bean);
	    }
	}
} // end of namespace
