///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression
{
    [TestFixture]
    public class TestExprNode 
    {
        [Test]
        public void TestGetValidatedSubtree()
        {
            SupportExprNode.SetValidateCount(0);
    
            // Confirms all child nodes validated
            // Confirms depth-first validation
            SupportExprNode topNode = new SupportExprNode(typeof(Boolean));
    
            SupportExprNode parent_1 = new SupportExprNode(typeof(Boolean));
            SupportExprNode parent_2 = new SupportExprNode(typeof(Boolean));
    
            topNode.AddChildNode(parent_1);
            topNode.AddChildNode(parent_2);
    
            SupportExprNode supportNode1_1 = new SupportExprNode(typeof(Boolean));
            SupportExprNode supportNode1_2 = new SupportExprNode(typeof(Boolean));
            SupportExprNode supportNode2_1 = new SupportExprNode(typeof(Boolean));
            SupportExprNode supportNode2_2 = new SupportExprNode(typeof(Boolean));
    
            parent_1.AddChildNode(supportNode1_1);
            parent_1.AddChildNode(supportNode1_2);
            parent_2.AddChildNode(supportNode2_1);
            parent_2.AddChildNode(supportNode2_2);

            ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, topNode, ExprValidationContextFactory.MakeEmpty());
    
            Assert.AreEqual(1, supportNode1_1.ValidateCountSnapshot);
            Assert.AreEqual(2, supportNode1_2.ValidateCountSnapshot);
            Assert.AreEqual(3, parent_1.ValidateCountSnapshot);
            Assert.AreEqual(4, supportNode2_1.ValidateCountSnapshot);
            Assert.AreEqual(5, supportNode2_2.ValidateCountSnapshot);
            Assert.AreEqual(6, parent_2.ValidateCountSnapshot);
            Assert.AreEqual(7, topNode.ValidateCountSnapshot);
        }
    
        [Test]
        public void TestDeepEquals()
        {
            Assert.IsFalse(ExprNodeUtility.DeepEquals(SupportExprNodeFactory.Make2SubNodeAnd(), SupportExprNodeFactory.Make3SubNodeAnd()));
            Assert.IsFalse(ExprNodeUtility.DeepEquals(SupportExprNodeFactory.MakeEqualsNode(), SupportExprNodeFactory.MakeMathNode()));
            Assert.IsTrue(ExprNodeUtility.DeepEquals(SupportExprNodeFactory.MakeMathNode(), SupportExprNodeFactory.MakeMathNode()));
            Assert.IsFalse(ExprNodeUtility.DeepEquals(SupportExprNodeFactory.MakeMathNode(), SupportExprNodeFactory.Make2SubNodeAnd()));
            Assert.IsTrue(ExprNodeUtility.DeepEquals(SupportExprNodeFactory.Make3SubNodeAnd(), SupportExprNodeFactory.Make3SubNodeAnd()));
        }
    
        [Test]
        public void TestParseMappedProp()
        {
            ExprNodeUtility.MappedPropertyParseResult result = ExprNodeUtility.ParseMappedProperty("a.b('c')");
            Assert.AreEqual("a", typeof(result)Name);
            Assert.AreEqual("b", result.MethodName);
            Assert.AreEqual("c", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))')");
            Assert.AreEqual("SupportStaticMethodLib", typeof(result)Name);
            Assert.AreEqual("DelimitPipe", result.MethodName);
            Assert.AreEqual("POLYGON ((100.0 100, \", 100 100, 400 400))", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("a.b.c.d.e('f.g.h,u.h')");
            Assert.AreEqual("a.b.c.d", typeof(result)Name);
            Assert.AreEqual("e", result.MethodName);
            Assert.AreEqual("f.g.h,u.h", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("a.b.c.d.E(\"hfhf f f f \")");
            Assert.AreEqual("a.b.c.d", typeof(result)Name);
            Assert.AreEqual("E", result.MethodName);
            Assert.AreEqual("hfhf f f f ", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("c.d.GetEnumerationSource(\"kf\"kf'kf\")");
            Assert.AreEqual("c.d", typeof(result)Name);
            Assert.AreEqual("GetEnumerationSource", result.MethodName);
            Assert.AreEqual("kf\"kf'kf", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("c.d.GetEnumerationSource('kf\"kf'kf\"')");
            Assert.AreEqual("c.d", typeof(result)Name);
            Assert.AreEqual("GetEnumerationSource", result.MethodName);
            Assert.AreEqual("kf\"kf'kf\"", result.ArgString);
    
            result = ExprNodeUtility.ParseMappedProperty("f('a')");
            Assert.AreEqual(null, typeof(result)Name);
            Assert.AreEqual("f", result.MethodName);
            Assert.AreEqual("a", result.ArgString);
    
            Assert.IsNull(ExprNodeUtility.ParseMappedProperty("('a')"));
            Assert.IsNull(ExprNodeUtility.ParseMappedProperty(""));
        }
    }
}
