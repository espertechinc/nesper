///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestCombinationEnumeration
    {
        [Test]
        public void TestEnumerate()
        {
            TryEnumerate("1A, 1B, 2A, 2B", new Object[][] { new Object[] {1, 2}, new Object[] {"A", "B"}});
            TryEnumerate("1AX, 1AY, 1BX, 1BY", new Object[][] { new Object[] {1}, new Object[] {"A", "B"}, new Object[] {"X", "Y"}});
            TryEnumerate("1A, 1B", new Object[][] { new Object[] {1}, new Object[] {"A", "B"}});
            TryEnumerate("1", new Object[][] { new Object[] {1}});
            TryEnumerate("", new Object[0][]);
            TryEnumerate("1A, 2A, 3A", new Object[][] { new Object[] {1, 2, 3}, new Object[] {"A"}});
            TryEnumerate("1AX, 1AY, 2AX, 2AY, 3AX, 3AY", new Object[][] { new Object[] {1, 2, 3}, new Object[] {"A"}, new Object[] {"X", "Y"}});
    
            try {
                new CombinationEnumeration(new Object[][] { new Object[] {1}, new Object[] {}});
                Assert.Fail();
            }
            catch(ArgumentException ex) {
                Assert.AreEqual("Expecting non-null element of minimum length 1", ex.Message);
            }
        }
    
        private void TryEnumerate(String expected, Object[][] objects)
        {
            var e = new CombinationEnumeration(objects);
    
            var results = new List<Object[]>();
            while (e.MoveNext())
            {
                var copy = new Object[objects.Length];
                var result = e.Current;
                Array.Copy(result, 0, copy, 0, result.Length);
                results.Add(copy);
            }
    
            Assert.IsFalse(e.MoveNext());
    
            var items = new List<String>();
            foreach (var result in results) {
                var writer = new StringWriter();
                foreach (var item in result) {
                    writer.Write(item.ToString());
                }
                items.Add(writer.ToString());
            }
    
            var resultStr = CollectionUtil.ToString(items);
            Assert.AreEqual(expected, resultStr);
        }
    }
}
