///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestIndent : AbstractCommonTest
    {
        [Test]
        public void SimpleIndentTest()
        {
            ClassicAssert.AreEqual("", Indent.CreateIndent(0));
            ClassicAssert.AreEqual(" ", Indent.CreateIndent(1));
            ClassicAssert.AreEqual("  ", Indent.CreateIndent(2));

            try
            {
                Indent.CreateIndent(-1);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
} // end of namespace
