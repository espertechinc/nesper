///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreDotExpressionDuckTyping : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			var epl = "@Name('s0') select " +
			          "(dt).MakeString() as strval, " +
			          "(dt).MakeInteger() as intval, " +
			          "(dt).MakeCommon().MakeString() as commonstrval, " +
			          "(dt).MakeCommon().MakeInteger() as commonintval, " +
			          "(dt).ReturnDouble() as commondoubleval " +
			          "from SupportBeanDuckType dt ";
			env.CompileDeploy(epl).AddListener("s0");

			var rows = new[] {
				new object[] {"strval", typeof(object)},
				new object[] {"intval", typeof(object)},
				new object[] {"commonstrval", typeof(object)},
				new object[] {"commonintval", typeof(object)},
				new object[] {"commondoubleval", typeof(double?)} // this one is strongly typed
			};
			for (var i = 0; i < rows.Length; i++) {
				var prop = env.Statement("s0").EventType.PropertyDescriptors[i];
				Assert.AreEqual(rows[i][0], prop.PropertyName);
				Assert.AreEqual(rows[i][1], prop.PropertyType);
			}

			var fields = "strval,intval,commonstrval,commonintval,commondoubleval".SplitCsv();

			env.SendEventBean(new SupportBeanDuckTypeOne("x"));
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "x", null, null, -1, 12.9876d);

			env.SendEventBean(new SupportBeanDuckTypeTwo(-10));
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, null, -10, "mytext", null, 11.1234d);

			env.UndeployAll();
		}
	}
} // end of namespace
