///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestUuidGenerator : AbstractCommonTest
    {
        [Test]
        public void TestGenerate()
        {
            string uuid = UuidGenerator.Generate();
            Console.Out.WriteLine(uuid + " length " + uuid.Length);

            string newuuid = Guid.NewGuid().ToString();
            Console.Out.WriteLine(newuuid + " length " + newuuid.Length);
        }
    }
} // end of namespace
