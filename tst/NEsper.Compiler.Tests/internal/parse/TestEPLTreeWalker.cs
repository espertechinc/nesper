///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
			Assert.AreEqual("MyGraph", graph.GraphName);
			Assert.AreEqual(1, graph.Operators.Count);
			var op = graph.Operators[0];
			Assert.AreEqual("MyOp", op.OperatorName);

			// assert input
			Assert.AreEqual(2, op.Input.StreamNamesAndAliases.Count);
			var in1 = op.Input.StreamNamesAndAliases[0];
			EPAssertionUtil.AssertEqualsExactOrder("s0,s1".SplitCsv(), in1.InputStreamNames);
			Assert.AreEqual("ST1", in1.OptionalAsName);
			var in2 = op.Input.StreamNamesAndAliases[1];
			EPAssertionUtil.AssertEqualsExactOrder("s2".SplitCsv(), in2.InputStreamNames);
			Assert.IsNull(in2.OptionalAsName);

			// assert output
			Assert.AreEqual(2, op.Output.Items.Count);
			var out1 = op.Output.Items[0];
			Assert.AreEqual("out1", out1.StreamName);
			Assert.AreEqual(0, out1.TypeInfo.Count);
			var out2 = op.Output.Items[1];
			Assert.AreEqual("out2", out2.StreamName);
			Assert.AreEqual(0, out1.TypeInfo.Count);

			GraphOperatorOutputItemType type;

			type = TryWalkGraphTypes("out<?>");
			Assert.IsTrue(type.IsWildcard);
			Assert.IsNull(type.TypeOrClassname);
			Assert.IsNull(type.TypeParameters);

			type = TryWalkGraphTypes("out<eventbean<?>>");
			Assert.IsFalse(type.IsWildcard);
			Assert.AreEqual("eventbean", type.TypeOrClassname);
			Assert.AreEqual(1, type.TypeParameters.Count);
			Assert.IsTrue(type.TypeParameters[0].IsWildcard);
			Assert.IsNull(type.TypeParameters[0].TypeOrClassname);
			Assert.IsNull(type.TypeParameters[0].TypeParameters);

			type = TryWalkGraphTypes("out<eventbean<someschema>>");
			Assert.IsFalse(type.IsWildcard);
			Assert.AreEqual("eventbean", type.TypeOrClassname);
			Assert.AreEqual(1, type.TypeParameters.Count);
			Assert.IsFalse(type.TypeParameters[0].IsWildcard);
			Assert.AreEqual("someschema", type.TypeParameters[0].TypeOrClassname);
			Assert.AreEqual(0, type.TypeParameters[0].TypeParameters.Count);

			type = TryWalkGraphTypes("out<Map<String, Integer>>");
			Assert.IsFalse(type.IsWildcard);
			Assert.AreEqual("Map", type.TypeOrClassname);
			Assert.AreEqual(2, type.TypeParameters.Count);
			Assert.AreEqual("String", type.TypeParameters[0].TypeOrClassname);
			Assert.AreEqual("Integer", type.TypeParameters[1].TypeOrClassname);
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
			Assert.AreEqual("MyName", schema.SchemaName);
			EPAssertionUtil.AssertEqualsExactOrder("com.company.SupportBean".SplitCsv(), schema.Types.ToArray());
			Assert.IsTrue(schema.Inherits.IsEmpty());
			Assert.IsTrue(schema.Columns.IsEmpty());
			Assert.AreEqual(AssignedType.NONE, schema.AssignedType);

			expression = "create schema MyName (col1 string, col2 int, col3 Type[]) inherits InheritedType";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			schema = walker.StatementSpec.CreateSchemaDesc;
			Assert.AreEqual("MyName", schema.SchemaName);
			Assert.IsTrue(schema.Types.IsEmpty());
			EPAssertionUtil.AssertEqualsExactOrder("InheritedType".SplitCsv(), schema.Inherits.ToArray());
			AssertSchema(schema.Columns[0], "col1", "string", false);
			AssertSchema(schema.Columns[1], "col2", "int", false);
			AssertSchema(schema.Columns[2], "col3", "Type", true);

			expression = "create variant schema MyName as MyNameTwo,MyNameThree";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			schema = walker.StatementSpec.CreateSchemaDesc;
			Assert.AreEqual("MyName", schema.SchemaName);
			EPAssertionUtil.AssertEqualsExactOrder("MyNameTwo,MyNameThree".SplitCsv(), schema.Types.ToArray());
			Assert.IsTrue(schema.Inherits.IsEmpty());
			Assert.IsTrue(schema.Columns.IsEmpty());
			Assert.AreEqual(AssignedType.VARIANT, schema.AssignedType);
		}

		private void AssertSchema(
			ColumnDesc element,
			string name,
			string type,
			bool isArray)
		{
			var clazz = ClassDescriptor.ParseTypeText(element.Type);
			Assert.AreEqual(name, element.Name);
			Assert.AreEqual(type, clazz.ClassIdentifier);
			Assert.AreEqual(isArray, clazz.ArrayDimensions > 0);
		}

		[Test]
		public void TestWalkCreateIndex()
		{
			var expression = "create index A_INDEX on B_NAMEDWIN (c, d btree)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var createIndex = walker.StatementSpec.CreateIndexDesc;
			Assert.AreEqual("A_INDEX", createIndex.IndexName);
			Assert.AreEqual("B_NAMEDWIN", createIndex.WindowName);
			Assert.AreEqual(2, createIndex.Columns.Count);
			Assert.AreEqual("c", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(createIndex.Columns[0].Expressions[0]));
			Assert.AreEqual(CreateIndexType.HASH.GetName().ToLowerInvariant(), createIndex.Columns[0].IndexType);
			Assert.AreEqual("d", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(createIndex.Columns[1].Expressions[0]));
			Assert.AreEqual(CreateIndexType.BTREE.GetName().ToLowerInvariant(), createIndex.Columns[1].IndexType);
		}

		[Test]
		public void TestWalkViewExpressions()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + ".win:x(intPrimitive, a.nested)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			var parameters = viewSpecs[0].ObjectParameters;
			Assert.AreEqual("intPrimitive", ((ExprIdentNode) parameters[0]).FullUnresolvedName);
			Assert.AreEqual("a.nested", ((ExprIdentNode) parameters[1]).FullUnresolvedName);
		}

		[Test]
		public void TestWalkJoinMethodStatement()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select distinct * from " + className + " unidirectional, method:com.MyClass.myMethod(string, 2*intPrimitive) as s0";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			Assert.IsTrue(statementSpec.SelectClauseSpec.IsDistinct);
			Assert.AreEqual(2, statementSpec.StreamSpecs.Count);
			Assert.IsTrue(statementSpec.StreamSpecs[0].Options.IsUnidirectional);
			Assert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			Assert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);

			var methodSpec = (MethodStreamSpec) statementSpec.StreamSpecs[1];
			Assert.AreEqual("method", methodSpec.Ident);
			Assert.AreEqual("com.MyClass", methodSpec.ClassName);
			Assert.AreEqual("myMethod", methodSpec.MethodName);
			Assert.AreEqual(2, methodSpec.Expressions.Count);
			Assert.IsTrue(methodSpec.Expressions[0] is ExprIdentNode);
			Assert.IsTrue(methodSpec.Expressions[1] is ExprMathNode);
		}

		[Test]
		public void TestWalkRetainKeywords()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + " retain-union";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			Assert.AreEqual(1, statementSpec.StreamSpecs.Count);
			Assert.IsTrue(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			Assert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);

			expression = "select * from " + className + " retain-intersection";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			statementSpec = walker.StatementSpec;
			Assert.AreEqual(1, statementSpec.StreamSpecs.Count);
			Assert.IsFalse(statementSpec.StreamSpecs[0].Options.IsRetainUnion);
			Assert.IsTrue(statementSpec.StreamSpecs[0].Options.IsRetainIntersection);
		}

		[Test]
		public void TestWalkCreateVariable()
		{
			var expression = "create constant variable sometype var1 = 1";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var createVarDesc = raw.CreateVariableDesc;
			Assert.AreEqual("sometype", createVarDesc.VariableType.ClassIdentifier);
			Assert.AreEqual("var1", createVarDesc.VariableName);
			Assert.IsTrue(createVarDesc.Assignment is ExprConstantNode);
			Assert.IsTrue(createVarDesc.IsConstant);
		}

		[Test]
		public void TestWalkOnUpdate()
		{
			var expression = "on com.MyClass as myevent update MyWindow as mw set prop1 = 'a', prop2=a.b*c where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			Assert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			Assert.AreEqual(0, streamSpec.RawFilterSpec.FilterExpressions.Count);
			Assert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var setDesc = (OnTriggerWindowUpdateDesc) raw.OnTriggerDesc;
			Assert.IsTrue(setDesc.OnTriggerType == OnTriggerType.ON_UPDATE);
			Assert.AreEqual(2, setDesc.Assignments.Count);

			var assign = setDesc.Assignments[0];
			Assert.AreEqual("prop1", ((ExprIdentNode) (assign.Expression.ChildNodes[0])).FullUnresolvedName);
			Assert.IsTrue(assign.Expression.ChildNodes[1] is ExprConstantNode);

			Assert.AreEqual("a=b", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(raw.WhereClause));
		}

		[Test]
		public void TestWalkOnSelectNoInsert()
		{
			var expression = "on com.MyClass(myval != 0) as myevent select *, mywin.* as abc, myevent.* from MyNamedWindow as mywin where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			Assert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			Assert.AreEqual(1, streamSpec.RawFilterSpec.FilterExpressions.Count);
			Assert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			Assert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			Assert.AreEqual("mywin", windowDesc.OptionalAsName);
			Assert.AreEqual(OnTriggerType.ON_SELECT, windowDesc.OnTriggerType);

			Assert.IsNull(raw.InsertIntoDesc);
			Assert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			Assert.AreEqual(3, raw.SelectClauseSpec.SelectExprList.Count);
			Assert.IsTrue(raw.SelectClauseSpec.SelectExprList[0] is SelectClauseElementWildcard);
			Assert.AreEqual("mywin", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).StreamName);
			Assert.AreEqual("mywin", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).StreamName);
			Assert.AreEqual("abc", ((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[1]).OptionalAsName);
			Assert.AreEqual("myevent", (((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[2]).StreamName));
			Assert.IsNull(((SelectClauseStreamRawSpec) raw.SelectClauseSpec.SelectExprList[2]).OptionalAsName);
			Assert.IsTrue(raw.WhereClause is ExprEqualsNode);
		}

		[Test]
		public void TestWalkOnSelectInsert()
		{
			var expression = "on pattern [com.MyClass] as pat insert into MyStream(a, b) select c, d from MyNamedWindow as mywin " +
			                 " where a=b group by symbol having c=d order by e";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			Assert.IsTrue(streamSpec.EvalForgeNode is EvalFilterForgeNode);
			Assert.AreEqual("pat", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			Assert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			Assert.AreEqual("mywin", windowDesc.OptionalAsName);
			Assert.AreEqual(OnTriggerType.ON_SELECT, windowDesc.OnTriggerType);
			Assert.IsTrue(raw.WhereClause is ExprEqualsNode);

			Assert.AreEqual("MyStream", raw.InsertIntoDesc.EventTypeName);
			Assert.AreEqual(2, raw.InsertIntoDesc.ColumnNames.Count);
			Assert.AreEqual("a", raw.InsertIntoDesc.ColumnNames[0]);
			Assert.AreEqual("b", raw.InsertIntoDesc.ColumnNames[1]);

			Assert.IsFalse(raw.SelectClauseSpec.IsUsingWildcard);
			Assert.AreEqual(2, raw.SelectClauseSpec.SelectExprList.Count);

			Assert.AreEqual(1, raw.GroupByExpressions.Count);
			Assert.IsTrue(raw.HavingClause is ExprEqualsNode);
			Assert.AreEqual(1, raw.OrderByList.Count);
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
			Assert.AreEqual("pat", streamSpec.OptionalStreamName);

			var triggerDesc = (OnTriggerSplitStreamDesc) raw.OnTriggerDesc;
			Assert.AreEqual(OnTriggerType.ON_SPLITSTREAM, triggerDesc.OnTriggerType);
			Assert.AreEqual(2, triggerDesc.SplitStreams.Count);

			Assert.AreEqual("MyStream", raw.InsertIntoDesc.EventTypeName);
			Assert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			Assert.AreEqual(1, raw.SelectClauseSpec.SelectExprList.Count);
			Assert.IsNotNull((ExprRelationalOpNode) raw.WhereClause);

			var splitStream = triggerDesc.SplitStreams[0];
			Assert.AreEqual("BStream", splitStream.InsertInto.EventTypeName);
			Assert.AreEqual(2, splitStream.InsertInto.ColumnNames.Count);
			Assert.AreEqual("a", splitStream.InsertInto.ColumnNames[0]);
			Assert.AreEqual("b", splitStream.InsertInto.ColumnNames[1]);
			Assert.IsTrue(splitStream.SelectClause.IsUsingWildcard);
			Assert.AreEqual(1, splitStream.SelectClause.SelectExprList.Count);
			Assert.IsNotNull((ExprEqualsNode) splitStream.WhereClause);

			splitStream = triggerDesc.SplitStreams[1];
			Assert.AreEqual("CStream", splitStream.InsertInto.EventTypeName);
			Assert.AreEqual(0, splitStream.InsertInto.ColumnNames.Count);
			Assert.IsFalse(splitStream.SelectClause.IsUsingWildcard);
			Assert.AreEqual(2, splitStream.SelectClause.SelectExprList.Count);
			Assert.IsNull(splitStream.WhereClause);
		}

		[Test]
		public void TestWalkOnDelete()
		{
			// try a filter
			var expression = "on com.MyClass(myval != 0) as myevent delete from MyNamedWindow as mywin where mywin.key = myevent.otherKey";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			var streamSpec = (FilterStreamSpecRaw) raw.StreamSpecs[0];
			Assert.AreEqual("com.MyClass", streamSpec.RawFilterSpec.EventTypeName);
			Assert.AreEqual(1, streamSpec.RawFilterSpec.FilterExpressions.Count);
			Assert.AreEqual("myevent", streamSpec.OptionalStreamName);

			var windowDesc = (OnTriggerWindowDesc) raw.OnTriggerDesc;
			Assert.AreEqual("MyNamedWindow", windowDesc.WindowName);
			Assert.AreEqual("mywin", windowDesc.OptionalAsName);
			Assert.AreEqual(OnTriggerType.ON_DELETE, windowDesc.OnTriggerType);

			Assert.IsTrue(raw.WhereClause is ExprEqualsNode);

			// try a pattern
			expression = "on pattern [every MyClass] as myevent delete from MyNamedWindow";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			raw = walker.StatementSpec;

			var patternSpec = (PatternStreamSpecRaw) raw.StreamSpecs[0];
			Assert.IsTrue(patternSpec.EvalForgeNode is EvalEveryForgeNode);
		}

		[Test]
		public void TestWalkCreateWindow()
		{
			var expression = "create window MyWindow#groupwin(symbol)#length(20) as select *, aprop, bprop as someval from com.MyClass insert where a=b";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var raw = walker.StatementSpec;

			// window name
			Assert.AreEqual("MyWindow", raw.CreateWindowDesc.WindowName);
			Assert.IsTrue(raw.CreateWindowDesc.IsInsert);
			Assert.IsTrue(raw.CreateWindowDesc.InsertFilter is ExprEqualsNode);

			// select clause
			Assert.IsTrue(raw.SelectClauseSpec.IsUsingWildcard);
			var selectSpec = raw.SelectClauseSpec.SelectExprList;
			Assert.AreEqual(3, selectSpec.Count);
			Assert.IsTrue(raw.SelectClauseSpec.SelectExprList[0] is SelectClauseElementWildcard);
			var rawSpec = (SelectClauseExprRawSpec) selectSpec[1];
			Assert.AreEqual("aprop", ((ExprIdentNode) rawSpec.SelectExpression).UnresolvedPropertyName);
			rawSpec = (SelectClauseExprRawSpec) selectSpec[2];
			Assert.AreEqual("bprop", ((ExprIdentNode) rawSpec.SelectExpression).UnresolvedPropertyName);
			Assert.AreEqual("someval", rawSpec.OptionalAsName);

			// 2 views
			Assert.AreEqual(2, raw.CreateWindowDesc.ViewSpecs.Count);
			Assert.AreEqual("groupwin", raw.CreateWindowDesc.ViewSpecs[0].ObjectName);
			Assert.AreEqual(null, raw.CreateWindowDesc.ViewSpecs[0].ObjectNamespace);
			Assert.AreEqual("length", raw.CreateWindowDesc.ViewSpecs[1].ObjectName);
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

				Assert.AreEqual(1, raw.MatchRecognizeSpec.Measures.Count);
				Assert.AreEqual(1, raw.MatchRecognizeSpec.Defines.Count);
				Assert.AreEqual(1, raw.MatchRecognizeSpec.PartitionByExpressions.Count);

				var writer = new StringWriter();
				raw.MatchRecognizeSpec.Pattern.ToEPL(writer, RowRecogExprNodePrecedenceEnum.MINIMUM);
				var received = writer.ToString();
				Assert.AreEqual(patternTests[i], received);
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
			Assert.AreEqual(2, raw.SubstitutionParameters.Count);

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
			Assert.AreEqual(2, matchNode.ChildNodes.Count);
			Assert.IsTrue(matchNode.ChildNodes[0] is EvalFilterForgeNode);
			Assert.IsTrue(matchNode.ChildNodes[1] is EvalOrForgeNode);

			var spec = GetMatchUntilSpec("A until (B or C)");
			Assert.IsNull(spec.LowerBounds);
			Assert.IsNull(spec.UpperBounds);

			spec = GetMatchUntilSpec("[1:10] A until (B or C)");
			Assert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			Assert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1 : 10] A until (B or C)");
			Assert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			Assert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1:10] A until (B or C)");
			Assert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			Assert.AreEqual(10, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[1:] A until (B or C)");
			Assert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			Assert.AreEqual(null, spec.UpperBounds);

			spec = GetMatchUntilSpec("[1 :] A until (B or C)");
			Assert.AreEqual(1, spec.LowerBounds.Forge.ExprEvaluator.Evaluate(null, true, null));
			Assert.AreEqual(null, spec.UpperBounds);
			Assert.AreEqual(null, spec.SingleBound);

			spec = GetMatchUntilSpec("[:2] A until (B or C)");
			Assert.AreEqual(null, spec.LowerBounds);
			Assert.AreEqual(null, spec.SingleBound);
			Assert.AreEqual(2, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[: 2] A until (B or C)");
			Assert.AreEqual(null, spec.LowerBounds);
			Assert.AreEqual(null, spec.SingleBound);
			Assert.AreEqual(2, spec.UpperBounds.Forge.ExprEvaluator.Evaluate(null, true, null));

			spec = GetMatchUntilSpec("[2] A until (B or C)");
			Assert.AreEqual(2, spec.SingleBound.Forge.ExprEvaluator.Evaluate(null, true, null));
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

			Assert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);

			var streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			Assert.AreEqual(2, streamSpec.ViewSpecs.Length);
			Assert.AreEqual(typeof(SupportBean).FullName, streamSpec.RawFilterSpec.EventTypeName);
			Assert.AreEqual("length", streamSpec.ViewSpecs[0].ObjectName);
			Assert.AreEqual("lastevent", streamSpec.ViewSpecs[1].ObjectName);
			Assert.AreEqual("win1", streamSpec.OptionalStreamName);

			streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			Assert.AreEqual("win2", streamSpec.OptionalStreamName);

			// Join expression tree validation
			Assert.IsTrue(walker.StatementSpec.WhereClause is ExprEqualsNode);
			var equalsNode = (walker.StatementSpec.WhereClause);
			Assert.AreEqual(2, equalsNode.ChildNodes.Length);

			var identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			Assert.AreEqual("win1", identNode.StreamOrPropertyName);
			Assert.AreEqual("f1", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			Assert.AreEqual("win2", identNode.StreamOrPropertyName);
			Assert.AreEqual("f2", identNode.UnresolvedPropertyName);
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
			Assert.AreEqual(3, walker.StatementSpec.StreamSpecs.Count);
			Assert.AreEqual("win1", walker.StatementSpec.StreamSpecs[0].OptionalStreamName);
			Assert.AreEqual("win2", walker.StatementSpec.StreamSpecs[1].OptionalStreamName);
			Assert.AreEqual("win3", walker.StatementSpec.StreamSpecs[2].OptionalStreamName);

			var streamSpec = (FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[2];
			Assert.AreEqual(2, streamSpec.ViewSpecs.Length);
			Assert.AreEqual(typeof(SupportBean).FullName, streamSpec.RawFilterSpec.EventTypeName);
			Assert.AreEqual("length", streamSpec.ViewSpecs[0].ObjectName);
			Assert.AreEqual("lastevent", streamSpec.ViewSpecs[1].ObjectName);

			// Join expression tree validation
			Assert.IsTrue(walker.StatementSpec.WhereClause is ExprAndNode);
			Assert.AreEqual(2, walker.StatementSpec.WhereClause.ChildNodes.Length);
			var equalsNode = (walker.StatementSpec.WhereClause.ChildNodes[0]);
			Assert.AreEqual(2, equalsNode.ChildNodes.Length);

			var identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			Assert.AreEqual("win1", identNode.StreamOrPropertyName);
			Assert.AreEqual("f1", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			Assert.AreEqual("win2", identNode.StreamOrPropertyName);
			Assert.AreEqual("f2", identNode.UnresolvedPropertyName);

			equalsNode = (walker.StatementSpec.WhereClause.ChildNodes[1]);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[0];
			Assert.AreEqual("win3", identNode.StreamOrPropertyName);
			Assert.AreEqual("f3", identNode.UnresolvedPropertyName);
			identNode = (ExprIdentNode) equalsNode.ChildNodes[1];
			Assert.IsNull(identNode.StreamOrPropertyName);
			Assert.AreEqual("f4", identNode.UnresolvedPropertyName);

			Assert.AreEqual(5, (int) walker.StatementSpec.RowLimitSpec.NumRows);
			Assert.AreEqual(10, (int) walker.StatementSpec.RowLimitSpec.OptionalOffset);
		}

		[Test]
		public void TestWalkPerRowFunctions()
		{
			Assert.AreEqual(9, TryExpression("max(6, 9)"));
			Assert.AreEqual(6.11, TryExpression("min(6.11, 6.12)"));
			Assert.AreEqual(6.10, TryExpression("min(6.11, 6.12, 6.1)"));
			Assert.AreEqual("ab", TryExpression("'a'||'b'"));
			Assert.AreEqual(null, TryExpression("coalesce(null, null)"));
			Assert.AreEqual(1, TryExpression("coalesce(null, 1)"));
			Assert.AreEqual(1L, TryExpression("coalesce(null, 1l)"));
			Assert.AreEqual("a", TryExpression("coalesce(null, 'a', 'b')"));
			Assert.AreEqual(13.5d, TryExpression("coalesce(null, null, 3*4.5)"));
			Assert.AreEqual(true, TryExpression("coalesce(null, true)"));
			Assert.AreEqual(5, TryExpression("coalesce(5, null, 6)"));
			Assert.AreEqual(2, TryExpression("(case 1 when 1 then 2 end)"));
		}

		[Test]
		public void TestWalkMath()
		{
			Assert.AreEqual(32.0, TryExpression("5*6-3+15/3"));
			Assert.AreEqual(-5, TryExpression("1-1-1-2-1-1"));
			Assert.AreEqual(2.8d, TryExpression("1.4 + 1.4"));
			Assert.AreEqual(1d, TryExpression("55.5/5/11.1"));
			Assert.AreEqual(2 / 3d, TryExpression("2/3"));
			Assert.AreEqual(2 / 3d, TryExpression("2.0/3"));
			Assert.AreEqual(10, TryExpression("(1+4)*2"));
			Assert.AreEqual(12, TryExpression("(3*(6-4))*2"));
			Assert.AreEqual(8.5, TryExpression("(1+(4*3)+2)/2+1"));
			Assert.AreEqual(1, TryExpression("10%3"));
			Assert.AreEqual(10.1 % 3, TryExpression("10.1%3"));
		}

		[Test]
		public void TestWalkRelationalOp()
		{
			Assert.AreEqual(true, TryRelationalOp("3>2"));
			Assert.AreEqual(true, TryRelationalOp("3*5/2 >= 7.5"));
			Assert.AreEqual(true, TryRelationalOp("3*5/2.0 >= 7.5"));
			Assert.AreEqual(false, TryRelationalOp("1.1 + 2.2 < 3.2"));
			Assert.AreEqual(false, TryRelationalOp("3<=2"));
			Assert.AreEqual(true, TryRelationalOp("4*(3+1)>=16"));

			Assert.AreEqual(false, TryRelationalOp("(4>2) and (2>3)"));
			Assert.AreEqual(true, TryRelationalOp("(4>2) or (2>3)"));

			Assert.AreEqual(false, TryRelationalOp("not 3>2"));
			Assert.AreEqual(true, TryRelationalOp("not (not 3>2)"));
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
			Assert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, desc.StreamSelector);
			Assert.AreEqual("MyAlias", desc.EventTypeName);
			Assert.AreEqual(0, desc.ColumnNames.Count);

			expression = "insert rstream into MyAlias(a, b, c) select * from " +
			             CLASSNAME +
			             "()#length(10)#lastevent as win1," +
			             CLASSNAME +
			             "(string='b')#length(9)#lastevent as win2";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);

			desc = walker.StatementSpec.InsertIntoDesc;
			Assert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ONLY, desc.StreamSelector);
			Assert.AreEqual("MyAlias", desc.EventTypeName);
			Assert.AreEqual(3, desc.ColumnNames.Count);
			Assert.AreEqual("a", desc.ColumnNames[0]);
			Assert.AreEqual("b", desc.ColumnNames[1]);
			Assert.AreEqual("c", desc.ColumnNames[2]);

			expression = "insert irstream into Test2 select * from " + CLASSNAME + "()#length(10)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			desc = walker.StatementSpec.InsertIntoDesc;
			Assert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH, desc.StreamSelector);
			Assert.AreEqual("Test2", desc.EventTypeName);
			Assert.AreEqual(0, desc.ColumnNames.Count);
		}

		[Test]
		public void TestWalkView()
		{
			var text = "select * from " + typeof(SupportBean).FullName + "(string=\"IBM\").win:lenght(10, 1.1, \"a\").stat:uni(price, false)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var filterSpec = ((FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0]).RawFilterSpec;

			// Check filter spec properties
			Assert.AreEqual(typeof(SupportBean).FullName, filterSpec.EventTypeName);
			Assert.AreEqual(1, filterSpec.FilterExpressions.Count);

			// Check views
			var viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			Assert.AreEqual(2, viewSpecs.Length);

			var specOne = viewSpecs[0];
			Assert.AreEqual("win", specOne.ObjectNamespace);
			Assert.AreEqual("lenght", specOne.ObjectName);
			Assert.AreEqual(3, specOne.ObjectParameters.Count);
			Assert.AreEqual(10, ((ExprConstantNode) specOne.ObjectParameters[0]).ConstantValue);
			Assert.AreEqual(1.1d, ((ExprConstantNode) specOne.ObjectParameters[1]).ConstantValue);
			Assert.AreEqual("a", ((ExprConstantNode) specOne.ObjectParameters[2]).ConstantValue);

			var specTwo = viewSpecs[1];
			Assert.AreEqual("stat", specTwo.ObjectNamespace);
			Assert.AreEqual("uni", specTwo.ObjectName);
			Assert.AreEqual(2, specTwo.ObjectParameters.Count);
			Assert.AreEqual("price", ((ExprIdentNode) specTwo.ObjectParameters[0]).FullUnresolvedName);
			Assert.AreEqual(false, ((ExprConstantNode) specTwo.ObjectParameters[1]).ConstantValue);
		}

		[Test]
		public void TestWalkPropertyExpr()
		{
			var text = "select * from " + typeof(SupportBean).FullName + "[a.b][select c,d.*,* from e as f where g]";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var filterSpec = ((FilterStreamSpecRaw) walker.StatementSpec.StreamSpecs[0]).RawFilterSpec;
			Assert.AreEqual(2, filterSpec.OptionalPropertyEvalSpec.Atoms.Count);
			Assert.AreEqual("a.b", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(filterSpec.OptionalPropertyEvalSpec.Atoms[0].SplitterExpression));
			Assert.AreEqual(0, filterSpec.OptionalPropertyEvalSpec.Atoms[0].OptionalSelectClause.SelectExprList.Count);

			var atomTwo = filterSpec.OptionalPropertyEvalSpec.Atoms[1];
			Assert.AreEqual("e", ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(atomTwo.SplitterExpression));
			Assert.AreEqual("f", atomTwo.OptionalAsName);
			Assert.IsNotNull(atomTwo.OptionalWhereClause);
			var list = atomTwo.OptionalSelectClause.SelectExprList;
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list[0] is SelectClauseExprRawSpec);
			Assert.IsTrue(list[1] is SelectClauseStreamRawSpec);
			Assert.IsTrue(list[2] is SelectClauseElementWildcard);
		}

		[Test]
		public void TestSelectList()
		{
			var text = "select intPrimitive, 2 * intBoxed, 5 as myConst, stream0.string as theString from " +
			           typeof(SupportBean).FullName +
			           "().win:lenght(10) as stream0";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var selectExpressions = walker.StatementSpec.SelectClauseSpec.SelectExprList;
			Assert.AreEqual(4, selectExpressions.Count);

			var rawSpec = (SelectClauseExprRawSpec) selectExpressions[0];
			Assert.IsTrue(rawSpec.SelectExpression is ExprIdentNode);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[1];
			Assert.IsTrue(rawSpec.SelectExpression is ExprMathNode);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[2];
			Assert.IsTrue(rawSpec.SelectExpression is ExprConstantNode);
			Assert.AreEqual("myConst", rawSpec.OptionalAsName);

			rawSpec = (SelectClauseExprRawSpec) selectExpressions[3];
			Assert.IsTrue(rawSpec.SelectExpression is ExprIdentNode);
			Assert.AreEqual("theString", rawSpec.OptionalAsName);
			Assert.IsNull(walker.StatementSpec.InsertIntoDesc);

			text = "select * from " + typeof(SupportBean).FullName + "().win:lenght(10)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			Assert.AreEqual(1, walker.StatementSpec.SelectClauseSpec.SelectExprList.Count);
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
			Assert.AreEqual(10, intParams[0]);
			Assert.AreEqual(11, intParams[1]);
			Assert.AreEqual(12, intParams[2]);

			// Check a list of objects
			text = "select * from " + typeof(SupportBean).FullName + "().win:lenght({false, 11.2, 's'})";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			viewSpecs = walker.StatementSpec.StreamSpecs[0].ViewSpecs;
			var param = viewSpecs[0].ObjectParameters[0];
			param.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
			var objParams = ((ExprArrayNode) param).Forge.ExprEvaluator.Evaluate(null, true, null).UnwrapIntoArray<object>();
			Assert.AreEqual(false, objParams[0]);
			Assert.AreEqual(11.2, objParams[1]);
			Assert.AreEqual("s", objParams[2]);
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
			var text = "select intPrimitive from SupportBean_N().win:lenght(10) as win1";
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		[Test]
		public void TestAggregateFunction()
		{
			var fromClause = "from " + typeof(SupportBean_N).FullName + "().win:lenght(10) as win1";
			var text = "select max(distinct intPrimitive) " + fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);

			text = "select sum(intPrimitive)," +
			       "sum(distinct doubleBoxed)," +
			       "avg(doubleBoxed)," +
			       "avg(distinct doubleBoxed)," +
			       "count(*)," +
			       "count(intPrimitive)," +
			       "count(distinct intPrimitive)," +
			       "max(distinct intPrimitive)," +
			       "min(distinct intPrimitive)," +
			       "max(intPrimitive)," +
			       "min(intPrimitive), " +
			       "median(intPrimitive), " +
			       "median(distinct intPrimitive)," +
			       "stddev(intPrimitive), " +
			       "stddev(distinct intPrimitive)," +
			       "avedev(intPrimitive)," +
			       "avedev(distinct intPrimitive) " +
			       fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);

			// try min-max aggregate versus row functions
			text = "select max(intPrimitive), min(intPrimitive)," +
			       "max(intPrimitive,intBoxed), min(intPrimitive,intBoxed)," +
			       "max(distinct intPrimitive), min(distinct intPrimitive)" +
			       fromClause;
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		[Test]
		public void TestGroupBy()
		{
			var text = "select sum(intPrimitive) from SupportBean_N().win:lenght(10) as win1 where intBoxed > 5 " +
			           "group by intBoxed, 3 * doubleBoxed, max(2, doublePrimitive)";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var groupByList = walker.StatementSpec.GroupByExpressions;
			Assert.AreEqual(3, groupByList.Count);

			var node = ((GroupByClauseElementExpr) groupByList[0]).Expr;
			Assert.IsTrue(node is ExprIdentNode);

			node = ((GroupByClauseElementExpr) groupByList[1]).Expr;
			Assert.IsTrue(node is ExprMathNode);
			Assert.IsTrue(node.ChildNodes[0] is ExprConstantNode);
			Assert.IsTrue(node.ChildNodes[1] is ExprIdentNode);

			node = ((GroupByClauseElementExpr) groupByList[2]).Expr;
			Assert.IsTrue(node is ExprMinMaxRowNode);
		}

		[Test]
		public void TestHaving()
		{
			var text = "select sum(intPrimitive) from SupportBean_N().win:lenght(10) as win1 where intBoxed > 5 " +
			           "group by intBoxed having sum(intPrimitive) > 5";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var havingNode = walker.StatementSpec.HavingClause;

			Assert.IsTrue(havingNode is ExprRelationalOpNode);
			Assert.IsTrue(havingNode.ChildNodes[0] is ExprSumNode);
			Assert.IsTrue(havingNode.ChildNodes[1] is ExprConstantNode);

			text = "select sum(intPrimitive) from SupportBean_N().win:lenght(10) as win1 where intBoxed > 5 " +
			       "having intPrimitive < avg(intPrimitive)";
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			havingNode = walker.StatementSpec.HavingClause;
			Assert.IsTrue(havingNode is ExprRelationalOpNode);
		}

		[Test]
		public void TestDistinct()
		{
			var text = "select sum(distinct intPrimitive) from SupportBean_N().win:lenght(10) as win1";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var rawElement = walker.StatementSpec.SelectClauseSpec.SelectExprList[0];
			var exprSpec = (SelectClauseExprRawSpec) rawElement;
			ExprAggregateNodeBase aggrNode = (ExprAggregateNodeBase) exprSpec.SelectExpression;
			Assert.IsTrue(aggrNode.IsDistinct);
		}

		[Test]
		public void TestComplexProperty()
		{
			var text = "select array [ 1 ],s0.map('a'),nested.nested2, a[1].b as x, nested.abcdef? " +
			           " from SupportBean_N().win:lenght(10) as win1 " +
			           " where a[1].b('a').nested.c[0] = 4";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 0).SelectExpression;
			Assert.AreEqual("array[1]", identNode.UnresolvedPropertyName);
			Assert.IsNull(identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 1).SelectExpression;
			Assert.AreEqual("map('a')", identNode.UnresolvedPropertyName);
			Assert.AreEqual("s0", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 2).SelectExpression;
			Assert.AreEqual("nested2", identNode.UnresolvedPropertyName);
			Assert.AreEqual("nested", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 3).SelectExpression;
			Assert.AreEqual("a[1].b", identNode.UnresolvedPropertyName);
			Assert.AreEqual(null, identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) GetSelectExprSpec(walker.StatementSpec, 4).SelectExpression;
			Assert.AreEqual("abcdef?", identNode.UnresolvedPropertyName);
			Assert.AreEqual("nested", identNode.StreamOrPropertyName);

			identNode = (ExprIdentNode) walker.StatementSpec.WhereClause.ChildNodes[0];
			Assert.AreEqual("a[1].b('a').nested.c[0]", identNode.UnresolvedPropertyName);
			Assert.AreEqual(null, identNode.StreamOrPropertyName);
		}

		[Test]
		public void TestBitWise()
		{
			var text = "select intPrimitive & intBoxed from " + typeof(SupportBean).FullName + "().win:lenght(10) as stream0";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var selectExpressions = walker.StatementSpec.SelectClauseSpec.SelectExprList;
			Assert.AreEqual(1, selectExpressions.Count);
			Assert.IsTrue(GetSelectExprSpec(walker.StatementSpec, 0).SelectExpression is ExprBitWiseNode);

			Assert.AreEqual(0, TryBitWise("1&2"));
			Assert.AreEqual(3, TryBitWise("1|2"));
			Assert.AreEqual(8, TryBitWise("10^2"));
		}

		[Test]
		public void TestPatternsOnly()
		{
			var patternOne = "a=" + typeof(SupportBean).FullName + " -> b=" + typeof(SupportBean).FullName;

			// Test simple case, one pattern and no "as streamName"
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "]");
			Assert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			Assert.AreEqual(typeof(EvalFollowedByForgeNode), patternStreamSpec.EvalForgeNode.GetType());
			Assert.IsNull(patternStreamSpec.OptionalStreamName);

			// Test case with "as s0"
			walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "] as s0");
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			Assert.AreEqual("s0", patternStreamSpec.OptionalStreamName);

			// Test case with multiple patterns
			var patternTwo = "c=" + typeof(SupportBean).FullName + " or " + typeof(SupportBean).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern [" + patternOne + "] as s0, pattern [" + patternTwo + "] as s1");
			Assert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			Assert.AreEqual("s0", patternStreamSpec.OptionalStreamName);
			Assert.AreEqual(typeof(EvalFollowedByForgeNode), patternStreamSpec.EvalForgeNode.GetType());

			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			Assert.AreEqual("s1", patternStreamSpec.OptionalStreamName);
			Assert.AreEqual(typeof(EvalOrForgeNode), patternStreamSpec.EvalForgeNode.GetType());

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
			Assert.AreEqual(3, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[2];
			Assert.AreEqual("s2", patternStreamSpec.OptionalStreamName);

			// Test patterns with views
			walker = SupportParserHelper.ParseAndWalkEPL(
				container,
				"select * from pattern [" + patternOne + "]#time(1), pattern [" + patternTwo + "]#length(1)#lastevent as s1");
			Assert.AreEqual(2, walker.StatementSpec.StreamSpecs.Count);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			Assert.AreEqual(1, patternStreamSpec.ViewSpecs.Length);
			Assert.AreEqual("time", patternStreamSpec.ViewSpecs[0].ObjectName);
			patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[1];
			Assert.AreEqual(2, patternStreamSpec.ViewSpecs.Length);
			Assert.AreEqual("length", patternStreamSpec.ViewSpecs[0].ObjectName);
			Assert.AreEqual("lastevent", patternStreamSpec.ViewSpecs[1].ObjectName);
		}

		[Test]
		public void TestIfThenElseCase()
		{
			string text;
			text = "select case when intPrimitive > shortPrimitive then count(intPrimitive) end from " + typeof(SupportBean).FullName + "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case when intPrimitive > shortPrimitive then count(intPrimitive) end as p1 from " +
			       typeof(SupportBean).FullName +
			       "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case when intPrimitive > shortPrimitive then count(intPrimitive) else shortPrimitive end from " +
			       typeof(SupportBean).FullName +
			       "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text =
				"select case when intPrimitive > shortPrimitive then count(intPrimitive) when longPrimitive > intPrimitive then count(longPrimitive) else shortPrimitive end from " +
				typeof(SupportBean).FullName +
				"().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case intPrimitive  when 1 then count(intPrimitive) end from " + typeof(SupportBean).FullName + "().win:lenght(10) as win";
			SupportParserHelper.ParseAndWalkEPL(container, text);
			text = "select case intPrimitive when longPrimitive then (intPrimitive + longPrimitive) end" +
			       " from " +
			       typeof(SupportBean).FullName +
			       "#length(3)";
			SupportParserHelper.ParseAndWalkEPL(container, text);
		}

		private void TryOuterJoin(
			string outerType,
			OuterJoinType typeExpected)
		{
			var text = "select intPrimitive from " +
			           typeof(SupportBean_A).FullName +
			           "().win:lenght(10) as win1 " +
			           outerType +
			           " outer join " +
			           typeof(SupportBean_A).FullName +
			           "().win:lenght(10) as win2 " +
			           "on win1.f1 = win2.f2[1]";
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);

			var descList = walker.StatementSpec.OuterJoinDescList;
			Assert.AreEqual(1, descList.Count);
			var desc = descList[0];
			Assert.AreEqual(typeExpected, desc.OuterJoinType);
			Assert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			Assert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			Assert.AreEqual("f2[1]", desc.OptRightNode.UnresolvedPropertyName);
			Assert.AreEqual("win2", desc.OptRightNode.StreamOrPropertyName);

			text = "select intPrimitive from " +
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
			Assert.AreEqual(2, descList.Count);

			desc = descList[0];
			Assert.AreEqual(typeExpected, desc.OuterJoinType);
			Assert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			Assert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			Assert.AreEqual("f2", desc.OptRightNode.UnresolvedPropertyName);
			Assert.AreEqual("win2", desc.OptRightNode.StreamOrPropertyName);

			desc = descList[1];
			Assert.AreEqual(typeExpected, desc.OuterJoinType);
			Assert.AreEqual("f1", desc.OptLeftNode.UnresolvedPropertyName);
			Assert.AreEqual("win1", desc.OptLeftNode.StreamOrPropertyName);
			Assert.AreEqual("f3", desc.OptRightNode.UnresolvedPropertyName);
			Assert.AreEqual("win3", desc.OptRightNode.StreamOrPropertyName);

			Assert.AreEqual(1, desc.AdditionalLeftNodes.Length);
			Assert.AreEqual("f11", desc.AdditionalLeftNodes[0].UnresolvedPropertyName);
			Assert.AreEqual("win1", desc.AdditionalLeftNodes[0].StreamOrPropertyName);
			Assert.AreEqual(1, desc.AdditionalRightNodes.Length);
			Assert.AreEqual("f31", desc.AdditionalRightNodes[0].UnresolvedPropertyName);
			Assert.AreEqual("win3", desc.AdditionalRightNodes[0].StreamOrPropertyName);
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
			Assert.AreEqual(2, merge.Items.Count);
			Assert.IsTrue(spec.WhereClause is ExprInNode);

			var first = merge.Items[0];
			Assert.AreEqual(4, first.Actions.Count);
			Assert.IsTrue(first.IsMatchedUnmatched);
			Assert.IsTrue(first.OptionalMatchCond is ExprEqualsNode);

			var insertOne = (OnTriggerMergeActionInsert) first.Actions[0];
			Assert.AreEqual("xyz1", insertOne.OptionalStreamName);
			Assert.AreEqual(0, insertOne.Columns.Count);
			Assert.AreEqual(2, insertOne.SelectClause.Count);
			Assert.IsTrue(insertOne.OptionalWhereClause is ExprRelationalOpNode);

			var updateOne = (OnTriggerMergeActionUpdate) first.Actions[1];
			Assert.AreEqual(1, updateOne.Assignments.Count);
			Assert.IsTrue(updateOne.OptionalWhereClause is ExprLikeNode);

			var delOne = (OnTriggerMergeActionDelete) first.Actions[2];
			Assert.IsTrue(delOne.OptionalWhereClause is ExprIdentNode);

			var delTwo = (OnTriggerMergeActionDelete) first.Actions[3];
			Assert.IsNull(delTwo.OptionalWhereClause);

			var second = merge.Items[1];
			Assert.IsFalse(second.IsMatchedUnmatched);
			Assert.IsTrue(second.OptionalMatchCond is ExprEqualsNode);
			Assert.AreEqual(2, second.Actions.Count);
		}

		[Test]
		public void TestWalkPattern()
		{
			var text = "every g=" + typeof(SupportBean).FullName + "(string=\"IBM\", intPrimitive != 1) where timer:within(20)";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + text + "]");

			Assert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			var rootNode = patternStreamSpec.EvalForgeNode;

			var everyNode = (EvalEveryForgeNode) rootNode;

			Assert.IsTrue(everyNode.ChildNodes[0] is EvalGuardForgeNode);
			var guardNode = (EvalGuardForgeNode) everyNode.ChildNodes[0];

			Assert.AreEqual(1, guardNode.ChildNodes.Count);
			Assert.IsTrue(guardNode.ChildNodes[0] is EvalFilterForgeNode);
			var filterNode = (EvalFilterForgeNode) guardNode.ChildNodes[0];

			Assert.AreEqual("g", filterNode.EventAsName);
			Assert.AreEqual(0, filterNode.ChildNodes.Count);
			Assert.AreEqual(2, filterNode.RawFilterSpec.FilterExpressions.Count);
			var equalsNode = (ExprEqualsNode) filterNode.RawFilterSpec.FilterExpressions[1];
			Assert.AreEqual(2, equalsNode.ChildNodes.Length);
		}

		[Test]
		public void TestWalkPropertyPatternCombination()
		{
			var EVENT = typeof(SupportBeanComplexProps).FullName;
			var property = TryWalkGetPropertyPattern(EVENT + "(Mapped ( 'key' )  = 'value')");
			Assert.AreEqual("Mapped('key')", property);

			property = TryWalkGetPropertyPattern(EVENT + "(Indexed [ 1 ]  = 1)");
			Assert.AreEqual("Indexed[1]", property);
			property = TryWalkGetPropertyPattern(EVENT + "(Nested . NestedValue  = 'value')");
			Assert.AreEqual("NestedValue", property);
		}

		[Test]
		public void TestWalkPatternUseResult()
		{
			var EVENT = typeof(SupportBean_N).FullName;
			var text = "na=" + EVENT + "() -> every nb=" + EVENT + "(doublePrimitive in [0:na.doublePrimitive])";
			SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + text + "]");
		}

		[Test]
		public void TestWalkIStreamRStreamSelect()
		{
			var text = "select rstream 'a' from " + typeof(SupportBean_N).FullName;
			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			Assert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select istream 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			Assert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			Assert.AreEqual(SelectClauseStreamSelectorEnum.ISTREAM_ONLY, walker.StatementSpec.SelectStreamSelectorEnum);

			text = "select irstream 'a' from " + typeof(SupportBean_N).FullName;
			walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			Assert.AreEqual(SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH, walker.StatementSpec.SelectStreamSelectorEnum);
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
				Assert.IsTrue(Math.Abs(delta) < 0.0000001, "Interval '" + interval + "' expected=" + expected + " actual=" + result);
			}

			TryIntervalInvalid(
				"1.5 month",
				"Time period expressions with month or year component require integer values, received a Double value");
		}

		[Test]
		public void TestWalkInAndBetween()
		{
			Assert.IsTrue((Boolean) TryRelationalOp("1 between 0 and 2"));
			Assert.IsFalse((Boolean) TryRelationalOp("-1 between 0 and 2"));
			Assert.IsFalse((Boolean) TryRelationalOp("1 not between 0 and 2"));
			Assert.IsTrue((Boolean) TryRelationalOp("-1 not between 0 and 2"));

			Assert.IsFalse((Boolean) TryRelationalOp("1 in (2,3)"));
			Assert.IsTrue((Boolean) TryRelationalOp("1 in (2,3,1)"));
			Assert.IsTrue((Boolean) TryRelationalOp("1 not in (2,3)"));
		}

		[Test]
		public void TestWalkLikeRegex()
		{
			Assert.IsTrue((Boolean) TryRelationalOp("'abc' like 'a__'"));
			Assert.IsFalse((Boolean) TryRelationalOp("'abcd' like 'a__'"));

			Assert.IsFalse((Boolean) TryRelationalOp("'abcde' not like 'a%'"));
			Assert.IsTrue((Boolean) TryRelationalOp("'bcde' not like 'a%'"));

			Assert.IsTrue((Boolean) TryRelationalOp("'a_' like 'a!_' escape '!'"));
			Assert.IsFalse((Boolean) TryRelationalOp("'ab' like 'a!_' escape '!'"));

			Assert.IsFalse((Boolean) TryRelationalOp("'a' not like 'a'"));
			Assert.IsTrue((Boolean) TryRelationalOp("'a' not like 'ab'"));
		}

		[Test]
		public void TestWalkDBJoinStatement()
		{
			var className = typeof(SupportBean).FullName;
			var sql = "select a from b where $x.id=c.d";
			var expression = "select * from " + className + ", sql:mydb ['" + sql + "']";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			var statementSpec = walker.StatementSpec;
			Assert.AreEqual(2, statementSpec.StreamSpecs.Count);
			var dbSpec = (DBStatementStreamSpec) statementSpec.StreamSpecs[1];
			Assert.AreEqual("mydb", dbSpec.DatabaseName);
			Assert.AreEqual(sql, dbSpec.SqlWithSubsParams);

			expression = "select * from " + className + ", sql:mydb ['" + sql + "' metadatasql 'select * from B']";

			walker = SupportParserHelper.ParseAndWalkEPL(container, expression);
			statementSpec = walker.StatementSpec;
			Assert.AreEqual(2, statementSpec.StreamSpecs.Count);
			dbSpec = (DBStatementStreamSpec) statementSpec.StreamSpecs[1];
			Assert.AreEqual("mydb", dbSpec.DatabaseName);
			Assert.AreEqual(sql, dbSpec.SqlWithSubsParams);
			Assert.AreEqual("select * from B", dbSpec.MetadataSQL);
		}

		[Test]
		public void TestRangeBetweenAndIn()
		{
			var className = typeof(SupportBean).FullName;
			var expression = "select * from " + className + "(intPrimitive in [1:2], intBoxed in (1,2), doubleBoxed between 2 and 3)";
			SupportParserHelper.ParseAndWalkEPL(container, expression);

			expression = "select * from " + className + "(intPrimitive not in [1:2], intBoxed not in (1,2), doubleBoxed not between 2 and 3)";
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
			Assert.AreEqual(1, spec.SelectClauseSpec.SelectExprList.Count);

			// check filter
			Assert.AreEqual(1, spec.StreamSpecs.Count);
			var filter = (FilterStreamSpecRaw) spec.StreamSpecs[0];
			Assert.AreEqual("B", filter.RawFilterSpec.EventTypeName);
			Assert.AreEqual(1, filter.RawFilterSpec.FilterExpressions.Count);

			// check where clause
			Assert.IsTrue(spec.WhereClause is ExprEqualsNode);
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
				Assert.AreEqual(message, ex.Message);
			}
		}

		private double TryInterval(string interval)
		{
			var text = "select * from " + typeof(SupportBean).FullName + "#win:time(" + interval + ")";

			var walker = SupportParserHelper.ParseAndWalkEPL(container, text);
			var viewSpec = walker.StatementSpec.StreamSpecs[0].ViewSpecs[0];
			Assert.AreEqual("win", viewSpec.ObjectNamespace);
			Assert.AreEqual("time", viewSpec.ObjectName);
			Assert.AreEqual(1, viewSpec.ObjectParameters.Count);
			var exprNode = (ExprTimePeriod) viewSpec.ObjectParameters[0];
			exprNode.Validate(SupportExprValidationContextFactory.MakeEmpty(container));
			return exprNode.EvaluateAsSeconds(null, true, null);
		}

		private string TryWalkGetPropertyPattern(string stmt)
		{
			var walker = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + stmt + "]");

			Assert.AreEqual(1, walker.StatementSpec.StreamSpecs.Count);
			var patternStreamSpec = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];

			var filterNode = (EvalFilterForgeNode) patternStreamSpec.EvalForgeNode;
			Assert.AreEqual(1, filterNode.RawFilterSpec.FilterExpressions.Count);
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
