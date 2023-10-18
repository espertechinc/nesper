///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // constantNull
using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
	// Further relevant tests in JSONUtil/PopulateUtil
	public class EPLDataflowCustomProperties {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLDataflowInvalid());
	        execs.Add(new EPLDataflowCustomProps());
	        execs.Add(new EPLDataflowNestedProps());
	        execs.Add(new EPLDataflowCatchAllProps());
	        return execs;
	    }

	    private class EPLDataflowInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            string epl;

	            epl = "create dataflow MyGraph ABC { field: { a: a}}";
	            env.TryInvalidCompile(epl, "Incorrect syntax near 'a' at line 1 column 42 [");

	            epl = "create dataflow MyGraph ABC { field: { a:1x b:2 }}";
	            env.TryInvalidCompile(epl, "Incorrect syntax near 'x' at line 1 column 42 [");
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.DATAFLOW, RegressionFlag.INVALIDITY);
	        }
	    }

	    private class EPLDataflowCatchAllProps : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            MyOperatorFourForge.Operators.Clear();
	            env.Compile("@name('flow') create dataflow MyGraph MyOperatorFourForge {" +
	                "    myTestParameter: 'abc' \n" +
	                "}");
	            Assert.AreEqual(1, MyOperatorFourForge.Operators.Count);
	            var instance = MyOperatorFourForge.Operators[0];

	            var node = instance.AllProperties.Get("myTestParameter");
	            Assert.AreEqual("abc", node.Forge.ExprEvaluator.Evaluate(null, true, null));
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.DATAFLOW);
	        }
	    }

	    private class EPLDataflowNestedProps : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            MyOperatorThreeForge.Operators.Clear();
	            env.Compile("@name('flow') create dataflow MyGraph MyOperatorThree {" +
	                "    settings: {\n" +
	                "      class: 'EPLDataflowCustomProperties$MyOperatorThreeSettingsABC',\n" +
	                "      parameterOne : 'ValueOne'" +
	                "    }\n" +
	                "}");
	            Assert.AreEqual(1, MyOperatorThreeForge.Operators.Count);
	            var instance = MyOperatorThreeForge.Operators[0];

	            var abc = (MyOperatorThreeSettingsABC) instance.Settings;
	            Assert.AreEqual("ValueOne", abc.parameterOne.Forge.ExprEvaluator.Evaluate(null, true, null));
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.DATAFLOW);
	        }
	    }

	    private class EPLDataflowCustomProps : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            // test simple properties
	            MyOperatorOneForge.Operators.Clear();
	            env.Compile("@name('flow') create dataflow MyGraph MyOperatorOne {" +
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
	            Assert.AreEqual(1.0, instanceOne.TheDoubleOne, 0);
	            Assert.AreEqual(2.0, instanceOne.TheDoubleTwo, 0);
	            Assert.AreEqual(1f, instanceOne.TheFloatOne, 0);
	            Assert.AreEqual(2f, instanceOne.TheFloatTwo, 0);
	            Assert.AreEqual(">b<", instanceOne.TheStringWithSetter);

	            // test array etc. properties
	            MyOperatorTwoForge.Operators.Clear();
	            env.Compile("@name('flow') create dataflow MyGraph MyOperatorTwo {\n" +
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
	            Assert.AreEqual(1, MyOperatorTwoForge.Operators.Count);
	            var instanceTwo = MyOperatorTwoForge.Operators[0];

	            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"a", "b"}, instanceTwo.TheStringArray);
	            EPAssertionUtil.AssertEqualsExactOrder(new int[]{1, 2, 3}, instanceTwo.TheIntArray);
	            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"a", 1}, instanceTwo.TheObjectArray);
	            EPAssertionUtil.AssertPropsMap(instanceTwo.TheMap, "a,b".SplitCsv(), new object[]{10, "xyz"});
	            Assert.AreEqual("x", instanceTwo.TheInnerOp.fieldOne);
	            Assert.AreEqual(2, instanceTwo.TheInnerOp.fieldTwo);
	            Assert.IsTrue(instanceTwo.TheInnerOpInterface is MyOperatorTwoInterfaceImplTwo);
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.DATAFLOW);
	        }
	    }

	    public class MyOperatorOneForge : DataFlowOperatorForge {

	        private static IList<MyOperatorOneForge> operators = new List<MyOperatorOneForge>();

	        public static IList<MyOperatorOneForge> GetOperators() {
	            return operators;
	        }

	        public MyOperatorOneForge() {
	            operators.Add(this);
	        }

	        [DataFlowOpParameter]
	        private string theString;
	        [DataFlowOpParameter]
	        private string theNotSetString;
	        [DataFlowOpParameter]
	        private int theInt;
	        [DataFlowOpParameter]
	        private bool theBool;
	        [DataFlowOpParameter]
	        private long? theLongOne;
	        [DataFlowOpParameter]
	        private long theLongTwo;
	        [DataFlowOpParameter]
	        private long? theLongThree;
	        [DataFlowOpParameter]
	        private double theDoubleOne;
	        [DataFlowOpParameter]
	        private double? theDoubleTwo;
	        [DataFlowOpParameter]
	        private float theFloatOne;
	        [DataFlowOpParameter]
	        private float? theFloatTwo;
	        [DataFlowOpParameter]
	        private string theSystemProperty;

	        private string theStringWithSetter;

	        public static IList<MyOperatorOneForge> Operators => operators;

	        public string TheString => theString;

	        public string TheNotSetString => theNotSetString;

	        public int TheInt => theInt;

	        public bool TheBool => theBool;

	        public long? TheLongOne => theLongOne;

	        public long TheLongTwo => theLongTwo;

	        public long? TheLongThree => theLongThree;

	        public double TheDoubleOne => theDoubleOne;

	        public double? TheDoubleTwo => theDoubleTwo;

	        public float TheFloatOne => theFloatOne;

	        public float? TheFloatTwo => theFloatTwo;

	        public string TheSystemProperty => theSystemProperty;

	        public string TheStringWithSetter => theStringWithSetter;

	        public void SetTheStringWithSetter(string theStringWithSetter) {
	            this.theStringWithSetter = ">" + theStringWithSetter + "<";
	        }

	        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context) {
	            return null;
	        }

	        public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	            return ConstantNull();
	        }
	    }

	    public class MyOperatorTwoForge : DataFlowOperatorForge {

	        private static IList<MyOperatorTwoForge> operators = new List<MyOperatorTwoForge>();

	        public static IList<MyOperatorTwoForge> Operators => operators;

	        public MyOperatorTwoForge() {
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
		        set => this.theStringArray = value;
	        }

	        public int[] TheIntArray {
		        get => theIntArray;
		        set => this.theIntArray = value;
	        }

	        public object[] TheObjectArray {
		        get => theObjectArray;
		        set => this.theObjectArray = value;
	        }

	        public IDictionary<string, object> TheMap {
		        get => theMap;
		        set => this.theMap = value;
	        }

	        public MyOperatorTwoInner TheInnerOp {
		        get => theInnerOp;
		        set => this.theInnerOp = value;
	        }

	        public MyOperatorTwoInterface TheInnerOpInterface {
		        get => theInnerOpInterface;
		        set => this.theInnerOpInterface = value;
	        }

	        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context) {
	            return null;
	        }

	        public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	            return ConstantNull();
	        }
	    }

	    public class MyOperatorTwoInner {
	        [DataFlowOpParameter]
	        internal string fieldOne;
	        [DataFlowOpParameter]
	        internal int fieldTwo;
	    }

	    public interface MyOperatorTwoInterface {
	    }

	    public class MyOperatorTwoInterfaceImplOne : MyOperatorTwoInterface {
	    }

	    public class MyOperatorTwoInterfaceImplTwo : MyOperatorTwoInterface {
	    }

	    public class MyOperatorThreeForge : DataFlowOperatorForge {

	        [DataFlowOpParameter]
	        private MyOperatorThreeSettings settings;

	        private static IList<MyOperatorThreeForge> operators = new List<MyOperatorThreeForge>();

	        public static IList<MyOperatorThreeForge> Operators => operators;

	        public MyOperatorThreeForge() {
	            operators.Add(this);
	        }

	        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context) {
	            return null;
	        }

	        public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	            return ConstantNull();
	        }

	        public MyOperatorThreeSettings Settings => settings;
	    }

	    public interface MyOperatorThreeSettings {
	    }

	    public class MyOperatorThreeSettingsABC : MyOperatorThreeSettings {
	        [DataFlowOpParameter]
	        internal ExprNode parameterOne;

	        public MyOperatorThreeSettingsABC() {
	        }
	    }

	    public class MyOperatorFourForge : DataFlowOperatorForge {
	        private IDictionary<string, ExprNode> allProperties = new LinkedHashMap<string, ExprNode>();

	        [DataFlowOpParameter(IsAll = true)]
	        public void SetProperty(string name, ExprNode value) {
	            allProperties.Put(name, value);
	        }

	        public IDictionary<string, ExprNode> AllProperties => allProperties;

	        private static IList<MyOperatorFourForge> operators = new List<MyOperatorFourForge>();

	        public static IList<MyOperatorFourForge> Operators => operators;

	        public MyOperatorFourForge() {
	            operators.Add(this);
	        }

	        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context) {
	            return null;
	        }

	        public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	            return ConstantNull();
	        }
	    }
	}
} // end of namespace
