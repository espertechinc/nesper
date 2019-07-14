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
                Assert.AreEqual(true, instanceOne.IsTheBool);
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
                    "  theStringArray: ['a', \"b\"],\n" +
                    "  theIntArray: [1, 2, 3],\n" +
                    "  theObjectArray: ['a', 1],\n" +
                    "  theMap: {\n" +
                    "    a : 10,\n" +
                    "    b : 'xyz'\n" +
                    "  },\n" +
                    "  theInnerOp: {\n" +
                    "    fieldOne: 'x',\n" +
                    "    fieldTwo: 2\n" +
                    "  },\n" +
                    "  theInnerOpInterface: {\n" +
                    "    class: '" +
                    typeof(MyOperatorTwoInterfaceImplTwo).Name +
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
                    "a,b".SplitCsv(),
                    10,
                    "xyz");
                Assert.AreEqual("x", instanceTwo.TheInnerOp.fieldOne);
                Assert.AreEqual(2, instanceTwo.TheInnerOp.fieldTwo);
                Assert.IsTrue(instanceTwo.TheInnerOpInterface is MyOperatorTwoInterfaceImplTwo);
            }
        }

        public class MyOperatorOneForge : DataFlowOperatorForge
        {
            [DataFlowOpParameter] private readonly bool _isTheBool;
            [DataFlowOpParameter] private readonly double _theDoubleOne;
            [DataFlowOpParameter] private readonly double? _theDoubleTwo;
            [DataFlowOpParameter] private readonly float _theFloatOne;
            [DataFlowOpParameter] private readonly float? _theFloatTwo;
            [DataFlowOpParameter] private readonly int _theInt;
            [DataFlowOpParameter] private readonly long? _theLongOne;
            [DataFlowOpParameter] private readonly long? _theLongThree;
            [DataFlowOpParameter] private readonly long _theLongTwo;
            [DataFlowOpParameter] private readonly string _theNotSetString;

            [DataFlowOpParameter] private readonly string _theString;
            [DataFlowOpParameter] private string _theStringWithSetter;

            public MyOperatorOneForge()
            {
                Operators.Add(this);
            }

            public string TheString => _theString;

            public string TheNotSetString => _theNotSetString;

            public int TheInt => _theInt;

            public bool IsTheBool => _isTheBool;

            public long? TheLongOne => _theLongOne;

            public long TheLongTwo => _theLongTwo;

            public long? TheLongThree => _theLongThree;

            public float TheFloatOne => _theFloatOne;

            public float? TheFloatTwo => _theFloatTwo;

            public double TheDoubleOne => _theDoubleOne;

            public double? TheDoubleTwo => _theDoubleTwo;

            public string TheStringWithSetter {
                get => _theStringWithSetter;
                set => _theStringWithSetter = ">" + value + "<";
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
            public MyOperatorTwoForge()
            {
                Operators.Add(this);
            }

            public string[] TheStringArray { get; set; }

            public int[] TheIntArray { get; set; }

            public object[] TheObjectArray { get; set; }

            public MyOperatorTwoInner TheInnerOp { get; set; }

            public MyOperatorTwoInterface TheInnerOpInterface { get; set; }

            public static IList<MyOperatorTwoForge> Operators { get; } = new List<MyOperatorTwoForge>();

            public IDictionary<string, object> TheMap { get; set; }

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