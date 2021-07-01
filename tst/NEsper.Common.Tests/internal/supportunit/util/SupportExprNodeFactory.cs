///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportExprNodeFactory
    {
        private readonly IContainer _container;

        private SupportExprNodeFactory(IContainer container)
        {
            _container = container;
        }

        public static SupportExprNodeFactory GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new SupportExprNodeFactory(container));
        }

        public static void RegisterSingleton(IContainer container)
        {
            container.Register<SupportExprNodeFactory>(
                xx => new SupportExprNodeFactory(container),
                Lifespan.Singleton);
        }

        public QueryGraphValueEntryHashKeyedForge MakeKeyed(string property)
        {
            return new QueryGraphValueEntryHashKeyedForgeExpr(new ExprIdentNodeImpl(property), false);
        }

        public QueryGraphValueEntryRangeForge MakeRangeLess(string prop)
        {
            return new QueryGraphValueEntryRangeRelOpForge(QueryGraphRangeEnum.LESS, new ExprIdentNodeImpl(prop), false);
        }

        public QueryGraphValueEntryRangeInForge MakeRangeIn(
            string start,
            string end)
        {
            return new QueryGraphValueEntryRangeInForge(
                QueryGraphRangeEnum.RANGE_OPEN,
                new ExprIdentNodeImpl(start),
                new ExprIdentNodeImpl(end),
                false);
        }

        public ExprNode[] MakeIdentExprNodes(params string[] props)
        {
            var nodes = new ExprNode[props.Length];
            for (var i = 0; i < props.Length; i++) {
                nodes[i] = new ExprIdentNodeImpl(props[i]);
            }

            return nodes;
        }

        public ExprNode[] MakeConstAndIdentNode(
            string constant,
            string property)
        {
            return new ExprNode[] {new ExprConstantNodeImpl(constant), new ExprIdentNodeImpl(property)};
        }

        public ExprNode[] MakeConstAndConstNode(
            string constantOne,
            string constantTwo)
        {
            return new ExprNode[] {new ExprConstantNodeImpl(constantOne), new ExprConstantNodeImpl(constantTwo)};
        }

        public ExprNode MakeIdentExprNode(string property)
        {
            return new ExprIdentNodeImpl(property);
        }

        public ExprNode MakeConstExprNode(string constant)
        {
            return new ExprConstantNodeImpl(constant);
        }

        public ExprEqualsNode MakeEqualsNode()
        {
            ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
            ExprIdentNode i1_1 = new ExprIdentNodeImpl("IntPrimitive", "s0");
            ExprIdentNode i1_2 = new ExprIdentNodeImpl("IntBoxed", "s1");
            topNode.AddChildNode(i1_1);
            topNode.AddChildNode(i1_2);

            Validate3Stream(topNode);

            return topNode;
        }

        public ExprInNode MakeInSetNode(bool isNotIn)
        {
            // Build :      s0.IntPrimitive in (1, 2)
            ExprInNode inNode = new ExprInNodeImpl(isNotIn);
            inNode.AddChildNode(MakeIdentNode("IntPrimitive", "s0"));
            inNode.AddChildNode(new SupportExprNode(1));
            inNode.AddChildNode(new SupportExprNode(2));
            Validate3Stream(inNode);
            return inNode;
        }

        public ExprCaseNode MakeCaseSyntax1Node()
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

        public ExprCaseNode MakeCaseSyntax2Node()
        {
            // Build (case 2 expression):
            // case s0.IntPrimitive
            //   when 1 then "a"
            //   when 2 then "b"
            //   else "c"
            // end
            var caseNode = new ExprCaseNode(true);
            caseNode.AddChildNode(MakeIdentNode("IntPrimitive", "s0"));

            caseNode.AddChildNode(new SupportExprNode(1));
            caseNode.AddChildNode(new SupportExprNode("a"));
            caseNode.AddChildNode(new SupportExprNode(2));
            caseNode.AddChildNode(new SupportExprNode("b"));
            caseNode.AddChildNode(new SupportExprNode("c"));

            Validate3Stream(caseNode);

            return caseNode;
        }

        public ExprRegexpNode MakeRegexpNode(bool isNot)
        {
            // Build :      s0.string regexp "[a-z][a-z]"  (with not)
            var node = new ExprRegexpNode(isNot);
            node.AddChildNode(MakeIdentNode("TheString", "s0"));
            node.AddChildNode(new SupportExprNode("[a-z][a-z]"));
            Validate3Stream(node);
            return node;
        }

        public ExprIdentNode MakeIdentNode(
            string fieldName,
            string streamName)
        {
            ExprIdentNode node = new ExprIdentNodeImpl(fieldName, streamName);
            Validate3Stream(node);
            return node;
        }

        public ExprLikeNode MakeLikeNode(
            bool isNot,
            string optionalEscape)
        {
            // Build :      s0.string like "%abc__"  (with or witout escape)
            var node = new ExprLikeNode(isNot);
            node.AddChildNode(MakeIdentNode("TheString", "s0"));
            node.AddChildNode(new SupportExprNode("%abc__"));
            if (optionalEscape != null) {
                node.AddChildNode(new SupportExprNode(optionalEscape));
            }

            Validate3Stream(node);
            return node;
        }

        public ExprNode Make2SubNodeAnd()
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

        public ExprNode Make3SubNodeAnd()
        {
            ExprNode topNode = new ExprAndNodeImpl();

            var equalNodes = new ExprEqualsNode[3];
            for (var i = 0; i < equalNodes.Length; i++) {
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

        public ExprNode MakeMathNode()
        {
            ExprIdentNode node1 = new ExprIdentNodeImpl("IntBoxed", "s0");
            ExprIdentNode node2 = new ExprIdentNodeImpl("IntPrimitive", "s0");
            var mathNode = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            mathNode.AddChildNode(node1);
            mathNode.AddChildNode(node2);

            Validate3Stream(mathNode);

            return mathNode;
        }

        public void Validate3Stream(
            ExprNode topNode)
        {
            var streamTypeService = new SupportStreamTypeSvc3Stream(
                SupportEventTypeFactory.GetInstance(_container));
            var validationContext = SupportExprValidationContextFactory.Make(_container, streamTypeService);

            try {
                ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, topNode, validationContext);
            }
            catch (ExprValidationException e) {
                throw new EPRuntimeException(e);
            }
        }

        private ExprEqualsNode MakeEqualsNode(
            string ident1,
            string stream1,
            object value)
        {
            ExprEqualsNode topNode = new ExprEqualsNodeImpl(false, false);
            ExprIdentNode i1_1 = new ExprIdentNodeImpl(ident1, stream1);
            var constantNode = new SupportExprNode(value);
            topNode.AddChildNode(i1_1);
            topNode.AddChildNode(constantNode);
            return topNode;
        }
    }
} // end of namespace
