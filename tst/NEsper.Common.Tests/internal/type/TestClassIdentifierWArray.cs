///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestClassIdentifierWArray : AbstractCommonTest
    {
        [Test]
        public void TestParse()
        {
            AssertParse("x[]", "x", 1, false);
            AssertParse("x[Primitive]", "x", 1, true);
            AssertParse("x", "x", 0, false);
            AssertParse("x.y", "x.y", 0, false);
            AssertParse("x[][]", "x", 2, false);
            AssertParse("x[primitive][]", "x", 2, true);
        }

        private void AssertParse(string classIdentifier, string name, int dimensions, bool arrayOfPrimitive)
        {
            ClassDescriptor ident = ClassDescriptor.ParseTypeText(classIdentifier);
            Assert.AreEqual(name, ident.ClassIdentifier);
            Assert.AreEqual(dimensions, ident.ArrayDimensions);
            Assert.AreEqual(arrayOfPrimitive, ident.IsArrayOfPrimitive);
        }
    }
} // end of namespace
