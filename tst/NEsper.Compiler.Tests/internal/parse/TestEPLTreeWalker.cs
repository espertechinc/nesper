///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.pattern.every;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.followedby;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.or;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compiler.@internal.parse
{
	[TestFixture]
	public class TestEPLTreeWalker : AbstractCompilerTest
	{
		private static string CLASSNAME = typeof(SupportBean).FullName;

		private static string EXPRESSION = "select * from " +
		                                   CLASSNAME +
		                                   "(string='a')#length(10)#lastevent as win1," +
		                                   CLASSNAME +
		                                   "(string='b')#length(10)#lastevent as win2 ";

		[Test]
		public void TestWalkGraph()
		{
			var expression = "create dataflow MyGraph MyOp((s0, s1) as ST1, s2) -> out1, out2 {}";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var graph = walker.StatementSpec.CreateDataFlowDesc;
			ClassicAssert.AreEqual("MyGraph", graph.GraphName);
			ClassicAssert.AreEqual(1, graph.Operators.Count);
			var op = graph.Operators[0];
			ClassicAssert.AreEqual("MyOp", op.OperatorName);

			// assert input
			ClassicAssert.AreEqual(2, op.Input.StreamNamesAndAliases.Count);
			var in1 = op.Input.StreamNamesAndAliases[0];
			EPAssertionUtil.AssertEqualsExactOrder("s0,s1".SplitCsv(), in1.InputStreamNames);
			ClassicAssert.AreEqual("ST1", in1.OptionalAsName);
			var in2 = op.Input.StreamNamesAndAliases[1];
			EPAssertionUtil.AssertEqualsExactOrder("s2".SplitCsv(), in2.InputStreamNames);
			ClassicAssert.IsNull(in2.OptionalAsName);

			// assert output
			ClassicAssert.AreEqual(2, op.Output.Items.Count);
			var out1 = op.Output.Items[0];
			ClassicAssert.AreEqual("out1", out1.StreamName);
			ClassicAssert.AreEqual(0, out1.TypeInfo.Count);
			var out2 = op.Output.Items[1];
			ClassicAssert.AreEqual("out2", out2.StreamName);
			ClassicAssert.AreEqual(0, out1.TypeInfo.Count);

			GraphOperatorOutputItemType type;

			type = TryWalkGraphTypes("out<?>");
			ClassicAssert.IsTrue(type.IsWildcard);
			ClassicAssert.IsNull(type.TypeOrClassname);
			ClassicAssert.IsNull(type.TypeParameters);

			type = TryWalkGraphTypes("out<eventbean<?>>");
			ClassicAssert.IsFalse(type.IsWildcard);
			ClassicAssert.AreEqual("eventbean", type.TypeOrClassname);
			ClassicAssert.AreEqual(1, type.TypeParameters.Count);
			ClassicAssert.IsTrue(type.TypeParameters[0].IsWildcard);
			ClassicAssert.IsNull(type.TypeParameters[0].TypeOrClassname);
			ClassicAssert.IsNull(type.TypeParameters[0].TypeParameters);

			type = TryWalkGraphTypes("out<eventbean<someschema>>");
			ClassicAssert.IsFalse(type.IsWildcard);
			ClassicAssert.AreEqual("eventbean", type.TypeOrClassname);
			ClassicAssert.AreEqual(1, type.TypeParameters.Count);
			ClassicAssert.IsFalse(type.TypeParameters[0].IsWildcard);
			ClassicAssert.AreEqual("someschema", type.TypeParameters[0].TypeOrClassname);
			ClassicAssert.AreEqual(0, type.TypeParameters[0].TypeParameters.Count);

			type = TryWalkGraphTypes("out<Map<String, Integer>>");
			ClassicAssert.IsFalse(type.IsWildcard);
			ClassicAssert.AreEqual("Map", type.TypeOrClassname);
			ClassicAssert.AreEqual(2, type.TypeParameters.Count);
			ClassicAssert.AreEqual("String", type.TypeParameters[0].TypeOrClassname);
			ClassicAssert.AreEqual("Integer", type.TypeParameters[1].TypeOrClassname);
		}

		private GraphOperatorOutputItemType TryWalkGraphTypes(string outstream)
		{
			var expression = "create dataflow MyGraph MyOp((s0, s1) as ST1, s2) -> " + outstream + " {}";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var graph = walker.StatementSpec.CreateDataFlowDesc;
			return graph.Operators[0].Output.Items[0].TypeInfo[0];
		}

		[Test]
		public void TestWalkCreateSchema()
		{
			var expression = "create schema MyName as com.company.SupportBean";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var schema = walker.StatementSpec.CreateSchemaDesc;
			ClassicAssert.AreEqual("MyName", schema.SchemaName);
			EPAssertionUtil.AssertEqualsExactOrder("com.company.SupportBean".SplitCsv(), schema.Types.ToArray());
			ClassicAssert.IsTrue(schema.Inherits.IsEmpty());
			ClassicAssert.IsTrue(schema.Columns.IsEmpty());
			ClassicAssert.AreEqual(AssignedType.NONE, schema.AssignedType);

			expression = "create schema MyName (col1 string, col2 int, col3 Type[]) inherits InheritedType";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			schema = walker.StatementSpec.CreateSchemaDesc;
			ClassicAssert.AreEqual("MyName", schema.SchemaName);
			ClassicAssert.IsTrue(schema.Types.IsEmpty());
			EPAssertionUtil.AssertEqualsExactOrder("InheritedType".SplitCsv(), schema.Inherits.ToArray());
			AssertSchema(schema.Columns[0], "col1", "string", false);
			AssertSchema(schema.Columns[1], "col2", "int", false);
			AssertSchema(schema.Columns[2], "col3", "Type", true);

			expression = "create variant schema MyName as MyNameTwo,MyNameThree";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			schema = walker.StatementSpec.CreateSchemaDesc;
			ClassicAssert.AreEqual("MyName", schema.SchemaName);
			EPAssertionUtil.AssertEqualsExactOrder("MyNameTwo,MyNameThree".SplitCsv(), schema.Types.ToArray());
			ClassicAssert.IsTrue(schema.Inherits.IsEmpty());
			ClassicAssert.IsTrue(schema.Columns.IsEmpty());
			ClassicAssert.AreEqual(AssignedType.VARIANT, schema.AssignedType);
		}

		private void AssertSchema(
			ColumnDesc element,
			string name,
			string type,
			bool isArray)
		{
			var clazz = ClassDescriptor.ParseTypeText(element.Type);
			ClassicAssert.AreEqual(name, element.Name);
			ClassicAssert.AreEqual(type, clazz.ClassIdentifier);
			ClassicAssert.AreEqual(isArray, clazz.ArrayDimensions > 0);
		}

		[Test]
		public void TestWalkCreateIndex()
		{
			var expression = "create index A_INDEX on B_NAMEDWIN (c, d btree)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var createIndex = walker.StatementSpec.CreateIndexDesc;
			ClassicAssert.AreEqual("A_INDEX", createIndex.IndexName);
			ClassicAssert.AreEqual("B_NAMEDWIN", createIndex.WindowName);
			ClassicAssert.AreEqual(2, createIndex.Columns.Count);
			ClassicAssert.AreEqual("c", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(createIndex.Columns[0].Expressions[0]));
			ClassicAssert.AreEqual(CreateIndexType.HASH.GetName().ToLowerInvariant(), createIndex.Columns[0].IndexType);
			ClassicAssert.AreEqual("d", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(createIndex.Columns[1].Expressions[0]));
			ClassicAssert.AreEqual(CreateIndexType.BTREE.GetName().ToLowerInvariant(), createIndex.Columns[1].IndexType);
		}

		[Test]
		public void TestWalkViewExpressions()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + ".win:x(IntPrimitive, a.Nested)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			var parameters = viewSpecs[0].ObjectParameters;
			ClassicAssert.AreEqual("IntPrimitive", ((ExprIdentNode) parameters[0]).FullUnresolvedName);
			ClassicAssert.AreEqual("a.Nested", ((ExprIdentNode) parameters[1]).FullUnresolvedName);
		}

		[Test]
		public void TestWalkJoinMethodStatement()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select distinct * from " + className + " unidirectional, method:com.MyClass.myMethod(string, 2*IntPrimitive) as s0";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			ClassicAssert.IsTrue(statementSpec.SelectClauseSpec.IsDistinct);
			ClassicAssert.AreEqual(2, statementSpec.StreamSpecs.Count);
			ClassicAssert.IsTrue(statementSpec.StreamSpecs[0].Options.IsUnidirectional);
			ClassicAssert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			ClassicAssert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);

			var methodSpec = (MethodStreamSpec) statementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual("method", methodSpec.Ident);
			ClassicAssert.AreEqual("com.MyClass", methodSpec.ClassName);
			ClassicAssert.AreEqual("myMethod", methodSpec.MethodName);
			ClassicAssert.AreEqual(2, methodSpec.Expressions.Count);
			ClassicAssert.IsTrue(methodSpec.Expressions[0] is ExprIdentNode);
			ClassicAssert.IsTrue(methodSpec.Expressions[1] is ExprMathNode);
		}

		[Test]
		public void TestWalkRetainKeywords()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + " retain-union";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			ClassicAssert.AreEqual(1, statementSpec.StreamSpecs.Count);
			ClassicAssert.IsTrue(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			ClassicAssert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);

			expression = "select * from " + className + " retain-intersection";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			statementSpec = walker.StatementSpec;
			ClassicAssert.AreEqual(1, statementSpec.StreamSpecs.Count);
			ClassicAssert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			ClassicAssert.IsTrue(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);
		}

		[Test]
		public void TestWalkCreateVariable()
		{
			var expression = "create constant variable sometype var1 = 1";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var createVarDesc = raw.CreateVariableDesc;
			ClassicAssert.AreEqual("sometype", createVarDesc.VariableType.ClassIdentifier);
			ClassicAssert.AreEqual("var1", createVarDesc.VariableName);
			ClassicAssert.IsTrue(createVarDesc.Assignment is ExprConstantNode);
			ClassicAssert.IsTrue(createVarDesc.IsConstant);
		}

		[Test]
		public void TestWalkOnUpdate()
		{
			var expression = "on com.MyClass as myevent update MyWindow as mw set prop1 = 'a', prop2=a.b*c where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual(0, streamSpec.RawFilterSpec.FilterExpressions.Count);
			ClassicAssert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var setDesc = (OnTriggerWindowUpdateDesc) raw.OnTriggerDesc;
			ClassicAssert.IsTrue(setDesc.OnTriggerType == OnTriggerType.ON_UPDATE);
			ClassicAssert.AreEqual(2, setDesc.Assignments.Count);

			var assign = setDesc.Assignments[0];
			ClassicAssert.AreEqual("prop1", ((ExprIdentNode) (assign.Expression.ChildNodes[0])).FullUnresolvedName);
			ClassicAssert.IsTrue(assign.Expression.ChildNodes[1] is ExprConstantNode);

			ClassicAssert.AreEqual("a=b", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(raw.WhereClause));
		}

		[Test]
		public void TestWalkOnSelectNoInsert()
		{
			var expression = "on com.MyClass(myval != 0) as myevent select *, mywin.* as abc, myevent.* from MyNamedWindow as mywin where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual(1, streamSpec.RawFilterSpec.FilterExpressions.Count);
			ClassicAssert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			ClassicAssert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			ClassicAssert.AreEqual("mywin", windowDesc.OptionalAsName);
			ClassicAssert.AreEqual(OnTriggerType.ON_SELECT, windowDesc.OnTriggerType);

			ClassicAssert.IsNull(raw.InsertIntoDesc);
			ClassicAssert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			ClassicAssert.AreEqual(3, raw.SelectClauseSpec.SelectExprList.Count);
			ClassicAssert.IsTrue(raw.SelectClauseSpec.SelectExprList[0] is SelectClauseElementWildcard);
			ClassicAssert.AreEqual("mywin", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).StreamName);
			ClassicAssert.AreEqual("mywin", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).StreamName);
			ClassicAssert.AreEqual("abc", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).OptionalAsName);
			ClassicAssert.AreEqual("myevent", (((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[2]).StreamName));
			ClassicAssert.IsNull(((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[2]).OptionalAsName);
			ClassicAssert.IsTrue(raw.WhereClause is ExprEqualsNode);
		}

		[Test]
		public void TestWalkOnSelectInsert()
		{
			var expression = "on pattern [com.MyClass] as pat insert into MyStream(a, b) select c, d from MyNamedWindow as mywin " +
" where a=b group by Symbol having c=d Order by e";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.IsTrue(streamSpec.EvalForgeNode is EvalFilterForgeNode);
			ClassicAssert.AreEqual("pat", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			ClassicAssert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			ClassicAssert.AreEqual("mywin", windowDesc.OptionalAsName);
			ClassicAssert.AreEqual(OnTriggerType.ON_SELECT, windowDesc.OnTriggerType);
			ClassicAssert.IsTrue(raw.WhereClause is ExprEqualsNode);

			ClassicAssert.AreEqual("MyStream", raw.InsertIntoDesc.EventTypeName);
			ClassicAssert.AreEqual(2, raw.InsertIntoDesc.ColumnNames.Count);
			ClassicAssert.AreEqual("a", raw.InsertIntoDesc.ColumnNames[0]);
			ClassicAssert.AreEqual("b", raw.InsertIntoDesc.ColumnNames[1]);

			ClassicAssert.IsFalse(raw.SelectClauseSpec.IsUsingWildcard);
			ClassicAssert.AreEqual(2, raw.SelectClauseSpec.SelectExprList.Count);

			ClassicAssert.AreEqual(1, raw.GroupByExpressions.Count);
			ClassicAssert.IsTrue(raw.HavingClause is ExprEqualsNode);
			ClassicAssert.AreEqual(1, raw.OrderByList.Count);
		}

		[Test]
		public void TestWalkOnSelectMultiInsert()
		{
			var expression = "on Bean as pat " +
			                 " insert into MyStream select * where 1>2" +
			                 " insert into BStream(a, b) select * where 1=2" +
			                 " insert into CStream select a,b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.AreEqual("pat", streamSpec.OptionalStreamName);

			var triggerDesc = (OnTriggerSplitStreamDesc) raw.OnTriggerDesc;
			ClassicAssert.AreEqual(OnTriggerType.ON_SPLITSTREAM, triggerDesc.OnTriggerType);
			ClassicAssert.AreEqual(2, triggerDesc.SplitStreams.Count);

			ClassicAssert.AreEqual("MyStream", raw.InsertIntoDesc.EventTypeName);
			ClassicAssert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			ClassicAssert.AreEqual(1, raw.SelectClauseSpec.SelectExprList.Count);
			ClassicAssert.IsNotNull((ExprRelationalOpNode) raw.WhereClause);

			var splitStream = triggerDesc.SplitStreams[0];
			ClassicAssert.AreEqual("BStream", splitStream.InsertInto.EventTypeName);
			ClassicAssert.AreEqual(2, splitStream.InsertInto.ColumnNames.Count);
			ClassicAssert.AreEqual("a", splitStream.InsertInto.ColumnNames[0]);
			ClassicAssert.AreEqual("b", splitStream.InsertInto.ColumnNames[1]);
			ClassicAssert.IsTrue(splitStream.SelectClause.IsUsingWildcard);
			ClassicAssert.AreEqual(1, splitStream.SelectClause.SelectExprList.Count);
			ClassicAssert.IsNotNull((ExprEqualsNode) splitStream.WhereClause);

			splitStream = triggerDesc.SplitStreams[1];
			ClassicAssert.AreEqual("CStream", splitStream.InsertInto.EventTypeName);
			ClassicAssert.AreEqual(0, splitStream.InsertInto.ColumnNames.Count);
			ClassicAssert.IsFalse(splitStream.SelectClause.IsUsingWildcard);
			ClassicAssert.AreEqual(2, splitStream.SelectClause.SelectExprList.Count);
			ClassicAssert.IsNull(splitStream.WhereClause);
		}

		[Test]
		public void TestWalkOnDelete()
		{
			// try a filter
			var expression = "on com.MyClass(myval != 0) as myevent delete from MyNamedWindow as mywin where mywin.key = myevent.otherKey";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual(1, streamSpec.RawFilterSpec.FilterExpressions.Count);
			ClassicAssert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			ClassicAssert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			ClassicAssert.AreEqual("mywin", windowDesc.OptionalAsName);
			ClassicAssert.AreEqual(OnTriggerType.ON_DELETE, windowDesc.OnTriggerType);

			ClassicAssert.IsTrue(raw.WhereClause is ExprEqualsNode);

			// try a pattern
			expression = "on pattern [every MyClass] as myevent delete from MyNamedWindow";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			raw = walker.StatementSpec;

			var patternSpec = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			ClassicAssert.IsTrue(patternSpec.EvalForgeNode is EvalEveryForgeNode);
		}

		[Test]
		public void TestWalkCreateWindow()
		{
			var expression = "create window MyWindow#groupwin(Symbol)#length(20) as select *, aprop, bprop as someval from com.MyClass insert where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			// window name
			ClassicAssert.AreEqual("MyWindow", raw.CreateWindowDesc.WindowName);
			ClassicAssert.IsTrue(raw.CreateWindowDesc.IsInsert);
			ClassicAssert.IsTrue(raw.CreateWindowDesc.InsertFilter is ExprEqualsNode);

			// select clause
			ClassicAssert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			var selectSpec = raw.SelectClauseSpec.SelectExprList;
			ClassicAssert.AreEqual(3, selectSpec.Count);
			ClassicAssert.IsTrue(raw.SelectClauseSpec.SelectExprList[0] is SelectClauseElementWildcard);
			var rawSpec = (SelectClauseExprRawSpec) selectSpec[1];
			ClassicAssert.AreEqual("aprop", ((ExprIdentNode) rawSpec.SelectExpression).UnresolvedPropertyName);
			rawSpec = (SelectClauseExprRawSpec) selectSpec[2];
			ClassicAssert.AreEqual("bprop", ((ExprIdentNode) rawSpec.SelectExpression).UnresolvedPropertyName);
			ClassicAssert.AreEqual("someval", rawSpec.OptionalAsName);

			// 2 views
			ClassicAssert.AreEqual(2, raw.CreateWindowDesc.ViewSpecs.Count);
			ClassicAssert.AreEqual("groupwin", raw.CreateWindowDesc.ViewSpecs[0].ObjectName);
			ClassicAssert.AreEqual(null, raw.CreateWindowDesc.ViewSpecs[0].ObjectNamespace);
			ClassicAssert.AreEqual("length", raw.CreateWindowDesc.ViewSpecs[1].ObjectName);
		}

		[Test]
		public void TestWalkMatchRecognize()
		{
			var patternTests = new string[] {
				"A", "A B", "A? B*", "(A|B)+", "A C|B C", "(G1|H1) (I1|J1)", "(G1*|H1)? (I1+|J1?)", "A B G (H H|(I P)?) K?"
			};

			for (var i = 0; i < patternTests.Length; i++) {
				var expression = "select * from MyEvent#keepall match_recognize (" +
				                 "  partition by string measures A.string as a_string pattern ( " +
				                 patternTests[i] +
				                 ") define A as (A.value = 1) )";

				var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
				var raw = walker.StatementSpec;

				ClassicAssert.AreEqual(1, raw.MatchRecognizeSpec.Measures.Count);
				ClassicAssert.AreEqual(1, raw.MatchRecognizeSpec.Defines.Count);
				ClassicAssert.AreEqual(1, raw.MatchRecognizeSpec.PartitionByExpressions.Count);

				var writer = new StringWriter();
				raw.MatchRecognizeSpec.Pattern.ToEPL(writer, RowRecogExprNodePrecedenceEnum.MINIMUM);
				var received = writer.ToString();
				ClassicAssert.AreEqual(patternTests[i], received);
			}
		}

		[Test]
		public void TestWalkSubstitutionParams()
		{
			// try EPL
			var expression = "select * from " + CLASSNAME + "(string=?, value=?)";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			walker.End();
			var raw = walker.StatementSpec;
			ClassicAssert.AreEqual(2, raw.SubstitutionParameters.Count);

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			var equalsFilter = (ExprEqualsNode) streamSpec.RawFilterSpec.FilterExpressions[0];
			equalsFilter = (ExprEqualsNode) streamSpec.RawFilterSpec.FilterExpressions[1];
		}

		[Test]
		public void TestWalkPatternMatchUntil()
		{
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[A until (B or C)]");
			var raw = walker.StatementSpec;
			var a = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			var matchNode = (EvalMatchUntilForgeNode) a.EvalForgeNode;
			ClassicAssert.AreEqual(2, matchNode.ChildNodes.Count);
			ClassicAssert.IsTrue(matchNode.ChildNodes[0] is EvalFilterForgeNode);
			ClassicAssert.IsTrue(matchNode.ChildNodes[1] is EvalOrForgeNode);

			var spec = GetMatchUntilSpec("A until (B or C)");
			ClassicAssert.IsNull(spec.LowerBounds);
			ClassicAssert.IsNull(spec.UpperBounds);

			spec = GetMatchUntilSpec("[1:10] A until (B or C)");
			ClassicAssert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			ClassicAssert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1 : 10] A until (B or C)");
			ClassicAssert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			ClassicAssert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1:10] A until (B or C)");
			ClassicAssert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			ClassicAssert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1:] A until (B or C)");
			ClassicAssert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			ClassicAssert.AreEqual(null, spec.UpperBounds);

			spec = GetMatchUntilSpec("[1 :] A until (B or C)");
			ClassicAssert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			ClassicAssert.AreEqual(null, spec.UpperBounds);
			ClassicAssert.AreEqual(null, spec.SingleBound);

			spec = GetMatchUntilSpec("[:2] A until (B or C)");
			ClassicAssert.AreEqual(null, spec.LowerBounds);
			ClassicAssert.AreEqual(null, spec.SingleBound);
			ClassicAssert.AreEqual(2, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[: 2] A until (B or C)");
			ClassicAssert.AreEqual(null, spec.LowerBounds);
			ClassicAssert.AreEqual(null, spec.SingleBound);
			ClassicAssert.AreEqual(2, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[2] A until (B or C)");
			ClassicAssert.AreEqual(2, spec.SingleBound.Forge.ExprEvaluator.Evaluate(null, true, null));
		}

		private EvalMatchUntilForgeNode GetMatchUntilSpec(string text)
		{
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + text + "]");
			var raw = walker.StatementSpec;
			var a = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			return (EvalMatchUntilForgeNode) a.EvalForgeNode;
		}

		[Test]
		public void TestWalkSimpleWhere()
		{
			var expression = EXPRESSION + "where win1.f1=win2.f2";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);

			ClassicAssert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);

			var streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			ClassicAssert.AreEqual(2, streamSpec.ViewSpecs.Length);
			ClassicAssert.AreEqual(typeof(SupportBean).FullName, streamSpec.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual("length", streamSpec.ViewSpecs[0].ObjectName);
			ClassicAssert.AreEqual("lastevent", streamSpec.ViewSpecs[1].ObjectName);
			ClassicAssert.AreEqual("win1", streamSpec.OptionalStreamName);

			streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual("win2", streamSpec.OptionalStreamName);

			// Join expression tree validation
			ClassicAssert.IsTrue(walker.StatementSpec.WhereClause is ExprEqualsNode);
			var equalsNode = (walker.StatementSpec.WhereClause);
			ClassicAssert.AreEqual(2, equalsNode.ChildNodes.Length);

			var identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			ClassicAssert.AreEqual("win1", identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f1", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			ClassicAssert.AreEqual("win2", identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f2", identNode.UnresolvedPropertyName);
		}

		[Test]
		public void TestWalkWhereWithAnd()
		{
			var expression = "select * from " +
			                 CLASSNAME +
			                 "(string='a')#length(10)#lastevent as win1," +
			                 CLASSNAME +
			                 "(string='b')#length(9)#lastevent as win2, " +
			                 CLASSNAME +
			                 "(string='c')#length(3)#lastevent as win3 " +
			                 "where win1.f1=win2.f2 and win3.f3=f4 limit 5 offset 10";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);

			// ProjectedStream spec validation
			ClassicAssert.AreEqual(3, walker.StatementSpec.StreamSpecs.Count);
			ClassicAssert.AreEqual("win1", walker.StatementSpec.StreamSpecs[0].OptionalStreamName);
			ClassicAssert.AreEqual("win2", walker.StatementSpec.StreamSpecs[1].OptionalStreamName);
			ClassicAssert.AreEqual("win3", walker.StatementSpec.StreamSpecs[2].OptionalStreamName);

			var streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[2];
			ClassicAssert.AreEqual(2, streamSpec.ViewSpecs.Length);
			ClassicAssert.AreEqual(typeof(SupportBean).FullName, streamSpec.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual("length", streamSpec.ViewSpecs[0].ObjectName);
			ClassicAssert.AreEqual("lastevent", streamSpec.ViewSpecs[1].ObjectName);

			// Join expression tree validation
			ClassicAssert.IsTrue(walker.StatementSpec.WhereClause is ExprAndNode);
			ClassicAssert.AreEqual(2, walker.StatementSpec.WhereClause.ChildNodes.Length);
			var equalsNode = (walker.StatementSpec.WhereClause.ChildNodes[0]);
			ClassicAssert.AreEqual(2, equalsNode.ChildNodes.Length);

			var identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			ClassicAssert.AreEqual("win1", identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f1", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			ClassicAssert.AreEqual("win2", identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f2", identNode.UnresolvedPropertyName);

			equalsNode = (walker.StatementSpec.WhereClause.ChildNodes[1]);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			ClassicAssert.AreEqual("win3", identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f3", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			ClassicAssert.IsNull(identNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f4", identNode.UnresolvedPropertyName);

			ClassicAssert.AreEqual(5, (int) walker.StatementSpec.RowLimitSpec.NumRows);
			ClassicAssert.AreEqual(10, (int) walker.StatementSpec.RowLimitSpec.OptionalOffset);
		}

		[Test]
		public void TestWalkPerRowFunctions()
		{
			ClassicAssert.AreEqual(9, TryExpression("max(6, 9)"));
			ClassicAssert.AreEqual(6.11, TryExpression("min(6.11, 6.12)"));
			ClassicAssert.AreEqual(6.10, TryExpression("min(6.11, 6.12, 6.1)"));
			ClassicAssert.AreEqual("ab", TryExpression("'a'||'b'"));
			ClassicAssert.AreEqual(null, TryExpression("coalesce(null, null)"));
			ClassicAssert.AreEqual(1, TryExpression("coalesce(null, 1)"));
			ClassicAssert.AreEqual(1L, TryExpression("coalesce(null, 1l)"));
			ClassicAssert.AreEqual("a", TryExpression("coalesce(null, 'a', 'b')"));
			ClassicAssert.AreEqual(13.5d, TryExpression("coalesce(null, null, 3*4.5)"));
			ClassicAssert.AreEqual(true, TryExpression("coalesce(null, true)"));
			ClassicAssert.AreEqual(5, TryExpression("coalesce(5, null, 6)"));
			ClassicAssert.AreEqual(2, TryExpression("(case 1 when 1 then 2 end)"));
		}

		[Test]
		public void TestWalkMath()
		{
			ClassicAssert.AreEqual(32.0, TryExpression("5*6-3+15/3"));
			ClassicAssert.AreEqual(-5, TryExpression("1-1-1-2-1-1"));
			ClassicAssert.AreEqual(2.8d, TryExpression("1.4 + 1.4"));
			ClassicAssert.AreEqual(1d, TryExpression("55.5/5/11.1"));
			ClassicAssert.AreEqual(2 / 3d, TryExpression("2/3"));
			ClassicAssert.AreEqual(2 / 3d, TryExpression("2.0/3"));
			ClassicAssert.AreEqual(10, TryExpression("(1+4)*2"));
			ClassicAssert.AreEqual(12, TryExpression("(3*(6-4))*2"));
			ClassicAssert.AreEqual(8.5, TryExpression("(1+(4*3)+2)/2+1"));
			ClassicAssert.AreEqual(1, TryExpression("10%3"));
			ClassicAssert.AreEqual(10.1 % 3, TryExpression("10.1%3"));
		}

		[Test]
		public void TestWalkRelationalOp()
		{
			ClassicAssert.AreEqual(true, TryRelationalOp("3>2"));
			ClassicAssert.AreEqual(true, TryRelationalOp("3*5/2 >= 7.5"));
			ClassicAssert.AreEqual(true, TryRelationalOp("3*5/2.0 >= 7.5"));
			ClassicAssert.AreEqual(false, TryRelationalOp("1.1 + 2.2 < 3.2"));
			ClassicAssert.AreEqual(false, TryRelationalOp("3<=2"));
			ClassicAssert.AreEqual(true, TryRelationalOp("4*(3+1)>=16"));

			ClassicAssert.AreEqual(false, TryRelationalOp("(4>2) and (2>3)"));
			ClassicAssert.AreEqual(true, TryRelationalOp("(4>2) or (2>3)"));

			ClassicAssert.AreEqual(false, TryRelationalOp("not 3>2"));
			ClassicAssert.AreEqual(true, TryRelationalOp("not (not 3>2)"));
		}

		[Test]
		public void TestWalkInsertInto()
		{
			var expression = "insert into MyAlias select * from " +
			                 CLASSNAME +
			                 "()#length(10)#lastevent as win1," +
			                 CLASSNAME +
			                 "(string='b')#length(9)#lastevent as win2";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);

			var desc = walker.StatementSpec.InsertIntoDesc;
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, desc.StreamSelector);
			ClassicAssert.AreEqual("MyAlias", desc.EventTypeName);
			ClassicAssert.AreEqual(0, desc.ColumnNames.Count);

			expression = "insert rstream into MyAlias(a, b, c) select * from " +
			             CLASSNAME +
			             "()#length(10)#lastevent as win1," +
			             CLASSNAME +
			             "(string='b')#length(9)#lastevent as win2";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);

			desc = walker.StatementSpec.InsertIntoDesc;
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ONLY, desc.StreamSelector);
			ClassicAssert.AreEqual("MyAlias", desc.EventTypeName);
			ClassicAssert.AreEqual(3, desc.ColumnNames.Count);
			ClassicAssert.AreEqual("a", desc.ColumnNames[0]);
			ClassicAssert.AreEqual("b", desc.ColumnNames[1]);
			ClassicAssert.AreEqual("c", desc.ColumnNames[2]);

			expression = "insert irstream into Test2 select * from " + CLASSNAME + "()#length(10)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			desc = walker.StatementSpec.InsertIntoDesc;
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH, desc.StreamSelector);
			ClassicAssert.AreEqual("Test2", desc.EventTypeName);
			ClassicAssert.AreEqual(0, desc.ColumnNames.Count);
		}

		[Test]
		public void TestWalkView()
		{
			var text = "select * from " + typeof(SupportBean).FullName + "(string=\"IBM\").win:lenght(10, 1.1, \"a\").stat:uni(Price, false)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var filterSpec = ((FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0]).RawFilterSpec;

			// Check filter spec properties
			ClassicAssert.AreEqual(typeof(SupportBean).FullName, filterSpec.EventTypeName);
			ClassicAssert.AreEqual(1, filterSpec.FilterExpressions.Count);

			// Check views
			var viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			ClassicAssert.AreEqual(2, viewSpecs.Length);

			var specOne = viewSpecs[0];
			ClassicAssert.AreEqual("win", specOne.ObjectNamespace);
			ClassicAssert.AreEqual("lenght", specOne.ObjectName);
			ClassicAssert.AreEqual(3, specOne.ObjectParameters.Count);
			ClassicAssert.AreEqual(10, ((ExprConstantNode) specOne.ObjectParameters[0]).ConstantValue);
			ClassicAssert.AreEqual(1.1d, ((ExprConstantNode) specOne.ObjectParameters[1]).ConstantValue);
			ClassicAssert.AreEqual("a", ((ExprConstantNode) specOne.ObjectParameters[2]).ConstantValue);

			var specTwo = viewSpecs[1];
			ClassicAssert.AreEqual("stat", specTwo.ObjectNamespace);
			ClassicAssert.AreEqual("uni", specTwo.ObjectName);
			ClassicAssert.AreEqual(2, specTwo.ObjectParameters.Count);
			ClassicAssert.AreEqual("Price", ((ExprIdentNode) specTwo.ObjectParameters[0]).FullUnresolvedName);
			ClassicAssert.AreEqual(false, ((ExprConstantNode) specTwo.ObjectParameters[1]).ConstantValue);
		}

		[Test]
		public void TestWalkPropertyExpr()
		{
			var text = "select * from " + typeof(SupportBean).FullName + "[a.b][select c,d.*,* from e as f where g]";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var filterSpec = ((FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0]).RawFilterSpec;
			ClassicAssert.AreEqual(2, filterSpec.OptionalPropertyEvalSpec.Atoms.Count);
			ClassicAssert.AreEqual("a.b", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(filterSpec.OptionalPropertyEvalSpec.Atoms[0].SplitterExpression));
			ClassicAssert.AreEqual(0, filterSpec.OptionalPropertyEvalSpec.Atoms[0].OptionalSelectClause.SelectExprList.Count);

			var atomTwo = filterSpec.OptionalPropertyEvalSpec.Atoms[1];
			ClassicAssert.AreEqual("e", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(atomTwo.SplitterExpression));
			ClassicAssert.AreEqual("f", atomTwo.OptionalAsName);
			ClassicAssert.IsNotNull(atomTwo.OptionalWhereClause);
			var list = atomTwo.OptionalSelectClause.SelectExprList;
			ClassicAssert.AreEqual(3, list.Count);
			ClassicAssert.IsTrue(list[0] is SelectClauseExprRawSpec);
			ClassicAssert.IsTrue(list[1] is SelectClauseStreamRawSpec);
			ClassicAssert.IsTrue(list[2] is SelectClauseElementWildcard);
		}

		[Test]
		public void TestSelectList()
		{
			var text = "select IntPrimitive, 2 * IntBoxed, 5 as myConst, stream0.string as TheString from "+
			           typeof(SupportBean).FullName +
			           "().win:lenght(10) as stream0";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var selectExpressions = walker.StatementSpec.SelectClauseSpec.SelectExprList;
			ClassicAssert.AreEqual(4, selectExpressions.Count);

			var rawSpec = (SelectClauseExprRawSpec) selectExpressions[0];
			ClassicAssert.IsTrue(rawSpec.SelectExpression is ExprIdentNode);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[1];
			ClassicAssert.IsTrue(rawSpec.SelectExpression is ExprMathNode);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[2];
			ClassicAssert.IsTrue(rawSpec.SelectExpression is ExprConstantNode);
			ClassicAssert.AreEqual("myConst", rawSpec.OptionalAsName);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[3];
			ClassicAssert.IsTrue(rawSpec.SelectExpression is ExprIdentNode);
			ClassicAssert.AreEqual("TheString", rawSpec.OptionalAsName);
			ClassicAssert.IsNull(walker.StatementSpec.InsertIntoDesc);

			text = "select * from " + typeof(SupportBean).FullName + "().win:lenght(10)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			ClassicAssert.AreEqual(1, walker.StatementSpec.SelectClauseSpec.SelectExprList.Count);
		}

		[Test]
		public void TestArrayViewParams()
		{
			// Check a list of integer as a view parameter
			var text = "select * from " + typeof(SupportBean).FullName + "().win:lenght({10, 11, 12})";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			var node = viewSpecs[0].ObjectParameters[0];
			node.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
			var intParams = ((ExprArrayNode) node).Forge.ExprEvaluator.Evaluate(null, true, null).UnwrapIntoArray<int>();
			ClassicAssert.AreEqual(10, intParams[0]);
			ClassicAssert.AreEqual(11, intParams[1]);
			ClassicAssert.AreEqual(12, intParams[2]);

			// Check a list of objects
			text = "select * from " + typeof(SupportBean).FullName + "().win:lenght({false, 11.2, 's'})";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			var param = viewSpecs[0].ObjectParameters[0];
			param.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
			var objParams = ((ExprArrayNode) param).Forge.ExprEvaluator.Evaluate(null, true, null).UnwrapIntoArray<object>();
			ClassicAssert.AreEqual(false, objParams[0]);
			ClassicAssert.AreEqual(11.2, objParams[1]);
			ClassicAssert.AreEqual("s", objParams[2]);
		}

		[Test]
		public void TestOuterJoin()
		{
			TryOuterJoin("left", OuterJoinType.LEFT);
			TryOuterJoin("right", OuterJoinType.RIGHT);
			TryOuterJoin("full", OuterJoinType.FULL);
		}

		[Test]
		public void TestNoPackageName()
		{
			var text = "select IntPrimitive from SupportBean_N().win:lenght(10) as win1";
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		[Test]
		public void TestAggregateFunction()
		{
			var fromClause = "from " + typeof(SupportBean_N).FullName + "().win:lenght(10) as win1";
			var text = "select max(distinct IntPrimitive) "+ fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);

			text = "select sum(IntPrimitive),"+
"sum(distinct DoubleBoxed),"+
"avg(DoubleBoxed),"+
"avg(distinct DoubleBoxed),"+
			       "count(*)," +
"count(IntPrimitive),"+
"count(distinct IntPrimitive),"+
"max(distinct IntPrimitive),"+
"min(distinct IntPrimitive),"+
"max(IntPrimitive),"+
"min(IntPrimitive), "+
"median(IntPrimitive), "+
"median(distinct IntPrimitive),"+
"stddev(IntPrimitive), "+
"stddev(distinct IntPrimitive),"+
"avedev(IntPrimitive),"+
"avedev(distinct IntPrimitive) "+
			       fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);

			// try min-max aggregate versus row functions
			text = "select max(IntPrimitive), min(IntPrimitive),"+
"max(IntPrimitive,IntBoxed), min(IntPrimitive,IntBoxed),"+
"max(distinct IntPrimitive), min(distinct IntPrimitive)"+
			       fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		[Test]
		public void TestGroupBy()
		{
			var text = "select sum(IntPrimitive) from SupportBean_N().win:lenght(10) as win1 where IntBoxed > 5 "+
"group by IntBoxed, 3 * DoubleBoxed, max(2, DoublePrimitive)";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var groupByList = walker.StatementSpec.GroupByExpressions;
			ClassicAssert.AreEqual(3, groupByList.Count);

			var node = ((GroupByClauseElementExpr) groupByList[0]).Expr;
			ClassicAssert.IsTrue(node is ExprIdentNode);

			node = ((GroupByClauseElementExpr) groupByList[1]).Expr;
			ClassicAssert.IsTrue(node is ExprMathNode);
			ClassicAssert.IsTrue(node.ChildNodes[0] is ExprConstantNode);
			ClassicAssert.IsTrue(node.ChildNodes[1] is ExprIdentNode);

			node = ((GroupByClauseElementExpr) groupByList[2]).Expr;
			ClassicAssert.IsTrue(node is ExprMinMaxRowNode);
		}

		[Test]
		public void TestHaving()
		{
			var text = "select sum(IntPrimitive) from SupportBean_N().win:lenght(10) as win1 where IntBoxed > 5 "+
"group by IntBoxed having sum(IntPrimitive) > 5";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var havingNode = walker.StatementSpec.HavingClause;

			ClassicAssert.IsTrue(havingNode is ExprRelationalOpNode);
			ClassicAssert.IsTrue(havingNode.ChildNodes[0] is ExprSumNode);
			ClassicAssert.IsTrue(havingNode.ChildNodes[1] is ExprConstantNode);

			text = "select sum(IntPrimitive) from SupportBean_N().win:lenght(10) as win1 where IntBoxed > 5 "+
"having IntPrimitive < avg(IntPrimitive)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			havingNode = walker.StatementSpec.HavingClause;
			ClassicAssert.IsTrue(havingNode is ExprRelationalOpNode);
		}

		[Test]
		public void TestDistinct()
		{
			var text = "select sum(distinct IntPrimitive) from SupportBean_N().win:lenght(10) as win1";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var rawElement = walker.StatementSpec.SelectClauseSpec.SelectExprList[0];
			var exprSpec = (SelectClauseExprRawSpec) rawElement;
			ExprAggregateNodeBase aggrNode = (ExprAggregateNodeBase) exprSpec.SelectExpression;
			ClassicAssert.IsTrue(aggrNode.IsDistinct);
		}

		[Test]
		public void TestComplexProperty()
		{
			var text = "select array [ 1 ],s0.map('a'),nested.nested2, a[1].b as x, nested.abcdef? " +
			           " from SupportBean_N().win:lenght(10) as win1 " +
			           " where a[1].b('a').Nested.c[0] = 4";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 0).SelectExpression;
			ClassicAssert.AreEqual("array[1]", identNode.UnresolvedPropertyName);
			ClassicAssert.IsNull(identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 1).SelectExpression;
			ClassicAssert.AreEqual("map('a')", identNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("s0", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 2).SelectExpression;
			ClassicAssert.AreEqual("nested2", identNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("nested", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 3).SelectExpression;
			ClassicAssert.AreEqual("a[1].b", identNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual(null, identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 4).SelectExpression;
			ClassicAssert.AreEqual("abcdef?", identNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("nested", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) walker.StatementSpec.WhereClause.ChildNodes[0];
			ClassicAssert.AreEqual("a[1].b('a').Nested.c[0]", identNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual(null, identNode.StreamOrPropertyName);
		}

		[Test]
		public void TestBitWise()
		{
			var text = "select IntPrimitive & IntBoxed from "+ typeof(SupportBean).FullName + "().win:lenght(10) as stream0";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var selectExpressions = walker.StatementSpec.SelectClauseSpec.SelectExprList;
			ClassicAssert.AreEqual(1, selectExpressions.Count);
			ClassicAssert.IsTrue(GetSelectExprSpec(walker.StatementSpec, 0).SelectExpression is ExprBitWiseNode);

			ClassicAssert.AreEqual(0, TryBitWise("1&2"));
			ClassicAssert.AreEqual(3, TryBitWise("1|2"));
			ClassicAssert.AreEqual(8, TryBitWise("10^2"));
		}

		[Test]
		public void TestPatternsOnly()
		{
			var patternOne = "a=" + typeof(SupportBean).FullName + " -> b=" + typeof(SupportBean).FullName;

			// Test simple case, one pattern and no "as streamName"
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "]");
			ClassicAssert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			ClassicAssert.AreEqual(typeof(EvalFollowedByForgeNode), patternStreamSpec.EvalForgeNode.GetType());
			ClassicAssert.IsNull(patternStreamSpec.OptionalStreamName);

			// Test case with "as s0"
			walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "] as s0");
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			ClassicAssert.AreEqual("s0", patternStreamSpec.OptionalStreamName);

			// Test case with multiple patterns
			var patternTwo = "c=" + typeof(SupportBean).FullName + " or " + typeof(SupportBean).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "] as s0, pattern [" + patternTwo + "] as s1");
			ClassicAssert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			ClassicAssert.AreEqual("s0", patternStreamSpec.OptionalStreamName);
			ClassicAssert.AreEqual(typeof(EvalFollowedByForgeNode), patternStreamSpec.EvalForgeNode.GetType());

			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual("s1", patternStreamSpec.OptionalStreamName);
			ClassicAssert.AreEqual(typeof(EvalOrForgeNode), patternStreamSpec.EvalForgeNode.GetType());

			// Test 3 patterns
			walker = SupportParserHelper.ParseAndWalkEPL(
				container,
				"select * from pattern [" +
				patternOne +
				"], pattern [" +
				patternTwo +
				"] as s1," +
				"pattern[x=" +
				typeof(SupportBean_S2).FullName +
				"] as s2");
			ClassicAssert.AreEqual(3, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[2];
			ClassicAssert.AreEqual("s2", patternStreamSpec.OptionalStreamName);

			// Test patterns with views
			walker = SupportParserHelper.ParseAndWalkEPL(
				container,
				"select * from pattern [" + patternOne + "]#time(1), pattern [" + patternTwo + "]#length(1)#lastevent as s1");
			ClassicAssert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			ClassicAssert.AreEqual(1, patternStreamSpec.ViewSpecs.Length);
			ClassicAssert.AreEqual("time", patternStreamSpec.ViewSpecs[0].ObjectName);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual(2, patternStreamSpec.ViewSpecs.Length);
			ClassicAssert.AreEqual("length", patternStreamSpec.ViewSpecs[0].ObjectName);
			ClassicAssert.AreEqual("lastevent", patternStreamSpec.ViewSpecs[1].ObjectName);
		}

		[Test]
		public void TestIfThenElseCase()
		{
			string text;
			text = "select case when IntPrimitive > ShortPrimitive then count(IntPrimitive) end from "+ typeof(SupportBean).FullName + "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case when IntPrimitive > ShortPrimitive then count(IntPrimitive) end as p1 from "+
			       typeof(SupportBean).FullName +
			       "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case when IntPrimitive > ShortPrimitive then count(IntPrimitive) else ShortPrimitive end from "+
			       typeof(SupportBean).FullName +
			       "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text =
"select case when IntPrimitive > ShortPrimitive then count(IntPrimitive) when LongPrimitive > IntPrimitive then count(LongPrimitive) else ShortPrimitive end from "+
				typeof(SupportBean).FullName +
				"().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case IntPrimitive  when 1 then count(IntPrimitive) end from "+ typeof(SupportBean).FullName + "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case IntPrimitive when LongPrimitive then (IntPrimitive + LongPrimitive) end"+
			       " from " +
			       typeof(SupportBean).FullName +
			       "#length(3)";
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		private void TryOuterJoin(
			string outerType,
			OuterJoinType typeExpected)
		{
			var text = "select IntPrimitive from "+
			           typeof(SupportBean_A).FullName +
			           "().win:lenght(10) as win1 " +
			           outerType +
			           " outer join " +
			           typeof(SupportBean_A).FullName +
			           "().win:lenght(10) as win2 " +
			           "on win1.f1 = win2.f2[1]";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var descList = walker.StatementSpec.OuterJoinDescList;
			ClassicAssert.AreEqual(1, descList.Count);
			var desc = descList[0];
			ClassicAssert.AreEqual(typeExpected, desc.OuterJoinType);
			ClassicAssert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f2[1]", desc.OptRightNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win2", desc.OptRightNode.StreamOrPropertyName);

			text = "select IntPrimitive from "+
			       typeof(SupportBean_A).FullName +
			       "().win:lenght(10) as win1 " +
			       outerType +
			       " outer join " +
			       typeof(SupportBean_A).FullName +
			       "().win:lenght(10) as win2 " +
			       "on win1.f1 = win2.f2 " +
			       outerType +
			       " outer join " +
			       typeof(SupportBean_A).FullName +
			       "().win:lenght(10) as win3 " +
			       "on win1.f1 = win3.f3 and win1.f11 = win3.f31";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			descList = walker.StatementSpec.OuterJoinDescList;
			ClassicAssert.AreEqual(2, descList.Count);

			desc = descList[0];
			ClassicAssert.AreEqual(typeExpected, desc.OuterJoinType);
			ClassicAssert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f2", desc.OptRightNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win2", desc.OptRightNode.StreamOrPropertyName);

			desc = descList[1];
			ClassicAssert.AreEqual(typeExpected, desc.OuterJoinType);
			ClassicAssert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			ClassicAssert.AreEqual("f3", desc.OptRightNode.UnresolvedPropertyName);
			ClassicAssert.AreEqual("win3", desc.OptRightNode.StreamOrPropertyName);

			ClassicAssert.AreEqual(1, desc.AdditionalLeftNodes.Length);
			ClassicAssert.AreEqual("f11", desc.AdditionalLeftNodes[0].UnresolvedPropertyName);
			ClassicAssert.AreEqual("win1", desc.AdditionalLeftNodes[0].StreamOrPropertyName);
			ClassicAssert.AreEqual(1, desc.AdditionalRightNodes.Length);
			ClassicAssert.AreEqual("f31", desc.AdditionalRightNodes[0].UnresolvedPropertyName);
			ClassicAssert.AreEqual("win3", desc.AdditionalRightNodes[0].StreamOrPropertyName);
		}

		[Test]
		public void TestOnMerge()
		{
			var text = "on MyEvent ev " +
			           "merge MyWindow " +
			           "where a not in (b) " +
			           "when matched and y=100 " +
			           "  then insert into xyz1 select g1,g2 where u>2" +
			           "  then update set a=b where e like '%a' " +
			           "  then delete where myvar " +
			           "  then delete " +
			           "when not matched and y=2 " +
			           "  then insert into xyz select * where e=4" +
			           "  then insert select * where t=2";

			var spec = SupportParserHelper.ParseAndWalkEPL(container, text).StatementSpec;
			var merge = (OnTriggerMergeDesc) spec.OnTriggerDesc;
			ClassicAssert.AreEqual(2, merge.Items.Count);
			ClassicAssert.IsTrue(spec.WhereClause is ExprInNode);

			var first = merge.Items[0];
			ClassicAssert.AreEqual(4, first.Actions.Count);
			ClassicAssert.IsTrue(first.IsMatchedUnmatched);
			ClassicAssert.IsTrue(first.OptionalMatchCond is ExprEqualsNode);

			var insertOne = (OnTriggerMergeActionInsert) first.Actions[0];
			ClassicAssert.AreEqual("xyz1", insertOne.OptionalStreamName);
			ClassicAssert.AreEqual(0, insertOne.Columns.Count);
			ClassicAssert.AreEqual(2, insertOne.SelectClause.Count);
			ClassicAssert.IsTrue(insertOne.OptionalWhereClause is ExprRelationalOpNode);

			var updateOne = (OnTriggerMergeActionUpdate) first.Actions[1];
			ClassicAssert.AreEqual(1, updateOne.Assignments.Count);
			ClassicAssert.IsTrue(updateOne.OptionalWhereClause is ExprLikeNode);

			var delOne = (OnTriggerMergeActionDelete) first.Actions[2];
			ClassicAssert.IsTrue(delOne.OptionalWhereClause is ExprIdentNode);

			var delTwo = (OnTriggerMergeActionDelete) first.Actions[3];
			ClassicAssert.IsNull(delTwo.OptionalWhereClause);

			var second = merge.Items[1];
			ClassicAssert.IsFalse(second.IsMatchedUnmatched);
			ClassicAssert.IsTrue(second.OptionalMatchCond is ExprEqualsNode);
			ClassicAssert.AreEqual(2, second.Actions.Count);
		}

		[Test]
		public void TestWalkPattern()
		{
			var text = "every g=" + typeof(SupportBean).FullName + "(string=\"IBM\", IntPrimitive != 1) where timer:within(20)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + text + "]");

			ClassicAssert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			var rootNode = patternStreamSpec.EvalForgeNode;

			var everyNode = (EvalEveryForgeNode) rootNode;

			ClassicAssert.IsTrue(everyNode.ChildNodes[0] is EvalGuardForgeNode);
			var guardNode = (EvalGuardForgeNode) everyNode.ChildNodes[0];

			ClassicAssert.AreEqual(1, guardNode.ChildNodes.Count);
			ClassicAssert.IsTrue(guardNode.ChildNodes[0] is EvalFilterForgeNode);
			var filterNode = (EvalFilterForgeNode) guardNode.ChildNodes[0];

			ClassicAssert.AreEqual("g", filterNode.EventAsName);
			ClassicAssert.AreEqual(0, filterNode.ChildNodes.Count);
			ClassicAssert.AreEqual(2, filterNode.RawFilterSpec.FilterExpressions.Count);
			var equalsNode = (ExprEqualsNode) filterNode.RawFilterSpec.FilterExpressions[1];
			ClassicAssert.AreEqual(2, equalsNode.ChildNodes.Length);
		}

		[Test]
		public void TestWalkPropertyPatternCombination()
		{
			var EVENT = typeof(SupportBeanComplexProps).FullName;
			var property = TryWalkGetPropertyPattern(EVENT + "(Mapped ( 'key' )  = 'value')");
			ClassicAssert.AreEqual("Mapped('key')", property);

			property = TryWalkGetPropertyPattern(EVENT + "(Indexed [ 1 ]  = 1)");
			ClassicAssert.AreEqual("Indexed[1]", property);
			property = TryWalkGetPropertyPattern(EVENT + "(Nested . NestedValue  = 'value')");
			ClassicAssert.AreEqual("NestedValue", property);
		}

		[Test]
		public void TestWalkPatternUseResult()
		{
			var EVENT = typeof(SupportBean_N).FullName;
			var text = "na=" + EVENT + "() -> every nb=" + EVENT + "(DoublePrimitive in [0:na.DoublePrimitive])";
			SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + text + "]");
		}

		[Test]
		public void TestWalkIStreamRStreamSelect()
		{
			var text = "select rstream 'a' from " + typeof(SupportBean_N).FullName;
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select istream 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select irstream 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			ClassicAssert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH, walker.StatementSpec.SelectStreamSelectorEnum);
		}

		[Test]
		public void TestWalkPatternIntervals()
		{
			object[][] intervals = {
				new object[] {"1E2 milliseconds", 0.1d},
				new object[] {"11 millisecond", 11 / 1000d},
				new object[] {"1.1 msec", 1.1 / 1000d},
				new object[] {"1 usec", 1 / 1000000d},
				new object[] {"1.1 microsecond", 1.1 / 1000000d},
				new object[] {"1E3 microsecond", 1000 / 1000000d},
				new object[] {"5 seconds 1 microsecond", 5d + 1 / 1000000d},
				new object[] {"5 seconds", 5d},
				new object[] {"0.1 second", 0.1d},
				new object[] {"135L sec", 135d},
				new object[] {"1.4 minutes", 1.4 * 60d},
				new object[] {"11 minute", 11 * 60d},
				new object[] {"123.2 min", 123.2 * 60d},
				new object[] {".2 hour", .2 * 60 * 60d},
				new object[] {"11.2 hours", 11.2 * 60 * 60d},
				new object[] {"2 day", 2 * 24 * 60 * 60d},
				new object[] {"11.2 days", 11.2 * 24 * 60 * 60d},
				new object[] {
					"0.2 day 3.3 hour 1E3 minute 0.33 second 10000 millisecond",
					0.2d * 24 * 60 * 60 + 3.3d * 60 * 60 + 1E3 * 60 + 0.33 + 10000 / 1000
				},
				new object[] {
					"0.2 day 3.3 hour 1E3 min 0.33 sec 10000 msec",
					0.2d * 24 * 60 * 60 + 3.3d * 60 * 60 + 1E3 * 60 + 0.33 + 10000 / 1000
				},
				new object[] {"1.01 hour 2 sec", 1.01d * 60 * 60 + 2},
				new object[] {"0.02 day 5 msec", 0.02d * 24 * 60 * 60 + 5 / 1000d},
				new object[] {"66 min 4 sec", 66 * 60 + 4d},
				new object[] {
					"1 days 6 hours 2 minutes 4 seconds 3 milliseconds",
					1 * 24 * 60 * 60 + 6 * 60 * 60 + 2 * 60 + 4 + 3 / 1000d
				},
				new object[] {"1 year", 365 * 24 * 60 * 60d},
				new object[] {"1 month", 30 * 24 * 60 * 60d},
				new object[] {"1 week", 7 * 24 * 60 * 60d},
				new object[] {
					"2 years 3 month 10 week 2 days 6 hours 2 minutes 4 seconds 3 milliseconds 7 microseconds",
					2 * 365 * 24 * 60 * 60d +
					3 * 30 * 24 * 60 * 60d +
					10 * 7 * 24 * 60 * 60d +
					2 * 24 * 60 * 60 +
					6 * 60 * 60 +
					2 * 60 +
					4 +
					3 / 1000d +
					7 / 1000000d
				},
			};

			for (var i = 0; i < intervals.Length; i++) {
				var interval = (string) intervals[i][0];
				var result = TryInterval(interval);
				var expected = (Double) intervals[i][1];
				var delta = result - expected;
				ClassicAssert.IsTrue(Math.Abs(delta) < 0.0000001, "Interval '" + interval + "' expected=" + expected + " actual=" + result);
			}

			TryIntervalInvalid(
				"1.5 month",
				"Time period expressions with month or year component require integer values, received a Double value");
		}

		[Test]
		public void TestWalkInAndBetween()
		{
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("1 between 0 and 2"));
			ClassicAssert.IsFalse((Boolean) TryRelationalOp("-1 between 0 and 2"));
			ClassicAssert.IsFalse((Boolean) TryRelationalOp("1 not between 0 and 2"));
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("-1 not between 0 and 2"));

			ClassicAssert.IsFalse((Boolean) TryRelationalOp("1 in (2,3)"));
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("1 in (2,3,1)"));
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("1 not in (2,3)"));
		}

		[Test]
		public void TestWalkLikeRegex()
		{
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("'abc' like 'a__'"));
			ClassicAssert.IsFalse((Boolean) TryRelationalOp("'abcd' like 'a__'"));

			ClassicAssert.IsFalse((Boolean) TryRelationalOp("'abcde' not like 'a%'"));
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("'bcde' not like 'a%'"));

			ClassicAssert.IsTrue((Boolean) TryRelationalOp("'a_' like 'a!_' escape '!'"));
			ClassicAssert.IsFalse((Boolean) TryRelationalOp("'ab' like 'a!_' escape '!'"));

			ClassicAssert.IsFalse((Boolean) TryRelationalOp("'a' not like 'a'"));
			ClassicAssert.IsTrue((Boolean) TryRelationalOp("'a' not like 'ab'"));
		}

		[Test]
		public void TestWalkDBJoinStatement()
		{
			var className = typeof(SupportBean).FullName;
			var sql = "select a from b where $x.Id=c.d";
			var expression = "select * from " + className + ", sql:mydb ['" + sql + "']";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			ClassicAssert.AreEqual(2, statementSpec.StreamSpecs.Count);
			var dbSpec = (DBStatementStreamSpec) statementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual("mydb", dbSpec.DatabaseName);
			ClassicAssert.AreEqual(sql, dbSpec.SqlWithSubsParams);

			expression = "select * from " + className + ", sql:mydb ['" + sql + "' metadatasql 'select * from B']";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			statementSpec = walker.StatementSpec;
			ClassicAssert.AreEqual(2, statementSpec.StreamSpecs.Count);
			dbSpec = (DBStatementStreamSpec) statementSpec.StreamSpecs[1];
			ClassicAssert.AreEqual("mydb", dbSpec.DatabaseName);
			ClassicAssert.AreEqual(sql, dbSpec.SqlWithSubsParams);
			ClassicAssert.AreEqual("select * from B", dbSpec.MetadataSQL);
		}

		[Test]
		public void TestRangeBetweenAndIn()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + "(IntPrimitive in [1:2], IntBoxed in (1,2), DoubleBoxed between 2 and 3)";
			SupportParserHelper.ParseAndWalkEPL(container, expression);

			expression = "select * from " + className + "(IntPrimitive not in [1:2], IntBoxed not in (1,2), DoubleBoxed not between 2 and 3)";
			SupportParserHelper.ParseAndWalkEPL(container, expression);
		}

		[Test]
		public void TestSubselect()
		{
			var expression = "select (select a from B(id=1) where cox=mox) from C";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var element = GetSelectExprSpec(walker.StatementSpec, 0);
			var exprNode = (ExprSubselectNode) element.SelectExpression;

			// check select expressions
			var spec = exprNode.StatementSpecRaw;
			ClassicAssert.AreEqual(1, spec.SelectClauseSpec.SelectExprList.Count);

			// check filter
			ClassicAssert.AreEqual(1, spec.StreamSpecs.Count);
			var filter = (FilterStreamSpecRaw) spec.StreamSpecs[0];
			ClassicAssert.AreEqual("B", filter.RawFilterSpec.EventTypeName);
			ClassicAssert.AreEqual(1, filter.RawFilterSpec.FilterExpressions.Count);

			// check where clause
			ClassicAssert.IsTrue(spec.WhereClause is ExprEqualsNode);
		}

		[Test]
		public void TestWalkPatternObject()
		{
			var expression = "select * from pattern [" + typeof(SupportBean).FullName + " -> timer:interval(100)]";
			SupportParserHelper.ParseAndWalkEPL(container, expression);

			expression = "select * from pattern [" + typeof(SupportBean).FullName + " where timer:within(100)]";
			SupportParserHelper.ParseAndWalkEPL(container, expression);
		}

		private void TryIntervalInvalid(
			string interval,
			string message)
		{
			try {
				TryInterval(interval);
				Assert.Fail();
			}
			catch (Exception ex) {
				ClassicAssert.AreEqual(message, ex.Message);
			}
		}

		private double TryInterval(string interval)
		{
			var text = "select * from " + typeof(SupportBean).FullName + "#win:time(" + interval + ")";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var viewSpec = walker.StatementSpec.StreamSpecs[0].ViewSpecs[0];
			ClassicAssert.AreEqual("win", viewSpec.ObjectNamespace);
			ClassicAssert.AreEqual("time", viewSpec.ObjectName);
			ClassicAssert.AreEqual(1, viewSpec.ObjectParameters.Count);
			var exprNode = (ExprTimePeriod) viewSpec.ObjectParameters[0];
			exprNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
			return exprNode.EvaluateAsSeconds(null, true, null);
		}

		private string TryWalkGetPropertyPattern(string stmt)
		{
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + stmt + "]");

			ClassicAssert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			var filterNode = (EvalFilterForgeNode) patternStreamSpec.EvalForgeNode;
			ClassicAssert.AreEqual(1, filterNode.RawFilterSpec.FilterExpressions.Count);
			var node = filterNode.RawFilterSpec.FilterExpressions[0];
			var identNode = (ExprIdentNode) node.ChildNodes[0];
			return identNode.UnresolvedPropertyName;
		}

		private object TryBitWise(string equation)
		{
			var expression = EXPRESSION + "where (" + equation + ")=win2.f2";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var exprNode = walker.StatementSpec.WhereClause.ChildNodes[0];
			var bitWiseNode = (ExprBitWiseNode) (exprNode);
			ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, bitWiseNode, SupportExprValidationContextFactory.MakeEmpty(container));
			return bitWiseNode.Forge.ExprEvaluator.Evaluate(null, false, null);
		}

		private object TryExpression(string equation)
		{
			var expression = EXPRESSION + "where " + equation + "=win2.f2";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var exprNode = (walker.StatementSpec.WhereClause.ChildNodes[0]);
			exprNode = ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, exprNode, SupportExprValidationContextFactory.MakeEmpty(container));
			return exprNode.Forge.ExprEvaluator.Evaluate(null, false, null);
		}

		private object TryRelationalOp(string subExpr)
		{
			var expression = EXPRESSION + "where " + subExpr;

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var filterExprNode = walker.StatementSpec.WhereClause;
			ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, filterExprNode, SupportExprValidationContextFactory.MakeEmpty(container));
			return filterExprNode.Forge.ExprEvaluator.Evaluate(null, false, null);
		}

		private SelectClauseExprRawSpec GetSelectExprSpec(
			StatementSpecRaw statementSpec,
			int index)
		{
			var raw = statementSpec.SelectClauseSpec.SelectExprList[index];
			return (SelectClauseExprRawSpec) raw;
		}

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
