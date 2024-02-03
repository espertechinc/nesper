///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestRelationalOpEnum : AbstractCommonTest
    {
        private readonly IDictionary<RelationalOpEnum, bool[]> expected =
            new Dictionary<RelationalOpEnum, bool[]>();

        public TestRelationalOpEnum()
        {
            expected[RelationalOpEnum.GT] = new bool[] {false, false, true};
            expected[RelationalOpEnum.GE] = new bool[] {false, true, true};
            expected[RelationalOpEnum.LT] = new bool[] {true, false, false};
            expected[RelationalOpEnum.LE] = new bool[] {true, true, false};
        }

        private void TryInvalid(Type clazz)
        {
            try
            {
                RelationalOpEnum.GE.GetComputer(clazz, clazz, clazz);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public void TestBigNumberComputers()
        {
            object[][] parameters = {
                new object[] {false, new BigInteger(10), RelationalOpEnum.LE, new BigInteger(10), true},
                new object[] {false, new BigInteger(10), RelationalOpEnum.GE, new BigInteger(10), true},
                new object[] {false, new BigInteger(10), RelationalOpEnum.LT, new BigInteger(10), false},
                new object[] {false, new BigInteger(10), RelationalOpEnum.GT, new BigInteger(10), false},
                new object[] {false, 9, RelationalOpEnum.GE, new BigInteger(10), false},
                new object[] {false, new BigInteger(10), RelationalOpEnum.LE, (long) 10, true},
                new object[] {false, new BigInteger(6), RelationalOpEnum.LT, (byte) 7, true},
                new object[] {false, new BigInteger(6), RelationalOpEnum.GT, (byte) 7, false},
                new object[] {false, new BigInteger(6), RelationalOpEnum.GT, (double) 6, false},
                new object[] {false, new BigInteger(6), RelationalOpEnum.GE, (double) 6, true},
                new object[] {false, new BigInteger(6), RelationalOpEnum.LE, (double) 6, true},
                new object[] {false, new BigInteger(6), RelationalOpEnum.LT, (double) 6, false},
                new object[] {true, 9, RelationalOpEnum.GE, 10.0m, false},
                new object[] {true, 6.0m, RelationalOpEnum.LT, 6.0m, false},
                new object[] {true, 6.0m, RelationalOpEnum.GT, 6.0m, false},
                new object[] {true, 6.0m, RelationalOpEnum.GE, 6.0m, true},
                new object[] {true, 6.0m, RelationalOpEnum.LE, 6.0m, true},
                new object[] {true, 10.0m, RelationalOpEnum.LE, (long) 10, true},
                new object[] {true, 6.0m, RelationalOpEnum.LT, (byte) 7, true},
                new object[] {true, 6.0m, RelationalOpEnum.GT, (byte) 7, false},
                new object[] {true, 6.0m, RelationalOpEnum.GT, (double) 6, false},
                new object[] {true, 6.0m, RelationalOpEnum.GE, (double) 6, true},
                new object[] {true, 6.0m, RelationalOpEnum.LE, (double) 6, true},
                new object[] {true, 6.0m, RelationalOpEnum.LT, (double) 6, false}
            };

            for (var i = 0; i < parameters.Length; i++)
            {
                var isDecimal = (bool) parameters[i][0];
                var lhs = parameters[i][1];
                var e = (RelationalOpEnum) parameters[i][2];
                var rhs = parameters[i][3];
                var expected = parameters[i][4];

                RelationalOpEnumComputer computer;
                if (isDecimal)
                {
                    computer = e.GetComputer(typeof(decimal), lhs.GetType(), rhs.GetType());
                }
                else
                {
                    computer = e.GetComputer(typeof(BigInteger), lhs.GetType(), rhs.GetType());
                }

                object result = computer.Compare(lhs, rhs);

                ClassicAssert.AreEqual(expected, result, "line " + i + " lhs=" + lhs + " op=" + e + " rhs=" + rhs);
            }
        }

        [Test]
        public void TestDoubleComputers()
        {
            double[][] parameters = {
                new double[] {1, 2},
                new double[] {1, 1},
                new double[] {2, 1}
            };

            foreach (var op in EnumHelper.GetValues<RelationalOpEnum>())
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var result = op
                        .GetComputer(
                            typeof(double?),
                            typeof(double),
                            typeof(double?))
                        .Compare(
                            parameters[i][0],
                            parameters[i][1]);
                    ClassicAssert.AreEqual(expected[op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }

        [Test]
        public void TestInvalidGetComputer()
        {
            // Since we only do double, long and string compares
            TryInvalid(typeof(bool));
            TryInvalid(typeof(byte));
            TryInvalid(typeof(short));
            TryInvalid(typeof(SupportBean));
        }

        [Test]
        public void TestLongComputers()
        {
            long[][] parameters = {
                new long[] {1, 2},
                new long[] {1, 1},
                new long[] {2, 1}
            };

            foreach (var op in EnumHelper.GetValues<RelationalOpEnum>())
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var result = op
                        .GetComputer(
                            typeof(long?),
                            typeof(long?),
                            typeof(long))
                        .Compare(
                            parameters[i][0],
                            parameters[i][1]);
                    ClassicAssert.AreEqual(expected[op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }

        [Test]
        public void TestStringComputers()
        {
            string[][] parameters = {
                new[] {"a", "b"},
                new[] {"a", "a"},
                new[] {"b", "a"}
            };

            foreach (var op in EnumHelper.GetValues<RelationalOpEnum>())
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var result = op
                        .GetComputer(
                            typeof(string),
                            typeof(string),
                            typeof(string))
                        .Compare(
                            parameters[i][0],
                            parameters[i][1]);
                    ClassicAssert.AreEqual(expected[op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }
    }
} // end of namespace
