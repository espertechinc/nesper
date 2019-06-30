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
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using java.math;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.JavaClassHelper;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
	public class TestJavaClassHelper  {
        [Test]
	    public void TestArrayTypeCompatible() {
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(int), typeof(int)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(int), typeof(int?)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(int?), typeof(int)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(Number), typeof(int)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(Number), typeof(int?)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(object), typeof(int)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(object), typeof(int?)));

	        Assert.IsTrue(IsArrayTypeCompatible(typeof(ICollection), typeof(ICollection)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(ICollection), typeof(List)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(object), typeof(List)));
	        Assert.IsTrue(IsArrayTypeCompatible(typeof(object), typeof(ICollection)));

	        Assert.IsFalse(IsArrayTypeCompatible(typeof(bool?), typeof(int)));
	        Assert.IsFalse(IsArrayTypeCompatible(typeof(int?), typeof(bool)));
	        Assert.IsFalse(IsArrayTypeCompatible(typlong?long?), typeof(int?)));
	        Assert.IsFalse(IsArrayTypeCompatible(typeof(int?), typeof(byte)));
	    }

        [Test]
	    public void TestIsCollectionMapOrArray() {
	        foreach (Type clazz in Arrays.AsList(typeof(Dictionary), typeof(IDictionary), typeof(ICollection), typeof(List), typeof(int[]), typeof(object[]))) {
	            Assert.IsTrue(IsCollectionMapOrArray(clazz));
	        }
	        foreach (Type clazz in Arrays.AsList(null, typeof(TypeHelper))) {
	            Assert.IsFalse(IsCollectionMapOrArray(clazz));
	        }
	    }

        [Test]
	    public void TestTakeFirstN() {
	        Type[] classes = new Type[]{typeof(string)};
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{typeof(string)}, TypeHelper.TakeFirstN(classes, 1));

	        classes = new Type[]{typeof(string), typeof(int?)};
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{typeof(string), typeof(int?)}, TypeHelper.TakeFirstN(classes, 2));

	        classes = new Type[]{typeof(string), typeof(int?), typeof(double?)};
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{typeof(string)}, TypeHelper.TakeFirstN(classes, 1));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{typeof(string), typeof(int?)}, TypeHelper.TakeFirstN(classes, 2));
	    }

        [Test]
	    public void TestIsFragmentableType() {
	        Type[] notFragmentables = new Type[]{
	                typeof(string), typeof(int), typeof(Character), typeof(long), typeof(IDictionary), typeof(Dictionary), typeof(SupportEnum),
	        };

	        Type[] yesFragmentables = new Type[]{
	                typeof(SupportBeanCombinedProps), typeof(SupportBeanCombinedProps.NestedLevOne), typeof(SupportBean)
	        };

	        foreach (Type notFragmentable in notFragmentables) {
	            Assert.IsFalse(TypeHelper.IsFragmentableType(notFragmentable));
	        }
	        foreach (Type yesFragmentable in yesFragmentables) {
	            Assert.IsTrue(TypeHelper.IsFragmentableType(yesFragmentable));
	        }
	    }

        [Test]
	    public void TestGetParameterAsString() {
	        object[][] testCases = {
	                {new Type[]{typeof(string), typeof(int)}, "String, int"},
	                {new Type[]{typeof(int?), typeof(bool?)}, "Integer, Boolean"},
	                {new Type[]{}, ""},
	                {new Type[]{null}, "null (any type)"},
	                {new Type[]{typeof(byte), null}, "byte, null (any type)"},
	                {new Type[]{typeof(SupportBean), typeof(int[]), typeof(int[][]), typeof(IDictionary)}, "SupportBean, int[], int[][], Map"},
	                {new Type[]{typeof(SupportBean[]), typeof(SupportEnum), typeof(SupportBeanComplexProps.SupportBeanSpecialGetterNested)}, "SupportBean[], SupportEnum, SupportBeanSpecialGetterNested"},
	        };

	        for (int i = 0; i < testCases.Length; i++) {
	            Type[] parameters = (Type[]) testCases[i][0];
	            Assert.AreEqual(testCases[i][1], TypeHelper.GetParameterAsString(parameters));
	        }
	    }

        [Test]
	    public void TestCanCoerce() {
	        Type[] primitiveClasses = {
	                typeof(float), typeof(double), typeof(byte), typeof(short), typeof(int), typeof(long)};

	        Type[] boxedClasses = {
	                typeof(float?), typeof(double?), typeof(Byte), typeof(Short), typeof(int?), typlong?long?)};

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

	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(BigInteger), typeof(BigInteger)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(long), typeof(BigInteger)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(int?), typeof(BigInteger)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(short), typeof(BigInteger)));

	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(float), typeof(BigDecimal)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(double?), typeof(BigDecimal)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(BigInteger), typeof(BigDecimal)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(long), typeof(BigDecimal)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(int?), typeof(BigDecimal)));
	        Assert.IsTrue(TypeHelper.CanCoerce(typeof(short), typeof(BigDecimal)));

	        try {
	            TypeHelper.CanCoerce(typeof(string), typeof(float?));
	            Fail();
	        } catch (ArgumentException ex) {
	            // expected
	        }

	        try {
	            TypeHelper.CanCoerce(typeof(float?), typeof(bool?));
	            Fail();
	        } catch (ArgumentException ex) {
	            // expected
	        }
	    }

        [Test]
	    public void TestCoerceBoxed() {
	        Assert.AreEqual(1d, TypeHelper.CoerceBoxed(1d, typeof(double?)));
	        Assert.AreEqual(5d, TypeHelper.CoerceBoxed(5, typeof(double?)));
	        Assert.AreEqual(6d, TypeHelper.CoerceBoxed((byte) 6, typeof(double?)));
	        Assert.AreEqual(3f, TypeHelper.CoerceBoxed((long) 3, typeof(float?)));
	        Assert.AreEqual((short) 2, TypeHelper.CoerceBoxed((long) 2, typeof(Short)));
	        Assert.AreEqual(4, TypeHelper.CoerceBoxed((long) 4, typeof(int?)));
	        Assert.AreEqual((byte) 5, TypeHelper.CoerceBoxed((long) 5, typeof(Byte)));
	        Assert.AreEqual(8l, TypeHelper.CoerceBoxed((long) 8, typlong?long?)));
	        Assert.AreEqual(BigInteger.ValueOf(8), TypeHelper.CoerceBoxed(8, typeof(BigInteger)));
	        Assert.AreEqual(new BigDecimal(8), TypeHelper.CoerceBoxed(8, typeof(BigDecimal)));
	        Assert.AreEqual(new BigDecimal(8d), TypeHelper.CoerceBoxed(8d, typeof(BigDecimal)));

	        try {
	            TypeHelper.CoerceBoxed(10, typeof(int));
	            Fail();
	        } catch (ArgumentException ex) {
	            // Expected
	        }
	    }

        [Test]
	    public void TestIsNumeric() {
	        Type[] numericClasses = {
	                typeof(float), typeof(float?), typeof(double), typeof(double?),
	                typeof(byte), typeof(Byte), typeof(short), typeof(Short), typeof(int), typeof(int?),
	                typeof(long), typlong?long?), typeof(BigInteger), typeof(BigDecimal)};

	        Type[] nonnumericClasses = {
	                typeof(string), typeof(bool), typeof(bool?), typeof(TestCase)};

	        foreach (Type clazz in numericClasses) {
	            Assert.IsTrue(TypeHelper.IsNumeric(clazz));
	        }

	        foreach (Type clazz in nonnumericClasses) {
	            Assert.IsFalse(TypeHelper.IsNumeric(clazz));
	        }
	    }

        [Test]
	    public void TestIsNumericNonFP() {
	        Type[] numericClasses = {
	                typeof(byte), typeof(Byte), typeof(short), typeof(Short), typeof(int), typeof(int?),
	                typeof(long), typlong?long?)};

	        Type[] nonnumericClasses = {
	                typeof(float), typeof(float?), typeof(double), typeof(double?), typeof(string), typeof(bool), typeof(bool?), typeof(TestCase)};

	        foreach (Type clazz in numericClasses) {
	            Assert.IsTrue(TypeHelper.IsNumericNonFP(clazz));
	        }

	        foreach (Type clazz in nonnumericClasses) {
	            Assert.IsFalse(TypeHelper.IsNumericNonFP(clazz));
	        }
	    }

        [Test]
	    public void TestGetBoxed() {
	        Type[] primitiveClasses = {
	                typeof(bool), typeof(float), typeof(double), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(char)};

	        Type[] boxedClasses = {
	                typeof(bool?), typeof(float?), typeof(double?), typeof(Byte), typeof(Short), typeof(int?), typlong?long?), typeof(Character)};

	        Type[] otherClasses = {
	                typeof(string), typeof(TestCase)};

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
	    public void TestGetPrimitive() {
	        Type[] primitiveClasses = {
	                typeof(bool), typeof(float), typeof(double), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(char)};

	        Type[] boxedClasses = {
	                typeof(bool?), typeof(float?), typeof(double?), typeof(Byte), typeof(Short), typeof(int?), typlong?long?), typeof(Character)};

	        Type[] otherClasses = {
	                typeof(string), typeof(TestCase)};

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
	    public void TestIsAssignmentCompatible() {
	        Type[][] successCases = new Type[][]{
	                {typeof(bool), typeof(bool?)},
	                {typeof(byte), typeof(short)},
	                {typeof(byte), typeof(Short)},
	                {typeof(byte), typeof(int)},
	                {typeof(byte), typeof(int?)},
	                {typeof(Byte), typeof(long)},
	                {typeof(byte), typlong?long?)},
	                {typeof(byte), typeof(double?)},
	                {typeof(byte), typeof(double)},
	                {typeof(Byte), typeof(float)},
	                {typeof(byte), typeof(float?)},
	                {typeof(short), typeof(short)},
	                {typeof(Short), typeof(Short)},
	                {typeof(short), typeof(int)},
	                {typeof(short), typeof(int?)},
	                {typeof(short), typeof(long)},
	                {typeof(Short), typlong?long?)},
	                {typeof(short), typeof(double?)},
	                {typeof(short), typeof(double)},
	                {typeof(short), typeof(float)},
	                {typeof(short), typeof(float?)},
	                {typeof(char), typeof(char)},
	                {typeof(Character), typeof(char)},
	                {typeof(char), typeof(Character)},
	                {typeof(char), typeof(int)},
	                {typeof(char), typeof(int?)},
	                {typeof(char), typeof(long)},
	                {typeof(Character), typlong?long?)},
	                {typeof(char), typeof(double?)},
	                {typeof(char), typeof(double)},
	                {typeof(Character), typeof(float)},
	                {typeof(char), typeof(float?)},
	                {typeof(int), typeof(long)},
	                {typeof(int?), typlong?long?)},
	                {typeof(int), typeof(double?)},
	                {typeof(int?), typeof(double)},
	                {typeof(int), typeof(float)},
	                {typeof(int), typeof(float?)},
	                {typlong?long?), typeof(long)},
	                {typeof(long), typlong?long?)},
	                {typeof(long), typeof(double?)},
	                {typlong?long?), typeof(double)},
	                {typeof(long), typeof(float)},
	                {typeof(long), typeof(float?)},
	                {typeof(float), typeof(double?)},
	                {typeof(float), typeof(double)},
	                {typeof(float), typeof(float)},
	                {typeof(float?), typeof(float?)},
	                {typeof(HashSet), typeof(ISet)},
	                {typeof(HashSet), typeof(ICollection)},
	                {typeof(HashSet), typeof(Iterable)},
	                {typeof(HashSet), typeof(Cloneable)},
	                {typeof(HashSet), typeof(Serializable)},
	                {typeof(LineNumberReader), typeof(BufferedReader)},
	                {typeof(LineNumberReader), typeof(Reader)},
	                {typeof(LineNumberReader), typeof(object)},
	                {typeof(LineNumberReader), typeof(Readable)},
	                {typeof(SortedSet), typeof(ISet)},
	                {typeof(ISet), typeof(ICollection)},
	                {typeof(ISet), typeof(object)},
	                // widening of arrays allowed if supertype
	                {typeof(int?[]), typeof(Number[])},
	                {typeof(int?[]), typeof(object[])},
	                {typeof(LineNumberReader[]), typeof(Reader[])},
	                {typeof(LineNumberReader[]), typeof(Readable[])},
	                {typeof(LineNumberReader[]), typeof(object[])},
	                {typeof(ISupportAImplSuperG), typeof(ISupportA)},
	                {typeof(ISupportAImplSuperGImpl), typeof(ISupportA)},
	                {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportA)},
	                {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportB)},
	                {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportC)},
	                {typeof(ISupportAImplSuperGImplPlus), typeof(ISupportAImplSuperG)},
	                {null, typeof(object)},
	        };

	        Type[][] failCases = new Type[][]{
	                {typeof(int), typeof(Byte)},
	                {typeof(short), typeof(byte)},
	                {typeof(string), typeof(bool?)},
	                {typeof(bool?), typeof(string)},
	                {typeof(Byte), typeof(string)},
	                {typeof(char), typeof(byte)},
	                {typeof(char), typeof(short)},
	                {typeof(Character), typeof(short)},
	                {typeof(int), typeof(short)},
	                {typeof(long), typeof(int)},
	                {typeof(float), typeof(long)},
	                {typeof(float?), typeof(byte)},
	                {typeof(double?), typeof(char)},
	                {typeof(double), typeof(long)},
	                {typeof(ICollection), typeof(ISet)},
	                {typeof(object), typeof(ICollection)},
	                {typeof(int?[]), typeof(Float[])},
	                {typeof(int?[]), typeof(int[])},
	                {typeof(int?[]), typeof(double[])},
	                {typeof(Reader[]), typeof(LineNumberReader[])},
	                {typeof(Readable[]), typeof(Reader[])},
	        };

	        for (int i = 0; i < successCases.Length; i++) {
	            Assert.IsTrue("Failed asserting success case " + successCases[i][0] +
	                    " and " + successCases[i][1], TypeHelper.IsAssignmentCompatible(successCases[i][0], successCases[i][1]));
	        }
	        for (int i = 0; i < failCases.Length; i++) {
	            Assert.IsFalse("Failed asserting fail case " + failCases[i][0] +
	                    " and " + failCases[i][1], TypeHelper.IsAssignmentCompatible(failCases[i][0], failCases[i][1]));
	        }
	    }

        [Test]
	    public void TestIsBoolean() {
	        Assert.IsTrue(TypeHelper.IsBoolean(typeof(bool?)));
	        Assert.IsTrue(TypeHelper.IsBoolean(typeof(bool)));
	        Assert.IsFalse(TypeHelper.IsBoolean(typeof(string)));
	    }

        [Test]
	    public void TestGetArrayType() {
	        Assert.AreEqual(typeof(int[]), GetArrayType(typeof(int)));
	        Assert.AreEqual(typeof(int?[]), GetArrayType(typeof(int?)));

	        Assert.AreEqual(typeof(int?), GetArrayType(typeof(int?), 0));
	        Assert.AreEqual(typeof(int?[]), GetArrayType(typeof(int?), 1));
	        Assert.AreEqual(typeof(int?[][]), GetArrayType(typeof(int?), 2));
	        Assert.AreEqual(typeof(int?[][][]), GetArrayType(typeof(int?), 3));
	    }

        [Test]
	    public void TestGetArithmaticCoercionType() {
	        Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(double?), typeof(int)));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(double)));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(long)));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(long)));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(float), typeof(long)));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(float)));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetArithmaticCoercionType(typeof(byte), typeof(int)));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetArithmaticCoercionType(typeof(int?), typeof(int)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetArithmaticCoercionType(typeof(int?), typeof(BigDecimal)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetArithmaticCoercionType(typeof(BigDecimal), typeof(int?)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetArithmaticCoercionType(typeof(BigInteger), typeof(float)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetArithmaticCoercionType(typeof(float), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetArithmaticCoercionType(typeof(int?), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetArithmaticCoercionType(typeof(BigInteger), typeof(int)));

	        try {
	            TypeHelper.GetArithmaticCoercionType(typeof(string), typeof(float));
	            Fail();
	        } catch (CoercionException ex) {
	            // Expected
	        }

	        try {
	            TypeHelper.GetArithmaticCoercionType(typeof(int), typeof(bool));
	            Fail();
	        } catch (CoercionException ex) {
	            // Expected
	        }
	    }

        [Test]
	    public void TestIsFloatingPointNumber() {
	        Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1d));
	        Assert.IsTrue(TypeHelper.IsFloatingPointNumber(1f));
	        Assert.IsTrue(TypeHelper.IsFloatingPointNumber(new Double(1)));
	        Assert.IsTrue(TypeHelper.IsFloatingPointNumber(new Float(1)));

	        Assert.IsFalse(TypeHelper.IsFloatingPointNumber(1));
	        Assert.IsFalse(TypeHelper.IsFloatingPointNumber(new int?(1)));
	    }

        [Test]
	    public void TestIsFloatingPointClass() {
	        Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(double)));
	        Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(float)));
	        Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(double?)));
	        Assert.IsTrue(TypeHelper.IsFloatingPointClass(typeof(float?)));

	        Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(string)));
	        Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(int)));
	        Assert.IsFalse(TypeHelper.IsFloatingPointClass(typeof(int?)));
	    }

        [Test]
	    public void TestGetCompareToCoercionType() {
	        Assert.AreEqual(typeof(string), TypeHelper.GetCompareToCoercionType(typeof(string), typeof(string)));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool?), typeof(bool?)));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool?), typeof(bool)));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool), typeof(bool?)));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCompareToCoercionType(typeof(bool), typeof(bool)));

	        Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(int), typeof(float)));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(double), typeof(byte)));
	        Assert.AreEqual(typeof(float?), TypeHelper.GetCompareToCoercionType(typeof(float), typeof(float)));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCompareToCoercionType(typeof(float), typeof(double?)));

	        Assert.AreEqual(typeof(int?), TypeHelper.GetCompareToCoercionType(typeof(int), typeof(int)));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetCompareToCoercionType(typeof(Short), typeof(int?)));

	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(BigDecimal), typeof(int)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(double?), typeof(BigDecimal)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(byte), typeof(BigDecimal)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(BigInteger), typeof(BigDecimal)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(BigDecimal), typeof(BigDecimal)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(double), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigDecimal), TypeHelper.GetCompareToCoercionType(typeof(float?), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetCompareToCoercionType(typeof(BigInteger), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetCompareToCoercionType(typeof(long), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetCompareToCoercionType(typeof(short), typeof(BigInteger)));
	        Assert.AreEqual(typeof(BigInteger), TypeHelper.GetCompareToCoercionType(typeof(int?), typeof(BigInteger)));

	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCompareToCoercionType(typeof(SupportBean), typeof(SupportBean)));
	        Assert.AreEqual(typeof(object), TypeHelper.GetCompareToCoercionType(typeof(SupportBean), typeof(SupportBean_A)));

	        Assert.AreEqual("Types cannot be compared: java.lang.Boolean and java.math.BigInteger",
	                TryInvalidGetRelational(typeof(bool?), typeof(BigInteger)));
	        TryInvalidGetRelational(typeof(string), typeof(BigDecimal));
	        TryInvalidGetRelational(typeof(string), typeof(int));
	        TryInvalidGetRelational(typlong?long?), typeof(string));
	        TryInvalidGetRelational(typlong?long?), typeof(bool?));
	        TryInvalidGetRelational(typeof(bool), typeof(int));
	    }

        [Test]
	    public void TestGetBoxedClassName() {
	        string[][] tests = new string[][]{
	                {typeof(int?).Name, typeof(int).Name},
	                {typlong?long?).Name, typeof(long).Name},
	                {typeof(Short).Name, typeof(short).Name},
	                {typeof(double?).Name, typeof(double).Name},
	                {typeof(float?).Name, typeof(float).Name},
	                {typeof(bool?).Name, typeof(bool).Name},
	                {typeof(Byte).Name, typeof(byte).Name},
	                {typeof(Character).Name, typeof(char).Name}
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual(tests[i][0], TypeHelper.GetBoxedClassName(tests[i][1]));
	        }
	    }

        [Test]
	    public void TestClassForName() {
	        object[][] tests = new object[][]{
	                {typeof(int), typeof(int).Name},
	                {typeof(long), typeof(long).Name},
	                {typeof(short), typeof(short).Name},
	                {typeof(double), typeof(double).Name},
	                {typeof(float), typeof(float).Name},
	                {typeof(bool), typeof(bool).Name},
	                {typeof(byte), typeof(byte).Name},
	                {typeof(char), typeof(char).Name}};

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual(tests[i][0], TypeHelper.GetClassForName((string) tests[i][1], ClassForNameProviderDefault.INSTANCE));
	        }
	    }

        [Test]
	    public void TestClassForSimpleName() {
	        object[][] tests = new object[][]{
	                {"Boolean", typeof(bool?)},
	                {"Bool", typeof(bool?)},
	                {"boolean", typeof(bool?)},
	                {"java.lang.Boolean", typeof(bool?)},
	                {"int", typeof(int?)},
	                {"inTeger", typeof(int?)},
	                {"java.lang.Integer", typeof(int?)},
	                {"long", typlong?long?)},
	                {"LONG", typlong?long?)},
	                {"java.lang.Short", typeof(Short)},
	                {"short", typeof(Short)},
	                {"  short  ", typeof(Short)},
	                {"double", typeof(double?)},
	                {" douBle", typeof(double?)},
	                {"java.lang.Double", typeof(double?)},
	                {"float", typeof(float?)},
	                {"float  ", typeof(float?)},
	                {"java.lang.Float", typeof(float?)},
	                {"byte", typeof(Byte)},
	                {"   bYte ", typeof(Byte)},
	                {"java.lang.Byte", typeof(Byte)},
	                {"char", typeof(Character)},
	                {"character", typeof(Character)},
	                {"java.lang.Character", typeof(Character)},
	                {"string", typeof(string)},
	                {"java.lang.String", typeof(string)},
	                {"varchar", typeof(string)},
	                {"varchar2", typeof(string)},
	                {typeof(SupportBean).Name, typeof(SupportBean)},
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual("error in row:" + i, tests[i][1], TypeHelper.GetClassForSimpleName((string) tests[i][0], ClassForNameProviderDefault.INSTANCE));
	        }
	    }

        [Test]
	    public void TestParse() {
	        object[][] tests = new object[][]{
	                {typeof(bool?), "TrUe", true},
	                {typeof(bool?), "false", false},
	                {typeof(bool), "false", false},
	                {typeof(bool), "true", true},
	                {typeof(int), "73737474 ", 73737474},
	                {typeof(int?), " -1 ", -1},
	                {typeof(long), "123456789001222L", 123456789001222L},
	                {typlong?long?), " -2 ", -2L},
	                {typlong?long?), " -2L ", -2L},
	                {typlong?long?), " -2l ", -2L},
	                {typeof(Short), " -3 ", (short) -3},
	                {typeof(short), "111", (short) 111},
	                {typeof(double?), " -3d ", -3d},
	                {typeof(double), "111.38373", 111.38373d},
	                {typeof(double?), " -3.1D ", -3.1D},
	                {typeof(float?), " -3f ", -3f},
	                {typeof(float), "111.38373", 111.38373f},
	                {typeof(float?), " -3.1F ", -3.1f},
	                {typeof(Byte), " -3 ", (byte) -3},
	                {typeof(byte), " 1 ", (byte) 1},
	                {typeof(char), "ABC", 'A'},
	                {typeof(Character), " AB", ' '},
	                {typeof(string), "AB", "AB"},
	                {typeof(string), " AB ", " AB "},
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual("error in row:" + i, tests[i][2], TypeHelper.Parse((Type) tests[i][0], (string) tests[i][1]));
	        }
	    }

        [Test]
	    public void TestGetParser() {
	        object[][] tests = new object[][]{
	                {typeof(bool?), "TrUe", true},
	                {typeof(bool?), "false", false},
	                {typeof(bool), "false", false},
	                {typeof(bool), "true", true},
	                {typeof(int), "73737474 ", 73737474},
	                {typeof(int?), " -1 ", -1},
	                {typeof(long), "123456789001222L", 123456789001222L},
	                {typlong?long?), " -2 ", -2L},
	                {typlong?long?), " -2L ", -2L},
	                {typlong?long?), " -2l ", -2L},
	                {typeof(Short), " -3 ", (short) -3},
	                {typeof(short), "111", (short) 111},
	                {typeof(double?), " -3d ", -3d},
	                {typeof(double), "111.38373", 111.38373d},
	                {typeof(double?), " -3.1D ", -3.1D},
	                {typeof(float?), " -3f ", -3f},
	                {typeof(float), "111.38373", 111.38373f},
	                {typeof(float?), " -3.1F ", -3.1f},
	                {typeof(Byte), " -3 ", (byte) -3},
	                {typeof(byte), " 1 ", (byte) 1},
	                {typeof(char), "ABC", 'A'},
	                {typeof(Character), " AB", ' '},
	                {typeof(string), "AB", "AB"},
	                {typeof(string), " AB ", " AB "},
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            SimpleTypeParser parser = SimpleTypeParserFactory.GetParser((Type) tests[i][0]);
	            Assert.AreEqual("error in row:" + i, tests[i][2], parser.Parse((string) tests[i][1]));
	        }
	    }

        [Test]
	    public void TestIsJavaBuiltinDataType() {
	        Type[] classesDataType = new Type[]{typeof(int), typlong?long?), typeof(double), typeof(bool), typeof(bool?),
	                typeof(char), typeof(Character), typeof(string), typeof(CharSequence)};
	        Type[] classesNotDataType = new Type[]{typeof(SupportBean), typeof(Math), typeof(Type), typeof(object)};

	        for (int i = 0; i < classesDataType.Length; i++) {
	            Assert.IsTrue(TypeHelper.IsJavaBuiltinDataType(classesDataType[i]));
	        }
	        for (int i = 0; i < classesNotDataType.Length; i++) {
	            Assert.IsFalse(TypeHelper.IsJavaBuiltinDataType(classesNotDataType[i]));
	        }
	        Assert.IsTrue(TypeHelper.IsJavaBuiltinDataType(null));
	    }

	    private string TryInvalidGetRelational(Type classOne, Type classTwo) {
	        try {
	            TypeHelper.GetCompareToCoercionType(classOne, classTwo);
	            Fail();
	            return null;
	        } catch (CoercionException ex) {
	            return ex.Message;
	        }
	    }

        [Test]
	    public void TestGetCommonCoercionType() {
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string)}));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(long)}));

	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string), null}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string), typeof(string)}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string), typeof(string), typeof(string)}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string), typeof(string), null}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{null, typeof(string), null}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{null, typeof(string), typeof(string)}));
	        Assert.AreEqual(typeof(string), TypeHelper.GetCommonCoercionType(new Type[]{null, null, typeof(string), typeof(string)}));

	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool?), typeof(bool?)}));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool?), typeof(bool)}));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool), typeof(bool?)}));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool), typeof(bool)}));
	        Assert.AreEqual(typeof(bool?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(bool?), typeof(bool), typeof(bool)}));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int), typeof(byte), typeof(int)}));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int?), typeof(Byte), typeof(Short)}));
	        Assert.AreEqual(typeof(int?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(byte), typeof(short), typeof(short)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int?), typeof(Byte), typeof(double?)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typlong?long?), typeof(double?), typeof(double?)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(double), typeof(byte)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(double), typeof(byte), null}));
	        Assert.AreEqual(typeof(float?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(float), typeof(float)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(float), typeof(int)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int?), typeof(int), typeof(float?)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int?), typeof(int), typeof(long)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(long), typeof(int)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(long), typeof(int), typeof(int), typeof(int), typeof(byte), typeof(short)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(long), null, typeof(int), null, typeof(int), typeof(int), null, typeof(byte), typeof(short)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int?), typeof(int), typeof(long)}));
	        Assert.AreEqual(typeof(Character), TypeHelper.GetCommonCoercionType(new Type[]{typeof(char), typeof(char), typeof(char)}));
	        Assert.AreEqual(typlong?long?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int), typeof(int), typeof(int), typeof(long), typeof(int), typeof(int)}));
	        Assert.AreEqual(typeof(double?), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int), typeof(long), typeof(int), typeof(double), typeof(int), typeof(int)}));
	        Assert.AreEqual(null, TypeHelper.GetCommonCoercionType(new Type[]{null, null}));
	        Assert.AreEqual(null, TypeHelper.GetCommonCoercionType(new Type[]{null, null, null}));
	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new Type[]{typeof(SupportBean), null, null}));
	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new Type[]{null, typeof(SupportBean), null}));
	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new Type[]{null, typeof(SupportBean)}));
	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new Type[]{null, null, typeof(SupportBean)}));
	        Assert.AreEqual(typeof(SupportBean), TypeHelper.GetCommonCoercionType(new Type[]{typeof(SupportBean), null, typeof(SupportBean), typeof(SupportBean)}));
	        Assert.AreEqual(typeof(object), TypeHelper.GetCommonCoercionType(new Type[]{typeof(SupportBean), typeof(SupportBean_A), null, typeof(SupportBean), typeof(SupportBean)}));

	        Assert.AreEqual(typeof(int[]), TypeHelper.GetCommonCoercionType(new Type[]{typeof(int[]), typeof(int[])}));
	        Assert.AreEqual(typeof(long[]), TypeHelper.GetCommonCoercionType(new Type[]{typeof(long[]), typeof(long[])}));
	        Assert.AreEqual(typeof(string[]), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string[]), typeof(string[])}));
	        Assert.AreEqual(typeof(object[]), TypeHelper.GetCommonCoercionType(new Type[]{typeof(string[]), typeof(int?[])}));
	        Assert.AreEqual(typeof(object[]), TypeHelper.GetCommonCoercionType(new Type[]{typeof(object[]), typeof(int?[])}));

	        Assert.AreEqual("Cannot coerce to String type java.lang.Boolean", TryInvalidGetCommonCoercionType(new Type[]{typeof(string), typeof(bool?)}));
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(string), typeof(string), typeof(bool?)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(bool?), typeof(string), typeof(bool?)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(bool?), typeof(bool?), typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(long), typeof(bool?), typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(double), typeof(long), typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{null, typeof(double), typeof(long), typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(string), typeof(string), typeof(long)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(string), typeof(SupportBean)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(bool), null, null, typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(int), null, null, typeof(string)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(SupportBean), typeof(bool?)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(string), typeof(SupportBean)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(SupportBean), typeof(string), typeof(SupportBean)});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(int[]), typeof(int?[])});
	        TryInvalidGetCommonCoercionType(new Type[]{typeof(object[]), typeof(bool[]), typeof(int?[])});

	        try {
	            TypeHelper.GetCommonCoercionType(new Type[0]);
	            Fail();
	        } catch (ArgumentException ex) {
	            // expected
	        }
	    }

        [Test]
	    public void TestGetPrimitiveClassForName() {
	        object[][] tests = new object[][]{
	                {"int", typeof(int)},
	                {"Long", typeof(long)},
	                {"SHort", typeof(short)},
	                {"DOUBLE", typeof(double)},
	                {"float", typeof(float)},
	                {"boolean", typeof(bool)},
	                {"ByTe", typeof(byte)},
	                {"char", typeof(char)},
	                {"jfjfjf", null},
	                {typeof(SupportBean).Name, null},
	                {"string", typeof(string)},
	                {"STRINg", typeof(string)}
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual(tests[i][1], TypeHelper.GetPrimitiveClassForName((string) tests[i][0]));
	        }
	    }

        [Test]
	    public void TestImplementsInterface() {
	        object[][] tests = new object[][]{
	                {typeof(Dictionary), typeof(IDictionary), true},
	                {typeof(AbstractMap), typeof(IDictionary), true},
	                {typeof(SortedDictionary), typeof(IDictionary), true},
	                {typeof(string), typeof(IDictionary), false},
	                {typeof(SupportBean_S0), typeof(SupportMarkerInterface), false},
	                {typeof(SupportBean_E), typeof(SupportMarkerInterface), true},
	                {typeof(SupportBean_F), typeof(SupportMarkerInterface), true},
	                {typeof(SupportBeanBase), typeof(SupportMarkerInterface), true},
	                {typeof(SupportOverrideOneB), typeof(SupportMarkerInterface), true}
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual("test failed for " + tests[i][0], tests[i][2], TypeHelper.IsImplementsInterface((Type) tests[i][0], (Type) tests[i][1]));
	        }
	    }

        [Test]
	    public void TestImplementsOrExtends() {
	        object[][] tests = new object[][]{
	                {typeof(Dictionary), typeof(IDictionary), true},
	                {typeof(AbstractMap), typeof(IDictionary), true},
	                {typeof(SortedDictionary), typeof(IDictionary), true},
	                {typeof(string), typeof(IDictionary), false},
	                {typeof(SupportBean_S0), typeof(SupportMarkerInterface), false},
	                {typeof(SupportBean_E), typeof(SupportMarkerInterface), true},
	                {typeof(SupportBean_F), typeof(SupportMarkerInterface), true},
	                {typeof(SupportBeanBase), typeof(SupportMarkerInterface), true},
	                {typeof(SupportOverrideOneB), typeof(SupportMarkerInterface), true},
	                {typeof(SupportOverrideBase), typeof(SupportOverrideBase), true},
	                {typeof(SupportBean_F), typeof(SupportOverrideBase), false},
	                {typeof(SupportOverrideOne), typeof(SupportOverrideBase), true},
	                {typeof(SupportOverrideOneA), typeof(SupportOverrideBase), true},
	                {typeof(SupportOverrideOneB), typeof(SupportOverrideBase), true},
	                {typeof(SupportOverrideOneB), typeof(Serializable), true},
	                {typeof(SupportOverrideOneB), typeof(string), false},
	                {typeof(IDictionary), typeof(object), true},
	                {typeof(int), typeof(object), false},
	                {typeof(long[]), typeof(long[]), true},
	                {typeof(long[][]), typeof(long[][]), true},
	                {typeof(long[][]), typeof(long[]), false},
	                {typeof(int?[][]), typeof(int?[]), false},
	                {typeof(object[][]), typeof(object[]), true},
	                {typeof(object[][]), typeof(object[][]), true},
	                {typeof(object[][][]), typeof(object[][]), true},
	                {typeof(Number[]), typeof(object[]), true},
	                {typeof(int?[]), typeof(object[]), true},
	                {typeof(int?[]), typeof(Number[]), true},
	                {typeof(int?[][]), typeof(Number[]), false},
	                {typeof(int?[][]), typeof(object[]), true},
	                {typeof(int[]), typeof(object), true},
	                {typeof(int?[][]), typeof(object), true}
	        };

	        for (int i = 0; i < tests.Length; i++) {
	            Assert.AreEqual("test failed for " + tests[i][0] + " and " + tests[i][1], tests[i][2],
	                    TypeHelper.IsSubclassOrImplementsInterface((Type) tests[i][0], (Type) tests[i][1]));
	        }
	    }

        [Test]
	    public void TestIsSimpleNameFullyQualfied() {
	        Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "ABC"));
	        Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "com.abc.ABC"));
	        Assert.IsTrue(TypeHelper.IsSimpleNameFullyQualfied("ABC", "abc.ABC"));
	        Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("DABC", "abc.ABC"));
	        Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("AB", "abc.ABC"));
	        Assert.IsFalse(TypeHelper.IsSimpleNameFullyQualfied("AB", "ABC"));
	    }

        [Test]
	    public void TestIsBigNumberType() {
	        Assert.IsTrue(TypeHelper.IsBigNumberType(typeof(BigInteger)));
	        Assert.IsTrue(TypeHelper.IsBigNumberType(typeof(BigDecimal)));
	        Assert.IsFalse(TypeHelper.IsBigNumberType(typeof(string)));
	        Assert.IsFalse(TypeHelper.IsBigNumberType(typeof(double?)));
	    }

        [Test]
	    public void TestGetGenericReturnType() {
	        object[][] testcases = new object[][]{
	                {"getList", typeof(string)},
	                {"getListObject", typeof(object)},
	                {"getListUndefined", null},
	                {"getIterator", typeof(int?)},
	                {"getNested", typeof(MyClassWithGetters)},
	                {"getIntPrimitive", null},
	                {"getIntBoxed", null},
	        };

	        for (int i = 0; i < testcases.Length; i++) {
	            string name = testcases[i][0].ToString();
	            Method m = typeof(MyClassWithGetters).GetMethod(name);
	            Type expected = (Type) testcases[i][1];
	            Assert.AreEqual("Testing " + name, expected, TypeHelper.GetGenericReturnType(m, true));
	        }
	    }

        [Test]
	    public void TestGetGenericFieldType() {
	        object[][] testcases = new object[][]{
	                {"list", typeof(string)},
	                {"listObject", typeof(object)},
	                {"listUndefined", null},
	                {"iterable", typeof(int?)},
	                {"nested", typeof(MyClassWithGetters)},
	                {"intPrimitive", null},
	                {"intBoxed", null},
	        };

	        for (int i = 0; i < testcases.Length; i++) {
	            string name = testcases[i][0].ToString();
	            Field f = typeof(MyClassWithFields).GetField(name);
	            Type expected = (Type) testcases[i][1];
	            Assert.AreEqual("Testing " + name, expected, TypeHelper.GetGenericFieldType(f, true));
	        }
	    }

        [Test]
	    public void TestGetGenericFieldTypeMap() {
	        object[][] testcases = new object[][]{
	                {"mapUndefined", null},
	                {"mapObject", typeof(object)},
	                {"mapBoolean", typeof(bool?)},
	                {"mapNotMap", null},
	        };

	        for (int i = 0; i < testcases.Length; i++) {
	            string name = testcases[i][0].ToString();
	            Field f = typeof(MyClassWithFields).GetField(name);
	            Type expected = (Type) testcases[i][1];
	            Assert.AreEqual("Testing " + name, expected, TypeHelper.GetGenericFieldTypeMap(f, true));
	        }
	    }

        [Test]
	    public void TestGetGenericReturnTypeMap() {
	        object[][] testcases = new object[][]{
	                {"getMapUndefined", null},
	                {"getMapObject", typeof(object)},
	                {"getMapBoolean", typeof(bool?)},
	                {"getMapNotMap", null},
	        };

	        for (int i = 0; i < testcases.Length; i++) {
	            string name = testcases[i][0].ToString();
	            Method m = typeof(MyClassWithGetters).GetMethod(name);
	            Type expected = (Type) testcases[i][1];
	            Assert.AreEqual("Testing " + name, expected, TypeHelper.GetGenericReturnTypeMap(m, true));
	        }
	    }

        [Test]
	    public void TestGetClassObjectFromPropertyTypeNames() {
	        Properties props = new Properties();
	        props.Put("p0", "string");
	        props.Put("p1", "int");
	        props.Put("p2", typeof(SupportBean).Name);

	        IDictionary<string, object> map = TypeHelper.GetClassObjectFromPropertyTypeNames(props, ClassForNameProviderDefault.INSTANCE);
	        Assert.AreEqual(typeof(string), map.Get("p0"));
	        Assert.AreEqual(typeof(int?), map.Get("p1"));
	        Assert.AreEqual(typeof(SupportBean), map.Get("p2"));
	    }

        [Test]
	    public void TestGetObjectValuePretty() {
	        AssertObjectValuePretty("(null)", null);
	        AssertObjectValuePretty("1", 1);
	        AssertObjectValuePretty("a", "a");
	        AssertObjectValuePretty("SupportBean(E1, 10)", new SupportBean("E1", 10));
	        AssertObjectValuePretty("[10, 20]", new int?[]{10, 20});
	        AssertObjectValuePretty("[10, 20]", new int[]{10, 20});
	        AssertObjectValuePretty("[a, b]", new string[]{"a", "b"});
	        AssertObjectValuePretty("[a, (null), b]", new string[]{"a", null, "b"});
	        AssertObjectValuePretty("[10, (null), 20]", new int?[]{10, null, 20});
	        AssertObjectValuePretty("[[10, 11], [20, 21]]", new int[][]{{10, 11}, {20, 21}});
	    }

        [Test]
	    public void TestGetArrayDimensions() {
	        Assert.AreEqual(0, GetArrayDimensions(typeof(int)));
	        Assert.AreEqual(1, GetArrayDimensions(typeof(int[])));
	        Assert.AreEqual(2, GetArrayDimensions(typeof(int[][])));
	        Assert.AreEqual(3, GetArrayDimensions(typeof(int[][][])));
	        Assert.AreEqual(1, GetArrayDimensions(typeof(int?[])));
	        Assert.AreEqual(1, GetArrayDimensions(typeof(object[])));
	    }

        [Test]
	    public void TestGetArrayComponentTypeInnermost() {
	        Assert.AreEqual(typeof(int), GetArrayComponentTypeInnermost(typeof(int)));
	        Assert.AreEqual(typeof(int), GetArrayComponentTypeInnermost(typeof(int[])));
	        Assert.AreEqual(typeof(int), GetArrayComponentTypeInnermost(typeof(int[][])));
	        Assert.AreEqual(typeof(int), GetArrayComponentTypeInnermost(typeof(int[][][])));
	        Assert.AreEqual(typeof(int?), GetArrayComponentTypeInnermost(typeof(int?[])));
	        Assert.AreEqual(typeof(object), GetArrayComponentTypeInnermost(typeof(object[])));
	    }

	    private void AssertObjectValuePretty(string expected, object input) {
	        StringWriter writer = new StringWriter();
	        TypeHelper.GetObjectValuePretty(input, writer);
	        Assert.AreEqual(expected, writer.ToString());
	    }

	    private string TryInvalidGetCommonCoercionType(Type[] types) {
	        try {
	            TypeHelper.GetCommonCoercionType(types);
	            Fail();
	            return null;
	        } catch (CoercionException ex) {
	            return ex.Message;
	        }
	    }

	    class MyStringList : List<string> {
	    }

	    class MyClassWithGetters {
	        public List<object> ListObject
	        {
	            get => null;
	        }

	        public List ListUndefined
	        {
	            get => null;
	        }

	        public List<string> List
	        {
	            get => null;
	        }

	        public IEnumerator<int?> Iterator
	        {
	            get => null;
	        }

	        public ISet<MyClassWithGetters> Nested
	        {
	            get => null;
	        }

	        public int? IntBoxed
	        {
	            get => null;
	        }

	        public int IntPrimitive
	        {
	            get => 1;
	        }

	        public IDictionary MapUndefined
	        {
	            get => null;
	        }

	        public IDictionary<string, object> GetMapObject() {
	            return null;
	        }

	        public IDictionary<string, Boolean> GetMapBoolean() {
	            return null;
	        }

	        public int? MapNotMap
	        {
	            get => null;
	        }
	    }

	    class MyClassWithFields {
	        public List<object> listObject;
	        public List listUndefined;
	        public List<string> list;
	        public Iterable<int?> iterable;
	        public ISet<MyClassWithGetters> nested;
	        public int? intBoxed;
	        public int intPrimitive;

	        public IDictionary mapUndefined;
	        public IDictionary<string, object> mapObject;
	        public IDictionary<string, Boolean> mapBoolean;
	        public int? mapNotMap;
	    }
	}
} // end of namespace