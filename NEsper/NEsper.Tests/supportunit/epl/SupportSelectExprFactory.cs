///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.type;
using com.espertech.esper.util.support;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportSelectExprFactory
    {
        public static SelectClauseElementCompiled[] MakeInvalidSelectList()
        {
            ExprIdentNode node = new ExprIdentNodeImpl("xxxx", "s0");
            return new SelectClauseElementCompiled[] {new SelectClauseExprCompiledSpec(node, null, null, false)};
        }
    
        public static IList<SelectClauseExprCompiledSpec> MakeSelectListFromIdent(String propertyName, String streamName)
        {
            IList<SelectClauseExprCompiledSpec> selectionList = new List<SelectClauseExprCompiledSpec>();
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode(propertyName, streamName);
            selectionList.Add(new SelectClauseExprCompiledSpec(identNode, "PropertyName", null, false));
            return selectionList;
        }
    
        public static IList<SelectClauseExprCompiledSpec> MakeNoAggregateSelectList()
        {
            IList<SelectClauseExprCompiledSpec> selectionList = new List<SelectClauseExprCompiledSpec>();
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
            ExprNode mathNode = SupportExprNodeFactory.MakeMathNode();
            selectionList.Add(new SelectClauseExprCompiledSpec(identNode, "resultOne", null, false));
            selectionList.Add(new SelectClauseExprCompiledSpec(mathNode, "resultTwo", null, false));
            return selectionList;
        }
    
        public static SelectClauseElementCompiled[] MakeNoAggregateSelectListUnnamed()
        {
            IList<SelectClauseElementCompiled> selectionList = new List<SelectClauseElementCompiled>();
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
            ExprNode mathNode = SupportExprNodeFactory.MakeMathNode();
            selectionList.Add(new SelectClauseExprCompiledSpec(identNode, null, null, false));
            selectionList.Add(new SelectClauseExprCompiledSpec(mathNode, "result", null, false));
            return selectionList.ToArray();
        }
    
        public static SelectClauseElementCompiled[] MakeAggregateSelectListWithProps()
        {
            ExprNode top = new ExprSumNode(false);
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
            top.AddChildNode(identNode);
    
            SelectClauseElementCompiled[] selectionList = new SelectClauseElementCompiled[] {
                new SelectClauseExprCompiledSpec(top, null, null, false) };
            return selectionList;
        }
    
        public static SelectClauseElementCompiled[] MakeAggregatePlusNoAggregate()
        {
            ExprNode top = new ExprSumNode(false);
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
            top.AddChildNode(identNode);
    
            ExprNode identNode2 = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
    
            IList<SelectClauseElementCompiled> selectionList = new List<SelectClauseElementCompiled>();
            selectionList.Add(new SelectClauseExprCompiledSpec(top, null, null, false));
            selectionList.Add(new SelectClauseExprCompiledSpec(identNode2, null, null, false));
            return selectionList.ToArray();
        }
    
        public static SelectClauseElementCompiled[] MakeAggregateMixed()
        {
            // make a "select DoubleBoxed, Sum(IntPrimitive)" -equivalent
            IList<SelectClauseElementCompiled> selectionList = new List<SelectClauseElementCompiled>();
    
            ExprNode identNode = SupportExprNodeFactory.MakeIdentNode("DoubleBoxed", "s0");
            selectionList.Add(new SelectClauseExprCompiledSpec(identNode, null, null, false));
    
            ExprNode top = new ExprSumNode(false);
            identNode = SupportExprNodeFactory.MakeIdentNode("IntPrimitive", "s0");
            top.AddChildNode(identNode);
            selectionList.Add(new SelectClauseExprCompiledSpec(top, null, null, false));
    
            return selectionList.ToArray();
        }
    
        public static IList<SelectClauseExprRawSpec> MakeAggregateSelectListNoProps()
        {
            var container = SupportContainer.Instance;

            /*
                                        top (*)
                      c1 (sum)                            c2 (10)
                      c1_1 (5)
            */
    
            ExprNode top = new ExprMathNode(MathArithTypeEnum.MULTIPLY, false, false);
            ExprNode c1 = new ExprSumNode(false);
            ExprNode c1_1 = new SupportExprNode(5);
            ExprNode c2 = new SupportExprNode(10);
    
            top.AddChildNode(c1);
            top.AddChildNode(c2);
            c1.AddChildNode(c1_1);

            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, top, SupportExprValidationContextFactory.MakeEmpty(container));
    
            IList<SelectClauseExprRawSpec> selectionList = new List<SelectClauseExprRawSpec>();
            selectionList.Add(new SelectClauseExprRawSpec(top, null, false));
            return selectionList;
        }
    }
}
