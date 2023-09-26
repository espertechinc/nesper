///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestMethodExecutableRank : AbstractCommonTest
    {
        [Test]
        public void TestRank()
        {
            Assert.AreEqual(-1, "a".CompareTo("b"));
            Assert.AreEqual(1, "b".CompareTo("a"));

            MethodExecutableRank r1 = new MethodExecutableRank(1, false);
            Assert.AreEqual(1, r1.CompareTo(0, false));
            Assert.AreEqual(-1, r1.CompareTo(2, false));
            Assert.AreEqual(0, r1.CompareTo(1, false));
            Assert.AreEqual(-1, r1.CompareTo(1, true));

            MethodExecutableRank r2 = new MethodExecutableRank(0, true);
            Assert.AreEqual(1, r2.CompareTo(0, false));
            Assert.AreEqual(0, r2.CompareTo(0, true));
            Assert.AreEqual(-1, r2.CompareTo(1, false));
            Assert.AreEqual(-1, r2.CompareTo(1, true));

            SortedSet<MethodExecutableRank> ranks = new SortedSet<MethodExecutableRank>(
                new ProxyComparer<MethodExecutableRank> {
                    ProcCompare = (o1, o2) => o1.CompareTo(o2),
                });
            ranks.Add(new MethodExecutableRank(2, true));
            ranks.Add(new MethodExecutableRank(1, false));
            ranks.Add(new MethodExecutableRank(2, false));
            ranks.Add(new MethodExecutableRank(0, true));
            ranks.Add(new MethodExecutableRank(1, true));
            ranks.Add(new MethodExecutableRank(0, false));

            using (IEnumerator<MethodExecutableRank> enumerator = ranks.GetEnumerator()) {
                for (int i = 0; i < 6; i++) {
                    enumerator.MoveNext();
                    MethodExecutableRank rank = enumerator.Current;
                    Assert.AreEqual(i / 2, rank.ConversionCount, "failed for " + i);
                    Assert.AreEqual(i % 2 == 1, rank.IsVarargs, "failed for " + i);
                }
            }
        }
    }
} // end of namespace
