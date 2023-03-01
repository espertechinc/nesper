///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // TryInvalidCompile

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreBitWiseOperators {
	    private const byte FIRST_EVENT = 1;
	    private const short SECOND_EVENT = 2;
	    private const int THIRD_EVENT = FIRST_EVENT | SECOND_EVENT;
	    private const long FOURTH_EVENT = 4;
	    private const bool FITH_EVENT = false;

	    private const string EPL = "select BytePrimitive&ByteBoxed as myFirstProperty, " +
	        "ShortPrimitive|ShortBoxed as mySecondProperty, " +
	        "IntPrimitive|IntBoxed as myThirdProperty, " +
	        "LongPrimitive^LongBoxed as myFourthProperty, " +
	        "BoolPrimitive&BoolBoxed as myFifthProperty " +
	        "from SupportBean";

	    public static ICollection<RegressionExecution> Executions() {
	        var executions = new List<RegressionExecution>();
	        executions.Add(new ExprCoreBitWiseOp());
	        executions.Add(new ExprCoreBitWiseOpOM());
	        executions.Add(new ExprCoreBitWiseInvalid());
	        return executions;
	    }

	    private class ExprCoreBitWiseInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            TryInvalidCompile(env, "select * from SupportBean(TheString = 'a' | 'x')",
	                "Failed to validate filter expression 'TheString=\"a\"|\"x\"': Invalid datatype for binary operator, System.String is not allowed");
	        }
	    }

	    private class ExprCoreBitWiseOpOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create()
		            .Add(Expressions.BinaryAnd().Add("BytePrimitive").Add("ByteBoxed"), "myFirstProperty")
		            .Add(Expressions.BinaryOr().Add("ShortPrimitive").Add("ShortBoxed"), "mySecondProperty")
		            .Add(Expressions.BinaryOr().Add("IntPrimitive").Add("IntBoxed"), "myThirdProperty")
		            .Add(Expressions.BinaryXor().Add("LongPrimitive").Add("LongBoxed"), "myFourthProperty")
		            .Add(Expressions.BinaryAnd().Add("BoolPrimitive").Add("BoolBoxed"), "myFifthProperty");

	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
	            model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
	            Assert.AreEqual(EPL, model.ToEPL());

	            env.CompileDeploy("@Name('s0')  " + EPL).AddListener("s0");

	            RunBitWiseOperators(env);

	            env.UndeployAll();
	        }
	    }

	    private class ExprCoreBitWiseOp : RegressionExecution
	    {
		    public void Run(RegressionEnvironment env)
		    {
			    env.CompileDeploy("@Name('s0') " + EPL).AddListener("s0");

			    var type = env.Statement("s0").EventType;
			    Assert.AreEqual(typeof(byte?), type.GetPropertyType("myFirstProperty"));
			    Assert.AreEqual(typeof(short?), type.GetPropertyType("mySecondProperty"));
			    Assert.AreEqual(typeof(int?), type.GetPropertyType("myThirdProperty"));
			    Assert.AreEqual(typeof(long?), type.GetPropertyType("myFourthProperty"));
			    Assert.AreEqual(typeof(bool?), type.GetPropertyType("myFifthProperty"));

			    RunBitWiseOperators(env);

			    env.UndeployAll();

			    var fields = "c0".SplitCsv();
			    var builder = new SupportEvalBuilder("SupportBean")
				    .WithExpression(fields[0], "BytePrimitive&ByteBoxed");
			    builder.WithAssertion(MakeEventBB(1, 1)).Expect(fields, (byte) 1);
			    builder.WithAssertion(MakeEventBB(1, null)).Expect(fields, new object[] {null});
			    builder.Run(env);
			    env.UndeployAll();
		    }

		    private SupportBean MakeEventBB(
			    byte bytePrimitive,
			    byte? byteBoxed)
		    {
			    var sb = new SupportBean();
			    sb.BytePrimitive = bytePrimitive;
			    sb.ByteBoxed = byteBoxed;
			    return sb;
		    }
	    }

	    private static void RunBitWiseOperators(RegressionEnvironment env) {
	        var sb = MakeEvent();
	        env.SendEventBean(sb);

	        var received = env.Listener("s0").GetAndResetLastNewData()[0];
	        Assert.AreEqual((byte) 1, received.Get("myFirstProperty"));
	        Assert.IsTrue(((short?) (received.Get("mySecondProperty")) & SECOND_EVENT) == SECOND_EVENT);
	        Assert.IsTrue(((int?) (received.Get("myThirdProperty")) & FIRST_EVENT) == FIRST_EVENT);
	        Assert.AreEqual(7L, received.Get("myFourthProperty"));
	        Assert.AreEqual(false, received.Get("myFifthProperty"));
	    }

	    private static SupportBean MakeEvent()
	    {
		    return MakeEvent(
			    FIRST_EVENT,
			    FIRST_EVENT,
			    SECOND_EVENT,
			    SECOND_EVENT,
			    FIRST_EVENT,
			    THIRD_EVENT,
			    3L,
			    FOURTH_EVENT,
			    FITH_EVENT,
			    FITH_EVENT);
	    }

	    protected static SupportBean MakeEvent(
		    byte bytePrimitive,
		    byte? byteBoxed,
		    short shortPrimitive,
		    short? shortBoxed,
		    int intPrimitive,
		    int? intBoxed,
		    long longPrimitive,
		    long? longBoxed,
		    bool boolPrimitive,
		    bool? boolBoxed)
	    {
		    var bean = new SupportBean();
		    bean.BytePrimitive = bytePrimitive;
		    bean.ByteBoxed = byteBoxed;
		    bean.ShortPrimitive = shortPrimitive;
		    bean.ShortBoxed = shortBoxed;
		    bean.IntPrimitive = intPrimitive;
		    bean.IntBoxed = intBoxed;
		    bean.LongPrimitive = longPrimitive;
		    bean.LongBoxed = longBoxed;
		    bean.BoolPrimitive = boolPrimitive;
		    bean.BoolBoxed = boolBoxed;
		    return bean;
	    }

	    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
