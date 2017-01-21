///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.plan
{
    [TestFixture]
    public class TestNestedIterationNode 
    {
        [Test]
        public void TestMakeExec()
        {
            try
            {
                new NestedIterationNode(new int[] {});
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
}
