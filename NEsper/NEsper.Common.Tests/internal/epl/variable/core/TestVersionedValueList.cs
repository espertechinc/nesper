///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    [TestFixture]
    public class TestVersionedValueList : AbstractTestBase
    {
        private VersionedValueList<string> list;

        [SetUp]
        public void SetUp()
        {
            list = new VersionedValueList<string>("abc", 2, "a", 1000, 10000, new MonitorLock(), 10, true);
        }

        [Test]
        public void TestFlowNoTime()
        {
            TryInvalid(0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("a", list.GetVersion(3));

            list.AddValue(4, "b", 0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("a", list.GetVersion(3));
            Assert.AreEqual("b", list.GetVersion(4));
            Assert.AreEqual("b", list.GetVersion(5));

            list.AddValue(6, "c", 0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("a", list.GetVersion(3));
            Assert.AreEqual("b", list.GetVersion(4));
            Assert.AreEqual("b", list.GetVersion(5));
            Assert.AreEqual("c", list.GetVersion(6));
            Assert.AreEqual("c", list.GetVersion(7));

            list.AddValue(7, "d", 0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("a", list.GetVersion(3));
            Assert.AreEqual("b", list.GetVersion(4));
            Assert.AreEqual("b", list.GetVersion(5));
            Assert.AreEqual("c", list.GetVersion(6));
            Assert.AreEqual("d", list.GetVersion(7));
            Assert.AreEqual("d", list.GetVersion(8));

            list.AddValue(9, "e", 0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("a", list.GetVersion(3));
            Assert.AreEqual("b", list.GetVersion(4));
            Assert.AreEqual("b", list.GetVersion(5));
            Assert.AreEqual("c", list.GetVersion(6));
            Assert.AreEqual("d", list.GetVersion(7));
            Assert.AreEqual("d", list.GetVersion(8));
            Assert.AreEqual("e", list.GetVersion(9));
            Assert.AreEqual("e", list.GetVersion(10));
        }

        [Test]
        public void TestHighWatermark()
        {
            list.AddValue(3, "b", 3000);
            list.AddValue(4, "c", 4000);
            list.AddValue(5, "d", 5000);
            list.AddValue(6, "e", 6000);
            list.AddValue(7, "f", 7000);
            list.AddValue(8, "g", 8000);
            list.AddValue(9, "h", 9000);
            list.AddValue(10, "i", 10000);
            list.AddValue(11, "j", 10500);
            list.AddValue(12, "k", 10600);
            Assert.AreEqual(9, list.OlderVersions.Count);

            TryInvalid(0);
            TryInvalid(1);
            Assert.AreEqual("a", list.GetVersion(2));
            Assert.AreEqual("b", list.GetVersion(3));
            Assert.AreEqual("c", list.GetVersion(4));
            Assert.AreEqual("d", list.GetVersion(5));
            Assert.AreEqual("e", list.GetVersion(6));
            Assert.AreEqual("f", list.GetVersion(7));
            Assert.AreEqual("g", list.GetVersion(8));
            Assert.AreEqual("k", list.GetVersion(12));
            Assert.AreEqual("k", list.GetVersion(13));

            list.AddValue(15, "x", 11000);  // 11th value added
            Assert.AreEqual(9, list.OlderVersions.Count);

            TryInvalid(0);
            TryInvalid(1);
            TryInvalid(2);
            Assert.AreEqual("b", list.GetVersion(3));
            Assert.AreEqual("c", list.GetVersion(4));
            Assert.AreEqual("d", list.GetVersion(5));
            Assert.AreEqual("k", list.GetVersion(13));
            Assert.AreEqual("k", list.GetVersion(14));
            Assert.AreEqual("x", list.GetVersion(15));

            // expire all before 5.5 sec
            list.AddValue(20, "y", 15500);  // 11th value added
            Assert.AreEqual(7, list.OlderVersions.Count);

            TryInvalid(0);
            TryInvalid(1);
            TryInvalid(2);
            TryInvalid(3);
            TryInvalid(4);
            TryInvalid(5);
            Assert.AreEqual("e", list.GetVersion(6));
            Assert.AreEqual("k", list.GetVersion(13));
            Assert.AreEqual("x", list.GetVersion(15));
            Assert.AreEqual("x", list.GetVersion(16));
            Assert.AreEqual("y", list.GetVersion(20));

            // expire all before 10.5 sec
            list.AddValue(21, "z1", 20500);
            list.AddValue(22, "z2", 20500);
            list.AddValue(23, "z3", 20501);
            Assert.AreEqual(4, list.OlderVersions.Count);
            TryInvalid(9);
            TryInvalid(10);
            TryInvalid(11);
            Assert.AreEqual("k", list.GetVersion(12));
            Assert.AreEqual("k", list.GetVersion(13));
            Assert.AreEqual("k", list.GetVersion(14));
            Assert.AreEqual("x", list.GetVersion(15));
            Assert.AreEqual("x", list.GetVersion(16));
            Assert.AreEqual("y", list.GetVersion(20));
            Assert.AreEqual("z1", list.GetVersion(21));
            Assert.AreEqual("z2", list.GetVersion(22));
            Assert.AreEqual("z3", list.GetVersion(23));
            Assert.AreEqual("z3", list.GetVersion(24));
        }

        private void TryInvalid(int version)
        {
            try
            {
                list.GetVersion(version);
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
            }
        }
    }
} // end of namespace