///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestCombinationEnumeration : AbstractTestBase
    {
        private void TryEnumerate(
            string expected,
            object[][] objects)
        {
            var e = new CombinationEnumeration(objects);

            IList<object[]> results = new List<object[]>();
            while (e.MoveNext())
            {
                var copy = new object[objects.Length];
                object[] result = e.Current;
                Array.Copy(result, 0, copy, 0, result.Length);
                results.Add(copy);
            }

            try
            {
                Assert.IsFalse(e.MoveNext());
                Assert.Fail();
            }
            catch (NoSuchElementException ex)
            {
                // expected
            }

            IList<string> items = new List<string>();
            foreach (var result in results)
            {
                var writer = new StringWriter();
                foreach (var item in result)
                {
                    writer.Write(item.ToString());
                }

                items.Add(writer.ToString());
            }

            var resultStr = CollectionUtil.ToString(items);
            Assert.AreEqual(expected, resultStr);
        }

        [Test]
        public void TestEnumerate()
        {
            TryEnumerate("1A, 1B, 2A, 2B", new[] { new object[] { 1, 2 }, new object[] { "A", "B" } });
            TryEnumerate("1AX, 1AY, 1BX, 1BY", new[] { new object[] { 1 }, new object[] { "A", "B" }, new object[] { "X", "Y" } });
            TryEnumerate("1A, 1B", new[] { new object[] { 1 }, new object[] { "A", "B" } });
            TryEnumerate("1", new[] { new object[] { 1 } });
            TryEnumerate("", new object[0][]);
            TryEnumerate("1A, 2A, 3A", new[] { new object[] { 1, 2, 3 }, new object[] { "A" } });
            TryEnumerate("1AX, 1AY, 2AX, 2AY, 3AX, 3AY", new[] { new object[] { 1, 2, 3 }, new object[] { "A" }, new object[] { "X", "Y" } });

            try
            {
                new CombinationEnumeration(new object[][] { new object[] { 1 }, new object[] { } });
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Expecting non-null element of minimum length 1", ex.Message);
            }
        }
    }
} // end of namespace
