///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    [TestFixture]
    public class TestExprNodeUtilityValidate : AbstractCommonTest
    {
        [Test]
        public void TestGetValidatedSubtree()
        {
            SupportExprNode.ValidateCount = 0;

            // Confirms all child nodes validated
            // Confirms depth-first validation
            var topNode = new SupportExprNode(typeof(bool?));

            var parent_1 = new SupportExprNode(typeof(bool?));
            var parent_2 = new SupportExprNode(typeof(bool?));

            topNode.AddChildNode(parent_1);
            topNode.AddChildNode(parent_2);

            var supportNode1_1 = new SupportExprNode(typeof(bool?));
            var supportNode1_2 = new SupportExprNode(typeof(bool?));
            var supportNode2_1 = new SupportExprNode(typeof(bool?));
            var supportNode2_2 = new SupportExprNode(typeof(bool?));

            parent_1.AddChildNode(supportNode1_1);
            parent_1.AddChildNode(supportNode1_2);
            parent_2.AddChildNode(supportNode2_1);
            parent_2.AddChildNode(supportNode2_2);

            ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.SELECT, topNode,
                SupportExprValidationContextFactory.MakeEmpty(container));

            ClassicAssert.AreEqual(1, supportNode1_1.ValidateCountSnapshot);
            ClassicAssert.AreEqual(2, supportNode1_2.ValidateCountSnapshot);
            ClassicAssert.AreEqual(3, parent_1.ValidateCountSnapshot);
            ClassicAssert.AreEqual(4, supportNode2_1.ValidateCountSnapshot);
            ClassicAssert.AreEqual(5, supportNode2_2.ValidateCountSnapshot);
            ClassicAssert.AreEqual(6, parent_2.ValidateCountSnapshot);
            ClassicAssert.AreEqual(7, topNode.ValidateCountSnapshot);
        }
    }
} // end of namespace
