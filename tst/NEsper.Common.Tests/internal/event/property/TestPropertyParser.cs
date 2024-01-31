///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.@event.property
{
    [TestFixture]
    public class TestPropertyParser : AbstractCommonTest
    {
        [Test]
        public void TestParse()
        {
            Property property = PropertyParser.ParseAndWalk("a", false);
            ClassicAssert.AreEqual("a", ((SimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("i[1]", false);
            ClassicAssert.AreEqual("i", ((IndexedProperty) property).PropertyNameAtomic);
            ClassicAssert.AreEqual(1, ((IndexedProperty) property).Index);

            property = PropertyParser.ParseAndWalk("m('key')", false);
            ClassicAssert.AreEqual("m", ((MappedProperty) property).PropertyNameAtomic);
            ClassicAssert.AreEqual("key", ((MappedProperty) property).Key);

            property = PropertyParser.ParseAndWalk("a.b[2].c('m')", false);
            IList<Property> nested = ((NestedProperty) property).Properties;
            ClassicAssert.AreEqual(3, nested.Count);
            ClassicAssert.AreEqual("a", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            ClassicAssert.AreEqual("b", ((IndexedProperty) nested[1]).PropertyNameAtomic);
            ClassicAssert.AreEqual(2, ((IndexedProperty) nested[1]).Index);
            ClassicAssert.AreEqual("c", ((MappedProperty) nested[2]).PropertyNameAtomic);
            ClassicAssert.AreEqual("m", ((MappedProperty) nested[2]).Key);

            property = PropertyParser.ParseAndWalk("a", true);
            ClassicAssert.AreEqual("a", ((DynamicSimpleProperty) property).PropertyNameAtomic);
        }

        [Test]
        public void TestParseMapKey()
        {
            ClassicAssert.AreEqual("a", TryKey("a"));
        }

        private string TryKey(string key)
        {
            string propertyName = "m(\"" + key + "\")";
            Log.Debug(".tryKey propertyName=" + propertyName + " key=" + key);
            Property property = PropertyParser.ParseAndWalk(propertyName, false);
            return ((MappedProperty) property).Key;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
