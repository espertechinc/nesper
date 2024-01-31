///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestArithTypeEnum : AbstractCommonTest
    {
        private void TryInvalid(Type clazz)
        {
            try
            {
                MathArithType.GetComputer(MathArithTypeEnum.ADD, clazz, clazz, clazz, false, false, null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void TestAddDouble()
        {
            var computer = MathArithType.GetComputer(
                MathArithTypeEnum.ADD, typeof(double?), typeof(double?), typeof(double?), false, false, null);
            ClassicAssert.AreEqual(12.1d, computer.Compute(5.5, 6.6));
        }

        [Test]
        public void TestAllComputers()
        {
            Type[] testClasses = {
                typeof(float), typeof(double), typeof(int), typeof(long)
            };

            foreach (var clazz in testClasses)
            {
                foreach (MathArithTypeEnum type in EnumHelper.GetValues<MathArithTypeEnum>())
                {
                    var computer = MathArithType.GetComputer(type, clazz, clazz, clazz, false, false, null);
                    var result = computer.Compute(3, 4);

                    if (type == MathArithTypeEnum.ADD)
                    {
                        ClassicAssert.AreEqual(clazz, result.GetType());
                        ClassicAssert.AreEqual(7d, result.AsDouble());
                    }

                    if (type == MathArithTypeEnum.SUBTRACT)
                    {
                        ClassicAssert.AreEqual(clazz, result.GetType());
                        ClassicAssert.AreEqual(-1d, result.AsDouble());
                    }

                    if (type == MathArithTypeEnum.MULTIPLY)
                    {
                        ClassicAssert.AreEqual(clazz, result.GetType());
                        ClassicAssert.AreEqual(12d, result.AsDouble());
                    }

                    if (type == MathArithTypeEnum.DIVIDE)
                    {
                        ClassicAssert.AreEqual(typeof(double), result.GetType());
                        if (clazz == typeof(int?) || clazz == typeof(long?))
                        {
                            ClassicAssert.AreEqual(0.75d, result.AsDouble(), "clazz=" + clazz);
                        }
                        else
                        {
                            ClassicAssert.AreEqual(3 / 4d, result.AsDouble(), "clazz=" + clazz);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestBigNumberComputers()
        {
            object[][] parameters = {
                new object[] {true, 6.0m, MathArithTypeEnum.DIVIDE, 3.0m, 2.0m},
                new object[] {false, new BigInteger(10), MathArithTypeEnum.ADD, new BigInteger(10), new BigInteger(20)},
                new object[] {false, new BigInteger(100), MathArithTypeEnum.SUBTRACT, new BigInteger(10), new BigInteger(90)},
                new object[] {false, new BigInteger(10), MathArithTypeEnum.MULTIPLY, new BigInteger(10), new BigInteger(100)},
                new object[] {false, new BigInteger(100), MathArithTypeEnum.DIVIDE, new BigInteger(5), new BigInteger(20)},

                new object[] {false, 9, MathArithTypeEnum.ADD, new BigInteger(10), new BigInteger(19)},
                new object[] {false, new BigInteger(6), MathArithTypeEnum.SUBTRACT, (byte) 7, new BigInteger(-1)},
                new object[] {false, new BigInteger(10), MathArithTypeEnum.DIVIDE, (long) 4, new BigInteger(2)},
                new object[] {false, new BigInteger(6), MathArithTypeEnum.MULTIPLY, (byte) 7, new BigInteger(42)},

                new object[] {true, new BigInteger(6), MathArithTypeEnum.ADD, (double) 7, 13.0m},
                new object[] {true, new BigInteger(6), MathArithTypeEnum.SUBTRACT, (double) 5, 1.0m},
                new object[] {true, new BigInteger(6), MathArithTypeEnum.MULTIPLY, (double) 5, 30.0m},
                new object[] {true, new BigInteger(6), MathArithTypeEnum.DIVIDE, (double) 2, 3m},

                new object[] {true, 9, MathArithTypeEnum.ADD, 10m, 19m},
                new object[] {true, 6m, MathArithTypeEnum.SUBTRACT, 5m, 1m},
                new object[] {true, 6m, MathArithTypeEnum.MULTIPLY, 5m, 30m},
                new object[] {true, 6m, MathArithTypeEnum.ADD, 7m, 13.0m},

                new object[] {true, 10.0m, MathArithTypeEnum.ADD, (long) 8, 18.0m},
                new object[] {true, 10.0m, MathArithTypeEnum.DIVIDE, (long) 8, 1.25m},
                new object[] {true, 6.0m, MathArithTypeEnum.SUBTRACT, (byte) 7, -1.0m},
                new object[] {true, 6.0m, MathArithTypeEnum.MULTIPLY, (byte) 7, 42.0m},

                new object[] {true, 6.0m, MathArithTypeEnum.MULTIPLY, (double) 3, 18.0m},
                new object[] {true, 6.0m, MathArithTypeEnum.ADD, (double) 2, 8.0m},
                new object[] {true, 6.0m, MathArithTypeEnum.DIVIDE, (double) 4, 1.5m},
                new object[] {true, 6.0m, MathArithTypeEnum.SUBTRACT, (double) 8, -2.0m}
            };

            for (var i = 0; i < parameters.Length; i++)
            {
                var isBigDec = (bool) parameters[i][0];
                var lhs = parameters[i][1];
                var e = (MathArithTypeEnum) parameters[i][2];
                var rhs = parameters[i][3];
                var expected = parameters[i][4];

                MathArithType.Computer computer;
                if (isBigDec)
                {
                    computer = MathArithType.GetComputer(e, typeof(decimal), lhs.GetType(), rhs.GetType(), false, false, null);
                }
                else
                {
                    computer = MathArithType.GetComputer(e, typeof(BigInteger), lhs.GetType(), rhs.GetType(), false, false, null);
                }

                object result = null;
                try
                {
                    result = computer.Compute(lhs, rhs);
                }
                catch (Exception ex) {
                    Log.Error("Exception expected", ex);
                }

                ClassicAssert.AreEqual(expected, result, "line " + i + " lhs=" + lhs + " op=" + e + " rhs=" + rhs);
            }
        }

        [Test]
        public void TestInvalidGetComputer()
        {
            // Since we only do Double, Float, Integer and Long as results
            TryInvalid(typeof(string));
            TryInvalid(typeof(char));
            TryInvalid(typeof(short));
            TryInvalid(typeof(byte));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
