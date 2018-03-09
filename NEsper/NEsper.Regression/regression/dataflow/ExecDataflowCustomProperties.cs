///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.dataflow {
    // Further relevant tests in JSONUtil/PopulateUtil
    public class ExecDataflowCustomProperties : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionCustomProps(epService);
        }

        /// <summary>
        /// - GraphSource always has output ports:
        /// (A) Either as declared through @OutputTypes annotation
        /// (B) Or as assigned via stream (GraphSource -> OutStream&lt;Type&gt;)
        /// <para>
        /// - Operator properties are explicit:
        /// (A) There is a public setter method
        /// (B) Or the @GraphOpProperty annotation is declared on a field or setter method (optionally can provide a name)
        /// (C) Or the @GraphOpProperty annotation is declared on a catch-all method
        /// </para>
        /// <para>
        /// - Graph op property types
        /// (A) Scalar type
        /// (B) or ExprNode
        /// (C) or Json for nested objects and array
        /// (D) or EPL select
        /// </para>
        /// <para>
        /// - Graph ops communicate the underlying events
        /// - should EventBean be need for event evaluation, the EventBean instance is pooled/shared locally by the op
        /// - if the event bus should evaluate the event, a new anonymous event gets created with the desired type attached dynamically
        /// </para>
        /// <para>
        /// - Exception handlings
        /// - Validation of syntax is performed during "CreateEPL"
        /// - Resolution of operators and types is performed during "instantiate"
        /// - Runtime exception handling depends on how the data flow gets started and always uses an exception handler (another subject therefore)
        /// </para>
        /// </summary>
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;

            epl = "create dataflow MyGraph ABC { field: { a: a}}";
            TryInvalid(
                epService, epl,
                "Incorrect syntax near 'a' at line 1 column 42 [create dataflow MyGraph ABC { field: { a: a}}]");

            epl = "create dataflow MyGraph ABC { field: { a:1; b:2 }}";
            TryInvalid(
                epService, epl,
                "Incorrect syntax near ';' at line 1 column 42 [create dataflow MyGraph ABC { field: { a:1; b:2 }}]");
        }

        private void RunAssertionCustomProps(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MyOperatorOne));
            epService.EPAdministrator.Configuration.AddImport(typeof(MyOperatorTwo));

            // test simple properties
            MyOperatorOne.Operators.Clear();
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(
                "create dataflow MyGraph " + typeof(MyOperatorOne).FullName + " {" +
                "  TheString = 'a'," +
                "  theInt: 1," +
                "  theBool: true," +
                "  theLongOne: 1L," +
                "  theLongTwo: 2," +
                "  theLongThree: null," +
                "  theDoubleOne: 1d," +
                "  theDoubleTwo: 2," +
                "  theFloatOne: 1f," +
                "  theFloatTwo: 2," +
                "  TheStringWithSetter: 'b'," +
                "  theSystemProperty: systemProperties('log4j.configuration')" +
                "}");
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph");
            Assert.AreEqual(1, MyOperatorOne.Operators.Count);
            MyOperatorOne instanceOne = MyOperatorOne.Operators[0];

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
            Assert.NotNull(instanceOne.TheSystemProperty);
            stmtGraph.Dispose();

            // test array etc. properties
            MyOperatorTwo.Operators.Clear();
            epService.EPAdministrator.CreateEPL(
                "create dataflow MyGraph " + typeof(MyOperatorTwo).FullName + " {\n" +
                "  TheStringArray: ['a', \"b\"],\n" +
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
                "    class: '" + typeof(MyOperatorTwoInterfaceImplTwo).FullName + "'\n" +
                "  },\n" + // NOTE the last comma here, it's acceptable
                "}");
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph");
            Assert.AreEqual(1, MyOperatorTwo.Operators.Count);
            MyOperatorTwo instanceTwo = MyOperatorTwo.Operators[0];

            EPAssertionUtil.AssertEqualsExactOrder(new string[] {"a", "b"}, instanceTwo.TheStringArray);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {1, 2, 3}, instanceTwo.TheIntArray);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {"a", 1}, instanceTwo.TheObjectArray);
            EPAssertionUtil.AssertPropsMap(instanceTwo.TheMap, "a,b".SplitCsv(), new object[] {10, "xyz"});
            Assert.AreEqual("x", instanceTwo.TheInnerOp.FieldOne);
            Assert.AreEqual(2, instanceTwo.TheInnerOp.FieldTwo);
            Assert.IsTrue(instanceTwo.TheInnerOpInterface is MyOperatorTwoInterfaceImplTwo);
        }

        [DataFlowOperator]
        public class MyOperatorOne {
            private static IList<MyOperatorOne> operators = new List<MyOperatorOne>();

            public static IList<MyOperatorOne> Operators => operators;

            public MyOperatorOne() {
                operators.Add(this);
            }

            [DataFlowOpParameter] private string theString;
            [DataFlowOpParameter] private string theNotSetString;
            [DataFlowOpParameter] private int theInt;
            [DataFlowOpParameter] private bool theBool;
            [DataFlowOpParameter] private long? theLongOne;
            [DataFlowOpParameter] private long theLongTwo;
            [DataFlowOpParameter] private long theLongThree;
            [DataFlowOpParameter] private double theDoubleOne;
            [DataFlowOpParameter] private double? theDoubleTwo;
            [DataFlowOpParameter] private float theFloatOne;
            [DataFlowOpParameter] private float? theFloatTwo;
            [DataFlowOpParameter] private string theSystemProperty;

            private string theStringWithSetter;

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
                set => TheStringWithSetter = ">" + value + "<";
            }

            public string TheSystemProperty => theSystemProperty;
        }

        [DataFlowOperator]
        public class MyOperatorTwo {

            private static readonly IList<MyOperatorTwo> operators = new List<MyOperatorTwo>();

            public static IList<MyOperatorTwo> Operators => operators;

            public MyOperatorTwo() {
                operators.Add(this);
            }

            private string[] theStringArray;
            private int[] theIntArray;
            private object[] theObjectArray;
            private IDictionary<string, object> theMap;
            private MyOperatorTwoInner theInnerOp;
            private MyOperatorTwoInterface theInnerOpInterface;

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

            public IDictionary<string, object> TheMap {
                get => theMap;
                set => theMap = value;
            }

            public MyOperatorTwoInner TheInnerOp {
                get => theInnerOp;
                set => theInnerOp = value;
            }

            public MyOperatorTwoInterface TheInnerOpInterface {
                get => theInnerOpInterface;
                set => theInnerOpInterface = value;
            }
        }

        public class MyOperatorTwoInner {
            [DataFlowOpParameter] private string fieldOne;
            [DataFlowOpParameter] private int fieldTwo;

            public string FieldOne => fieldOne;

            public int FieldTwo => fieldTwo;
        }

        public interface MyOperatorTwoInterface { }

        public class MyOperatorTwoInterfaceImplOne : MyOperatorTwoInterface { }

        public class MyOperatorTwoInterfaceImplTwo : MyOperatorTwoInterface { }
    }
}
