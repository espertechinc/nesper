///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogPermute : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunPermute(epService, false);
            RunPermute(epService, true);
    
            RunDocSamples(epService);
    
            RunEquivalent(epService, "MAtCh_Recognize_Permute(A)",
                    "(A)");
            RunEquivalent(epService, "match_recognize_permute(A,B)",
                    "(A B|B A)");
            RunEquivalent(epService, "match_recognize_permute(A,B,C)",
                    "(A B C|A C B|B A C|B C A|C A B|C B A)");
            RunEquivalent(epService, "match_recognize_permute(A,B,C,D)",
                    "(A B C D|A B D C|A C B D|A C D B|A D B C|A D C B|B A C D|B A D C|B C A D|B C D A|B D A C|B D C A|C A B D|C A D B|C B A D|C B D A|C D A B|C D B A|D A B C|D A C B|D B A C|D B C A|D C A B|D C B A)");
    
            RunEquivalent(epService, "match_recognize_permute((A B), C)",
                    "((A B) C|C (A B))");
            RunEquivalent(epService, "match_recognize_permute((A|B), (C D), E)",
                    "((A|B) (C D) E|(A|B) E (C D)|(C D) (A|B) E|(C D) E (A|B)|E (A|B) (C D)|E (C D) (A|B))");
    
            RunEquivalent(epService, "A match_recognize_permute(B,C) D",
                    "A (B C|C B) D");
    
            RunEquivalent(epService, "match_recognize_permute(A, match_recognize_permute(B, C))",
                    "(A (B C|C B)|(B C|C B) A)");
        }
    
        private void RunDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema TemperatureSensorEvent(id string, device string, temp int)");
    
            RunDocSampleUpToN(epService);
        }
    
        private void RunDocSampleUpToN(EPServiceProvider epService) {
            string[] fields = "a_id,b_id".Split(',');
            string epl = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A.id as a_id, B.id as b_id\n" +
                    "  pattern (match_recognize_permute(A, B))\n" +
                    "  define \n" +
                    "\tA as A.temp < 100, \n" +
                    "\tB as B.temp >= 100)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{"E1", "1", 99}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new object[]{"E2", "1", 100}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E2"});
    
            epService.EPRuntime.SendEvent(new object[]{"E3", "1", 100}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new object[]{"E4", "1", 99}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "E3"});
    
            epService.EPRuntime.SendEvent(new object[]{"E5", "1", 98}, "TemperatureSensorEvent");
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunPermute(EPServiceProvider epService, bool soda) {
            TryPermute(epService, soda, "(A B C)|(A C B)|(B A C)|(B C A)|(C A B)|(C B A)");
            TryPermute(epService, soda, "(match_recognize_permute(A,B,C))");
        }
    
        public void TryPermute(EPServiceProvider epService, bool soda, string pattern) {
            string epl = "select * from SupportBean " +
                    "match_recognize (" +
                    " partition by IntPrimitive" +
                    " measures A as a, B as b, C as c" +
                    " pattern (" + pattern + ")" +
                    " define" +
                    " A as A.TheString like \"A%\"," +
                    " B as B.TheString like \"B%\"," +
                    " C as C.TheString like \"C%\"" +
                    ")";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] prefixes = "A,B,C".Split(',');
            string[] fields = "a,b,c".Split(',');
            var e = PermutationEnumerator.Create(3);
            int count = 0;
    
            foreach (var indexes in e) {
                var expected = new Object[3];
                for (int i = 0; i < 3; i++) {
                    expected[indexes[i]] = SendEvent(epService, prefixes[indexes[i]] + Convert.ToString(count), count);
                }
                count++;
    
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
            }
    
            stmt.Dispose();
        }
    
        private static void RunEquivalent(EPServiceProvider epService, string before, string after) {
            ExecRowRecogRepetition.RunEquivalent(epService, before, after);
        }
    
        private SupportBean SendEvent(EPServiceProvider epService, string theString, int intPrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(sb);
            return sb;
        }
    }
} // end of namespace
