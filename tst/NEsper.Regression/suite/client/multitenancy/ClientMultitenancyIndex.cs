///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.multitenancy
{
	public class ClientMultitenancyIndex {

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientMultitenancyIndexTable());
	        return execs;
	    }

	    public class ClientMultitenancyIndexTable : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = "module com_test_app;\n" +
	                      "@public @buseventtype create schema VWAPrice(symbol string);\n" +
	                      "@public create table Basket(basket_id string primary key, symbol string primary key, weight double);\n" +
	                      "@public create index BasketIndex on Basket(symbol);\n";
	            env.CompileDeploy(epl, path);
	            env.CompileExecuteFAFNoResult("insert into Basket select '1' as basket_id, 'A' as symbol, 1 as weight", path);
	            env.CompileExecuteFAFNoResult("insert into Basket select '2' as basket_id, 'B' as symbol, 2 as weight", path);

	            epl = "@name('s0') select weight from Basket as bask, VWAPrice as v where bask.symbol = v.symbol;\n";
	            env.CompileDeploy(epl, path).AddListener("s0");

	            env.SendEventMap(Collections.SingletonDataMap("symbol", "A"), "VWAPrice");
	            env.AssertEqualsNew("s0", "weight", 1.0);

	            env.Milestone(0);

	            env.SendEventMap(Collections.SingletonDataMap("symbol", "B"), "VWAPrice");
	            env.AssertEqualsNew("s0", "weight", 2.0);

	            env.UndeployAll();
	        }
	    }
	}
} // end of namespace
