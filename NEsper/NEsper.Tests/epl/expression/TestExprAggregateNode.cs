///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprAggregateNode 
    {
        [Test]
        public void TestGetAggregatesBottomUp()
        {
            ExprNode top = new SupportAggregateExprNode(null);
            ExprNode c1 = new SupportExprNode(null);
            ExprNode c2 = new SupportExprNode(null);
            top.AddChildNode(c1);
            top.AddChildNode(c2);
    
            ExprNode c1_1 = new SupportAggregateExprNode(null);
            ExprNode c1_2 = new SupportAggregateExprNode(null);
            c1.AddChildNode(c1_1);
            c1.AddChildNode(c1_2);
            c1_1.AddChildNode(new SupportExprNode(null));
            c1_2.AddChildNode(new SupportExprNode(null));
    
            ExprNode c2_1 = new SupportExprNode(null);
            ExprNode c2_2 = new SupportExprNode(null);
            c2.AddChildNode(c2_1);
            c2.AddChildNode(c2_2);
            c2_2.AddChildNode(new SupportExprNode(null));
    
            ExprNode c2_1_1 = new SupportAggregateExprNode(null);
            ExprNode c2_1_2 = new SupportAggregateExprNode(null);
            c2_1.AddChildNode(c2_1_1);
            c2_1.AddChildNode(c2_1_2);
    
            List<ExprAggregateNode> aggregates = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(top, aggregates);
    
            Assert.AreEqual(5, aggregates.Count);
            Assert.AreSame(c2_1_1, aggregates[0]);
            Assert.AreSame(c2_1_2, aggregates[1]);
            Assert.AreSame(c1_1, aggregates[2]);
            Assert.AreSame(c1_2, aggregates[3]);
            Assert.AreSame(top, aggregates[4]);
    
            // Test no aggregates
            aggregates.Clear();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(new SupportExprNode(null), aggregates);
            Assert.IsTrue(aggregates.IsEmpty());
        }
    }
}
