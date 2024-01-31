///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat.logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.@event.propertyparser.PropertyParserNoDep;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    [TestFixture]
    public class TestPropertyParserNoDep : AbstractCommonTest
    {
        [Test]
        public void TestParse()
        {
            Property property;
            IList<Property> nested;

            property = PropertyParser.ParseAndWalk("a", false);
            ClassicAssert.AreEqual("a", ((SimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("i[1]", false);
            ClassicAssert.AreEqual("i", ((IndexedProperty) property).PropertyNameAtomic);
            ClassicAssert.AreEqual(1, ((IndexedProperty) property).Index);

            property = PropertyParser.ParseAndWalk("m('key')", false);
            ClassicAssert.AreEqual("m", ((MappedProperty) property).PropertyNameAtomic);
            ClassicAssert.AreEqual("key", ((MappedProperty) property).Key);

            property = PropertyParser.ParseAndWalk("a.b[2].c('m')", false);
            nested = ((NestedProperty) property).Properties;
            ClassicAssert.AreEqual(3, nested.Count);
            ClassicAssert.AreEqual("a", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            ClassicAssert.AreEqual("b", ((IndexedProperty) nested[1]).PropertyNameAtomic);
            ClassicAssert.AreEqual(2, ((IndexedProperty) nested[1]).Index);
            ClassicAssert.AreEqual("c", ((MappedProperty) nested[2]).PropertyNameAtomic);
            ClassicAssert.AreEqual("m", ((MappedProperty) nested[2]).Key);

            property = PropertyParser.ParseAndWalk("a", true);
            ClassicAssert.AreEqual("a", ((DynamicSimpleProperty) property).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`Order`.p0", false);
            nested = ((NestedProperty) property).Properties;
            ClassicAssert.AreEqual(2, nested.Count);
            ClassicAssert.AreEqual("Order", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            ClassicAssert.AreEqual("p0", ((SimpleProperty) nested[1]).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`jim's strings`.p0", false);
            nested = ((NestedProperty) property).Properties;
            ClassicAssert.AreEqual(2, nested.Count);
            ClassicAssert.AreEqual("jim's strings", ((SimpleProperty) nested[0]).PropertyNameAtomic);
            ClassicAssert.AreEqual("p0", ((SimpleProperty) nested[1]).PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("`children's books`[0]", false);
            IndexedProperty indexed = (IndexedProperty) property;
            ClassicAssert.AreEqual(0, indexed.Index);
            ClassicAssert.AreEqual("children's books", indexed.PropertyNameAtomic);

            property = PropertyParser.ParseAndWalk("x\\.y", false);
            ClassicAssert.AreEqual("x.y", ((SimpleProperty) property).PropertyNameAtomic);
            property = PropertyParser.ParseAndWalk("x\\.\\.y", false);
            ClassicAssert.AreEqual("x..y", ((SimpleProperty) property).PropertyNameAtomic);
        }

        [Test]
        public void TestParseMapKey()
        {
            ClassicAssert.AreEqual("a", TryKey("a"));
        }

        [Test]
        public void TestParseMappedProp()
        {
            MappedPropertyParseResult result = ParseMappedProperty("a.b('c')");
            ClassicAssert.AreEqual("a", result.ClassName);
            ClassicAssert.AreEqual("b", result.MethodName);
            ClassicAssert.AreEqual("c", result.ArgString);

            result = ParseMappedProperty("SupportStaticMethodLib.DelimitPipe('POLYGON ((100.0 100, \", 100 100, 400 400))')");
            ClassicAssert.AreEqual("SupportStaticMethodLib", result.ClassName);
            ClassicAssert.AreEqual("DelimitPipe", result.MethodName);
            ClassicAssert.AreEqual("POLYGON ((100.0 100, \", 100 100, 400 400))", result.ArgString);

            result = ParseMappedProperty("a.b.c.d.e('f.g.h,u.h')");
            ClassicAssert.AreEqual("a.b.c.d", result.ClassName);
            ClassicAssert.AreEqual("e", result.MethodName);
            ClassicAssert.AreEqual("f.g.h,u.h", result.ArgString);

            result = ParseMappedProperty("a.b.c.d.E(\"hfhf f f f \")");
            ClassicAssert.AreEqual("a.b.c.d", result.ClassName);
            ClassicAssert.AreEqual("E", result.MethodName);
            ClassicAssert.AreEqual("hfhf f f f ", result.ArgString);

            result = ParseMappedProperty("c.d.getEnumerationSource(\"kf\"kf'kf\")");
            ClassicAssert.AreEqual("c.d", result.ClassName);
            ClassicAssert.AreEqual("getEnumerationSource", result.MethodName);
            ClassicAssert.AreEqual("kf\"kf'kf", result.ArgString);

            result = ParseMappedProperty("c.d.getEnumerationSource('kf\"kf'kf\"')");
            ClassicAssert.AreEqual("c.d", result.ClassName);
            ClassicAssert.AreEqual("getEnumerationSource", result.MethodName);
            ClassicAssert.AreEqual("kf\"kf'kf\"", result.ArgString);

            result = ParseMappedProperty("f('a')");
            ClassicAssert.AreEqual(null, result.ClassName);
            ClassicAssert.AreEqual("f", result.MethodName);
            ClassicAssert.AreEqual("a", result.ArgString);

            result = ParseMappedProperty("f('.')");
            ClassicAssert.AreEqual(null, result.ClassName);
            ClassicAssert.AreEqual("f",  result.MethodName);
            ClassicAssert.AreEqual(".", result.ArgString);

            ClassicAssert.IsNull(ParseMappedProperty("('a')"));
            ClassicAssert.IsNull(ParseMappedProperty(""));
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
