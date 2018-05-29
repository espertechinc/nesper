///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogRegex : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            Run(new SupportTestCaseHolder("a,b,c,d", "(A?) (B)? (C D)?")
                            .Add("a", new string[]{"a,null,null,null"})
                            .Add("b", new string[]{"null,b,null,null"})
                            .Add("x", null)
                            .Add("d", null)
                            .Add("c", null)
                            .Add("d,c", null)
                            .Add("c,d", new string[]{"null,null,c,d"})
                            .Add("a,c,d", new string[]{"a,null,null,null", "a,null,c,d", "null,null,c,d"})
                            .Add("b,c,d", new string[]{"null,b,null,null", "null,null,c,d", "null,b,c,d"})
                            .Add("a,b,c,d", new string[]{"a,b,null,null", "a,null,null,null", "null,b,null,null", "null,b,c,d", "a,b,c,d", "null,null,c,d"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c,d", "(A | B) (C | D)")
                            .Add("a", null)
                            .Add("c", null)
                            .Add("d,c", null)
                            .Add("a,b", null)
                            .Add("a,d", new string[]{"a,null,null,d"})
                            .Add("a,d,c", new string[]{"a,null,null,d"})
                            .Add("b,c", new string[]{"null,b,c,null"})
                            .Add("b,d", new string[]{"null,b,null,d"})
                            .Add("b,a,d,c", new string[]{"a,null,null,d"})
                            .Add("x,a,x,b,x,b,c,x", new string[]{"null,b,c,null"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c,d,e", "A ((B C)? | (D E)?)")
                            .Add("a", new string[]{"a,null,null,null,null"})
                            .Add("a,b,c", new string[]{"a,null,null,null,null", "a,b,c,null,null"})
                            .Add("a,d,e", new string[]{"a,null,null,null,null", "a,null,null,d,e"})
                            .Add("b,c", null)
                            .Add("x,d,e", null),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c", "(A? B) | (A? C)")
                            .Add("a", null)
                            .Add("a,b", new string[]{"a,b,null", "null,b,null"})
                            .Add("a,c", new string[]{"a,null,c", "null,null,c"})
                            .Add("b", new string[]{"null,b,null"})
                            .Add("c", new string[]{"null,null,c"})
                            .Add("a,x,b", new string[]{"null,b,null"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c", "(A B? C)?")
                            .Add("x", null)
                            .Add("a", null)
                            .Add("a,c", new string[]{"a,null,c"})
                            .Add("a,b,c", new string[]{"a,b,c"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c", "(A? B C?)")
                            .Add("x", null)
                            .Add("a", null)
                            .Add("a,c", null)
                            .Add("b", new string[]{"null,b,null"})
                            .Add("a,b,c", new string[]{"a,b,null", "null,b,null", "a,b,c", "null,b,c"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a[0],b[0],a[1],b[1],c,d", "(A B)* C D")
                            .Add("c,d", new string[]{"null,null,null,null,c,d"})
                            .Add("a1,b1,c,d", new string[]{"a1,b1,null,null,c,d", "null,null,null,null,c,d"})
                            .Add("a2,b2,x,c,d", new string[]{"null,null,null,null,c,d"})
                            .Add("a1,b1,a2,b2,c,d", new string[]{"null,null,null,null,c,d", "a2,b2,null,null,c,d", "a1,b1,a2,b2,c,d"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a[0],b[0],c[0],a[1],b[1],c[1],d[0],e[0],d[1],e[1]", "(A (B C))* (D E)+")
                            .Add("a,b,c", null)
                            .Add("d,e", new string[]{"null,null,null,null,null,null,d,e,null,null"})
                            .Add("a,b,c,d,e", new string[]{"a,b,c,null,null,null,d,e,null,null", "null,null,null,null,null,null,d,e,null,null"})
                            .Add("a1,b1,c1,a2,b2,c2,d,e", new string[]{"a1,b1,c1,a2,b2,c2,d,e,null,null", "a2,b2,c2,null,null,null,d,e,null,null", "null,null,null,null,null,null,d,e,null,null"})
                            .Add("d1,e1,d2,e2", new string[]{"null,null,null,null,null,null,d1,e1,null,null", "null,null,null,null,null,null,d2,e2,null,null", "null,null,null,null,null,null,d1,e1,d2,e2"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a[0],a[1],d[0],e[0],d[1],e[1]", "A+ (D E)+")
                            .Add("a,e,a,d,d,e,a,e,e,a,d,d,e,d,e", null)
                            .Add("a,d,e", new string[]{"a,null,d,e,null,null"})
                            .Add("a1,a2,d,e", new string[]{"a1,a2,d,e,null,null", "a2,null,d,e,null,null"})
                            .Add("a1,d1,e1,d2,e2", new string[]{"a1,null,d1,e1,null,null", "a1,null,d1,e1,d2,e2"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c,d,e,f", "(A (B | C)) | (D (E | F))")
                            .Add("a,e,d,b,a,f,f,d,c,a", null)
                            .Add("a,f,c,b,a,d,f", new string[]{"null,null,null,d,null,f"})
                            .Add("c,b,d,a,b,x,y", new string[]{"a,b,null,null,null,null"})
                            .Add("a,d,c,f,d,e,x,a,c", new string[]{"null,null,null,d,e,null", "a,null,c,null,null,null"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a,b,c,d,e,f", "(A (B | C)? ) | (D? (E | F))")
                            .Add("a1,f1,c,b,a2,d,f2", new string[]{"a1,null,null,null,null,null", "a2,null,null,null,null,null", "null,null,null,null,null,f1", "null,null,null,null,null,f2", "null,null,null,d,null,f2"})
                            .Add("d,f", new string[]{"null,null,null,d,null,f", "null,null,null,null,null,f"}),
                    epService);
    
            Run(new SupportTestCaseHolder("a[0],a[1],b,c,d", "(A B C) | (A+ B D)")
                            .Add("a1,c,a2,b,d", new string[]{"a2,null,b,null,d"})
                            .Add("a1,b1,x,a2,b2,c1", new string[]{"a2,null,b2,c1,null"}),
                    epService);
        }
    
        private void Run(SupportTestCaseHolder testDesc, EPServiceProvider epService) {
            var buf = new StringBuilder();
            buf.Append("select * from MyEvent#keepall " +
                    "match_recognize (\n" +
                    "  measures ");
    
            string delimiter = "";
            foreach (string measure in testDesc.Measures.Split(',')) {
                buf.Append(delimiter);
                buf.Append(measure.ToUpperInvariant()).Append(".TheString as ").Append(ReplaceBrackets(measure)).Append("val");
                delimiter = ",";
            }
            buf.Append("\n all matches ");
            buf.Append("\n after match skip to current row ");
            buf.Append("\n pattern (").Append(testDesc.Pattern).Append(") \n");
            buf.Append("  define ");
    
            var defines = new HashSet<string>();
            foreach (string measure in testDesc.Measures.Split(',')) {
                defines.Add(RemoveBrackets(measure).ToUpperInvariant());
            }
    
            delimiter = "";
            foreach (string define in defines) {
                buf.Append(delimiter);
                buf.Append(define).Append(" as (").Append(define).Append(".TheString like '").Append(define.ToLowerInvariant()).Append("%')");
                delimiter = ",\n";
            }
            buf.Append(")");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(buf.ToString());
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            foreach (SupportTestCaseItem testcase in testDesc.TestCases) {
                int count = 0;
                foreach (string testchar in testcase.Testdata.Split(',')) {
                    epService.EPRuntime.SendEvent(new SupportRecogBean(testchar, count++));
                }
    
                EventBean[] iteratorData = EPAssertionUtil.EnumeratorToArray(stmt.GetEnumerator());
                Compare(testcase.Testdata, iteratorData, testDesc.Measures, testcase);
    
                EventBean[] listenerData = listener.GetNewDataListFlattened();
                listener.Reset();
                Compare(testcase.Testdata, listenerData, testDesc.Measures, testcase);
    
                stmt.Stop();
                stmt.Start();
            }
        }
    
        private string ReplaceBrackets(string indexed) {
            return indexed.Replace("[", "").Replace("]", "");
        }
    
        private string RemoveBrackets(string indexed) {
            int index = indexed.IndexOf('[');
            if (index == -1) {
                return indexed;
            }
            return indexed.Substring(0, indexed.Length - index - 2);
        }
    
        private void Compare(string sent, EventBean[] received, string measures, SupportTestCaseItem testDesc) {
    
            string message = "For sent: " + sent;
            if (testDesc.Expected == null) {
                Assert.AreEqual(0, received.Length, message);
                return;
            }
    
            var receivedText = new string[received.Length];
            for (int i = 0; i < received.Length; i++) {
                var buf = new StringBuilder();
                string delimiter = "";
                foreach (string measure in measures.Split(',')) {
                    buf.Append(delimiter);
                    Object value = received[i].Get(ReplaceBrackets(measure) + "val");
                    buf.Append(value.RenderAny());
                    delimiter = ",";
                }
                receivedText[i] = buf.ToString();
            }
    
            if (testDesc.Expected.Length != received.Length) {
                Log.Info("expected: " + testDesc.Expected.Render());
                Log.Info("received: " + receivedText.Render());
                Assert.AreEqual(testDesc.Expected.Length, received.Length, message);
            }
    
            Log.Debug("comparing: " + message);
            EPAssertionUtil.AssertEqualsAnyOrder(testDesc.Expected, receivedText);
        }
    }
} // end of namespace
