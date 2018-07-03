///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestArithTypeEnum 
    {
        [Test]
        public void TestAddDouble()
        {
            MathArithTypeEnumExtensions.Computer computer = MathArithTypeEnum.ADD.GetComputer(typeof(double?), typeof(double?), typeof(double?), false, false, null);
            Assert.AreEqual(12.1d, computer.Invoke(5.5,6.6));
        }
    
        [Test]
        public void TestInvalidGetComputer()
        {
            // Since we only do double?, Float, Integer and Long as results
            TryInvalid(typeof(string));
            //TryInvalid(typeof(long));
            TryInvalid(typeof(short));
            TryInvalid(typeof(byte));
        }
    
        [Test]
        public void TestAllComputers()
        {
            Type[] testClasses =
                {
                    typeof(float),
                    typeof(double),
                    typeof(decimal),
                    typeof(int),
                    typeof(long)
                };
    
            foreach (Type clazz in testClasses)
            {
                foreach (MathArithTypeEnum type in EnumHelper.GetValues<MathArithTypeEnum>())
                {
                    var computer = type.GetComputer(clazz,clazz,clazz, false, false, null);
                    var result = computer.Invoke(3, 4);
    
                    if (Equals(type, MathArithTypeEnum.ADD))
                    {
                        Assert.AreEqual(clazz, result.GetType());
                        Assert.AreEqual(7d, Convert.ToDouble(result));
                    }
                    if (Equals(type, MathArithTypeEnum.SUBTRACT))
                    {
                        Assert.AreEqual(clazz, result.GetType());
                        Assert.AreEqual(-1d, Convert.ToDouble(result));
                    }
                    if (Equals(type, MathArithTypeEnum.MULTIPLY))
                    {
                        Assert.AreEqual(clazz, result.GetType());
                        Assert.AreEqual(12d, Convert.ToDouble(result));
                    }
                    if (Equals(type, MathArithTypeEnum.DIVIDE))
                    {
                        if (clazz == typeof(decimal))
                            Assert.AreEqual(typeof(decimal), result.GetType());
                        else
                            Assert.AreEqual(typeof(double), result.GetType());

                        if ((clazz == typeof(int)) || (clazz == typeof(long)))
                        {
                            Assert.AreEqual(0.75d,Convert.ToDouble(result),"clazz=" + clazz);
                        }
                        else
                        {
                            Assert.AreEqual(3/4d,Convert.ToDouble(result),"clazz=" + clazz);
                        }
                    }
                }
            }
        }
    
        [Test]
        public void TestDecimalComputers()
        {
            var paramList = new[]
                            {
                                new Object[] {6m, MathArithTypeEnum.DIVIDE, 3m, 2m},

                                new Object[] {9, MathArithTypeEnum.ADD, 10m, 19m},
                                new Object[] {6m, MathArithTypeEnum.SUBTRACT, 5m, 1m},
                                new Object[] {6m, MathArithTypeEnum.MULTIPLY, 5m, 30m},
                                new Object[] {6m, MathArithTypeEnum.ADD, 7m, 13m},

                                new Object[] {10m, MathArithTypeEnum.ADD, (long) 8, 18m},
                                new Object[] {10m, MathArithTypeEnum.DIVIDE, (long) 8, 1.25m},
                                new Object[] {6m, MathArithTypeEnum.SUBTRACT, (byte) 7, -1m},
                                new Object[] {6m, MathArithTypeEnum.MULTIPLY, (byte) 7, 42m},

                                new Object[] {6m, MathArithTypeEnum.MULTIPLY, (double) 3, 18.0m},
                                new Object[] {6m, MathArithTypeEnum.ADD, (double) 2, 8.0m},
                                new Object[] {6m, MathArithTypeEnum.DIVIDE, (double) 4, 1.5m},
                                new Object[] {6m, MathArithTypeEnum.SUBTRACT, (double) 8, -2.0m},
                            };
    
            for (int i = 0; i < paramList.Length; i++)
            {
                var lhs = paramList[i][0];
                var e = (MathArithTypeEnum) paramList[i][1];
                var rhs = paramList[i][2];
                var expected = paramList[i][3];

                var computer = e.GetComputer(typeof(decimal), lhs.GetType(), rhs.GetType(), false, false, null);
    
                Object result = null;
                try
                {
                    result = computer.Invoke(lhs, rhs);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
                Assert.AreEqual(expected,result,"line " + i + " lhs=" + lhs + " op=" + e.ToString() + " rhs=" + rhs);
            }
        }
    
        private static void TryInvalid(Type clazz)
        {
            try
            {
                MathArithTypeEnum.ADD.GetComputer(clazz, clazz, clazz, false, false, null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    }
}
