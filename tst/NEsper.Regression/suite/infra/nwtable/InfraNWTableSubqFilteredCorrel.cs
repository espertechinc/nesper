///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
	public class InfraNWTableSubqFilteredCorrel {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        // named window tests
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, false, false, false));  // no-share
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, false, false, true));   // no-share create
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, false, false));   // share no-create
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, true, false));    // disable share no-create
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(true, true, true, true));     // disable share create

	        // table tests
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(false, false, false, false));  // table no-create
	        execs.Add(new InfraNWTableSubqFilteredCorrelAssertion(false, false, false, true));  // table create
	        return execs;
	    }

	    private class InfraNWTableSubqFilteredCorrelAssertion : RegressionExecution {
	        private readonly bool namedWindow;
	        private readonly bool enableIndexShareCreate;
	        private readonly bool disableIndexShareConsumer;
	        private readonly bool createExplicitIndex;

	        public InfraNWTableSubqFilteredCorrelAssertion(bool namedWindow, bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex) {
	            this.namedWindow = namedWindow;
	            this.enableIndexShareCreate = enableIndexShareCreate;
	            this.disableIndexShareConsumer = disableIndexShareConsumer;
	            this.createExplicitIndex = createExplicitIndex;
	        }

	        public void Run(RegressionEnvironment env) {
	            var createEpl = namedWindow ?
	                "@public create window MyInfra#keepall as select * from SupportBean" :
	                "@public create table MyInfra (theString string primary key, intPrimitive int primary key)";
	            if (enableIndexShareCreate) {
	                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
	            }
	            env.CompileDeploy(createEpl);
	            env.CompileDeploy("insert into MyInfra select theString, intPrimitive from SupportBean");

	            if (createExplicitIndex) {
	                env.CompileDeploy("@name('index') create index MyIndex on MyInfra(theString)");
	            }

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", -2));

	            var consumeEpl = "@name('consume') select (select intPrimitive from MyInfra(intPrimitive<0) sw where s0.p00=sw.theString) as val from S0 s0";
	            if (disableIndexShareConsumer) {
	                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
	            }
	            env.CompileDeploy(consumeEpl).AddListener("consume");

	            env.SendEventBean(new SupportBean_S0(10, "E1"));
	            AssertVal(env, null);

	            env.SendEventBean(new SupportBean_S0(20, "E2"));
	            AssertVal(env, -2);

	            env.SendEventBean(new SupportBean("E3", -3));
	            env.SendEventBean(new SupportBean("E4", 4));

	            env.SendEventBean(new SupportBean_S0(-3, "E3"));
	            AssertVal(env, -3);

	            env.SendEventBean(new SupportBean_S0(20, "E4"));
	            AssertVal(env, null);

	            env.UndeployModuleContaining("consume");
	            env.UndeployAll();
	        }

	        private void AssertVal(RegressionEnvironment env, object expected) {
	            env.AssertEqualsNew("s0", "val", expected);
	        }
	    }
	}
} // end of namespace
