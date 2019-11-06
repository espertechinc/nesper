///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    // Further relevant tests in JSONUtil/PopulateUtil
    public class EPLDataflowCustomProperties
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowInvalid());
            execs.Add(new EPLDataflowCustomProps());
            return execs;
        }

        /// <summary>
        ///     - GraphSource always has output ports:
        ///     (A) Either as declared through @OutputTypes annotation
        ///     (B) Or as assigned via stream (GraphSource -&gt; OutStream&lt;Type&gt;)
        ///     <para />
        ///     - Operator properties are explicit:
        ///     (A) There is a public setter method
        ///     (B) Or the @GraphOpProperty annotation is declared on a field or setter method (optionally can provide a name)
        ///     (C) Or the @GraphOpProperty annotation is declared on a catch-all method
        ///     <para />
        ///     - Graph op property types
        ///     (A) Scalar type
        ///     (B) or ExprNode
        ///     (C) or Json for nested objects and array
        ///     (D) or EPL select
        ///     <para />
        ///     - Graph op communicate the underlying events
        ///     - should EventBean be need for event evaluation, the EventBean instance is pooled/shared locally by the op
        ///     - if the event bus should evaluate the event, a new anonymous event gets created with the desired type attached
        ///     dynamically
        ///     <para />
        ///     - Exception handlings
        ///     - Validation of syntax is performed during "createEPL"
        ///     - Resolution of operators and types is performed during "instantiate"
        ///     - Runtime exception handling depends on how the data flow gets started and always uses an exception handler
        ///     (another subject therefore)
        /// </summary>
        internal class EPLDataflowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "create dataflow MyGraph ABC { field: { a: a}}";
                TryInvalidCompile(env, epl, "Incorrect syntax near 'a' at line 1 column 42 [");

                epl = "create dataflow MyGraph ABC { field: { a:1x b:2 }}";
                TryInvalidCompile(env, epl, "Incorrect syntax near 'x' at line 1 column 42 [");
            }
        }

        internal class EPLDataflowCustomProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test simple properties
                MyOperatorOneForge.Operators.Clear();
                env.Compile(
                    "@Name('flow') create dataflow MyGraph MyOperatorOne {" +
                    "  theString = 'a'," +
                    "  theInt: 1," +
                    "  theBool: true," +
                    "  theLongOne: 1L," +
                    "  theLongTwo: 2," +
                    "  theLongThree: null," +
                    "  theDoubleOne: 1d," +
                    "  theDoubleTwo: 2," +
                    "  theFloatOne: 1f," +
                    "  theFloatTwo: 2," +
                    "  theStringWithSetter: 'b'," +
                    "  theSystemProperty: systemProperties('log4j.configuration')" +
                    "}");
                Assert.AreEqual(1, MyOperatorOneForge.Operators.Count);
                var instanceOne = MyOperatorOneForge.Operators[0];

                Assert.AreEqual("a", instanceOne.TheString);
                Assert.AreEqual(null, instanceOne.TheNotSetString);
                Assert.AreEqual(1, instanceOne.TheInt);
                Assert.AreEqual(true, instanceOne.TheBool);
                Assert.AreEqual(1L, (long) instanceOne.TheLongOne);
                Assert.AreEqual(2, instanceOne.TheLongTwo);
                Assert.AreEqual(null, instanceOne.TheLongThree);
                Assert.AreEqual(1.0, instanceOne.TheDoubleOne);
                Assert.AreEqual(2.0, instanceOne.TheDoubleTwo);
                Assert.AreEqual(1f, instanceOne.TheFloatOne);
                Assert.AreEqual(2f, instanceOne.TheFloatTwo);
                Assert.AreEqual(">b<", instanceOne.TheStringWithSetter);
                Assert.IsNotNull(instanceOne.TheSystemProperty);

                // test array etc. properties
                MyOperatorTwoForge.Operators.Clear();
                env.Compile(
                    "@Name('flow') create dataflow MyGraph MyOperatorTwo {\n" +
                    "  TheStringArray: ['a', \"b\"],\n" +
                    "  TheIntArray: [1, 2, 3],\n" +
                    "  TheObjectArray: ['a', 1],\n" +
                    "  TheMap: {\n" +
                    "    a : 10,\n" +
                    "    b : 'xyz'\n" +
                    "  },\n" +
                    "  TheInnerOp: {\n" +
                    "    fieldOne: 'x',\n" +
                    "    fieldTwo: 2\n" +
                    "  },\n" +
                    "  TheInnerOpInterface: {\n" +
                    "    class: '" +
                    typeof(MyOperatorTwoInterfaceImplTwo).MaskTypeName() +
                    "'\n" +
                    "  },\n" + // NOTE the last comma here, it's acceptable
                    "}");
                Assert.AreEqual(1, MyOperatorTwoForge.Operators.Count);
                var instanceTwo = MyOperatorTwoForge.Operators[0];

                EPAssertionUtil.AssertEqualsExactOrder(new[] {"a", "b"}, instanceTwo.TheStringArray);
                EPAssertionUtil.AssertEqualsExactOrder(new[] {1, 2, 3}, instanceTwo.TheIntArray);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", 1}, instanceTwo.TheObjectArray);
                EPAssertionUtil.AssertPropsMap(
                    instanceTwo.TheMap,
                    new [] { "a","b" },
                    10,
                    "xyz");
                Assert.AreEqual("x", instanceTwo.TheInnerOp.fieldOne);
                Assert.AreEqual(2, instanceTwo.TheInnerOp.fieldTwo);
                Assert.IsTrue(instanceTwo.TheInnerOpInterface is MyOperatorTwoInterfaceImplTwo);
            }
        }

        public class MyOperatorOneForge : DataFlowOperatorForge
        {
            [DataFlowOpParameter] private readonly bool theBool;
            [DataFlowOpParameter] private readonly double theDoubleOne;
            [DataFlowOpParameter] private readonly double? theDoubleTwo;
            [DataFlowOpParameter] private readonly float theFloatOne;
            [DataFlowOpParameter] private readonly float? theFloatTwo;
            [DataFlowOpParameter] private readonly int theInt;
            [DataFlowOpParameter] private readonly long? theLongOne;
            [DataFlowOpParameter] private readonly long? theLongThree;
            [DataFlowOpParameter] private readonly long theLongTwo;
            [DataFlowOpParameter] private readonly string theNotSetString;
            [DataFlowOpParameter] private readonly string theString;
            [DataFlowOpParameter] private string theStringWithSetter;

            public MyOperatorOneForge()
            {
                Operators.Add(this);
            }

            public string TheString => theString;

            public string TheNotSetString => theNotSetString;

            public int TheInt => theInt;

            public bool TheBool => theBool;

            public long? TheLongOne => theLongOne;

            public long TheLongTwo => theLongTwo;

            public long? TheLongThree => theLongThree;

            public float TheFloatOne => theFloatOne;

            public float? TheFloatTwo => theFloatTwo;

            public double TheDoubleOne => theDoubleOne;

            public double? TheDoubleTwo => theDoubleTwo;

            public string TheStringWithSetter {
                get => theStringWithSetter;
                set => theStringWithSetter = ">" + value + "<";
            }

            public string TheSystemProperty { get; }

            public static IList<MyOperatorOneForge> Operators { get; } = new List<MyOperatorOneForge>();

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return ConstantNull();
            }
        }

        public class MyOperatorTwoForge : DataFlowOperatorForge
        {
            private static readonly IList<MyOperatorTwoForge> OPERATORS = new List<MyOperatorTwoForge>();

            private string[] theStringArray;
            private int[] theIntArray;
            private object[] theObjectArray;
            private MyOperatorTwoInner theInnerOp;
            private MyOperatorTwoInterface theInnerOpInterface;
            private IDictionary<string, object> theMap;

            public MyOperatorTwoForge()
            {
                Operators.Add(this);
            }

            public string[] TheStringArray {
                get => theStringArray;
                set => theStringArray = value;
            }

            public int[] TheIntArray {
                get => theIntArray;
                set => theIntArray = value;
            }

            public object[] TheObjectArray {
                get => theObjectArray;
                set => theObjectArray = value;
            }

            public MyOperatorTwoInner TheInnerOp {
                get => theInnerOp;
                set => theInnerOp = value;
            }

            public MyOperatorTwoInterface TheInnerOpInterface {
                get => theInnerOpInterface;
                set => theInnerOpInterface = value;
            }

            public static IList<MyOperatorTwoForge> Operators => OPERATORS;

            public IDictionary<string, object> TheMap {
                get => theMap;
                set => theMap = value;
            }

            public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
            {
                return null;
            }

            public CodegenExpression Make(
                CodegenMethodScope parent,
                SAIFFInitializeSymbol symbols,
                CodegenClassScope classScope)
            {
                return ConstantNull();
            }
        }

        public class MyOperatorTwoInner
        {
            [DataFlowOpParameter] internal string fieldOne;
            [DataFlowOpParameter] internal int fieldTwo;
        }

        public interface MyOperatorTwoInterface
        {
        }

        public class MyOperatorTwoInterfaceImplOne : MyOperatorTwoInterface
        {
        }

        public class MyOperatorTwoInterfaceImplTwo : MyOperatorTwoInterface
        {
        }
    }
} // end of namespace