///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
	/// <summary>
	/// NOTE: More table-related tests in "nwtable"
	/// </summary>
	public class InfraTableFilters : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        var path = new RegressionPath();
	        env.CompileDeploy("@public create table MyTable(pkey string primary key, col0 int)", path);
	        env.CompileDeploy("insert into MyTable select theString as pkey, intPrimitive as col0 from SupportBean", path);

	        for (var i = 0; i < 5; i++) {
	            env.SendEventBean(new SupportBean("E" + i, i));
	        }
	        var fields = "col0".SplitCsv();

	        // test FAF filter
	        env.AssertThat(() => {
	            var events = env.CompileExecuteFAF("select col0 from MyTable(pkey='E1')", path).Array;
	            EPAssertionUtil.AssertPropsPerRow(events, fields, new object[][]{new object[] {1}});
	        });

	        // test iterate
	        env.CompileDeploy("@name('iterate') select col0 from MyTable(pkey='E2')", path);
	        env.AssertPropsPerRowIterator("iterate", fields, new object[][]{new object[] {2}});
	        env.UndeployModuleContaining("iterate");

	        // test subquery
	        env.CompileDeploy("@name('subq') select (select col0 from MyTable(pkey='E3')) as col0 from SupportBean_S0", path).AddListener("subq");
	        env.SendEventBean(new SupportBean_S0(0));
	        env.AssertEqualsNew("subq", "col0", 3);
	        env.UndeployModuleContaining("subq");

	        // test join
	        env.TryInvalidCompile(path, "select col0 from SupportBean_S0, MyTable(pkey='E4')",
	            "Joins with tables do not allow table filter expressions, please add table filters to the where-clause instead [");

	        env.UndeployAll();
	    }
	}
} // end of namespace
