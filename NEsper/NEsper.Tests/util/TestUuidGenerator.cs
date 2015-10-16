///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestUuidGenerator 
    {
        [Test]
        public void TestGenerate()
        {
            String uuid = UuidGenerator.Generate();
            Console.WriteLine(uuid + " length " + uuid.Length);

            String newuuid = Guid.NewGuid().ToString();
            Console.WriteLine(newuuid + " length " + newuuid.Length);
        }
    }
}
