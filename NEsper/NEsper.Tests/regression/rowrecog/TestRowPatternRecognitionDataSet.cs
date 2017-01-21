///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionDataSet
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            config.AddEventType<SupportBean>();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestExampleFinancialWPattern() {
            String text = "select * " + "from SupportBean " + "match_recognize ("
                    + " measures A.TheString as beginA, last(Z.TheString) as lastZ"
                    + " all matches" + " after match skip to current row"
                    + " pattern (A W+ X+ Y+ Z+)" + " define"
                    + " W as W.IntPrimitive<prev(W.IntPrimitive),"
                    + " X as X.IntPrimitive>prev(X.IntPrimitive),"
                    + " Y as Y.IntPrimitive<prev(Y.IntPrimitive),"
                    + " Z as Z.IntPrimitive>prev(Z.IntPrimitive)" + ")";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            Object[][] data = new Object[][] {
                new Object[] {
                    "E1", 8
                }, // 0
                new Object[] {
                    "E2", 8
                },
                new Object[] {
                    "E3", 8
                }, // A
                new Object[] {
                    "E4", 6
                }, // W
                new Object[] {
                    "E5", 3
                }, // W
                new Object[] {
                    "E6", 7
                },
                new Object[] {
                    "E7", 6
                },
                new Object[] {
                    "E8", 2
                },
                new Object[] {
                    "E9", 6, // Z
                    new String[] {
                        "beginA=E3,lastZ=E9", "beginA=E4,lastZ=E9"
                    }
                },
                new Object[] {
                    "E10", 2
                },
                new Object[] {
                    "E11", 9, // 10
                    new String[] {
                        "beginA=E6,lastZ=E11", "beginA=E7,lastZ=E11"
                    }
                },
                new Object[] {
                    "E12", 9
                },
                new Object[] {
                    "E13", 8
                },
                new Object[] {
                    "E14", 5
                },
                new Object[] {
                    "E15", 0
                },
                new Object[] {
                    "E16", 9
                },
                new Object[] {
                    "E17", 2
                },
                new Object[] {
                    "E18", 0
                },
                new Object[] {
                    "E19", 2,
                    new String[] {
                        "beginA=E12,lastZ=E19", "beginA=E13,lastZ=E19",
                        "beginA=E14,lastZ=E19"
                    }
                },
                new Object[] {
                    "E20", 3,
                    new String[] {
                        "beginA=E12,lastZ=E20", "beginA=E13,lastZ=E20",
                        "beginA=E14,lastZ=E20"
                    }
                },
                new Object[] {
                    "E21", 8,
                    new String[] {
                        "beginA=E12,lastZ=E21", "beginA=E13,lastZ=E21",
                        "beginA=E14,lastZ=E21"
                    }
                },
                new Object[] {
                    "E22", 5
                },
                new Object[] {
                    "E23", 9, new String[] {
                        "beginA=E16,lastZ=E23", "beginA=E17,lastZ=E23"
                    }
                },
                new Object[] {
                    "E24", 9
                },
                new Object[] {
                    "E25", 4
                },
                new Object[] {
                    "E26", 7
                },
                new Object[] {
                    "E27", 2
                },
                new Object[] {
                    "E28", 8, new String[] {
                        "beginA=E24,lastZ=E28"
                    }
                },
                new Object[] {
                    "E29", 0
                },
                new Object[] {
                    "E30", 4, new String[] {
                        "beginA=E26,lastZ=E30"
                    }
                },
                new Object[] {
                    "E31", 4
                },
                new Object[] {
                    "E32", 7
                },
                new Object[] {
                    "E33", 8
                },
                new Object[] {
                    "E34", 6
                },
                new Object[] {
                    "E35", 4
                },
                new Object[] {
                    "E36", 5
                },
                new Object[] {
                    "E37", 1
                },
                new Object[] {
                    "E38", 7, new String[] {
                        "beginA=E33,lastZ=E38", "beginA=E34,lastZ=E38"
                    }
                },
                new Object[] {
                    "E39", 5
                },
                new Object[] {
                    "E40", 8, new String[] {
                        "beginA=E36,lastZ=E40"
                    }
                },
                new Object[] {
                    "E41", 6
                },
                new Object[] {
                    "E42", 6
                },
                new Object[] {
                    "E43", 0
                },
                new Object[] {
                    "E44", 6
                },
                new Object[] {
                    "E45", 8
                },
                new Object[] {
                    "E46", 4
                },
                new Object[] {
                    "E47", 3
                },
                new Object[] {
                    "E48", 8, new String[] {
                        "beginA=E42,lastZ=E48"
                    }
                },
                new Object[] {
                    "E49", 2
                },
                new Object[] {
                    "E50", 5, new String[] {
                        "beginA=E45,lastZ=E50", "beginA=E46,lastZ=E50"
                    }
                },
                new Object[] {
                    "E51", 3
                },
                new Object[] {
                    "E52", 3
                },
                new Object[] {
                    "E53", 9
                },
                new Object[] {
                    "E54", 8
                },
                new Object[] {
                    "E55", 5
                },
                new Object[] {
                    "E56", 5
                },
                new Object[] {
                    "E57", 9
                },
                new Object[] {
                    "E58", 7
                },
                new Object[] {
                    "E59", 3
                },
                new Object[] {
                    "E60", 3
                }
            };
    
            int rowCount = 0;
    
            foreach (Object[] row in data) {
                SupportBean theEvent = new SupportBean((String) row[0], (int) row[1]);
    
                _epService.EPRuntime.SendEvent(theEvent);
    
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
    
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(
                    text);
    
            Assert.AreEqual(text, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(text, stmt.Text);
            
            foreach (Object[] row in data) {
                SupportBean theEvent = new SupportBean((String) row[0], (int) row[1]);
                _epService.EPRuntime.SendEvent(theEvent);
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
        }
    
        [Test]
        public void TestExampleWithPREV() {
            String query = "SELECT * " + "FROM MyEvent.win:keepall()"
                    + "   MATCH_RECOGNIZE ("
                    + "       MEASURES A.TheString AS a_string,"
                    + "         A.value AS a_value,"
                    + "         B.TheString AS b_string,"
                    + "         B.value AS b_value,"
                    + "         C[0].TheString AS c0_string,"
                    + "         C[0].value AS c0_value,"
                    + "         C[1].TheString AS c1_string,"
                    + "         C[1].value AS c1_value,"
                    + "         C[2].TheString AS c2_string,"
                    + "         C[2].value AS c2_value,"
                    + "         D.TheString AS d_string,"
                    + "         D.value AS d_value,"
                    + "         E[0].TheString AS e0_string,"
                    + "         E[0].value AS e0_value,"
                    + "         E[1].TheString AS e1_string,"
                    + "         E[1].value AS e1_value,"
                    + "         F[0].TheString AS f0_string,"
                    + "         F[0].value AS f0_value,"
                    + "         F[1].TheString AS f1_string,"
                    + "         F[1].value AS f1_value" + "       ALL MATCHES"
                    + "       after match skip to current row"
                    + "       PATTERN ( A B C* D E* F+ )"
                    + "       DEFINE /* A is unspecified, defaults to TRUE, matches any row */"
                    + "            B AS (B.value < PREV (B.value)),"
                    + "            C AS (C.value <= PREV (C.value)),"
                    + "            D AS (D.value < PREV (D.value)),"
                    + "            E AS (E.value >= PREV (E.value)),"
                    + "            F AS (F.value >= PREV (F.value) and F.value > A.value)"
                    + ")";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(query);
            SupportUpdateListener listener = new SupportUpdateListener();
    
            stmt.Events += listener.Update;
    
            Object[][] data = new Object[][] {
                new Object[] {
                    "E1", 100, null
                },
                new Object[] {
                    "E2", 98, null
                },
                new Object[] {
                    "E3", 75, null
                },
                new Object[] {
                    "E4", 61, null
                },
                new Object[] {
                    "E5", 50, null
                },
                new Object[] {
                    "E6", 49, null
                },
                new Object[] {
                    "E7", 64,
                    new String[] {
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=null,f1_value=null"
                    }
                },
                new Object[] {
                    "E8", 78,
                    new String[] {
                        "a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=null,f1_value=null",
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=null,f1_value=null",
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=E8,f1_value=78"
                    }
                },
                new Object[] {
                    "E9", 84,
                    new String[] {
                        "a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=E9,f1_value=84",
                        "a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=E8,e1_value=78,f0_string=E9,f0_value=84,f1_string=null,f1_value=null",                                
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=E8,e1_value=78,f0_string=E9,f0_value=84,f1_string=null,f1_value=null",
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=E9,f1_value=84",
                        "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=E8,f1_value=78"
                    }
                },
            };
    
            int rowCount = 0;
    
            foreach (Object[] row in data) {
                rowCount++;
                SupportRecogBean theEvent = new SupportRecogBean((String) row[0], (int) row[1]);
    
                _epService.EPRuntime.SendEvent(theEvent);
    
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
    
            stmt.Dispose();
        }
    
        private static void Compare(Object[] row, int rowCount, Object theEvent, SupportUpdateListener listener) {
            EventBean[] matches;
            if (row.Length < 3 || row[2] == null) {
                if (listener.IsInvoked)
                {
                    matches = listener.LastNewData;

                    if (matches != null) {
                        for (int i = 0; i < matches.Length; i++) {
                            Log.Info("Received matches: " + GetProps(matches[i]));
                        }
                    }
                }
                Assert.IsFalse(listener.IsInvoked, "For event " + theEvent + " row " + rowCount);
                return;
            }
    
            String[] expected = (String[]) row[2];
    
            matches = listener.LastNewData;
            String[] matchesText = null;
    
            if (matches != null) {
                matchesText = new String[matches.Length];
                for (int i = 0; i < matches.Length; i++) {
                    matchesText[i] = GetProps(matches[i]);
                    Log.Debug(GetProps(matches[i]));
                }
            } else {
                if (expected != null) {
                    Log.Info("Received no matches but expected: ");
                    for (int i = 0; i < expected.Length; i++) {
                        Log.Info(expected[i]);
                    }
                    Assert.Fail();
                }
            }

            expected.SortInPlace();
            matchesText.SortInPlace();

            Assert.AreEqual(matches.Length, expected.Length, "For event " + theEvent);
            for (int i = 0; i < expected.Length; i++) {
                if (!expected[i].Equals(matchesText[i])) {
                    Log.Info("expected:" + expected[i]);
                    Log.Info("  actual:" + expected[i]);
                    Assert.AreEqual(expected[i], matchesText[i], "Sending event " + theEvent + " row " + rowCount);
                }
            }
    
            listener.Reset();
        }
    
        private static String GetProps(EventBean theEvent) {
            StringBuilder buf = new StringBuilder();
            String delimiter = "";
    
            foreach (EventPropertyDescriptor prop in theEvent.EventType.PropertyDescriptors) {
                buf.Append(delimiter);
                buf.Append(prop.PropertyName);
                buf.Append("=");
                buf.Append(theEvent.Get(prop.PropertyName).Render());
                delimiter = ",";
            }
            return buf.ToString();
        }
    }
}
