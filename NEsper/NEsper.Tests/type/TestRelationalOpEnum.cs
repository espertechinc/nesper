///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestRelationalOpEnum
    {
        private readonly bool[][] expected = new bool[][]
        {
            new bool[]
            {
                false, false, true
            }, // GT
            new bool[]
            {
                false, true, true
            }, // GE
            new bool[]
            {
                true, false, false
            }, // LT
            new bool[]
            {
                true, true, false
            }, // LE
        };
    
        [Test]
        public void TestStringComputers()
        {
            var parameters = new String[][]
            {
                new string[]
                {
                    "a", "b"
                },
                new string[]
                {
                    "a", "a"
                },
                new string[]
                {
                    "b", "a"
                }
            };
    
            foreach (RelationalOpEnum op in EnumHelper.GetValues<RelationalOpEnum>()) {
                for (int i = 0; i < parameters.Length; i++) {
                    var result = op.GetComputer(typeof(string), typeof(string), typeof(string)).Invoke(
                            parameters[i][0], parameters[i][1]);

                    Assert.AreEqual(expected[(int) op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }
    
        [Test]
        public void TestLongComputers()
        {
            long[][] parameters = new long[][]
            {
                new long[]
                {
                    1, 2
                },
                new long[]
                {
                    1, 1
                },
                new long[]
                {
                    2, 1
                }
            };

            foreach (RelationalOpEnum op in EnumHelper.GetValues<RelationalOpEnum>())
            {
                for (int i = 0; i < parameters.Length; i++) {
                    var result = op.GetComputer(typeof(long), typeof(long), typeof(long)).Invoke(
                            parameters[i][0], parameters[i][1]);
    
                    Assert.AreEqual(expected[(int) op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }
    
        [Test]
        public void TestDoubleComputers()
        {
            var parameters = new double[][]
            {
                new double[]
                {
                    1, 2
                },
                new double[]
                {
                    1, 1
                },
                new double[]
                {
                    2, 1
                }
            };

            foreach (RelationalOpEnum op in EnumHelper.GetValues<RelationalOpEnum>())
            {
                for (int i = 0; i < parameters.Length; i++) {
                    var result = op.GetComputer(typeof(double?), typeof(double), typeof(double?)).Invoke(
                            parameters[i][0], parameters[i][1]);
    
                    Assert.AreEqual(expected[(int) op][i], result, "op=" + op + ",i=" + i);
                }
            }
        }
    
        [Test]
        public void TestBigNumberComputers()
        {
            var parameters = new Object[][]
            {
                new Object[]
                {
                    true, 9, RelationalOpEnum.GE, 10.0m, false
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.LT, 6.0m,
                    false
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.GT, 6.0m,
                    false
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.GE, 6.0m,
                    true
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.LE, 6.0m,
                    true
                },
                new Object[]
                {
                    true, 10.0m, RelationalOpEnum.LE, (long) 10, true
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.LT, (byte) 7, true
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.GT, (byte) 7, false
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.GT, (double) 6, false
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.GE, (double) 6, true
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.LE, (double) 6, true
                },
                new Object[]
                {
                    true, 6.0m, RelationalOpEnum.LT, (double) 6, false
                },
            };
    
            for (int i = 0; i < parameters.Length; i++) {
                var isBigDec = (bool?) parameters[i][0];
                var lhs = parameters[i][1];
                var e = (RelationalOpEnum) parameters[i][2];
                var rhs = parameters[i][3];
                var expected = parameters[i][4];
    
                RelationalOpEnumExtensions.Computer computer;

                if (isBigDec.GetValueOrDefault())
                {
                    computer = e.GetComputer(typeof(decimal), lhs.GetType(),
                            rhs.GetType());
                }
                else
                {
                    throw new Exception("unexpected, use cases not applicable");
                }

                Object result = null;
    
                try {
                    result = computer.Invoke(lhs, rhs);
                } 
                catch (Exception ex)
                {
                    Console.Error.WriteLine("{0}", ex.StackTrace);
                }
                Assert.AreEqual(
                    expected,
                    result,
                    "line " + i + " lhs=" + lhs + " op=" + e + " rhs=" + rhs);
            }
        }
    
        [Test]
        public void TestInvalidGetComputer()
        {
            TryInvalid(typeof(Object));
            TryInvalid(typeof(Random));
            TryInvalid(typeof(SupportBean));
        }
    
        private void TryInvalid(Type clazz) {
            try {
                RelationalOpEnum.GE.GetComputer(clazz, clazz, clazz);
                Assert.Fail();
            } catch (ArgumentException ex) {// Expected
            }
        }
    }
}
