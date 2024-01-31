///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogRegex : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            Run(
                new SupportTestCaseHolder("a,b,c,d", "(A?) (B)? (C D)?")
                    .Add(
                        "a",
                        new[] {
                            "\"a\",null,null,null"
                        })
                    .Add(
                        "b",
                        new[] {
                            "null,\"b\",null,null"
                        })
                    .Add("x", null)
                    .Add("d", null)
                    .Add("c", null)
                    .Add("d,c", null)
                    .Add(
                        "c,d",
                        new[] {
                            "null,null,\"c\",\"d\""
                        })
                    .Add(
                        "a,c,d",
                        new[] {
                            "\"a\",null,null,null",
                            "\"a\",null,\"c\",\"d\"",
                            "null,null,\"c\",\"d\""
                        })
                    .Add(
                        "b,c,d",
                        new[] {
                            "null,\"b\",null,null",
                            "null,null,\"c\",\"d\"",
                            "null,\"b\",\"c\",\"d\""
                        })
                    .Add(
                        "a,b,c,d",
                        new[] {
                            "\"a\",\"b\",null,null",
                            "\"a\",null,null,null",
                            "null,\"b\",null,null",
                            "null,\"b\",\"c\",\"d\"",
                            "\"a\",\"b\",\"c\",\"d\"",
                            "null,null,\"c\",\"d\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c,d", "(A | B) (C | D)")
                    .Add("a", null)
                    .Add("c", null)
                    .Add("d,c", null)
                    .Add("a,b", null)
                    .Add(
                        "a,d",
                        new[] {
                            "\"a\",null,null,\"d\""
                        })
                    .Add(
                        "a,d,c",
                        new[] {
                            "\"a\",null,null,\"d\""
                        })
                    .Add(
                        "b,c",
                        new[] {
                            "null,\"b\",\"c\",null"
                        })
                    .Add(
                        "b,d",
                        new[] {
                            "null,\"b\",null,\"d\""
                        })
                    .Add(
                        "b,a,d,c",
                        new[] {
                            "\"a\",null,null,\"d\""
                        })
                    .Add(
                        "x,a,x,b,x,b,c,x",
                        new[] {
                            "null,\"b\",\"c\",null"
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c,d,e", "A ((B C)? | (D E)?)")
                    .Add(
                        "a",
                        new[] {
                            "\"a\",null,null,null,null"
                        })
                    .Add(
                        "a,b,c",
                        new[] {
                            "\"a\",null,null,null,null", "\"a\",\"b\",\"c\",null,null"
                        })
                    .Add(
                        "a,d,e",
                        new[] {
                            "\"a\",null,null,null,null", "\"a\",null,null,\"d\",\"e\""
                        })
                    .Add("b,c", null)
                    .Add("x,d,e", null),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c", "(A? B) | (A? C)")
                    .Add("a", null)
                    .Add(
                        "a,b",
                        new[] {
                            "\"a\",\"b\",null", "null,\"b\",null"
                        })
                    .Add(
                        "a,c",
                        new[] {
                            "\"a\",null,\"c\"", "null,null,\"c\""
                        })
                    .Add(
                        "b",
                        new[] {
                            "null,\"b\",null"
                        })
                    .Add(
                        "c",
                        new[] {
                            "null,null,\"c\""
                        })
                    .Add(
                        "a,x,b",
                        new[] {
                            "null,\"b\",null"
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c", "(A B? C)?")
                    .Add("x", null)
                    .Add("a", null)
                    .Add(
                        "a,c",
                        new[] {
                            "\"a\",null,\"c\""
                        })
                    .Add(
                        "a,b,c",
                        new[] {
                            "\"a\",\"b\",\"c\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c", "(A? B C?)")
                    .Add("x", null)
                    .Add("a", null)
                    .Add("a,c", null)
                    .Add(
                        "b",
                        new[] {
                            "null,\"b\",null"
                        })
                    .Add(
                        "a,b,c",
                        new[] {
                            "\"a\",\"b\",null",
                            "null,\"b\",null",
                            "\"a\",\"b\",\"c\"",
                            "null,\"b\",\"c\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a[0],b[0],a[1],b[1],c,d", "(A B)* C D")
                    .Add(
                        "c,d",
                        new[] {
                            "null,null,null,null,\"c\",\"d\""
                        })
                    .Add(
                        "a1,b1,c,d",
                        new[] {
                            "\"a1\",\"b1\",null,null,\"c\",\"d\"",
                            "null,null,null,null,\"c\",\"d\""
                        })
                    .Add(
                        "a2,b2,x,c,d",
                        new[] {
                            "null,null,null,null,\"c\",\"d\""
                        })
                    .Add(
                        "a1,b1,a2,b2,c,d",
                        new[] {
                            "null,null,null,null,\"c\",\"d\"",
                            "\"a2\",\"b2\",null,null,\"c\",\"d\"",
                            "\"a1\",\"b1\",\"a2\",\"b2\",\"c\",\"d\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a[0],b[0],c[0],a[1],b[1],c[1],d[0],e[0],d[1],e[1]", "(A (B C))* (D E)+")
                    .Add("a,b,c", null)
                    .Add(
                        "d,e",
                        new[] {
                            "null,null,null,null,null,null,\"d\",\"e\",null,null"
                        })
                    .Add(
                        "a,b,c,d,e",
                        new[] {
                            "\"a\",\"b\",\"c\",null,null,null,\"d\",\"e\",null,null",
                            "null,null,null,null,null,null,\"d\",\"e\",null,null"
                        })
                    .Add(
                        "a1,b1,c1,a2,b2,c2,d,e",
                        new[] {
                            "\"a1\",\"b1\",\"c1\",\"a2\",\"b2\",\"c2\",\"d\",\"e\",null,null",
                            "\"a2\",\"b2\",\"c2\",null,null,null,\"d\",\"e\",null,null",
                            "null,null,null,null,null,null,\"d\",\"e\",null,null"
                        })
                    .Add(
                        "d1,e1,d2,e2",
                        new[] {
                            "null,null,null,null,null,null,\"d1\",\"e1\",null,null",
                            "null,null,null,null,null,null,\"d2\",\"e2\",null,null",
                            "null,null,null,null,null,null,\"d1\",\"e1\",\"d2\",\"e2\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a[0],a[1],d[0],e[0],d[1],e[1]", "A+ (D E)+")
                    .Add("a,e,a,d,d,e,a,e,e,a,d,d,e,d,e", null)
                    .Add(
                        "a,d,e",
                        new[] {
                            "\"a\",null,\"d\",\"e\",null,null"
                        })
                    .Add(
                        "a1,a2,d,e",
                        new[] {
                            "\"a1\",\"a2\",\"d\",\"e\",null,null",
                            "\"a2\",null,\"d\",\"e\",null,null"
                        })
                    .Add(
                        "a1,d1,e1,d2,e2",
                        new[] {
                            "\"a1\",null,\"d1\",\"e1\",null,null",
                            "\"a1\",null,\"d1\",\"e1\",\"d2\",\"e2\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c,d,e,f", "(A (B | C)) | (D (E | F))")
                    .Add("a,e,d,b,a,f,f,d,c,a", null)
                    .Add(
                        "a,f,c,b,a,d,f",
                        new[] {
                            "null,null,null,\"d\",null,\"f\""
                        })
                    .Add(
                        "c,b,d,a,b,x,y",
                        new[] {
                            "\"a\",\"b\",null,null,null,null"
                        })
                    .Add(
                        "a,d,c,f,d,e,x,a,c",
                        new[] {
                            "null,null,null,\"d\",\"e\",null",
                            "\"a\",null,\"c\",null,null,null"
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a,b,c,d,e,f", "(A (B | C)? ) | (D? (E | F))")
                    .Add(
                        "a1,f1,c,b,a2,d,f2",
                        new[] {
                            "\"a1\",null,null,null,null,null",
                            "\"a2\",null,null,null,null,null",
                            "null,null,null,null,null,\"f1\"",
                            "null,null,null,null,null,\"f2\"",
                            "null,null,null,\"d\",null,\"f2\""
                        })
                    .Add(
                        "d,f",
                        new[] {
                            "null,null,null,\"d\",null,\"f\"",
                            "null,null,null,null,null,\"f\""
                        }),
                env);

            Run(
                new SupportTestCaseHolder("a[0],a[1],b,c,d", "(A B C) | (A+ B D)")
                    .Add(
                        "a1,c,a2,b,d",
                        new[] {
                            "\"a2\",null,\"b\",null,\"d\""
                        })
                    .Add(
                        "a1,b1,x,a2,b2,c1",
                        new[] {
                            "\"a2\",null,\"b2\",\"c1\",null"
                        }),
                env);
        }

        private void Run(
            SupportTestCaseHolder testDesc,
            RegressionEnvironment env)
        {
            var buf = new StringBuilder();
            buf.Append(
                "@name('s0') select * from SupportRecogBean#keepall " +
                "match_recognize (\n" +
                "  measures ");

            var delimiter = "";
            foreach (var measure in testDesc.Measures.SplitCsv()) {
                buf.Append(delimiter);
                buf.Append(measure.ToUpperInvariant())
                    .Append(".TheString as ")
                    .Append(ReplaceBrackets(measure))
                    .Append("val");
                delimiter = ",";
            }

            buf.Append("\n all matches ");
            buf.Append("\n after match skip to current row ");
            buf.Append("\n pattern (").Append(testDesc.Pattern).Append(") \n");
            buf.Append("  define ");

            ISet<string> defines = new HashSet<string>();
            foreach (var measure in testDesc.Measures.SplitCsv()) {
                defines.Add(RemoveBrackets(measure).ToUpperInvariant());
            }

            delimiter = "";
            foreach (var define in defines) {
                buf.Append(delimiter);
                buf.Append(define)
                    .Append(" as (")
                    .Append(define)
                    .Append(".TheString like '")
                    .Append(define.ToLowerInvariant())
                    .Append("%')");
                delimiter = ",\n";
            }

            buf.Append(")");

            var compiled = env.Compile(buf.ToString());

            foreach (var testcase in testDesc.TestCases) {
                env.Deploy(compiled).AddListener("s0");

                var count = 0;
                foreach (var testchar in testcase.Testdata.SplitCsv()) {
                    env.SendEventBean(new SupportRecogBean(testchar, count++));
                }

                env.AssertIterator(
                    "s0",
                    enumerator => {
                        var iteratorData = EPAssertionUtil.EnumeratorToArray(enumerator);
                        Compare(testcase.Testdata, iteratorData, testDesc.Measures, testcase);
                    });

                env.AssertListener(
                    "s0",
                    listener => {
                        var listenerData = listener.NewDataListFlattened;
                        listener.Reset();
                        Compare(testcase.Testdata, listenerData, testDesc.Measures, testcase);
                    });

                env.UndeployModuleContaining("s0");
            }
        }

        private string ReplaceBrackets(string indexed)
        {
            return indexed.Replace("[", "").Replace("]", "");
        }

        private string RemoveBrackets(string indexed)
        {
            var index = indexed.IndexOf('[');
            if (index == -1) {
                return indexed;
            }

            return indexed.Substring(0, indexed.Length - index - 2);
        }

        private void Compare(
            string sent,
            EventBean[] received,
            string measures,
            SupportTestCaseItem testDesc)
        {
            var message = "For sent: " + sent;
            if (testDesc.Expected == null) {
                ClassicAssert.AreEqual(0, received.Length, message);
                return;
            }

            var receivedText = new string[received.Length];
            for (var i = 0; i < received.Length; i++) {
                var buf = new StringBuilder();
                var delimiter = "";
                foreach (var measure in measures.SplitCsv()) {
                    buf.Append(delimiter);
                    var value = received[i].Get(ReplaceBrackets(measure) + "val");
                    buf.Append(value.RenderAny());
                    delimiter = ",";
                }

                receivedText[i] = buf.ToString();
            }

            if (testDesc.Expected.Length != received.Length) {
                Log.Info("expected: " + testDesc.Expected.RenderAny());
                Log.Info("received: " + receivedText.RenderAny());
                ClassicAssert.AreEqual(testDesc.Expected.Length, received.Length, message);
            }

            Log.Debug("comparing: " + message);
            EPAssertionUtil.AssertEqualsAnyOrder(testDesc.Expected, receivedText);
        }
    }
} // end of namespace