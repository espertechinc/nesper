///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

// TryInvalidCompile
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreBitWiseOperators
    {
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

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithOp(execs);
            WithOpOM(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBitWiseInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithOpOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBitWiseOpOM());
            return execs;
        }

        public static IList<RegressionExecution> WithOp(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreBitWiseOp());
            return execs;
        }

        private class ExprCoreBitWiseInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBean(TheString = 'a' | 'x')",
                    "Failed to validate filter expression 'TheString=\"a\"|\"x\"': Invalid datatype for binary operator, System.String is not allowed");
            }
        }

        private class ExprCoreBitWiseOpOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create()
                    .Add(Expressions.BinaryAnd().Add("BytePrimitive").Add("ByteBoxed"), "myFirstProperty")
                    .Add(Expressions.BinaryOr().Add("ShortPrimitive").Add("ShortBoxed"), "mySecondProperty")
                    .Add(Expressions.BinaryOr().Add("IntPrimitive").Add("IntBoxed"), "myThirdProperty")
                    .Add(Expressions.BinaryXor().Add("LongPrimitive").Add("LongBoxed"), "myFourthProperty")
                    .Add(Expressions.BinaryAnd().Add("BoolPrimitive").Add("BoolBoxed"), "myFifthProperty");

                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model = SerializableObjectCopier.GetInstance(env.Container).Copy(model);
                ClassicAssert.AreEqual(EPL, model.ToEPL());

                env.CompileDeploy("@name('s0')  " + EPL).AddListener("s0");

                RunBitWiseOperators(env);

                env.UndeployAll();
            }
        }

        private class ExprCoreBitWiseOp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') " + EPL).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var type = statement.EventType;
                        ClassicAssert.AreEqual(typeof(byte?), type.GetPropertyType("myFirstProperty"));
                        ClassicAssert.AreEqual(typeof(short?), type.GetPropertyType("mySecondProperty"));
                        ClassicAssert.AreEqual(typeof(int?), type.GetPropertyType("myThirdProperty"));
                        ClassicAssert.AreEqual(typeof(long?), type.GetPropertyType("myFourthProperty"));
                        ClassicAssert.AreEqual(typeof(bool?), type.GetPropertyType("myFifthProperty"));
                    });

                RunBitWiseOperators(env);

                env.UndeployAll();

                var fields = "c0".SplitCsv();
                var builder = new SupportEvalBuilder("SupportBean")
                    .WithExpression(fields[0], "BytePrimitive&ByteBoxed");
                builder.WithAssertion(MakeEventBB(1, 1)).Expect(fields, (byte)1);
                builder.WithAssertion(MakeEventBB(1, null)).Expect(fields, new object[] { null });
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

        private static void RunBitWiseOperators(RegressionEnvironment env)
        {
            var sb = MakeEvent();
            env.SendEventBean(sb);

            env.AssertEventNew(
                "s0",
                received => {
                    ClassicAssert.AreEqual((byte)1, received.Get("myFirstProperty"));
                    ClassicAssert.IsTrue(((short?)(received.Get("mySecondProperty")) & SECOND_EVENT) == SECOND_EVENT);
                    ClassicAssert.IsTrue(((int?)(received.Get("myThirdProperty")) & FIRST_EVENT) == FIRST_EVENT);
                    ClassicAssert.AreEqual(7L, received.Get("myFourthProperty"));
                    ClassicAssert.AreEqual(false, received.Get("myFifthProperty"));
                });
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

        internal static SupportBean MakeEvent(
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