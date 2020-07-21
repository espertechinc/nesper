///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;


namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreConcat : RegressionExecution {

	    public void Run(RegressionEnvironment env) {
	        var fields = "c1,c2,c3".SplitCsv();
	        var builder = new SupportEvalBuilder("SupportBean_S0")
	            .WithExpression(fields[0], "p00 || p01")
	            .WithExpression(fields[1], "p00 || p01 || p02")
	            .WithExpression(fields[2], "p00 || '|' || p01");

	        builder.WithAssertion(new SupportBean_S0(1, "a", "b", "c")).Expect(fields, "ab", "abc", "a|b");
	        builder.WithAssertion(new SupportBean_S0(1, null, "b", "c")).Expect(fields, null, null, null);
	        builder.WithAssertion(new SupportBean_S0(1, "", "b", "c")).Expect(fields, "b", "bc", "|b");
	        builder.WithAssertion(new SupportBean_S0(1, "123", null, "c")).Expect(fields, null, null, null);
	        builder.WithAssertion(new SupportBean_S0(1, "123", "456", "c")).Expect(fields, "123456", "123456c", "123|456");
	        builder.WithAssertion(new SupportBean_S0(1, "123", "456", null)).Expect(fields, "123456", null, "123|456");

	        builder.Run(env);
	        env.UndeployAll();
	    }
	}
} // end of namespace
