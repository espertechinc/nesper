///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreMinMaxNonAgg {
	    private const string EPL = "select max(longBoxed,intBoxed) as myMax, " +
	        "max(longBoxed,intBoxed,shortBoxed) as myMaxEx, " +
	        "min(longBoxed,intBoxed) as myMin, " +
	        "min(longBoxed,intBoxed,shortBoxed) as myMinEx" +
	        " from SupportBean#length(3)";

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ExecCoreMinMax());
	        execs.Add(new ExecCoreMinMaxOM());
	        execs.Add(new ExecCoreMinMaxCompile());
	        return execs;
	    }

	    private class ExecCoreMinMax : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "myMax,myMaxEx,myMin,myMinEx".SplitCsv();
	            var builder = new SupportEvalBuilder("SupportBean")
	                .WithExpression(fields[0], "max(longBoxed,intBoxed)")
	                .WithExpression(fields[1], "max(longBoxed,intBoxed,shortBoxed)")
	                .WithExpression(fields[2], "min(longBoxed,intBoxed)")
	                .WithExpression(fields[3], "min(longBoxed,intBoxed,shortBoxed)");

	            builder.WithStatementConsumer(stmt => {
	                var type = stmt.EventType;
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMax"));
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMin"));
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMinEx"));
	                Assert.AreEqual(typeof(long?), type.GetPropertyType("myMaxEx"));
	            });

	            builder.WithAssertion(MakeBoxedEvent(10L, 20, 4)).Expect(fields, 20L, 20L, 10L, 4L);
	            builder.WithAssertion(MakeBoxedEvent(-10L, -20, -30)).Expect(fields, -10L, -10L, -20L, -30L);
	            builder.WithAssertion(MakeBoxedEvent(null, null, null)).Expect(fields, null, null, null, null);
	            builder.WithAssertion(MakeBoxedEvent(1L, null, 1)).Expect(fields, null, null, null, null);

	            builder.Run(env);
	            env.UndeployAll();
	        }
	    }

	    private class ExecCoreMinMaxOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create()
	                .Add(Expressions.Max("longBoxed", "intBoxed"), "myMax")
	                .Add(Expressions.Max(Expressions.Property("longBoxed"), Expressions.Property("intBoxed"), Expressions.Property("shortBoxed")), "myMaxEx")
	                .Add(Expressions.Min("longBoxed", "intBoxed"), "myMin")
	                .Add(Expressions.Min(Expressions.Property("longBoxed"), Expressions.Property("intBoxed"), Expressions.Property("shortBoxed")), "myMinEx");

	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)).AddView("length", Expressions.Constant(3)));
	            model = env.CopyMayFail(model);
	            Assert.AreEqual(EPL, model.ToEPL());

	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");

	            TryMinMaxWindowStats(env);

	            env.UndeployAll();
	        }
	    }

	    private class ExecCoreMinMaxCompile : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.EplToModelCompileDeploy("@name('s0') " + EPL).AddListener("s0");
	            TryMinMaxWindowStats(env);
	            env.UndeployAll();
	        }
	    }

	    private static void TryMinMaxWindowStats(RegressionEnvironment env) {
	        SendEvent(env, 10, 20, 4);
	        env.AssertListener("s0", listener => {
	            var received = listener.GetAndResetLastNewData()[0];
	            Assert.AreEqual(20L, received.Get("myMax"));
	            Assert.AreEqual(10L, received.Get("myMin"));
	            Assert.AreEqual(4L, received.Get("myMinEx"));
	            Assert.AreEqual(20L, received.Get("myMaxEx"));
	        });

	        SendEvent(env, -10, -20, -30);
	        env.AssertListener("s0", listener => {
	            var received = listener.GetAndResetLastNewData()[0];
	            Assert.AreEqual(-10L, received.Get("myMax"));
	            Assert.AreEqual(-20L, received.Get("myMin"));
	            Assert.AreEqual(-30L, received.Get("myMinEx"));
	            Assert.AreEqual(-10L, received.Get("myMaxEx"));
	        });
	    }

	    private static void SendEvent(RegressionEnvironment env, long longBoxed, int intBoxed, short shortBoxed) {
	        env.SendEventBean(MakeBoxedEvent(longBoxed, intBoxed, shortBoxed));
	    }

	    private static SupportBean MakeBoxedEvent(long? longBoxed, int? intBoxed, short? shortBoxed) {
	        var bean = new SupportBean();
	        bean.LongBoxed = longBoxed;
	        bean.IntBoxed = intBoxed;
	        bean.ShortBoxed = shortBoxed;
	        return bean;
	    }
	}
} // end of namespace
