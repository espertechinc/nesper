///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestClassIdentifierWArrayParser : AbstractCommonTest
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

            AssertParse("System.Collections.Generic.IList<String>", "System.Collections.Generic.IList", 0, false, new ClassDescriptor("String"));
            AssertParse("System.Collections.Generic.IList< String >", "System.Collections.Generic.IList", 0, false, new ClassDescriptor("String"));
            AssertParse("IList<String, Integer>", "IList", 0, false, new ClassDescriptor("String"), new ClassDescriptor("Integer"));
            AssertParse("System.Collections.Generic.IList<String>[]", "System.Collections.Generic.IList", 1, false, new ClassDescriptor("String"));
            AssertParse("x<y>[][]", "x", 2, false, new ClassDescriptor("y"));
            AssertParse("x<y>[primitive][][]", "x", 3, true, new ClassDescriptor("y"));
            AssertParse("x<y[]>", "x", 0, false, new ClassDescriptor("y", EmptyList<ClassDescriptor>.Instance, 1, false));
            AssertParse("x<y[primitive]>", "x", 0, false, new ClassDescriptor("y", EmptyList<ClassDescriptor>.Instance, 1, true));
            AssertParse("x<a,b[],c[][]>", "x", 0, false,
                new ClassDescriptor("a", EmptyList<ClassDescriptor>.Instance, 0, false),
                new ClassDescriptor("b", EmptyList<ClassDescriptor>.Instance, 1, false),
                new ClassDescriptor("c", EmptyList<ClassDescriptor>.Instance, 2, false));
            AssertParse("x<a<b>>", "x", 0, false,
                new ClassDescriptor("a", Collections.SingletonList(new ClassDescriptor("b")), 0, false));
            AssertParse("x<a<b<c>>>", "x", 0, false,
                new ClassDescriptor("a", Collections.SingletonList(new ClassDescriptor("b", Collections.SingletonList(new ClassDescriptor("c")), 0, false)), 0, false));

            TryInvalid("x[", "Failed to parse class identifier 'x[': Unexpected token END value '', expecting RIGHT_BRACKET");
            TryInvalid("String[][", "Failed to parse class identifier 'String[][': Unexpected token END value '', expecting RIGHT_BRACKET");
            TryInvalid("", "Failed to parse class identifier '': Empty class identifier");
            TryInvalid("<String", "Failed to parse class identifier '<String': Unexpected token LESSER_THAN value '<', expecting IDENTIFIER");
            TryInvalid("Abc<String", "Failed to parse class identifier 'Abc<String': Unexpected token END value '', expecting GREATER_THAN");
            TryInvalid("Abc<String,", "Failed to parse class identifier 'Abc<String,': Unexpected token END value '', expecting IDENTIFIER");
            TryInvalid("Abc<String,Integer", "Failed to parse class identifier 'Abc<String,Integer': Unexpected token END value '', expecting GREATER_THAN");
            TryInvalid("A<>", "Failed to parse class identifier 'A<>': Unexpected token GREATER_THAN value '>', expecting IDENTIFIER");
        }

        private void TryInvalid(string classIdentifier, string expected) {
            try {
                ClassDescriptor.ParseTypeText(classIdentifier);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(ex.Message, expected);
            }
        }
        
        private void AssertParse(
            string classIdentifier,
            string name,
            int dimensions,
            bool arrayOfPrimitive,
            params ClassDescriptor[] typeParams)
        {
            ClassDescriptor ident = ClassDescriptor.ParseTypeText(classIdentifier);
            ClassDescriptor expected = new ClassDescriptor(name, typeParams, dimensions, arrayOfPrimitive);
            Assert.AreEqual(expected, ident);
        }
    }
} // end of namespace
