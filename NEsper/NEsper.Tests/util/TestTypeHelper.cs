///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using com.espertech.esper.supportunit.bean;
using NUnit.Framework;

using DataMap = System.Collections.Generic.IDictionary<string, object>;
using HashMap = System.Collections.Generic.Dictionary<string, object>;
using TreeMap = System.Collections.Generic.SortedDictionary<string, object>;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestTypeHelper
    {
        private static String TryInvalidGetRelational(Type classOne, Type classTwo)
        {
            try {
                TypeHelper.GetCompareToCoercionType(classOne, classTwo);
                Assert.Fail();
                return null;
            }
            catch (CoercionException ex) {
                return ex.Message;
            }
        }

        private static String TryInvalidGetCommonCoercionType(Type[] types)
        {
            try {
                TypeHelper.GetCommonCoercionType(types);
                Assert.Fail();
                return null;
            }
            catch (CoercionException ex) {
                return ex.Message;
            }
        }

        private class MyStringList : List<String>
        {
        }

        private class MyClassWithGetters
        {
            public IList<Object> GetListObject()
            {
                return null;
            }

            public ArrayList GetListUndefined()
            {
                return null;
            }

            public IList<String> GetList()
            {
                return null;
            }

            public IEnumerable<int?> GetIterable()
            {
                return null;
            }

            public ICollection<MyClassWithGetters> GetNested()
            {
                return null;
            }

            public int? GetIntBoxed()
            {
                return null;
            }

            public int GetIntPrimitive()
            {
                return 1;
            }

            public Hashtable GetMapUndefined()
            {
                return null;
            }

            public IDictionary<String, Object> GetMapObject()
            {
                return null;
            }

            public IDictionary<String, Boolean> GetMapBoolean()
            {
                return null;
            }

            public int? GetMapNotMap()
            {
                return null;
            }
        }

        private class MyClassWithFields
        {
#pragma warning disable CS0649
            public int? IntBoxed;
            public int IntPrimitive;
            public IEnumerable<int?> iterable;
            public IList<String> list;
            public IList<Object> listObject;
            public ArrayList listUndefined;

            public IDictionary<String, Boolean> mapBoolean;
            public int? mapNotMap;
            public IDictionary<String, Object> mapObject;
            public Hashtable mapUndefined;
            public ICollection<MyClassWithGetters> nested;
#pragma warning restore CS0649
        }

        [Test]
        public void TestCanCoerce()
        {
            Type[] primitiveClasses = {
                typeof(float), typeof(double), typeof(byte), typeof(short?), typeof(int), typeof(long)
            };

            Type[] boxedClasses = {
                typeof(float?), typeof(double?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?)
            };

            for (int i = 0; i < primitiveClasses.Length; i++) {
                Assert.IsTrue(TypeHelper.CanCoerce(primitiveClasses[i], boxedClasses[i]));
                Assert.IsTrue(TypeHelper.CanCoerce(boxedClasses[i], boxedClasses[i]));
                Assert.IsTrue(TypeHelper.CanCoerce(primitiveClasses[i], primitiveClasses[i]));
                Assert.IsTrue(TypeHelper.CanCoerce(boxedClasses[i], primitiveClasses[i]));
            }

            Assert.IsTrue(TypeHelper.CanCoerce(typeof(float), typeof(double?)));
            Assert.IsFalse(TypeHelper.CanCoerce(typeof(double), typeof(float)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(int), typeof(long)));
            Assert.IsFalse(TypeHelper.CanCoerce(typeof(long), typeof(int)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(long), typeof(double)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(int), typeof(double)));

            //Assert.IsTrue(TypeHelper.CanCoerce(typeof(BigInteger), typeof(BigInteger)));
            //Assert.IsTrue(TypeHelper.CanCoerce(typeof(long), typeof(BigInteger)));
            //Assert.IsTrue(TypeHelper.CanCoerce(typeof(int?), typeof(BigInteger)));
            //Assert.IsTrue(TypeHelper.CanCoerce(typeof(short), typeof(BigInteger)));

            Assert.IsTrue(TypeHelper.CanCoerce(typeof(float), typeof(decimal)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(double?), typeof(decimal)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(long), typeof(decimal)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(int?), typeof(decimal)));
            Assert.IsTrue(TypeHelper.CanCoerce(typeof(short?), typeof(decimal)));

            try {
                TypeHelper.CanCoerce(typeof(string), typeof(float?));
                Assert.Fail();
            }
            catch (CoercionException)
            {
                // expected
            }

            try {
                TypeHelper.CanCoerce(typeof(float?), typeof(bool?));
                Assert.Fail();
            }
            catch (CoercionException) {
                // expected
            }
        }

        [Test]
        public void TestClassForName()
        {
            var tests = new[]
            {
                new object[] {typeof(int), typeof(int).FullName},
                new object[] {typeof(long), typeof(long).FullName},
                new object[] {typeof(short), typeof(short).Name},
                new object[] {typeof(double), typeof(double).FullName},
                new object[] {typeof(float), typeof(float).FullName},
                new object[] {typeof(bool), typeof(bool).FullName},
                new object[] {typeof(byte), typeof(byte).FullName},
                new object[] {typeof(char), typeof(char).FullName}
            };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][0], TypeHelper.GetTypeForSimpleName((String) tests[i][1]));
            }
        }

        [Test]
        public void TestClassForSimpleName()
        {
            var tests = new[]
            {
                new object[] {"Boolean", typeof(bool)},
                new object[] {"Bool", typeof(bool)},
                new object[] {"boolean", typeof(bool)},
                new object[] {"System.Boolean", typeof(bool)},
                new object[] {"int", typeof(int)},
                new object[] {"inTeger", typeof(int)},
                new object[] {"System.Int32", typeof(int)},
                new object[] {"long", typeof(long)},
                new object[] {"LONG", typeof(long)},
                new object[] {"System.Int16", typeof(short)},
                new object[] {"short", typeof(short)},
                new object[] {"  short  ", typeof(short)},
                new object[] {"double", typeof(double)},
                new object[] {" douBle", typeof(double)},
                new object[] {"System.Double", typeof(double)},
                new object[] {"float", typeof(float)},
                new object[] {"float  ", typeof(float)},
                new object[] {"System.Single", typeof(float)},
                new object[] {"byte", typeof(byte)},
                new object[] {"   bYte ", typeof(byte)},
                new object[] {"System.Byte", typeof(byte)},
                new object[] {"char", typeof(char)},
                new object[] {"character", typeof(char)},
                new object[] {"System.Char", typeof(char)},
                new object[] {"String", typeof(string)},
                new object[] {"System.String", typeof(string)},
                new object[] {"varchar", typeof(string)},
                new object[] {"varchar2", typeof(string)},
                new object[] {typeof(SupportBean).FullName, typeof(SupportBean)},
            };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][1], TypeHelper.GetTypeForSimpleName((String) tests[i][0]), "error in row:" + i);
            }
        }

        [Test]
        public void TestCoerceBoxed()
        {
            Assert.AreEqual(1d, CoercerFactory.CoerceBoxed(1d, typeof(double?)));
            Assert.AreEqual(5d, CoercerFactory.CoerceBoxed(5, typeof(double?)));
            Assert.AreEqual(6d, CoercerFactory.CoerceBoxed((byte) 6, typeof(double?)));
            Assert.AreEqual(3f, CoercerFactory.CoerceBoxed((long) 3, typeof(float?)));
            Assert.AreEqual((short) 2, CoercerFactory.CoerceBoxed((long) 2, typeof(short?)));
            Assert.AreEqual(4, CoercerFactory.CoerceBoxed((long) 4, typeof(int?)));
            Assert.AreEqual((byte) 5, CoercerFactory.CoerceBoxed((long) 5, typeof(Byte)));
            Assert.AreEqual(8L, CoercerFactory.CoerceBoxed((long) 8, typeof(long?)));
            //Assert.AreEqual(BigInteger.ValueOf(8), CoercerFactory.CoerceBoxed(8, typeof(BigInteger)));
            Assert.AreEqual(8.0m, CoercerFactory.CoerceBoxed(8, typeof(decimal)));
            Assert.AreEqual(8.0m, CoercerFactory.CoerceBoxed(8d, typeof(decimal)));
        }

        [Test]
        public void TestGetArithmaticCoercionType()
        {
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(double?), typeof(int)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(double)));
            Assert.AreEqual(typeof(long?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(long)));
            Assert.AreEqual(typeof(long?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(long)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(float), typeof(long)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(float)));
            Assert.AreEqual(typeof(int?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(int)));
            Assert.AreEqual(typeof(int?), TypeHelper.GetArithmaticCoercionType(typeof(int?), typeof(int)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetArithmaticCoercionType(typeof(int?), typeof(decimal)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetArithmaticCoercionType(typeof(decimal?), typeof(int?)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(int), typeof(float)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(float), typeof(long)));

            try {
                TypeHelper.GetArithmaticCoercionType(typeof(string), typeof(float));
                Assert.Fail();
            }
            catch (CoercionException) {
                // Expected
            }

            try {
                TypeHelper.GetArithmaticCoercionType(typeof(int), typeof(bool));
                Assert.Fail();
            }
            catch (CoercionException) {
                // Expected
            }
        }

        [Test]
        public void TestGetBoxed()
        {
            Type[] primitiveClasses = {
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(byte),
                typeof(short?),
                typeof(int),
                typeof(long),
                typeof(sbyte),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(char)
            };

            Type[] boxedClasses = {
                typeof(bool?),
                typeof(float?),
                typeof(double?),
                typeof(decimal?),
                typeof(byte?),
                typeof(short?),
                typeof(int?),
                typeof(long?),
                typeof(sbyte?),
                typeof(ushort?),
                typeof(uint?),
                typeof(ulong?),
                typeof(char?)
            };

            Type[] otherClasses = {
                typeof(string), typeof(EventArgs)
            };

            for (int i = 0; i < primitiveClasses.Length; i++) {
                Type boxed = TypeHelper.GetBoxedType(primitiveClasses[i]);
                Assert.AreEqual(boxed, boxedClasses[i]);
            }

            for (int i = 0; i < boxedClasses.Length; i++) {
                Type boxed = TypeHelper.GetBoxedType(boxedClasses[i]);
                Assert.AreEqual(boxed, boxedClasses[i]);
            }

            for (int i = 0; i < otherClasses.Length; i++) {
                Type boxed = TypeHelper.GetBoxedType(otherClasses[i]);
                Assert.AreEqual(boxed, otherClasses[i]);
            }
        }

        [Test]
        public void TestGetBoxedClassName()
        {
            var tests = new[]
            {
                new[] {typeof(int?).FullName, typeof(int).FullName},
                new[] {typeof(long?).FullName, typeof(long).FullName},
                new[] {typeof(short?).FullName, typeof(short).FullName},
                new[] {typeof(double?).FullName, typeof(double).FullName},
                new[] {typeof(float?).FullName, typeof(float).FullName},
                new[] {typeof(bool?).FullName, typeof(bool).FullName},
                new[] {typeof(byte?).FullName, typeof(byte).FullName},
                new[] {typeof(char?).FullName, typeof(char).FullName}
            };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][0], TypeHelper.GetBoxedTypeName(tests[i][1]));
            }
        }

        [Test]
        public void TestGetCommonCoercionType()
        {
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {typeof(string)}));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool)}));
            Assert.AreEqual(typeof(long?), TypeHelper.GetCommonCoercionType(new[] {typeof(long)}));

            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {typeof(string), null}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {typeof(string), typeof(string)}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {typeof(string), typeof(string), typeof(string)}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {typeof(string), typeof(string), null}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {null, typeof(string), null}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {null, typeof(string), typeof(string)}));
            Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new[] {null, null, typeof(string), typeof(string)}));

            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool?), typeof(bool?)}));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool?), typeof(bool)}));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool), typeof(bool?)}));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool), typeof(bool)}));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new[] {typeof(bool?), typeof(bool), typeof(bool)}));
            Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new[] {typeof(int), typeof(byte), typeof(int)}));
            Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new[] {typeof(int?), typeof(Byte), typeof(short?)}));
            Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new[] {typeof(byte), typeof(short?), typeof(short?)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(int?), typeof(Byte), typeof(double?)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(long?), typeof(double?), typeof(double?)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(double), typeof(byte)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(double), typeof(byte), null}));
            Assert.AreEqual(typeof(float?), TypeHelper.GetCommonCoercionType(new[] {typeof(float), typeof(float)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(float), typeof(int)}));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new[] {typeof(int?), typeof(int), typeof(float?)}));
            Assert.AreEqual(typeof(long?), TypeHelper.GetCommonCoercionType(new[] {typeof(int?), typeof(int), typeof(long)}));
            Assert.AreEqual(typeof(long?), TypeHelper.GetCommonCoercionType(new[] {typeof(long), typeof(int)}));
            Assert.AreEqual(typeof(long?), TypeHelper.GetCommonCoercionType(new[]{typeof(long), typeof(int), typeof(int), typeof(int), typeof(byte), typeof(short?)}));
            Assert.AreEqual(typeof(long?),
                            TypeHelper.GetCommonCoercionType(new[]
                                                             {
                                                                 typeof(long), null, typeof(int), null, typeof(int),
                                                                 typeof(int), null, typeof(byte), typeof(short?)
                                                             }));
            Assert.AreEqual(typeof(long?),
                            TypeHelper.GetCommonCoercionType(new[] {typeof(int?), typeof(int), typeof(long)}));
            Assert.AreEqual(typeof(char?),
                            TypeHelper.GetCommonCoercionType(new[] {typeof(char), typeof(char), typeof(char)}));
            Assert.AreEqual(typeof(long?),
                            TypeHelper.GetCommonCoercionType(new[] { typeof(int), typeof(int), typeof(int), typeof(long), typeof(int), typeof(int) }));
            Assert.AreEqual(typeof(double?),
                            TypeHelper.GetCommonCoercionType(new[] { typeof(int), typeof(long), typeof(int), typeof(double) , typeof(int), typeof(int) }));
            Assert.AreEqual(null, TypeHelper.GetCommonCoercionType(new Type[] {null, null}));
            Assert.AreEqual(null, TypeHelper.GetCommonCoercionType(new Type[] {null, null, null}));
            Assert.AreEqual(typeof(SupportBean),
                            TypeHelper.GetCommonCoercionType(new[] {typeof(SupportBean), null, null}));
            Assert.AreEqual(typeof(SupportBean),
                            TypeHelper.GetCommonCoercionType(new[] {null, typeof(SupportBean), null}));
            Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new[] {null, typeof(SupportBean)}));
            Assert.AreEqual(typeof(SupportBean),
                            TypeHelper.GetCommonCoercionType(new[] {null, null, typeof(SupportBean)}));
            Assert.AreEqual(typeof(SupportBean),
                            TypeHelper.GetCommonCoercionType(new[]
                                                             {
                                                                 typeof(SupportBean), null, typeof(SupportBean),
                                                                 typeof(SupportBean)
                                                             }));
            Assert.AreEqual(typeof(Object),
                            TypeHelper.GetCommonCoercionType(new[]
                                                             {
                                                                 typeof(SupportBean), typeof(SupportBean_A), null,
                                                                 typeof(SupportBean), typeof(SupportBean)
                                                             }));

            Assert.AreEqual("Cannot coerce to String type " + typeof(bool?).GetCleanName(),
                            TryInvalidGetCommonCoercionType(new[] {typeof(string), typeof(bool?)}));
            TryInvalidGetCommonCoercionType(new[] {typeof(string), typeof(string), typeof(bool?)});
            TryInvalidGetCommonCoercionType(new[] {typeof(bool?), typeof(string), typeof(bool?)});
            TryInvalidGetCommonCoercionType(new[] {typeof(bool?), typeof(bool?), typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {typeof(long), typeof(bool?), typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {typeof(double), typeof(long), typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {null, typeof(double), typeof(long), typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {typeof(string), typeof(string), typeof(long)});
            TryInvalidGetCommonCoercionType(new[] {typeof(string), typeof(SupportBean)});
            TryInvalidGetCommonCoercionType(new[] {typeof(bool), null, null, typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {typeof(int), null, null, typeof(string)});
            TryInvalidGetCommonCoercionType(new[] {typeof(SupportBean), typeof(bool?)});
            TryInvalidGetCommonCoercionType(new[] {typeof(string), typeof(SupportBean)});
            TryInvalidGetCommonCoercionType(new[] {typeof(SupportBean), typeof(string), typeof(SupportBean)});

            try {
                TypeHelper.GetCommonCoercionType(new Type[0]);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // expected
            }
        }

        [Test]
        public void TestGetCompareToCoercionType()
        {
            Assert.AreEqual(typeof(string), TypeHelper.GetCompareToCoercionType(typeof(string), typeof(string)));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool?), typeof(bool?)));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool?), typeof(bool)));
            Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool), typeof(bool?)));
            Assert.AreEqual(typeof(bool), TypeHelper.GetCompareToCoercionType(typeof(bool), typeof(bool)));

            Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(int), typeof(float)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(double), typeof(byte)));
            Assert.AreEqual(typeof(float), TypeHelper.GetCompareToCoercionType(typeof(float), typeof(float)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(float), typeof(double?)));

            Assert.AreEqual(typeof(int), TypeHelper.GetCompareToCoercionType(typeof(int), typeof(int)));
            Assert.AreEqual(typeof(int?), TypeHelper.GetCompareToCoercionType(typeof(short?), typeof(int?)));

            Assert.AreEqual(typeof(decimal?), TypeHelper.GetCompareToCoercionType(typeof(decimal?), typeof(int)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetCompareToCoercionType(typeof(double?), typeof(decimal?)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetCompareToCoercionType(typeof(byte), typeof(decimal?)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetCompareToCoercionType(typeof(long?), typeof(decimal?)));
            Assert.AreEqual(typeof(decimal?), TypeHelper.GetCompareToCoercionType(typeof(decimal?), typeof(decimal?)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(double), typeof(long)));
            Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(float?), typeof(long)));

            Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCompareToCoercionType(typeof(SupportBean), typeof(SupportBean)));
            Assert.AreEqual(typeof(Object), TypeHelper.GetCompareToCoercionType(typeof(SupportBean), typeof(SupportBean_A)));

            Assert.AreEqual("Types cannot be compared: System.Boolean and System.Int32",
                            TryInvalidGetRelational(typeof(bool), typeof(int)));
            TryInvalidGetRelational(typeof(string), typeof(decimal?));
            TryInvalidGetRelational(typeof(string), typeof(int));
            TryInvalidGetRelational(typeof(long?), typeof(string));
            TryInvalidGetRelational(typeof(long?), typeof(bool?));
            TryInvalidGetRelational(typeof(bool), typeof(int));
        }

        [Test]
        public void TestGetGenericFieldType()
        {
            var testcases = new[]
                            {
                                new object[] {"list", typeof(string)},
                                new object[] {"listObject", typeof(Object)},
                                new object[] {"listUndefined", null},
                                new object[] {"iterable", typeof(int?)},
                                new object[] {"nested", typeof(MyClassWithGetters)},
                                new object[] {"IntPrimitive", null},
                                new object[] {"IntBoxed", null},
                            };

            for (int i = 0; i < testcases.Length; i++) {
                var name = testcases[i][0].ToString();
                var f = typeof(MyClassWithFields).GetField(name);
                var expected = (Type) testcases[i][1];
                Assert.AreEqual(expected, TypeHelper.GetGenericFieldType(f, true), "Testing " + name);
            }
        }

        [Test]
        public void TestGetGenericFieldTypeMap()
        {
            var testcases = new[]
                            {
                                new object[] {"mapUndefined", null},
                                new object[] {"mapObject", typeof(Object)},
                                new object[] {"mapBoolean", typeof(bool)},
                                new object[] {"mapNotMap", null},
                            };

            for (int i = 0; i < testcases.Length; i++) {
                String name = testcases[i][0].ToString();
                FieldInfo f = typeof(MyClassWithFields).GetField(name);
                var expected = (Type) testcases[i][1];
                Assert.AreEqual(expected, TypeHelper.GetGenericFieldTypeMap(f, true), "Testing " + name);
            }
        }

        [Test]
        public void TestGetGenericReturnType()
        {
            var testcases = new[]
                            {
                                new object[] {"GetList", typeof(string)},
                                new object[] {"GetListObject", typeof(Object)},
                                new object[] {"GetListUndefined", null},
                                new object[] {"GetIterable", typeof(int?)},
                                new object[] {"GetNested", typeof(MyClassWithGetters)},
                                new object[] {"GetIntPrimitive", null},
                                new object[] {"GetIntBoxed", null},
                            };

            for (int i = 0; i < testcases.Length; i++) {
                String name = testcases[i][0].ToString();
                MethodInfo m = typeof(MyClassWithGetters).GetMethod(name);
                var expected = (Type) testcases[i][1];
                Assert.AreEqual(expected, TypeHelper.GetGenericReturnType(m, true), "Testing " + name);
            }
        }

        [Test]
        public void TestGetGenericReturnTypeMap()
        {
            var testcases = new[]
                            {
                                new object[] {"GetMapUndefined", null},
                                new object[] {"GetMapObject", typeof(Object)},
                                new object[] {"GetMapBoolean", typeof(bool)},
                                new object[] {"GetMapNotMap", null},
                            };

            for (int i = 0; i < testcases.Length; i++) {
                String name = testcases[i][0].ToString();
                MethodInfo m = typeof(MyClassWithGetters).GetMethod(name);
                var expected = (Type) testcases[i][1];
                Assert.AreEqual(expected, TypeHelper.GetGenericReturnTypeMap(m, true), "Testing " + name);
            }
        }

        [Test]
        public void TestGetParameterAsString()
        {
            var testCases = new[]
                            {
                                new object[] {new[] {typeof(string), typeof(int)}, "System.String, System.Int32"},
                                new object[] {new[] {typeof(int?), typeof(bool?)}, "System.Nullable<System.Int32>, System.Nullable<System.Boolean>"},
                                new object[] {new Type[] {}, ""},
                                new object[] {new Type[] {null}, "null (any type)"},
                                new object[] {new[] {typeof(byte), null}, "System.Byte, null (any type)"},
                                new object[]
                                {
                                    new[] {typeof(SupportBean), typeof(int[]), typeof(int[][]), typeof(DataMap)},
                                    "com.espertech.esper.supportunit.bean.SupportBean, " +
                                    "System.Int32[], " +
                                    "System.Int32[][], " +
                                    "System.Collections.Generic.IDictionary<System.String, System.Object>"
                                },
                                new object[]
                                {
                                    new[]
                                    {
                                        typeof(SupportBean[]), typeof(SupportEnum),
                                        typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested)
                                    },
                                    "com.espertech.esper.supportunit.bean.SupportBean[], " +
                                    "com.espertech.esper.supportunit.bean.SupportEnum, " +
                                    "com.espertech.esper.supportunit.bean.SupportBeanComplexProps+SupportBeanSpecialGetterNested"
                                },
                            };

            for (int i = 0; i < testCases.Length; i++) {
                var paramList = (Type[]) testCases[i][0];
                Assert.AreEqual(testCases[i][1], TypeHelper.GetParameterAsString(paramList, true));
            }
        }

        [Test]
        public void TestGetParser()
        {
            var tests = new[]
                        {
                            new object[] {typeof(bool?), "TrUe", true},
                            new object[] {typeof(bool?), "false", false},
                            new object[] {typeof(bool), "false", false},
                            new object[] {typeof(bool), "true", true},
                            new object[] {typeof(int), "73737474 ", 73737474},
                            new object[] {typeof(int?), " -1 ", -1},
                            new object[] {typeof(long), "123456789001222L", 123456789001222L},
                            new object[] {typeof(long?), " -2 ", -2L},
                            new object[] {typeof(long?), " -2L ", -2L},
                            new object[] {typeof(long?), " -2l ", -2L},
                            new object[] {typeof(short?), " -3 ", (short) -3},
                            new object[] {typeof(short?), "111", (short) 111},
                            new object[] {typeof(double?), " -3d ", -3d},
                            new object[] {typeof(double), "111.38373", 111.38373d},
                            new object[] {typeof(double?), " -3.1D ", -3.1D},
                            new object[] {typeof(float?), " -3f ", -3f},
                            new object[] {typeof(float), "111.38373", 111.38373f},
                            new object[] {typeof(float?), " -3.1F ", -3.1f},
                            new object[] {typeof(sbyte?), " -3 ", (sbyte) -3},
                            new object[] {typeof(byte), " 1 ", (byte) 1},
                            new object[] {typeof(char), "ABC", 'A'},
                            new object[] {typeof(char?), " AB", ' '},
                            new object[] {typeof(string), "AB", "AB"},
                            new object[] {typeof(string), " AB ", " AB "},
                        };

            for (int i = 0; i < tests.Length; i++) {
                SimpleTypeParser parser = SimpleTypeParserFactory.GetParser((Type) tests[i][0]);
                Assert.AreEqual(tests[i][2], parser.Invoke((String) tests[i][1]), "error in row:" + i);
            }
        }

        [Test]
        public void TestGetPrimitive()
        {
            Type[] primitiveClasses = {
                                          typeof(bool),
                                          typeof(float),
                                          typeof(double),
                                          typeof(decimal),
                                          typeof(byte),
                                          typeof(short),
                                          typeof(int),
                                          typeof(long),
                                          typeof(sbyte),
                                          typeof(ushort),
                                          typeof(uint),
                                          typeof(ulong),
                                          typeof(char)
                                      };

            Type[] boxedClasses = {
                                      typeof(bool?),
                                      typeof(float?),
                                      typeof(double?),
                                      typeof(decimal?),
                                      typeof(byte?),
                                      typeof(short?),
                                      typeof(int?),
                                      typeof(long?),
                                      typeof(sbyte?),
                                      typeof(ushort?),
                                      typeof(uint?),
                                      typeof(ulong?),
                                      typeof(char?)
                                  };

            Type[] otherClasses = {
                                      typeof(string), typeof(EventArgs)
                                  };

            for (int i = 0; i < primitiveClasses.Length; i++) {
                Type primitive = TypeHelper.GetPrimitiveType(boxedClasses[i]);
                Assert.AreEqual(primitive, primitiveClasses[i]);
            }

            for (int i = 0; i < boxedClasses.Length; i++) {
                Type primitive = TypeHelper.GetPrimitiveType(primitiveClasses[i]);
                Assert.AreEqual(primitive, primitiveClasses[i]);
            }

            for (int i = 0; i < otherClasses.Length; i++) {
                Type clazz = TypeHelper.GetPrimitiveType(otherClasses[i]);
                Assert.AreEqual(clazz, otherClasses[i]);
            }
        }

        [Test]
        public void TestGetPrimitiveClassForName()
        {
            var tests = new[]
                        {
                            new object[] {"int", typeof(int)},
                            new object[] {"Long", typeof(long)},
                            new object[] {"SHort", typeof(short)},
                            new object[] {"DOUBLE", typeof(double)},
                            new object[] {"float", typeof(float)},
                            new object[] {"boolean", typeof(bool)},
                            new object[] {"ByTe", typeof(byte)},
                            new object[] {"char", typeof(char)},
                            new object[] {"jfjfjf", null},
                            new object[] {typeof(SupportBean).FullName, null},
                            new object[] {"String", typeof(string)},
                            new object[] {"STRINg", typeof(string)}
                        };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][1], TypeHelper.GetPrimitiveTypeForName((String) tests[i][0]));
            }
        }

        [Test]
        public void TestImplementsInterface()
        {
            var tests = new[]
                        {
                            new object[] {typeof(HashMap), typeof(DataMap), true},
                            new object[] {typeof(TreeMap), typeof(DataMap), true},
                            new object[] {typeof(string), typeof(DataMap), false},
                            new object[] {typeof(SupportBean_S0), typeof(SupportMarkerInterface), false},
                            new object[] {typeof(SupportBean_E), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportBean_F), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportBeanBase), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportOverrideOneB), typeof(SupportMarkerInterface), true}
                        };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][2],
                                TypeHelper.IsImplementsInterface((Type) tests[i][0], (Type) tests[i][1]),
                                "test failed for " + tests[i][0]);
            }
        }

        [Test]
        public void TestImplementsOrExtends()
        {
            var tests = new[]
                        {
                            new object[] {typeof(HashMap), typeof(DataMap), true},
                            new object[] {typeof(TreeMap), typeof(DataMap), true},
                            new object[] {typeof(string), typeof(DataMap), false},
                            new object[] {typeof(SupportBean_S0), typeof(SupportMarkerInterface), false},
                            new object[] {typeof(SupportBean_E), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportBean_F), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportBeanBase), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportOverrideOneB), typeof(SupportMarkerInterface), true},
                            new object[] {typeof(SupportOverrideBase), typeof(SupportOverrideBase), true},
                            new object[] {typeof(SupportBean_F), typeof(SupportOverrideBase), false},
                            new object[] {typeof(SupportOverrideOne), typeof(SupportOverrideBase), true},
                            new object[] {typeof(SupportOverrideOneA), typeof(SupportOverrideBase), true},
                            new object[] {typeof(SupportOverrideOneB), typeof(SupportOverrideBase), true},
                            new object[] {typeof(SupportOverrideOneB), typeof(ISerializable), false},
                            new object[] {typeof(SupportOverrideOneB), typeof(string), false},
                        };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][2],
                                TypeHelper.IsSubclassOrImplementsInterface((Type) tests[i][0], (Type) tests[i][1]),
                                "test failed for " + tests[i][0] + " and " + tests[i][1]);
            }
        }

        [Test]
        public void TestIsAssignmentCompatible()
        {
            var successCases = new[] {
                new[] {typeof(bool), typeof(bool?)},

                new[] {typeof(Byte), typeof(Byte)},
                new[] {typeof(Byte), typeof(Char)},
                new[] {typeof(Byte), typeof(Double)},
                new[] {typeof(Byte), typeof(Int16)},
                new[] {typeof(Byte), typeof(Int32)},
                new[] {typeof(Byte), typeof(Int64)},
                new[] {typeof(Byte), typeof(Byte?)},
                new[] {typeof(Byte), typeof(Single)},
                new[] {typeof(Byte), typeof(UInt16)},
                new[] {typeof(Byte), typeof(UInt32)},
                new[] {typeof(Byte), typeof(UInt64)},
                new[] {typeof(Decimal), typeof(Decimal)},
                new[] {typeof(Decimal), typeof(Decimal?)},
                new[] {typeof(Double), typeof(Double)},
                new[] {typeof(Double), typeof(Double?) },
                new[] {typeof(Int16), typeof(Double)},
                new[] {typeof(Int16), typeof(Int16)},
                new[] {typeof(Int16), typeof(Int32)},
                new[] {typeof(Int16), typeof(Int64)},
                new[] {typeof(Int16), typeof(Int16?)},
                new[] {typeof(Int16), typeof(Single)},
                new[] {typeof(Int32), typeof(Double)},
                new[] {typeof(Int32), typeof(Int32)},
                new[] {typeof(Int32), typeof(Int64)},
                new[] {typeof(Int32), typeof(Int32?)},
                new[] {typeof(Int32), typeof(Single)},
                new[] {typeof(Int64), typeof(Double)},
                new[] {typeof(Int64), typeof(Int64)},
                new[] {typeof(Int64), typeof(Int64?)},
                new[] {typeof(Int64), typeof(Single)},
                new[] {typeof(Byte?), typeof(Byte)},
                new[] {typeof(Byte?), typeof(Char)},
                new[] {typeof(Byte?), typeof(Double)},
                new[] {typeof(Byte?), typeof(Int16)},
                new[] {typeof(Byte?), typeof(Int32)},
                new[] {typeof(Byte?), typeof(Int64)},
                new[] {typeof(Byte?), typeof(Byte?)},
                new[] {typeof(Byte?), typeof(Single)},
                new[] {typeof(Byte?), typeof(UInt16)},
                new[] {typeof(Byte?), typeof(UInt32)},
                new[] {typeof(Byte?), typeof(UInt64)},
                new[] {typeof(Decimal?), typeof(Decimal)},
                new[] {typeof(Decimal?), typeof(Decimal?)},
                new[] {typeof(Double?), typeof(Double)},
                new[] {typeof(Double?), typeof(Double?)},
                new[] {typeof(Int16?), typeof(Double)},
                new[] {typeof(Int16?), typeof(Int16)},
                new[] {typeof(Int16?), typeof(Int32)},
                new[] {typeof(Int16?), typeof(Int64)},
                new[] {typeof(Int16?), typeof(Int16?)},
                new[] {typeof(Int16?), typeof(Single)},
                new[] {typeof(Int32?), typeof(Double)},
                new[] {typeof(Int32?), typeof(Int32)},
                new[] {typeof(Int32?), typeof(Int64)},
                new[] {typeof(Int32?), typeof(Int32?)},
                new[] {typeof(Int32?), typeof(Single)},
                new[] {typeof(Int64?), typeof(Double)},
                new[] {typeof(Int64?), typeof(Int64)},
                new[] {typeof(Int64?), typeof(Int64?)},
                new[] {typeof(Int64?), typeof(Single)},
                new[] {typeof(SByte?), typeof(Double)},
                new[] {typeof(SByte?), typeof(Int16)},
                new[] {typeof(SByte?), typeof(Int32)},
                new[] {typeof(SByte?), typeof(Int64)},
                new[] {typeof(SByte?), typeof(SByte?)},
                new[] {typeof(SByte?), typeof(SByte)},
                new[] {typeof(SByte?), typeof(Single)},
                new[] {typeof(Single?), typeof(Double)},
                new[] {typeof(Single?), typeof(Single?)},
                new[] {typeof(Single?), typeof(Single)},
                new[] {typeof(UInt16?), typeof(Char)},
                new[] {typeof(UInt16?), typeof(Double)},
                new[] {typeof(UInt16?), typeof(Int32)},
                new[] {typeof(UInt16?), typeof(Int64)},
                new[] {typeof(UInt16?), typeof(UInt16?)},
                new[] {typeof(UInt16?), typeof(Single)},
                new[] {typeof(UInt16?), typeof(UInt16)},
                new[] {typeof(UInt16?), typeof(UInt32)},
                new[] {typeof(UInt16?), typeof(UInt64)},
                new[] {typeof(UInt32?), typeof(Double)},
                new[] {typeof(UInt32?), typeof(Int64)},
                new[] {typeof(UInt32?), typeof(UInt32?)},
                new[] {typeof(UInt32?), typeof(Single)},
                new[] {typeof(UInt32?), typeof(UInt32)},
                new[] {typeof(UInt32?), typeof(UInt64)},
                new[] {typeof(UInt64?), typeof(Double)},
                new[] {typeof(UInt64?), typeof(UInt64?)},
                new[] {typeof(UInt64?), typeof(Single)},
                new[] {typeof(UInt64?), typeof(UInt64)},
                new[] {typeof(SByte), typeof(Double)},
                new[] {typeof(SByte), typeof(Int16)},
                new[] {typeof(SByte), typeof(Int32)},
                new[] {typeof(SByte), typeof(Int64)},
                new[] {typeof(SByte), typeof(SByte?)},
                new[] {typeof(SByte), typeof(SByte)},
                new[] {typeof(SByte), typeof(Single)},
                new[] {typeof(Single), typeof(Double)},
                new[] {typeof(Single), typeof(Single?)},
                new[] {typeof(Single), typeof(Single)},
                new[] {typeof(UInt16), typeof(Char)},
                new[] {typeof(UInt16), typeof(Double)},
                new[] {typeof(UInt16), typeof(Int32)},
                new[] {typeof(UInt16), typeof(Int64)},
                new[] {typeof(UInt16), typeof(UInt16?)},
                new[] {typeof(UInt16), typeof(Single)},
                new[] {typeof(UInt16), typeof(UInt16)},
                new[] {typeof(UInt16), typeof(UInt32)},
                new[] {typeof(UInt16), typeof(UInt64)},
                new[] {typeof(UInt32), typeof(Double)},
                new[] {typeof(UInt32), typeof(Int64)},
                new[] {typeof(UInt32), typeof(UInt32?)},
                new[] {typeof(UInt32), typeof(Single)},
                new[] {typeof(UInt32), typeof(UInt32)},
                new[] {typeof(UInt32), typeof(UInt64)},
                new[] {typeof(UInt64), typeof(Double)},
                new[] {typeof(UInt64), typeof(UInt64?)},
                new[] {typeof(UInt64), typeof(Single)},
                new[] {typeof(UInt64), typeof(UInt64)},

                new[] {typeof(HashSet<object>), typeof(ICollection<object>)},
                new[] {typeof(HashSet<object>), typeof(IEnumerable<object>)},
                new[] {typeof(SortedSet<object>), typeof(ICollection<object>)},
                new[] {typeof(ArrayList), typeof(Object)},

                // widening of arrays allowed if supertype
                new[] {typeof(ISupportAImplSuperG), typeof(ISupportA)},
                new[] {typeof(ISupportAImplSuperGImpl), typeof(ISupportA)},
                new[] {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportA)},
                new[] {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportB)},
                new[] {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportC)},
                new[] {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportAImplSuperG)},
                new[] {null, typeof(Object)}
            };

            var failCases = new[] {
                new[] {typeof(Byte), typeof(Decimal)},
                new[] {typeof(Byte), typeof(Char?)},
                new[] {typeof(Byte), typeof(Decimal?)},
                new[] {typeof(Byte), typeof(Double?)},
                new[] {typeof(Byte), typeof(Int16?)},
                new[] {typeof(Byte), typeof(Int32?)},
                new[] {typeof(Byte), typeof(Int64?)},
                new[] {typeof(Byte), typeof(SByte?)},
                new[] {typeof(Byte), typeof(Single?)},
                new[] {typeof(Byte), typeof(UInt16?)},
                new[] {typeof(Byte), typeof(UInt32?)},
                new[] {typeof(Byte), typeof(UInt64?)},
                new[] {typeof(Byte), typeof(SByte)},
                new[] {typeof(Decimal), typeof(Byte)},
                new[] {typeof(Decimal), typeof(Char)},
                new[] {typeof(Decimal), typeof(Double)},
                new[] {typeof(Decimal), typeof(Int16)},
                new[] {typeof(Decimal), typeof(Int32)},
                new[] {typeof(Decimal), typeof(Int64)},
                new[] {typeof(Decimal), typeof(Byte?)},
                new[] {typeof(Decimal), typeof(Char?)},
                new[] {typeof(Decimal), typeof(Double?)},
                new[] {typeof(Decimal), typeof(Int16?)},
                new[] {typeof(Decimal), typeof(Int32?)},
                new[] {typeof(Decimal), typeof(Int64?)},
                new[] {typeof(Decimal), typeof(SByte?)},
                new[] {typeof(Decimal), typeof(Single?)},
                new[] {typeof(Decimal), typeof(UInt16?)},
                new[] {typeof(Decimal), typeof(UInt32?)},
                new[] {typeof(Decimal), typeof(UInt64?)},
                new[] {typeof(Decimal), typeof(SByte)},
                new[] {typeof(Decimal), typeof(Single)},
                new[] {typeof(Decimal), typeof(UInt16)},
                new[] {typeof(Decimal), typeof(UInt32)},
                new[] {typeof(Decimal), typeof(UInt64)},
                new[] {typeof(Double), typeof(Byte)},
                new[] {typeof(Double), typeof(Char)},
                new[] {typeof(Double), typeof(Decimal)},
                new[] {typeof(Double), typeof(Int16)},
                new[] {typeof(Double), typeof(Int32)},
                new[] {typeof(Double), typeof(Int64)},
                new[] {typeof(Double), typeof(Byte?)},
                new[] {typeof(Double), typeof(Char?)},
                new[] {typeof(Double), typeof(Decimal?)},
                new[] {typeof(Double), typeof(Int16?)},
                new[] {typeof(Double), typeof(Int32?)},
                new[] {typeof(Double), typeof(Int64?)},
                new[] {typeof(Double), typeof(SByte?)},
                new[] {typeof(Double), typeof(Single?)},
                new[] {typeof(Double), typeof(UInt16?)},
                new[] {typeof(Double), typeof(UInt32?)},
                new[] {typeof(Double), typeof(UInt64?)},
                new[] {typeof(Double), typeof(SByte)},
                new[] {typeof(Double), typeof(Single)},
                new[] {typeof(Double), typeof(UInt16)},
                new[] {typeof(Double), typeof(UInt32)},
                new[] {typeof(Double), typeof(UInt64)},
                new[] {typeof(Int16), typeof(Byte)},
                new[] {typeof(Int16), typeof(Char)},
                new[] {typeof(Int16), typeof(Decimal)},
                new[] {typeof(Int16), typeof(Byte?)},
                new[] {typeof(Int16), typeof(Char?)},
                new[] {typeof(Int16), typeof(Decimal?)},
                new[] {typeof(Int16), typeof(Double?)},
                new[] {typeof(Int16), typeof(Int32?)},
                new[] {typeof(Int16), typeof(Int64?)},
                new[] {typeof(Int16), typeof(SByte?)},
                new[] {typeof(Int16), typeof(Single?)},
                new[] {typeof(Int16), typeof(UInt16?)},
                new[] {typeof(Int16), typeof(UInt32?)},
                new[] {typeof(Int16), typeof(UInt64?)},
                new[] {typeof(Int16), typeof(SByte)},
                new[] {typeof(Int16), typeof(UInt16)},
                new[] {typeof(Int16), typeof(UInt32)},
                new[] {typeof(Int16), typeof(UInt64)},
                new[] {typeof(Int32), typeof(Byte)},
                new[] {typeof(Int32), typeof(Char)},
                new[] {typeof(Int32), typeof(Decimal)},
                new[] {typeof(Int32), typeof(Int16)},
                new[] {typeof(Int32), typeof(Byte?)},
                new[] {typeof(Int32), typeof(Char?)},
                new[] {typeof(Int32), typeof(Decimal?)},
                new[] {typeof(Int32), typeof(Double?)},
                new[] {typeof(Int32), typeof(Int16?)},
                //new[] {typeof(Int32), typeof(Int64?)},
                new[] {typeof(Int32), typeof(SByte?)},
                new[] {typeof(Int32), typeof(Single?)},
                new[] {typeof(Int32), typeof(UInt16?)},
                new[] {typeof(Int32), typeof(UInt32?)},
                new[] {typeof(Int32), typeof(UInt64?)},
                new[] {typeof(Int32), typeof(SByte)},
                new[] {typeof(Int32), typeof(UInt16)},
                new[] {typeof(Int32), typeof(UInt32)},
                new[] {typeof(Int32), typeof(UInt64)},
                new[] {typeof(Int64), typeof(Byte)},
                new[] {typeof(Int64), typeof(Char)},
                new[] {typeof(Int64), typeof(Decimal)},
                new[] {typeof(Int64), typeof(Int16)},
                new[] {typeof(Int64), typeof(Int32)},
                new[] {typeof(Int64), typeof(Byte?)},
                new[] {typeof(Int64), typeof(Char?)},
                new[] {typeof(Int64), typeof(Decimal?)},
                new[] {typeof(Int64), typeof(Double?)},
                new[] {typeof(Int64), typeof(Int16?)},
                new[] {typeof(Int64), typeof(Int32?)},
                new[] {typeof(Int64), typeof(SByte?)},
                new[] {typeof(Int64), typeof(Single?)},
                new[] {typeof(Int64), typeof(UInt16?)},
                new[] {typeof(Int64), typeof(UInt32?)},
                new[] {typeof(Int64), typeof(UInt64?)},
                new[] {typeof(Int64), typeof(SByte)},
                new[] {typeof(Int64), typeof(UInt16)},
                new[] {typeof(Int64), typeof(UInt32)},
                new[] {typeof(Int64), typeof(UInt64)},
                new[] {typeof(Byte?), typeof(Decimal)},
                new[] {typeof(Byte?), typeof(Char?)},
                new[] {typeof(Byte?), typeof(Decimal?)},
                new[] {typeof(Byte?), typeof(Double?)},
                new[] {typeof(Byte?), typeof(Int16?)},
                new[] {typeof(Byte?), typeof(Int32?)},
                new[] {typeof(Byte?), typeof(Int64?)},
                new[] {typeof(Byte?), typeof(SByte?)},
                new[] {typeof(Byte?), typeof(Single?)},
                new[] {typeof(Byte?), typeof(UInt16?)},
                new[] {typeof(Byte?), typeof(UInt32?)},
                new[] {typeof(Byte?), typeof(UInt64?)},
                new[] {typeof(Byte?), typeof(SByte)},
                new[] {typeof(Decimal?), typeof(Byte)},
                new[] {typeof(Decimal?), typeof(Char)},
                new[] {typeof(Decimal?), typeof(Double)},
                new[] {typeof(Decimal?), typeof(Int16)},
                new[] {typeof(Decimal?), typeof(Int32)},
                new[] {typeof(Decimal?), typeof(Int64)},
                new[] {typeof(Decimal?), typeof(Byte?)},
                new[] {typeof(Decimal?), typeof(Char?)},
                new[] {typeof(Decimal?), typeof(Double?)},
                new[] {typeof(Decimal?), typeof(Int16?)},
                new[] {typeof(Decimal?), typeof(Int32?)},
                new[] {typeof(Decimal?), typeof(Int64?)},
                new[] {typeof(Decimal?), typeof(SByte?)},
                new[] {typeof(Decimal?), typeof(Single?)},
                new[] {typeof(Decimal?), typeof(UInt16?)},
                new[] {typeof(Decimal?), typeof(UInt32?)},
                new[] {typeof(Decimal?), typeof(UInt64?)},
                new[] {typeof(Decimal?), typeof(SByte)},
                new[] {typeof(Decimal?), typeof(Single)},
                new[] {typeof(Decimal?), typeof(UInt16)},
                new[] {typeof(Decimal?), typeof(UInt32)},
                new[] {typeof(Decimal?), typeof(UInt64)},
                new[] {typeof(Double?), typeof(Byte)},
                new[] {typeof(Double?), typeof(Char)},
                new[] {typeof(Double?), typeof(Decimal)},
                new[] {typeof(Double?), typeof(Int16)},
                new[] {typeof(Double?), typeof(Int32)},
                new[] {typeof(Double?), typeof(Int64)},
                new[] {typeof(Double?), typeof(Byte?)},
                new[] {typeof(Double?), typeof(Char?)},
                new[] {typeof(Double?), typeof(Decimal?)},
                new[] {typeof(Double?), typeof(Int16?)},
                new[] {typeof(Double?), typeof(Int32?)},
                new[] {typeof(Double?), typeof(Int64?)},
                new[] {typeof(Double?), typeof(SByte?)},
                new[] {typeof(Double?), typeof(Single?)},
                new[] {typeof(Double?), typeof(UInt16?)},
                new[] {typeof(Double?), typeof(UInt32?)},
                new[] {typeof(Double?), typeof(UInt64?)},
                new[] {typeof(Double?), typeof(SByte)},
                new[] {typeof(Double?), typeof(Single)},
                new[] {typeof(Double?), typeof(UInt16)},
                new[] {typeof(Double?), typeof(UInt32)},
                new[] {typeof(Double?), typeof(UInt64)},
                new[] {typeof(Int16?), typeof(Byte)},
                new[] {typeof(Int16?), typeof(Char)},
                new[] {typeof(Int16?), typeof(Decimal)},
                new[] {typeof(Int16?), typeof(Byte?)},
                new[] {typeof(Int16?), typeof(Char?)},
                new[] {typeof(Int16?), typeof(Decimal?)},
                new[] {typeof(Int16?), typeof(Double?)},
                new[] {typeof(Int16?), typeof(Int32?)},
                new[] {typeof(Int16?), typeof(Int64?)},
                new[] {typeof(Int16?), typeof(SByte?)},
                new[] {typeof(Int16?), typeof(Single?)},
                new[] {typeof(Int16?), typeof(UInt16?)},
                new[] {typeof(Int16?), typeof(UInt32?)},
                new[] {typeof(Int16?), typeof(UInt64?)},
                new[] {typeof(Int16?), typeof(SByte)},
                new[] {typeof(Int16?), typeof(UInt16)},
                new[] {typeof(Int16?), typeof(UInt32)},
                new[] {typeof(Int16?), typeof(UInt64)},
                new[] {typeof(Int32?), typeof(Byte)},
                new[] {typeof(Int32?), typeof(Char)},
                new[] {typeof(Int32?), typeof(Decimal)},
                new[] {typeof(Int32?), typeof(Int16)},
                new[] {typeof(Int32?), typeof(Byte?)},
                new[] {typeof(Int32?), typeof(Char?)},
                new[] {typeof(Int32?), typeof(Decimal?)},
                new[] {typeof(Int32?), typeof(Double?)},
                new[] {typeof(Int32?), typeof(Int16?)},
                new[] {typeof(Int32?), typeof(Int64?)},
                new[] {typeof(Int32?), typeof(SByte?)},
                new[] {typeof(Int32?), typeof(Single?)},
                new[] {typeof(Int32?), typeof(UInt16?)},
                new[] {typeof(Int32?), typeof(UInt32?)},
                new[] {typeof(Int32?), typeof(UInt64?)},
                new[] {typeof(Int32?), typeof(SByte)},
                new[] {typeof(Int32?), typeof(UInt16)},
                new[] {typeof(Int32?), typeof(UInt32)},
                new[] {typeof(Int32?), typeof(UInt64)},
                new[] {typeof(Int64?), typeof(Byte)},
                new[] {typeof(Int64?), typeof(Char)},
                new[] {typeof(Int64?), typeof(Decimal)},
                new[] {typeof(Int64?), typeof(Int16)},
                new[] {typeof(Int64?), typeof(Int32)},
                new[] {typeof(Int64?), typeof(Byte?)},
                new[] {typeof(Int64?), typeof(Char?)},
                new[] {typeof(Int64?), typeof(Decimal?)},
                new[] {typeof(Int64?), typeof(Double?)},
                new[] {typeof(Int64?), typeof(Int16?)},
                new[] {typeof(Int64?), typeof(Int32?)},
                new[] {typeof(Int64?), typeof(SByte?)},
                new[] {typeof(Int64?), typeof(Single?)},
                new[] {typeof(Int64?), typeof(UInt16?)},
                new[] {typeof(Int64?), typeof(UInt32?)},
                new[] {typeof(Int64?), typeof(UInt64?)},
                new[] {typeof(Int64?), typeof(SByte)},
                new[] {typeof(Int64?), typeof(UInt16)},
                new[] {typeof(Int64?), typeof(UInt32)},
                new[] {typeof(Int64?), typeof(UInt64)},
                new[] {typeof(SByte?), typeof(Byte)},
                new[] {typeof(SByte?), typeof(Char)},
                new[] {typeof(SByte?), typeof(Decimal)},
                new[] {typeof(SByte?), typeof(Byte?)},
                new[] {typeof(SByte?), typeof(Char?)},
                new[] {typeof(SByte?), typeof(Decimal?)},
                new[] {typeof(SByte?), typeof(Double?)},
                new[] {typeof(SByte?), typeof(Int16?)},
                new[] {typeof(SByte?), typeof(Int32?)},
                new[] {typeof(SByte?), typeof(Int64?)},
                new[] {typeof(SByte?), typeof(Single?)},
                new[] {typeof(SByte?), typeof(UInt16?)},
                new[] {typeof(SByte?), typeof(UInt32?)},
                new[] {typeof(SByte?), typeof(UInt64?)},
                new[] {typeof(SByte?), typeof(UInt16)},
                new[] {typeof(SByte?), typeof(UInt32)},
                new[] {typeof(SByte?), typeof(UInt64)},
                new[] {typeof(Single?), typeof(Byte)},
                new[] {typeof(Single?), typeof(Char)},
                new[] {typeof(Single?), typeof(Decimal)},
                new[] {typeof(Single?), typeof(Int16)},
                new[] {typeof(Single?), typeof(Int32)},
                new[] {typeof(Single?), typeof(Int64)},
                new[] {typeof(Single?), typeof(Byte?)},
                new[] {typeof(Single?), typeof(Char?)},
                new[] {typeof(Single?), typeof(Decimal?)},
                new[] {typeof(Single?), typeof(Double?)},
                new[] {typeof(Single?), typeof(Int16?)},
                new[] {typeof(Single?), typeof(Int32?)},
                new[] {typeof(Single?), typeof(Int64?)},
                new[] {typeof(Single?), typeof(SByte?)},
                new[] {typeof(Single?), typeof(UInt16?)},
                new[] {typeof(Single?), typeof(UInt32?)},
                new[] {typeof(Single?), typeof(UInt64?)},
                new[] {typeof(Single?), typeof(SByte)},
                new[] {typeof(Single?), typeof(UInt16)},
                new[] {typeof(Single?), typeof(UInt32)},
                new[] {typeof(Single?), typeof(UInt64)},
                new[] {typeof(UInt16?), typeof(Byte)},
                new[] {typeof(UInt16?), typeof(Decimal)},
                new[] {typeof(UInt16?), typeof(Int16)},
                new[] {typeof(UInt16?), typeof(Byte?)},
                new[] {typeof(UInt16?), typeof(Char?)},
                new[] {typeof(UInt16?), typeof(Decimal?)},
                new[] {typeof(UInt16?), typeof(Double?)},
                new[] {typeof(UInt16?), typeof(Int16?)},
                new[] {typeof(UInt16?), typeof(Int32?)},
                new[] {typeof(UInt16?), typeof(Int64?)},
                new[] {typeof(UInt16?), typeof(SByte?)},
                new[] {typeof(UInt16?), typeof(Single?)},
                new[] {typeof(UInt16?), typeof(UInt32?)},
                new[] {typeof(UInt16?), typeof(UInt64?)},
                new[] {typeof(UInt16?), typeof(SByte)},
                new[] {typeof(UInt32?), typeof(Byte)},
                new[] {typeof(UInt32?), typeof(Char)},
                new[] {typeof(UInt32?), typeof(Decimal)},
                new[] {typeof(UInt32?), typeof(Int16)},
                new[] {typeof(UInt32?), typeof(Int32)},
                new[] {typeof(UInt32?), typeof(Byte?)},
                new[] {typeof(UInt32?), typeof(Char?)},
                new[] {typeof(UInt32?), typeof(Decimal?)},
                new[] {typeof(UInt32?), typeof(Double?)},
                new[] {typeof(UInt32?), typeof(Int16?)},
                new[] {typeof(UInt32?), typeof(Int32?)},
                new[] {typeof(UInt32?), typeof(Int64?)},
                new[] {typeof(UInt32?), typeof(SByte?)},
                new[] {typeof(UInt32?), typeof(Single?)},
                new[] {typeof(UInt32?), typeof(UInt16?)},
                new[] {typeof(UInt32?), typeof(UInt64?)},
                new[] {typeof(UInt32?), typeof(SByte)},
                new[] {typeof(UInt32?), typeof(UInt16)},
                new[] {typeof(UInt64?), typeof(Byte)},
                new[] {typeof(UInt64?), typeof(Char)},
                new[] {typeof(UInt64?), typeof(Decimal)},
                new[] {typeof(UInt64?), typeof(Int16)},
                new[] {typeof(UInt64?), typeof(Int32)},
                new[] {typeof(UInt64?), typeof(Int64)},
                new[] {typeof(UInt64?), typeof(Byte?)},
                new[] {typeof(UInt64?), typeof(Char?)},
                new[] {typeof(UInt64?), typeof(Decimal?)},
                new[] {typeof(UInt64?), typeof(Double?)},
                new[] {typeof(UInt64?), typeof(Int16?)},
                new[] {typeof(UInt64?), typeof(Int32?)},
                new[] {typeof(UInt64?), typeof(Int64?)},
                new[] {typeof(UInt64?), typeof(SByte?)},
                new[] {typeof(UInt64?), typeof(Single?)},
                new[] {typeof(UInt64?), typeof(UInt16?)},
                new[] {typeof(UInt64?), typeof(UInt32?)},
                new[] {typeof(UInt64?), typeof(SByte)},
                new[] {typeof(UInt64?), typeof(UInt16)},
                new[] {typeof(UInt64?), typeof(UInt32)},
                new[] {typeof(SByte), typeof(Byte)},
                new[] {typeof(SByte), typeof(Char)},
                new[] {typeof(SByte), typeof(Decimal)},
                new[] {typeof(SByte), typeof(Byte?)},
                new[] {typeof(SByte), typeof(Char?)},
                new[] {typeof(SByte), typeof(Decimal?)},
                new[] {typeof(SByte), typeof(Double?)},
                new[] {typeof(SByte), typeof(Int16?)},
                new[] {typeof(SByte), typeof(Int32?)},
                new[] {typeof(SByte), typeof(Int64?)},
                new[] {typeof(SByte), typeof(Single?)},
                new[] {typeof(SByte), typeof(UInt16?)},
                new[] {typeof(SByte), typeof(UInt32?)},
                new[] {typeof(SByte), typeof(UInt64?)},
                new[] {typeof(SByte), typeof(UInt16)},
                new[] {typeof(SByte), typeof(UInt32)},
                new[] {typeof(SByte), typeof(UInt64)},
                new[] {typeof(Single), typeof(Byte)},
                new[] {typeof(Single), typeof(Char)},
                new[] {typeof(Single), typeof(Decimal)},
                new[] {typeof(Single), typeof(Int16)},
                new[] {typeof(Single), typeof(Int32)},
                new[] {typeof(Single), typeof(Int64)},
                new[] {typeof(Single), typeof(Byte?)},
                new[] {typeof(Single), typeof(Char?)},
                new[] {typeof(Single), typeof(Decimal?)},
                new[] {typeof(Single), typeof(Double?)},
                new[] {typeof(Single), typeof(Int16?)},
                new[] {typeof(Single), typeof(Int32?)},
                new[] {typeof(Single), typeof(Int64?)},
                new[] {typeof(Single), typeof(SByte?)},
                new[] {typeof(Single), typeof(UInt16?)},
                new[] {typeof(Single), typeof(UInt32?)},
                new[] {typeof(Single), typeof(UInt64?)},
                new[] {typeof(Single), typeof(SByte)},
                new[] {typeof(Single), typeof(UInt16)},
                new[] {typeof(Single), typeof(UInt32)},
                new[] {typeof(Single), typeof(UInt64)},
                new[] {typeof(UInt16), typeof(Byte)},
                new[] {typeof(UInt16), typeof(Decimal)},
                new[] {typeof(UInt16), typeof(Int16)},
                new[] {typeof(UInt16), typeof(Byte?)},
                new[] {typeof(UInt16), typeof(Char?)},
                new[] {typeof(UInt16), typeof(Decimal?)},
                new[] {typeof(UInt16), typeof(Double?)},
                new[] {typeof(UInt16), typeof(Int16?)},
                new[] {typeof(UInt16), typeof(Int32?)},
                new[] {typeof(UInt16), typeof(Int64?)},
                new[] {typeof(UInt16), typeof(SByte?)},
                new[] {typeof(UInt16), typeof(Single?)},
                new[] {typeof(UInt16), typeof(UInt32?)},
                new[] {typeof(UInt16), typeof(UInt64?)},
                new[] {typeof(UInt16), typeof(SByte)},
                new[] {typeof(UInt32), typeof(Byte)},
                new[] {typeof(UInt32), typeof(Char)},
                new[] {typeof(UInt32), typeof(Decimal)},
                new[] {typeof(UInt32), typeof(Int16)},
                new[] {typeof(UInt32), typeof(Int32)},
                new[] {typeof(UInt32), typeof(Byte?)},
                new[] {typeof(UInt32), typeof(Char?)},
                new[] {typeof(UInt32), typeof(Decimal?)},
                new[] {typeof(UInt32), typeof(Double?)},
                new[] {typeof(UInt32), typeof(Int16?)},
                new[] {typeof(UInt32), typeof(Int32?)},
                new[] {typeof(UInt32), typeof(Int64?)},
                new[] {typeof(UInt32), typeof(SByte?)},
                new[] {typeof(UInt32), typeof(Single?)},
                new[] {typeof(UInt32), typeof(UInt16?)},
                new[] {typeof(UInt32), typeof(UInt64?)},
                new[] {typeof(UInt32), typeof(SByte)},
                new[] {typeof(UInt32), typeof(UInt16)},
                new[] {typeof(UInt64), typeof(Byte)},
                new[] {typeof(UInt64), typeof(Char)},
                new[] {typeof(UInt64), typeof(Decimal)},
                new[] {typeof(UInt64), typeof(Int16)},
                new[] {typeof(UInt64), typeof(Int32)},
                new[] {typeof(UInt64), typeof(Int64)},
                new[] {typeof(UInt64), typeof(Byte?)},
                new[] {typeof(UInt64), typeof(Char?)},
                new[] {typeof(UInt64), typeof(Decimal?)},
                new[] {typeof(UInt64), typeof(Double?)},
                new[] {typeof(UInt64), typeof(Int16?)},
                new[] {typeof(UInt64), typeof(Int32?)},
                new[] {typeof(UInt64), typeof(Int64?)},
                new[] {typeof(UInt64), typeof(SByte?)},
                new[] {typeof(UInt64), typeof(Single?)},
                new[] {typeof(UInt64), typeof(UInt16?)},
                new[] {typeof(UInt64), typeof(UInt32?)},
                new[] {typeof(UInt64), typeof(SByte)},
                new[] {typeof(UInt64), typeof(UInt16)},
                new[] {typeof(UInt64), typeof(UInt32)},

                new[] {typeof(string), typeof(bool?)},
                new[] {typeof(bool?), typeof(string)},
                new[] {typeof(byte), typeof(string)},
                new[] {typeof(Hashtable), typeof(List<object>)},
                new[] {typeof(int[]), typeof(float?[])},
                new[] {typeof(int[]), typeof(int?[])},
                new[] {typeof(int[]), typeof(double[])},
            };

            for (int i = 0; i < successCases.Length; i++) {
                Assert.IsTrue(TypeHelper.IsAssignmentCompatible(successCases[i][0], successCases[i][1]),
                              "Failed asserting success case #" + i + " " + successCases[i][0] +
                              " and " + successCases[i][1]);
            }
            for (int i = 0; i < failCases.Length; i++) {
                Assert.IsFalse(TypeHelper.IsAssignmentCompatible(failCases[i][0], failCases[i][1]),
                               "Failed asserting fail case #" + i + " " + failCases[i][0] +
                               " and " + failCases[i][1]);
            }
        }

        [Test]
        public void TestIsBoolean()
        {
            Assert.IsTrue(TypeHelper.IsBoolean(typeof(bool?)));
            Assert.IsTrue(TypeHelper.IsBoolean(typeof(bool)));
            Assert.IsFalse(TypeHelper.IsBoolean(typeof(string)));
        }

        [Test]
        public void TestIsFloatingPointClass()
        {
            Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(double)));
            Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(float)));
            Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(double?)));
            Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(float?)));

            Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(string)));
            Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(int)));
            Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(int?)));
        }

        [Test]
        public void TestIsFloatingPointNumber()
        {
            Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1d));
            Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1.0f));
            Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1.0m));
            Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1.0));

            Assert.IsFalse(TypeHelper.IsFloatingPointNumber(1));
            Assert.IsFalse(TypeHelper.IsFloatingPointNumber(1L));
        }

        [Test]
        public void TestIsFragmentableType()
        {
            var notFragmentables = new[]
                                   {
                                       typeof(string), typeof(int), typeof(char?), typeof(long), typeof(DataMap),
                                       typeof(HashMap), typeof(SupportEnum),
                                   };

            var yesFragmentables = new[]
                                   {
                                       typeof(SupportBeanCombinedProps), typeof(SupportBeanCombinedProps.NestedLevOne)
                                       , typeof(SupportBean)
                                   };

            foreach (Type notFragmentable in notFragmentables) {
                Assert.IsFalse(TypeHelper.IsFragmentableType(notFragmentable));
            }
            foreach (Type yesFragmentable in yesFragmentables) {
                Assert.IsTrue(TypeHelper.IsFragmentableType(yesFragmentable));
            }
        }

        [Test]
        public void TestIsBuiltinDataType()
        {
            var classesDataType = new[]
                                  {
                                      typeof(int), typeof(long?), typeof(double), typeof(bool), typeof(bool?),
                                      typeof(char), typeof(char?), typeof(string)
                                  };
            var classesNotDataType = new[] {typeof(SupportBean), typeof(Math), typeof(Type), typeof(Object)};

            for (int i = 0; i < classesDataType.Length; i++) {
                Assert.IsTrue(TypeHelper.IsBuiltinDataType(classesDataType[i]));
            }
            for (int i = 0; i < classesNotDataType.Length; i++) {
                Assert.IsFalse(TypeHelper.IsBuiltinDataType(classesNotDataType[i]));
            }
            Assert.IsTrue(TypeHelper.IsBuiltinDataType(null));
        }

        [Test]
        public void TestIsNumeric()
        {
            Type[] numericClasses = {
                                        typeof(float), typeof(float?),
                                        typeof(double), typeof(double?),
                                        typeof(decimal), typeof(decimal?),
                                        typeof(byte), typeof(byte?),
                                        typeof(sbyte), typeof(sbyte?),
                                        typeof(short?), typeof(short?),
                                        typeof(ushort), typeof(ushort?),
                                        typeof(int), typeof(int?),
                                        typeof(uint), typeof(uint?),
                                        typeof(long), typeof(long?),
                                        typeof(ulong), typeof(ulong?)
                                    };

            Type[] nonnumericClasses = {
                                           typeof(string), typeof(bool), typeof(bool?), typeof(EventArgs)
                                       };

            foreach (Type clazz in numericClasses) {
                Assert.IsTrue(TypeHelper.IsNumeric(clazz));
            }

            foreach (Type clazz in nonnumericClasses) {
                Assert.IsFalse(TypeHelper.IsNumeric(clazz));
            }
        }

        [Test]
        public void TestIsNumericNonFP()
        {
            Type[] numericClasses = {
                                        typeof(byte), typeof(byte?),
                                        typeof(sbyte), typeof(sbyte?),
                                        typeof(short?), typeof(short?),
                                        typeof(ushort), typeof(ushort?),
                                        typeof(int), typeof(int?),
                                        typeof(uint), typeof(uint?),
                                        typeof(long), typeof(long?),
                                        typeof(ulong), typeof(ulong?)
                                    };

            Type[] nonnumericClasses = {
                                           typeof(float), typeof(float?),
                                           typeof(double), typeof(double?),
                                           typeof(string),
                                           typeof(bool), typeof(bool?),
                                           typeof(EventArgs)
                                       };

            foreach (Type clazz in numericClasses) {
                Assert.IsTrue(TypeHelper.IsNumericNonFP(clazz));
            }

            foreach (Type clazz in nonnumericClasses) {
                Assert.IsFalse(TypeHelper.IsNumericNonFP(clazz));
            }
        }

        [Test]
        public void TestIsSimpleNameFullyQualfied()
        {
            //Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "ABC"));
            //Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "com.abc.ABC"));
            //Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "abc.ABC"));
            //Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("DABC", "abc.ABC"));
            //Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("AB", "abc.ABC"));
            //Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("AB", "ABC"));
        }

        [Test]
        public void TestParse()
        {
            var tests = new[]
                        {
                            new object[] {typeof(bool?), "TrUe", true},
                            new object[] {typeof(bool?), "false", false},
                            new object[] {typeof(bool), "false", false},
                            new object[] {typeof(bool), "true", true},
                            new object[] {typeof(int), "73737474 ", 73737474},
                            new object[] {typeof(int?), " -1 ", -1},
                            new object[] {typeof(long), "123456789001222L", 123456789001222L},
                            new object[] {typeof(long?), " -2 ", -2L},
                            new object[] {typeof(long?), " -2L ", -2L},
                            new object[] {typeof(long?), " -2l ", -2L},
                            new object[] {typeof(short?), " -3 ", (short) -3},
                            new object[] {typeof(short?), "111", (short) 111},
                            new object[] {typeof(double?), " -3d ", -3d},
                            new object[] {typeof(double), "111.38373", 111.38373d},
                            new object[] {typeof(double?), " -3.1D ", -3.1D},
                            new object[] {typeof(float?), " -3f ", -3f},
                            new object[] {typeof(float), "111.38373", 111.38373f},
                            new object[] {typeof(float?), " -3.1F ", -3.1f},
                            new object[] {typeof(sbyte?), " -3 ", (sbyte) -3},
                            new object[] {typeof(byte), " 1 ", (byte) 1},
                            new object[] {typeof(char), "ABC", 'A'},
                            new object[] {typeof(char?), " AB", ' '},
                            new object[] {typeof(string), "AB", "AB"},
                            new object[] {typeof(string), " AB ", " AB "},
                        };

            for (int i = 0; i < tests.Length; i++) {
                Assert.AreEqual(tests[i][2],
                                TypeHelper.Parse((Type) tests[i][0], (String) tests[i][1]),
                                "error in row:" + i);
            }
        }

        [Test]
        public void TestCanUseAlternativeClass()
        {
            Assert.That(TypeHelper.TypeResolver, Is.Null);

            try
            {
                var type = TypeHelper.ResolveType(typeof (MyStringList).FullName);
                Assert.That(type, Is.Not.Null);
                Assert.That(type, Is.SameAs(typeof (MyStringList)));

                // now lets switch it up
                TypeHelper.TypeResolver = (args) =>
                {
                    if (args.TypeName == typeof (MyStringList).FullName)
                        return typeof (string);
                    // returning null without setting args.Handled will force the
                    // type helper to invoke its standard logic.
                    return null;
                };

                type = TypeHelper.ResolveType(typeof(MyStringList).FullName);
                Assert.That(type, Is.Not.Null);
                Assert.That(type, Is.SameAs(typeof(string)));

                // now lets make it return a true null
                TypeHelper.TypeResolver = (args) =>
                {
                    if (args.TypeName == typeof (MyStringList).FullName)
                    {
                        args.Handled = true;
                    }

                    return null;
                };

                type = TypeHelper.ResolveType(typeof(MyStringList).FullName);
                Assert.That(type, Is.Null);
            }
            finally
            {
                TypeHelper.TypeResolver = null;
            }
        }
    }
}
