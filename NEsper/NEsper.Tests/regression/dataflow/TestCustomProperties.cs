///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    // Further relevant tests in JSONUtil/PopulateUtil
    [TestFixture]
    public class TestCustomProperties
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        /// <summary>
        /// - GraphSource always has output ports:
        ///   (A) Either as declared through @OutputTypes annotation
        ///   (B) Or as assigned via stream (GraphSource -> OutStream&lt;Type&gt;)
        ///
        /// - Operator properties are explicit:
        ///   (A) There is a public setter method
        ///   (B) Or the @GraphOpProperty annotation is declared on a field or setter method (optionally can provide a name)
        ///   (C) Or the @GraphOpProperty annotation is declared on a catch-all method
        ///
        /// - Graph op property types
        ///   (A) Scalar type
        ///   (B) or ExprNode
        ///   (C) or Json for nested objects and array
        ///   (D) or EPL select
        ///
        /// - Graph ops communicate the underlying events
        ///   - should EventBean be need for event evaluation, the EventBean instance is pooled/shared locally by the op
        ///   - if the event bus should evaluate the event, a new anonymous event gets created with the desired type attached dynamically
        ///
        /// - Exception handlings
        ///   - Validation of syntax is performed during "createEPL"
        ///   - Resolution of operators and types is performed during "instantiate"
        ///   - Runtime exception handling depends on how the data flow gets started and always uses an exception handler (another subject therefore)
        /// </summary>
        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "create dataflow MyGraph ABC { field: { a: a}}";
            TryInvalid(epl, "Incorrect syntax near 'a' at line 1 column 42 [create dataflow MyGraph ABC { field: { a: a}}]");

            epl = "create dataflow MyGraph ABC { field: { a:1; b:2 }}";
            TryInvalid(epl, "Incorrect syntax near ';' at line 1 column 42 [create dataflow MyGraph ABC { field: { a:1; b:2 }}]");
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        [Test]
        public void TestCustomProps()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyOperatorOne));
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyOperatorTwo));

            // test simple properties
            MyOperatorOne.Operators.Clear();
            EPStatement stmtGraph = _epService.EPAdministrator.CreateEPL("create dataflow MyGraph " + typeof(MyOperatorOne).Name + " {" +
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
                    "  theSystemProperty: env('Path')" +
                    "}");
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph");
            Assert.AreEqual(1, MyOperatorOne.Operators.Count);
            MyOperatorOne instanceOne = MyOperatorOne.Operators[0];

            Assert.AreEqual("a", instanceOne.TheString);
            Assert.AreEqual(null, instanceOne.TheNotSetString);
            Assert.AreEqual(1, instanceOne.TheInt);
            Assert.AreEqual(true, instanceOne.IsTheBool);
            Assert.AreEqual(1L, (long)instanceOne.TheLongOne);
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
            _epService.EPAdministrator.CreateEPL("create dataflow MyGraph " + typeof(MyOperatorTwo).Name + " {\n" +
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
                    "    class: '" + typeof(MyOperatorTwoInterfaceImplTwo).FullName + "'\n" +
                    "  },\n" +  // NOTE the last comma here, it's acceptable
                    "}");
            _epService.EPRuntime.DataFlowRuntime.Instantiate("MyGraph");
            Assert.AreEqual(1, MyOperatorTwo.Operators.Count);
            MyOperatorTwo instanceTwo = MyOperatorTwo.Operators[0];

            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "a", "b" }, instanceTwo.TheStringArray);
            EPAssertionUtil.AssertEqualsExactOrder(new int[] { 1, 2, 3 }, instanceTwo.TheIntArray);
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "a", 1 }, instanceTwo.TheObjectArray);
            EPAssertionUtil.AssertPropsMap(instanceTwo.TheMap, "a,b".Split(','), new Object[] { 10, "xyz" });
            Assert.AreEqual("x", instanceTwo.TheInnerOp.fieldOne);
            Assert.AreEqual(2, instanceTwo.TheInnerOp.fieldTwo);
            Assert.IsTrue(instanceTwo.TheInnerOpInterface is MyOperatorTwoInterfaceImplTwo);
        }

        [DataFlowOperator]
        public class MyOperatorOne
        {
            private static List<MyOperatorOne> operators = new List<MyOperatorOne>();

            public static List<MyOperatorOne> Operators
            {
                get { return operators; }
            }

            public MyOperatorOne()
            {
                operators.Add(this);
            }

            [DataFlowOpParameter] private String theString;
            [DataFlowOpParameter] private String theNotSetString;
            [DataFlowOpParameter] private int theInt;
            [DataFlowOpParameter] private bool theBool;
            [DataFlowOpParameter] private long? theLongOne;
            [DataFlowOpParameter] private long theLongTwo;
            [DataFlowOpParameter] private long? theLongThree;
            [DataFlowOpParameter] private double theDoubleOne;
            [DataFlowOpParameter] private Double theDoubleTwo;
            [DataFlowOpParameter] private float theFloatOne;
            [DataFlowOpParameter] private float? theFloatTwo;
            [DataFlowOpParameter] private String theSystemProperty;

            private String _theStringWithSetter;

            public string TheString
            {
                get { return theString; }
            }

            public string TheNotSetString
            {
                get { return theNotSetString; }
            }

            public int TheInt
            {
                get { return theInt; }
            }

            public bool IsTheBool
            {
                get { return theBool; }
            }

            public long? TheLongOne
            {
                get { return theLongOne; }
            }

            public long TheLongTwo
            {
                get { return theLongTwo; }
            }

            public long? TheLongThree
            {
                get { return theLongThree; }
            }

            public float TheFloatOne
            {
                get { return theFloatOne; }
            }

            public float? TheFloatTwo
            {
                get { return theFloatTwo; }
            }

            public double TheDoubleOne
            {
                get { return theDoubleOne; }
            }

            public double TheDoubleTwo
            {
                get { return theDoubleTwo; }
            }

            public string TheStringWithSetter
            {
                get { return _theStringWithSetter; }
                set { _theStringWithSetter = ">" + value + "<"; }
            }

            public string TheSystemProperty
            {
                get { return theSystemProperty; }
            }
        }

        [DataFlowOperator]
        public class MyOperatorTwo
        {
            public static List<MyOperatorTwo> Operators { get; private set; }

            public MyOperatorTwo()
            {
                Operators.Add(this);
            }

            static MyOperatorTwo()
            {
                Operators = new List<MyOperatorTwo>();
            }

            public string[] TheStringArray { get; set; }

            public int[] TheIntArray { get; set; }

            public object[] TheObjectArray { get; set; }

            public IDictionary<string, object> TheMap { get; set; }

            public MyOperatorTwoInner TheInnerOp { get; set; }

            public MyOperatorTwoInterface TheInnerOpInterface { get; set; }
        }

        public class MyOperatorTwoInner
        {
            [DataFlowOpParameter] internal string fieldOne;
            [DataFlowOpParameter] internal int fieldTwo;
        }

        public interface MyOperatorTwoInterface { }

        public class MyOperatorTwoInterfaceImplOne : MyOperatorTwoInterface { }
        public class MyOperatorTwoInterfaceImplTwo : MyOperatorTwoInterface { }
    }
}
