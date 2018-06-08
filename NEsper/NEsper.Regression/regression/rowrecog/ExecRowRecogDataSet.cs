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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogDataSet : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionExampleFinancialWPattern(epService);
            RunAssertionExampleWithPREV(epService);
        }
    
        private void RunAssertionExampleFinancialWPattern(EPServiceProvider epService) {
            string text = "select * " +
                    "from SupportBean " +
                    "match_recognize (" +
                    " measures A.TheString as beginA, last(Z.TheString) as lastZ" +
                    " all matches" +
                    " after match skip to current row" +
                    " pattern (A W+ X+ Y+ Z+)" +
                    " define" +
                    " W as W.IntPrimitive<prev(W.IntPrimitive)," +
                    " X as X.IntPrimitive>prev(X.IntPrimitive)," +
                    " Y as Y.IntPrimitive<prev(Y.IntPrimitive)," +
                    " Z as Z.IntPrimitive>prev(Z.IntPrimitive)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var data = new object[][]{
                    new object[] {"E1", 8},   // 0
                    new object[] {"E2", 8},
                    new object[] {"E3", 8},       // A
                    new object[] {"E4", 6},       // W
                    new object[] {"E5", 3},       // W
                    new object[] {"E6", 7},
                    new object[] {"E7", 6},
                    new object[] {"E8", 2},
                    new object[] {"E9", 6,        // Z
                            new string[]{"beginA=E3,lastZ=E9", "beginA=E4,lastZ=E9"}},
                    new object[] {"E10", 2},
                    new object[] {"E11", 9,  // 10
                            new string[]{"beginA=E6,lastZ=E11", "beginA=E7,lastZ=E11"}},
                    new object[] {"E12", 9},
                    new object[] {"E13", 8},
                    new object[] {"E14", 5},
                    new object[] {"E15", 0},
                    new object[] {"E16", 9},
                    new object[] {"E17", 2},
                    new object[] {"E18", 0},
                    new object[] {"E19", 2,
                            new string[]{"beginA=E12,lastZ=E19", "beginA=E13,lastZ=E19", "beginA=E14,lastZ=E19"}},
                    new object[] {"E20", 3,
                            new string[]{"beginA=E12,lastZ=E20", "beginA=E13,lastZ=E20", "beginA=E14,lastZ=E20"}},
                    new object[] {"E21", 8,
                            new string[]{"beginA=E12,lastZ=E21", "beginA=E13,lastZ=E21", "beginA=E14,lastZ=E21"}},
                    new object[] {"E22", 5},
                    new object[] {"E23", 9,
                            new string[]{"beginA=E16,lastZ=E23", "beginA=E17,lastZ=E23"}},
                    new object[] {"E24", 9},
                    new object[] {"E25", 4},
                    new object[] {"E26", 7},
                    new object[] {"E27", 2},
                    new object[] {"E28", 8,
                            new string[]{"beginA=E24,lastZ=E28"}},
                    new object[] {"E29", 0},
                    new object[] {"E30", 4,
                            new string[]{"beginA=E26,lastZ=E30"}},
                    new object[] {"E31", 4},
                    new object[] {"E32", 7},
                    new object[] {"E33", 8},
                    new object[] {"E34", 6},
                    new object[] {"E35", 4},
                    new object[] {"E36", 5},
                    new object[] {"E37", 1},
                    new object[] {"E38", 7,
                            new string[]{"beginA=E33,lastZ=E38", "beginA=E34,lastZ=E38"}},
                    new object[] {"E39", 5},
                    new object[] {"E40", 8,
                            new string[]{"beginA=E36,lastZ=E40"}},
                    new object[] {"E41", 6},
                    new object[] {"E42", 6},
                    new object[] {"E43", 0},
                    new object[] {"E44", 6},
                    new object[] {"E45", 8},
                    new object[] {"E46", 4},
                    new object[] {"E47", 3},
                    new object[] {"E48", 8,
                            new string[]{"beginA=E42,lastZ=E48"}},
                    new object[] {"E49", 2},
                    new object[] {"E50", 5,
                            new string[]{"beginA=E45,lastZ=E50", "beginA=E46,lastZ=E50"}},
                    new object[] {"E51", 3},
                    new object[] {"E52", 3},
                    new object[] {"E53", 9},
                    new object[] {"E54", 8},
                    new object[] {"E55", 5},
                    new object[] {"E56", 5},
                    new object[] {"E57", 9},
                    new object[] {"E58", 7},
                    new object[] {"E59", 3},
                    new object[] {"E60", 3}
            };
    
            int rowCount = 0;
            foreach (object[] row in data) {
                var theEvent = new SupportBean((string) row[0], (int) row[1]);
                epService.EPRuntime.SendEvent(theEvent);
    
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
    
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(text);
            Assert.AreEqual(text, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            stmt.Events += listener.Update;
            Assert.AreEqual(text, stmt.Text);
    
            foreach (object[] row in data) {
                var theEvent = new SupportBean((string) row[0], (int) row[1]);
                epService.EPRuntime.SendEvent(theEvent);
    
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
        }
    
        private void RunAssertionExampleWithPREV(EPServiceProvider epService) {
            string query = "SELECT * " +
                    "FROM MyEvent#keepall" +
                    "   MATCH_RECOGNIZE (" +
                    "       MEASURES A.TheString AS a_string," +
                    "         A.value AS a_value," +
                    "         B.TheString AS b_string," +
                    "         B.value AS b_value," +
                    "         C[0].TheString AS c0_string," +
                    "         C[0].value AS c0_value," +
                    "         C[1].TheString AS c1_string," +
                    "         C[1].value AS c1_value," +
                    "         C[2].TheString AS c2_string," +
                    "         C[2].value AS c2_value," +
                    "         D.TheString AS d_string," +
                    "         D.value AS d_value," +
                    "         E[0].TheString AS e0_string," +
                    "         E[0].value AS e0_value," +
                    "         E[1].TheString AS e1_string," +
                    "         E[1].value AS e1_value," +
                    "         F[0].TheString AS f0_string," +
                    "         F[0].value AS f0_value," +
                    "         F[1].TheString AS f1_string," +
                    "         F[1].value AS f1_value" +
                    "       ALL MATCHES" +
                    "       after match skip to current row" +
                    "       PATTERN ( A B C* D E* F+ )" +
                    "       DEFINE /* A is unspecified, defaults to TRUE, matches any row */" +
                    "            B AS (B.value < PREV (B.value))," +
                    "            C AS (C.value <= PREV (C.value))," +
                    "            D AS (D.value < PREV (D.value))," +
                    "            E AS (E.value >= PREV (E.value))," +
                    "            F AS (F.value >= PREV (F.value) and F.value > A.value)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(query);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var data = new object[][]{
                    new object[] {"E1", 100, null},
                    new object[] {"E2", 98, null},
                    new object[] {"E3", 75, null},
                    new object[] {"E4", 61, null},
                    new object[] {"E5", 50, null},
                    new object[] {"E6", 49, null},
                    new object[] {"E7", 64,
                            new string[]{"a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=null,f1_value=null"}},
                    new object[] {"E8", 78,
                            new string[]{"a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=null,f1_value=null",
                                    "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=null,f1_value=null",
                                    "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=E8,f1_value=78"}},
                    new object[] {"E9", 84,
                            new string[]{"a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=E9,f1_value=84",
                                    "a_string=E3,a_value=75,b_string=E4,b_value=61,c0_string=E5,c0_value=50,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=E8,e1_value=78,f0_string=E9,f0_value=84,f1_string=null,f1_value=null",
                                    "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=E8,e1_value=78,f0_string=E9,f0_value=84,f1_string=null,f1_value=null",
                                    "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=E7,e0_value=64,e1_string=null,e1_value=null,f0_string=E8,f0_value=78,f1_string=E9,f1_value=84",
                                    "a_string=E4,a_value=61,b_string=E5,b_value=50,c0_string=null,c0_value=null,c1_string=null,c1_value=null,c2_string=null,c2_value=null,d_string=E6,d_value=49,e0_string=null,e0_value=null,e1_string=null,e1_value=null,f0_string=E7,f0_value=64,f1_string=E8,f1_value=78"
                            }},
            };
    
            int rowCount = 0;
            foreach (object[] row in data) {
                rowCount++;
                var theEvent = new SupportRecogBean((string) row[0], (int) row[1]);
                epService.EPRuntime.SendEvent(theEvent);
    
                Compare(row, rowCount, theEvent, listener);
                rowCount++;
            }
    
            stmt.Dispose();
        }
    
        private static void Compare(object[] row, int rowCount, Object theEvent, SupportUpdateListener listener) {
            if (row.Length < 3 || row[2] == null) {
                if (listener.IsInvoked) {
                    EventBean[] matchesInner = listener.LastNewData;
                    if (matchesInner != null) {
                        for (int i = 0; i < matchesInner.Length; i++) {
                            Log.Info("Received matches: " + GetProps(matchesInner[i]));
                        }
                    }
                }
                Assert.IsFalse(listener.IsInvoked, "For event " + theEvent + " row " + rowCount);
                return;
            }
    
            string[] expected = (string[]) row[2];
    
            EventBean[] matches = listener.LastNewData;
            string[] matchesText = null;
            if (matches != null) {
                matchesText = new string[matches.Length];
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
    
        private static string GetProps(EventBean theEvent) {
            var buf = new StringBuilder();
            string delimiter = "";
            foreach (EventPropertyDescriptor prop in theEvent.EventType.PropertyDescriptors) {
                buf.Append(delimiter);
                buf.Append(prop.PropertyName);
                buf.Append("=");
                buf.Append(theEvent.Get(prop.PropertyName).RenderAny());
                delimiter = ",";
            }
            return buf.ToString();
        }
    }
} // end of namespace
