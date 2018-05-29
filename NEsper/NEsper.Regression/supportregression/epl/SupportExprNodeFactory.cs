///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.container;
using com.espertech.esper.core.start;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.timer;
using com.espertech.esper.type;
using com.espertech.esper.util.support;
using com.espertech.esper.view;
using com.espertech.esper.view.window;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportExprNodeFactory
    {
	    public static QueryGraphValueEntryHashKeyed MakeKeyed(string property) {
	        return new QueryGraphValueEntryHashKeyedExpr(new ExprIdentNodeImpl(property), false);
	    }

	    public static QueryGraphValueEntryRange MakeRangeLess(string prop) {
	        return new QueryGraphValueEntryRangeRelOp(QueryGraphRangeEnum.LESS, new ExprIdentNodeImpl(prop), false);
	    }

	    public static QueryGraphValueEntryRange MakeRangeIn(string start, string end) {
	        return new QueryGraphValueEntryRangeIn(QueryGraphRangeEnum.RANGE_OPEN, new ExprIdentNodeImpl(start), new ExprIdentNodeImpl(end), false);
	    }

	    public static ExprNode[] MakeIdentExprNodes(params string[] props) {
	        var nodes = new ExprNode[props.Length];
	        for (var i = 0; i < props.Length; i++) {
	            nodes[i] = new ExprIdentNodeImpl(props[i]);
	        }
	        return nodes;
	    }

	    public static ExprNode[] MakeConstAndIdentNode(string constant, string property) {
	        return new ExprNode[] {new ExprConstantNodeImpl(constant), new ExprIdentNodeImpl(property)};
	    }

	    public static ExprNode[] MakeConstAndConstNode(string constantOne, string constantTwo) {
	        return new ExprNode[] {new ExprConstantNodeImpl(constantOne), new ExprConstantNodeImpl(constantTwo)};
	    }

	    public static ExprNode MakeIdentExprNode(string property) {
	        return new ExprIdentNodeImpl(property);
	    }

	    public static ExprNode MakeConstExprNode(string constant) {
	        return new ExprConstantNodeImpl(constant);
	    }

	    public static ExprNode[] MakeIdentNodesBean(params string[] names)
	    {
	        var nodes = new ExprNode[names.Length];
	        for (var i = 0; i < names.Length; i++)
	        {
	            nodes[i] = new ExprIdentNodeImpl(names[i]);
	            Validate1StreamBean(nodes[i]);
	        }
	        return nodes;
	    }

	    public static ExprNode[] MakeIdentNodesMD(params string[] names)
	    {
	        var nodes = new ExprNode[names.Length];
	        for (var i = 0; i < names.Length; i++)
	        {
	            nodes[i] = new ExprIdentNodeImpl(names[i]);
	            Validate1StreamMD(nodes[i]);
	        }
	        return nodes;
	    }

	    public static ExprNode MakeIdentNodeBean(string names)
	    {
	        ExprNode node = new ExprIdentNodeImpl(names);
	        Validate1StreamBean(node);
	        return node;
	    }

	    public static ExprNode MakeIdentNodeMD(string names)
	    {
	        ExprNode node = new ExprIdentNodeImpl(names);
	        Validate1StreamMD(node);
	        return node;
	    }

	    public static ExprNode MakeIdentNodeNoValid(string names)
	    {
	        return new ExprIdentNodeImpl(names);
	    }

	    public static ExprEqualsNode MakeEqualsNode()
	    {
	        ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
	        ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed", "s1");
	        topNode.AddChildNode(i1_1);
	        topNode.AddChildNode(i1_2);

	        Validate3Stream(topNode);

	        return topNode;
	    }

	    public static ExprPreviousNode MakePreviousNode()
	    {
	        var prevNode = new ExprPreviousNode(ExprPreviousNodePreviousType.PREV);
	        ExprNode indexNode = new ExprIdentNodeImpl("IntPrimitive", "s1");
	        prevNode.AddChildNode(indexNode);
	        ExprNode propNode = new ExprIdentNodeImpl("DoublePrimitive", "s1");
	        prevNode.AddChildNode(propNode);

	        Validate3Stream(prevNode);

	        return prevNode;
	    }

	    public static ExprPriorNode MakePriorNode()
	    {
	        var priorNode = new ExprPriorNode();
	        ExprNode indexNode = new ExprConstantNodeImpl(1);
	        priorNode.AddChildNode(indexNode);
	        ExprNode propNode = new ExprIdentNodeImpl("DoublePrimitive", "s0");
	        priorNode.AddChildNode(propNode);

	        Validate3Stream(priorNode);

	        return priorNode;
	    }

	    public static ExprAndNode Make2SubNodeAnd()
	    {
	        ExprAndNode topNode = new ExprAndNodeImpl();

	        ExprEqualsNode e1 = new ExprEqualsNodeImpl(false, false);
	        ExprEqualsNode e2 = new ExprEqualsNodeImpl(false, false);

	        topNode.AddChildNode(e1);
	        topNode.AddChildNode(e2);

	        ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed", "s1");
	        e1.AddChildNode(i1_1);
	        e1.AddChildNode(i1_2);

	        ExprIdentNode i2_1 = new ExprIdentNodeImpl("TheString", "s1");
	        ExprIdentNode i2_2 = new ExprIdentNodeImpl("TheString", "s0");
	        e2.AddChildNode(i2_1);
	        e2.AddChildNode(i2_2);

	        Validate3Stream(topNode);

	        return topNode;
	    }

	    public static ExprNode Make3SubNodeAnd()
	    {
	        ExprNode topNode = new ExprAndNodeImpl();

	        var equalNodes = new ExprEqualsNode[3];
	        for (var i = 0; i < equalNodes.Length; i++)
	        {
	            equalNodes[i] = new ExprEqualsNodeImpl(false, false);
	            topNode.AddChildNode(equalNodes[i]);
	        }

	        ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed", "s1");
	        equalNodes[0].AddChildNode(i1_1);
	        equalNodes[0].AddChildNode(i1_2);

	        ExprIdentNode i2_1 = new ExprIdentNodeImpl("TheString", "s1");
	        ExprIdentNode i2_2 = new ExprIdentNodeImpl("TheString", "s0");
	        equalNodes[1].AddChildNode(i2_1);
	        equalNodes[1].AddChildNode(i2_2);

	        ExprIdentNode i3_1 = new ExprIdentNodeImpl("BoolBoxed", "s0");
	        ExprIdentNode i3_2 = new ExprIdentNodeImpl("BoolPrimitive", "s1");
	        equalNodes[2].AddChildNode(i3_1);
	        equalNodes[2].AddChildNode(i3_2);

	        Validate3Stream(topNode);

	        return topNode;
	    }

	    public static ExprNode MakeIdentNode(string fieldName, string streamName)
	    {
	        ExprIdentNode node = new ExprIdentNodeImpl(fieldName, streamName);
	        Validate3Stream(node);
	        return node;
	    }

	    public static ExprNode MakeMathNode()
	    {
	        ExprIdentNode node1 = new ExprIdentNodeImpl("IntBoxed", "s0");
	        ExprIdentNode node2 = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        var mathNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
	        mathNode.AddChildNode(node1);
	        mathNode.AddChildNode(node2);

	        Validate3Stream(mathNode);

	        return mathNode;
	    }

	    public static ExprNode MakeMathNode(MathArithTypeEnum @operator, object valueLeft, object valueRight)
	    {
	        var mathNode = new ExprMathNode(@operator, false, false);
	        mathNode.AddChildNode(new SupportExprNode(valueLeft));
	        mathNode.AddChildNode(new SupportExprNode(valueRight));
	        Validate3Stream(mathNode);
	        return mathNode;
	    }

	    public static ExprNode MakeSumAndFactorNode(IContainer container)
	    {
	        // sum node
	        var sum = new ExprSumNode(false);
	        ExprIdentNode ident = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        sum.AddChildNode(ident);

	        ExprIdentNode node = new ExprIdentNodeImpl("IntBoxed", "s0");
	        var mathNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
	        mathNode.AddChildNode(node);
	        mathNode.AddChildNode(sum);

	        Validate3Stream(mathNode);

	        return mathNode;
	    }

	    public static ExprAggregateNode MakeSumAggregateNode()
	    {
	        var top = new ExprSumNode(false);
	        ExprIdentNode ident = new ExprIdentNodeImpl("IntPrimitive", "s0");
	        top.AddChildNode(ident);

	        Validate3Stream(top);

	        return top;
	    }

	    public static ExprNode MakeCountNode(object value, Type type)
	    {
	        var countNode = new ExprCountNode(false);
	        countNode.AddChildNode(new SupportExprNode(value, type));
	        var future = new SupportAggregationResultFuture(new object[] {10, 20});
	        countNode.SetAggregationResultFuture(future, 1);
	        Validate3Stream(countNode);
	        return countNode;
	    }

	    public static ExprNode MakeRelationalOpNode(
	        RelationalOpEnum @operator, object valueLeft, Type typeLeft, object valueRight, Type typeRight)
	    {
	        ExprRelationalOpNode opNode = new ExprRelationalOpNodeImpl(@operator);
	        opNode.AddChildNode(new SupportExprNode(valueLeft, typeLeft));
	        opNode.AddChildNode(new SupportExprNode(valueRight, typeRight));
	        Validate3Stream(opNode);
	        return opNode;
	    }

	    public static ExprNode MakeRelationalOpNode( 
	        RelationalOpEnum @operator, Type typeLeft, Type typeRight)
	    {
	        ExprRelationalOpNode opNode = new ExprRelationalOpNodeImpl(@operator);
	        opNode.AddChildNode(new SupportExprNode(typeLeft));
	        opNode.AddChildNode(new SupportExprNode(typeRight));
	        Validate3Stream(opNode);
	        return opNode;
	    }

	    public static ExprNode MakeRelationalOpNode( 
	        RelationalOpEnum @operator, ExprNode nodeLeft, ExprNode nodeRight)
	    {
	        ExprRelationalOpNode opNode = new ExprRelationalOpNodeImpl(@operator);
	        opNode.AddChildNode(nodeLeft);
	        opNode.AddChildNode(nodeRight);
	        Validate3Stream(opNode);
	        return opNode;
	    }

	    public static ExprInNode MakeInSetNode(bool isNotIn)
	    {
	        // Build :      s0.intPrimitive in (1, 2)
	        ExprInNode inNode = new ExprInNodeImpl(isNotIn);
	        inNode.AddChildNode(MakeIdentNode("IntPrimitive","s0"));
	        inNode.AddChildNode(new SupportExprNode(1));
	        inNode.AddChildNode(new SupportExprNode(2));
	        Validate3Stream(inNode);
	        return inNode;
	    }

	    public static ExprRegexpNode MakeRegexpNode(bool isNot)
	    {
	        // Build :      s0.string regexp "[a-z][a-z]"  (with not)
	        var node = new ExprRegexpNode(isNot);
	        node.AddChildNode(MakeIdentNode("TheString","s0"));
	        node.AddChildNode(new SupportExprNode("[a-z][a-z]"));
	        Validate3Stream(node);
	        return node;
	    }

	    public static ExprLikeNode MakeLikeNode(bool isNot, string optionalEscape)
	    {
	        // Build :      s0.string like "%abc__"  (with or witout escape)
	        var node = new ExprLikeNode(isNot);
	        node.AddChildNode(MakeIdentNode("TheString","s0"));
	        node.AddChildNode(new SupportExprNode("%abc__"));
	        if (optionalEscape != null)
	        {
	            node.AddChildNode(new SupportExprNode(optionalEscape));
	        }
	        Validate3Stream(node);
	        return node;
	    }

	    public static ExprCaseNode MakeCaseSyntax1Node()
	    {
	        // Build (case 1 expression):
	        // case when s0.IntPrimitive = 1 then "a"
	        //      when s0.IntPrimitive = 2 then "b"
	        //      else "c"
	        // end
	        var caseNode = new ExprCaseNode(false);

	        ExprNode node = MakeEqualsNode("IntPrimitive", "s0", 1);
	        caseNode.AddChildNode(node);
	        caseNode.AddChildNode(new SupportExprNode("a"));

	        node = MakeEqualsNode("IntPrimitive", "s0", 2);
	        caseNode.AddChildNode(node);
	        caseNode.AddChildNode(new SupportExprNode("b"));

	        caseNode.AddChildNode(new SupportExprNode("c"));

	        Validate3Stream(caseNode);

	        return caseNode;
	    }

	    public static ExprCaseNode MakeCaseSyntax2Node()
	    {
	        // Build (case 2 expression):
	        // case s0.intPrimitive
	        //   when 1 then "a"
	        //   when 2 then "b"
	        //   else "c"
	        // end
	        var caseNode = new ExprCaseNode(true);
	        caseNode.AddChildNode(MakeIdentNode("IntPrimitive","s0"));

	        caseNode.AddChildNode(new SupportExprNode(1));
	        caseNode.AddChildNode(new SupportExprNode("a"));
	        caseNode.AddChildNode(new SupportExprNode(2));
	        caseNode.AddChildNode(new SupportExprNode("b"));
	        caseNode.AddChildNode(new SupportExprNode("c"));

	        Validate3Stream(caseNode);

	        return (caseNode);
	    }

	    private static ExprEqualsNode MakeEqualsNode(string ident1, string stream1, object value)
	    {
	        ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
	        ExprIdentNode i1_1 = new ExprIdentNodeImpl(ident1, stream1);
	        var constantNode = new SupportExprNode(value);
	        topNode.AddChildNode(i1_1);
	        topNode.AddChildNode(constantNode);
	        return topNode;
	    }

	    public static void Validate3Stream( ExprNode topNode)
	    {
	        var supportContainer = SupportContainer.Instance;
	        var streamTypeService = new SupportStreamTypeSvc3Stream();

	        var factoriesPerStream = new ViewFactoryChain[3];
	        for (var i = 0; i < factoriesPerStream.Length; i++)
	        {
	            var factories = new List<ViewFactory>();
	            factories.Add(new LengthWindowViewFactory());
	            factoriesPerStream[i] = new ViewFactoryChain(streamTypeService.EventTypes[i], factories);
	        }
	        var viewResources = new ViewResourceDelegateUnverified();

            EngineImportService engineImportService = SupportEngineImportServiceFactory.Make(supportContainer);

	        VariableService variableService = new VariableServiceImpl(
	            supportContainer, 0,
	            new SchedulingServiceImpl(new TimeSourceServiceImpl(), supportContainer),
	            SupportContainer.Resolve<EventAdapterService>(),
	            null);
	        variableService.CreateNewVariable(null, "IntPrimitive", typeof(int?).FullName, false, false, false, 10, engineImportService);
	        variableService.AllocateVariableState("IntPrimitive", EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, null, false);
	        variableService.CreateNewVariable(null, "var1", typeof(string).FullName, false, false, false, "my_variable_value", engineImportService);
            variableService.AllocateVariableState("var1", EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, null, false);

	        ExprNodeUtility.GetValidatedSubtree(
	            ExprNodeOrigin.SELECT, topNode, new ExprValidationContext(
	                supportContainer,
                    streamTypeService, 
	                SupportEngineImportServiceFactory.Make(supportContainer), 
                    null, viewResources,
                    null, variableService, null,
	                new SupportExprEvaluatorContext(supportContainer, null),
	                null, null, 1, null, null, null,
                    false, false, false, false, null, false));
	    }

	    public static void Validate1StreamBean( ExprNode topNode)
	    {
	        var supportContainer = SupportContainer.Instance;
	        var eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
	        StreamTypeService streamTypeService = new StreamTypeServiceImpl(eventType, "s0", false, "uri");
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, topNode, SupportExprValidationContextFactory.Make(supportContainer, streamTypeService));
	    }

	    public static void Validate1StreamMD( ExprNode topNode)
	    {
	        var supportContainer = SupportContainer.Instance;
	        var eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportMarketDataBean));
	        StreamTypeService streamTypeService = new StreamTypeServiceImpl(eventType, "s0", false, "uri");
            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, topNode, SupportExprValidationContextFactory.Make(supportContainer, streamTypeService));
	    }
	}
} // end of namespace
