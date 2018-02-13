///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.rowregex;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogRepetition : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionRepeat(epService, false);
            RunAssertionRepeat(epService, true);
    
            RunAssertionPrev(epService);
    
            RunAssertionInvalid(epService);
    
            RunAssertionDocSamples(epService);
    
            RunAssertionEquivalent(epService);
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema TemperatureSensorEvent(id string, device string, temp int)");
    
            RunDocSampleExactlyN(epService);
            RunDocSampleNOrMore_and_BetweenNandM(epService, "A{2,} B");
            RunDocSampleNOrMore_and_BetweenNandM(epService, "A{2,3} B");
            RunDocSampleUpToN(epService);
        }
    
        private void RunDocSampleUpToN(EPServiceProvider epService) {
            string[] fields = "a0_id,a1_id,b_id".Split(',');
            string epl = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A[0].id as a0_id, A[1].id as a1_id, B.id as b_id\n" +
                    "  pattern (A{,2} B)\n" +
                    "  define \n" +
                    "\tA as A.temp >= 100,\n" +
                    "\tB as B.temp >= 102)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Object[]{"E1", "1", 99}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E2", "1", 100}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E3", "1", 100}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E4", "1", 101}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E5", "1", 102}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3", "E4", "E5"});
    
            stmt.Dispose();
        }
    
        private void RunDocSampleNOrMore_and_BetweenNandM(EPServiceProvider epService, string pattern) {
            string[] fields = "a0_id,a1_id,a2_id,b_id".Split(',');
            string epl = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A[0].id as a0_id, A[1].id as a1_id, A[2].id as a2_id, B.id as b_id\n" +
                    "  pattern (" + pattern + ")\n" +
                    "  define \n" +
                    "\tA as A.temp >= 100,\n" +
                    "\tB as B.temp >= 102)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Object[]{"E1", "1", 99}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E2", "1", 100}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E3", "1", 100}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E4", "1", 101}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E5", "1", 102}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", "E3", "E4", "E5"});
    
            stmt.Dispose();
        }
    
        private void RunDocSampleExactlyN(EPServiceProvider epService) {
            string[] fields = "a0_id,a1_id".Split(',');
            string epl = "select * from TemperatureSensorEvent\n" +
                    "match_recognize (\n" +
                    "  partition by device\n" +
                    "  measures A[0].id as a0_id, A[1].id as a1_id\n" +
                    "  pattern (A{2})\n" +
                    "  define \n" +
                    "\tA as A.temp >= 100)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new Object[]{"E1", "1", 99}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E2", "1", 100}, "TemperatureSensorEvent");
    
            epService.EPRuntime.SendEvent(new Object[]{"E3", "1", 100}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", "E3"});
    
            epService.EPRuntime.SendEvent(new Object[]{"E4", "1", 101}, "TemperatureSensorEvent");
            epService.EPRuntime.SendEvent(new Object[]{"E5", "1", 102}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{"E4", "E5"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string template = "select * from SupportBean " +
                    "match_recognize (" +
                    "  measures A as a" +
                    "  pattern (REPLACE) " +
                    ")";
            epService.EPAdministrator.CreateEPL("create variable int myvariable = 0");
    
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{}"),
                    "Invalid match-recognize quantifier '{}', expecting an expression");
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{null}"),
                    "Error starting statement: pattern quantifier 'null' must return an integer-type value");
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{myvariable}"),
                    "Error starting statement: pattern quantifier 'myvariable' must return a constant value");
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{Prev(A)}"),
                    "Error starting statement: Invalid match-recognize pattern expression 'Prev(A)': Aggregation, sub-select, previous or prior functions are not supported in this context");
    
            string expected = "Error starting statement: Invalid pattern quantifier value -1, expecting a minimum of 1";
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{-1}"), expected);
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{,-1}"), expected);
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{-1,10}"), expected);
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{-1,}"), expected);
            SupportMessageAssertUtil.TryInvalid(epService, template.ReplaceAll("REPLACE", "A{5,3}"),
                    "Error starting statement: Invalid pattern quantifier value 5, expecting a minimum of 1 and maximum of 3");
        }
    
        private void RunAssertionPrev(EPServiceProvider epService) {
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    "  measures A as a" +
                    "  pattern (A{3}) " +
                    "  define " +
                    "    A as A.intPrimitive > Prev(A.intPrimitive)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            SendEvent("A1", 1, epService);
            SendEvent("A2", 4, epService);
            SendEvent("A3", 2, epService);
            SendEvent("A4", 6, epService);
            SendEvent("A5", 5, epService);
            SupportBean b6 = SendEvent("A6", 6, epService);
            SupportBean b7 = SendEvent("A7", 7, epService);
            SupportBean b8 = SendEvent("A9", 8, epService);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[]{new Object[]{b6, b7, b8}});
        }
    
        private void RunAssertionRepeat(EPServiceProvider epService, bool soda) {
            // Atom Assertions
            //
            //
    
            // single-bound assertions
            RunAssertionRepeatSingleBound(epService, soda);
    
            // defined-range assertions
            RunAssertionsRepeatRange(epService, soda);
    
            // lower-bounds assertions
            RunAssertionsUpTo(epService, soda);
    
            // upper-bounds assertions
            RunAssertionsAtLeast(epService, soda);
    
            // Nested Assertions
            //
            //
    
            // single-bound nested assertions
            RunAssertionNestedRepeatSingle(epService, soda);
    
            // defined-range nested assertions
            RunAssertionNestedRepeatRange(epService, soda);
    
            // lower-bounds nested assertions
            RunAssertionsNestedUpTo(epService, soda);
    
            // upper-bounds nested assertions
            RunAssertionsNestedAtLeast(epService, soda);
        }
    
        private void RunAssertionEquivalent(EPServiceProvider epService) {
            //
            // Single-bounds Repeat.
            //
            RunEquivalent(epService, "A{1}", "A");
            RunEquivalent(epService, "A{2}", "A A");
            RunEquivalent(epService, "A{3}", "A A A");
            RunEquivalent(epService, "A{1} B{2}", "A B B");
            RunEquivalent(epService, "A{1} B{2} C{3}", "A B B C C C");
            RunEquivalent(epService, "(A{2})", "(A A)");
            RunEquivalent(epService, "A?{2}", "A? A?");
            RunEquivalent(epService, "A*{2}", "A* A*");
            RunEquivalent(epService, "A+{2}", "A+ A+");
            RunEquivalent(epService, "A??{2}", "A?? A??");
            RunEquivalent(epService, "A*?{2}", "A*? A*?");
            RunEquivalent(epService, "A+?{2}", "A+? A+?");
            RunEquivalent(epService, "(A B){1}", "(A B)");
            RunEquivalent(epService, "(A B){2}", "(A B) (A B)");
            RunEquivalent(epService, "(A B)?{2}", "(A B)? (A B)?");
            RunEquivalent(epService, "(A B)*{2}", "(A B)* (A B)*");
            RunEquivalent(epService, "(A B)+{2}", "(A B)+ (A B)+");
    
            RunEquivalent(epService, "A B{2} C", "A B B C");
            RunEquivalent(epService, "A (B{2}) C", "A (B B) C");
            RunEquivalent(epService, "(A{2}) C", "(A A) C");
            RunEquivalent(epService, "A (B{2}|C{2})", "A (B B|C C)");
            RunEquivalent(epService, "A{2} B{2} C{2}", "A A B B C C");
            RunEquivalent(epService, "A{2} B C{2}", "A A B C C");
            RunEquivalent(epService, "A B{2} C{2}", "A B B C C");
    
            // range bounds
            RunEquivalent(epService, "A{1, 3}", "A A? A?");
            RunEquivalent(epService, "A{2, 4}", "A A A? A?");
            RunEquivalent(epService, "A?{1, 3}", "A? A? A?");
            RunEquivalent(epService, "A*{1, 3}", "A* A* A*");
            RunEquivalent(epService, "A+{1, 3}", "A+ A* A*");
            RunEquivalent(epService, "A??{1, 3}", "A?? A?? A??");
            RunEquivalent(epService, "A*?{1, 3}", "A*? A*? A*?");
            RunEquivalent(epService, "A+?{1, 3}", "A+? A*? A*?");
            RunEquivalent(epService, "(A B)?{1, 3}", "(A B)? (A B)? (A B)?");
            RunEquivalent(epService, "(A B)*{1, 3}", "(A B)* (A B)* (A B)*");
            RunEquivalent(epService, "(A B)+{1, 3}", "(A B)+ (A B)* (A B)*");
    
            // lower-only bounds
            RunEquivalent(epService, "A{2,}", "A A A*");
            RunEquivalent(epService, "A?{2,}", "A? A? A*");
            RunEquivalent(epService, "A*{2,}", "A* A* A*");
            RunEquivalent(epService, "A+{2,}", "A+ A+ A*");
            RunEquivalent(epService, "A??{2,}", "A?? A?? A*?");
            RunEquivalent(epService, "A*?{2,}", "A*? A*? A*?");
            RunEquivalent(epService, "A+?{2,}", "A+? A+? A*?");
            RunEquivalent(epService, "(A B)?{2,}", "(A B)? (A B)? (A B)*");
            RunEquivalent(epService, "(A B)*{2,}", "(A B)* (A B)* (A B)*");
            RunEquivalent(epService, "(A B)+{2,}", "(A B)+ (A B)+ (A B)*");
    
            // upper-only bounds
            RunEquivalent(epService, "A{,2}", "A? A?");
            RunEquivalent(epService, "A?{,2}", "A? A?");
            RunEquivalent(epService, "A*{,2}", "A* A*");
            RunEquivalent(epService, "A+{,2}", "A* A*");
            RunEquivalent(epService, "A??{,2}", "A?? A??");
            RunEquivalent(epService, "A*?{,2}", "A*? A*?");
            RunEquivalent(epService, "A+?{,2}", "A*? A*?");
            RunEquivalent(epService, "(A B){,2}", "(A B)? (A B)?");
            RunEquivalent(epService, "(A B)?{,2}", "(A B)? (A B)?");
            RunEquivalent(epService, "(A B)*{,2}", "(A B)* (A B)*");
            RunEquivalent(epService, "(A B)+{,2}", "(A B)* (A B)*");
    
            //
            // Nested Repeat.
            //
            RunEquivalent(epService, "(A B){2}", "(A B) (A B)");
            RunEquivalent(epService, "(A){2}", "A A");
            RunEquivalent(epService, "(A B C){3}", "(A B C) (A B C) (A B C)");
            RunEquivalent(epService, "(A B){2} (C D){2}", "(A B) (A B) (C D) (C D)");
            RunEquivalent(epService, "((A B){2} C){2}", "((A B) (A B) C) ((A B) (A B) C)");
            RunEquivalent(epService, "((A|B){2} (C|D){2}){2}", "((A|B) (A|B) (C|D) (C|D)) ((A|B) (A|B) (C|D) (C|D))");
        }
    
        private void RunAssertionNestedRepeatSingle(EPServiceProvider epService, bool soda) {
            RunTwiceAB(epService, soda, "(A B) (A B)");
            RunTwiceAB(epService, soda, "(A B){2}");
    
            RunAThenTwiceBC(epService, soda, "A (B C) (B C)");
            RunAThenTwiceBC(epService, soda, "A (B C){2}");
        }
    
        private void RunAssertionNestedRepeatRange(EPServiceProvider epService, bool soda) {
            RunOnceOrTwiceABThenC(epService, soda, "(A B) (A B)? C");
            RunOnceOrTwiceABThenC(epService, soda, "(A B){1,2} C");
        }
    
        private void RunAssertionsAtLeast(EPServiceProvider epService, bool soda) {
            RunAtLeast2AThenB(epService, soda, "A A A* B");
            RunAtLeast2AThenB(epService, soda, "A{2,} B");
            RunAtLeast2AThenB(epService, soda, "A{2,4} B");
        }
    
        private void RunAssertionsUpTo(EPServiceProvider epService, bool soda) {
            RunUpTo2AThenB(epService, soda, "A? A? B");
            RunUpTo2AThenB(epService, soda, "A{,2} B");
        }
    
        private void RunAssertionsRepeatRange(EPServiceProvider epService, bool soda) {
            Run2To3AThenB(epService, soda, "A A A? B");
            Run2To3AThenB(epService, soda, "A{2,3} B");
        }
    
        private void RunAssertionsNestedUpTo(EPServiceProvider epService, bool soda) {
            RunUpTo2ABThenC(epService, soda, "(A B)? (A B)? C");
            RunUpTo2ABThenC(epService, soda, "(A B){,2} C");
        }
    
        private void RunAssertionsNestedAtLeast(EPServiceProvider epService, bool soda) {
            RunAtLeast2ABThenC(epService, soda, "(A B) (A B) (A B)* C");
            RunAtLeast2ABThenC(epService, soda, "(A B){2,} C");
        }
    
        private void RunAssertionRepeatSingleBound(EPServiceProvider epService, bool soda) {
            RunExactly2A(epService, soda, "A A");
            RunExactly2A(epService, soda, "A{2}");
            RunExactly2A(epService, soda, "(A{2})");
    
            // concatenation
            RunAThen2BThenC(epService, soda, "A B B C");
            RunAThen2BThenC(epService, soda, "A B{2} C");
    
            // nested
            RunAThen2BThenC(epService, false, "A (B B) C");
            RunAThen2BThenC(epService, false, "A (B{2}) C");
    
            // alteration
            RunAThen2BOr2C(epService, soda, "A (B B|C C)");
            RunAThen2BOr2C(epService, soda, "A (B{2}|C{2})");
    
            // multiple
            Run2AThen2B(epService, soda, "A A B B");
            Run2AThen2B(epService, soda, "A{2} B{2}");
        }
    
        private void RunAtLeast2ABThenC(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{true, true, false}, new string[]{
                    "A1,B1,A2,B2,C1",
                    "A1,B1,A2,B2,A3,B3,C1"
            }, new string[]{"A1,B1,C1", "A1,B1,A2,C1", "B1,A1,B2,C1"});
        }
    
        private void RunOnceOrTwiceABThenC(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{true, true, false}, new string[]{
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1"
            }, new string[]{"C1", "A1,A2,C2", "B1,A1,C1"});
        }
    
        private void RunAtLeast2AThenB(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b", new bool[]{true, false}, new string[]{
                    "A1,A2,B1",
                    "A1,A2,A3,B1"
            }, new string[]{"A1,B1", "B1"});
        }
    
        private void RunUpTo2AThenB(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b", new bool[]{true, false}, new string[]{
                    "B1",
                    "A1,B1",
                    "A1,A2,B1"
            }, new string[]{"A1"});
        }
    
        private void Run2AThen2B(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b", new bool[]{true, true}, new string[]{
                    "A1,A2,B1,B2",
            }, new string[]{"A1,A2,B1", "B1,B2,A1,A2", "A1,B1,A2,B2"});
        }
    
        private void RunUpTo2ABThenC(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{true, true, false}, new string[]{
                    "C1",
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1",
            }, new string[]{"A1,B1,A2,B2", "A1,A2"});
        }
    
        private void Run2To3AThenB(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b", new bool[]{true, false}, new string[]{
                    "A1,A2,A3,B1",
                    "A1,A2,B1",
            }, new string[]{"A1,B1", "A1,A2", "B1"});
        }
    
        private void RunAThen2BOr2C(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{false, true, true}, new string[]{
                    "A1,C1,C2",
                    "A2,B1,B2",
            }, new string[]{"B1,B2", "C1,C2", "A1,B1,C1", "A1,C1,B1"});
        }
    
        private void RunTwiceAB(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b", new bool[]{true, true}, new string[]{
                    "A1,B1,A2,B2",
            }, new string[]{"A1,A2,B1", "A1,A2,B1,B2", "A1,B1,B2,A2"});
        }
    
        private void RunAThenTwiceBC(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{false, true, true}, new string[]{
                    "A1,B1,C1,B2,C2",
            }, new string[]{"A1,B1,C1,B2", "A1,B1,C1,C2", "A1,B1,B2,C1,C2"});
        }
    
        private void RunAThen2BThenC(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a,b,c", new bool[]{false, true, false}, new string[]{
                    "A1,B1,B2,C1",
            }, new string[]{"B1,B2,C1", "A1,B1,C1", "A1,B1,B2"});
        }
    
        private void RunExactly2A(EPServiceProvider epService, bool soda, string pattern) {
            RunAssertion(epService, soda, pattern, "a", new bool[]{true}, new string[]{
                    "A1,A2",
                    "A3,A4",
            }, new string[]{"A5"});
        }
    
        private void RunAssertion(EPServiceProvider epService, bool soda, string pattern, string propertyNames, bool[] arrayProp,
                                  string[] sequencesWithMatch,
                                  string[] sequencesNoMatch) {
            string[] props = propertyNames.Split(',');
            string measures = MakeMeasures(props);
            string defines = MakeDefines(props);
    
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    " partition by intPrimitive" +
                    " measures " + measures +
                    " pattern (" + pattern + ")" +
                    " define " + defines +
                    ")";
            var listener = new SupportUpdateListener();
            SupportModelHelper.CreateByCompileOrParse(epService, soda, text).AddListener(listener);
    
            int sequenceNum = 0;
            foreach (string aSequencesWithMatch in sequencesWithMatch) {
                RunAssertionSequence(epService, listener, true, props, arrayProp, sequenceNum, aSequencesWithMatch);
                sequenceNum++;
            }
    
            foreach (string aSequencesNoMatch in sequencesNoMatch) {
                RunAssertionSequence(epService, listener, false, props, arrayProp, sequenceNum, aSequencesNoMatch);
                sequenceNum++;
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSequence(EPServiceProvider epService, SupportUpdateListener listener, bool match, string[] propertyNames, bool[] arrayProp, int sequenceNum, string sequence) {
    
            // send events
            string[] events = sequence.Split(',');
            var sent = new Dictionary<string, object>();
            foreach (string anEvent in events) {
                var type = new String(new char[]{anEvent.CharAt(0)});
                SupportBean bean = SendEvent(anEvent, sequenceNum, epService);
                string propName = type.ToLowerCase(Locale.ENGLISH);
                if (!sent.ContainsKey(propName)) {
                    sent.Put(propName, new List<>());
                }
                sent.Get(propName).Add(bean);
            }
    
            // prepare expected
            var expected = new Object[propertyNames.Length];
            for (int i = 0; i < propertyNames.Length; i++) {
                List<SupportBean> sentForType = sent.Get(propertyNames[i]);
                if (arrayProp[i]) {
                    expected[i] = sentForType == null ? null : sentForType.ToArray(new SupportBean[0]);
                } else {
                    if (match) {
                        Assert.IsTrue(sentForType.Count == 1);
                        expected[i] = sentForType[0];
                    }
                }
            }
    
            if (match) {
                EventBean @event = listener.AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(@event, propertyNames, expected);
            } else {
                Assert.IsFalse("Failed at " + sequence, listener.IsInvoked);
            }
        }
    
        private string MakeDefines(string[] props) {
            string delimiter = "";
            var buf = new StringWriter();
            foreach (string prop in props) {
                buf.Append(delimiter);
                delimiter = ", ";
                buf.Append(prop.ToUpperCase(Locale.ENGLISH));
                buf.Append(" as ");
                buf.Append(prop.ToUpperCase(Locale.ENGLISH));
                buf.Append(".theString like \"");
                buf.Append(prop.ToUpperCase(Locale.ENGLISH));
                buf.Append("%\"");
            }
            return Buf.ToString();
        }
    
        private string MakeMeasures(string[] props) {
            string delimiter = "";
            var buf = new StringWriter();
            foreach (string prop in props) {
                buf.Append(delimiter);
                delimiter = ", ";
                buf.Append(prop.ToUpperCase(Locale.ENGLISH));
                buf.Append(" as ");
                buf.Append(prop);
            }
            return Buf.ToString();
        }
    
        private SupportBean SendEvent(string theString, int intPrimitive, EPServiceProvider epService) {
            var sb = new SupportBean(theString, intPrimitive);
            epService.EPRuntime.SendEvent(sb);
            return sb;
        }
    
        internal static void RunEquivalent(EPServiceProvider epService, string before, string after) {
            string epl = "select * from SupportBean#keepall " +
                    "match_recognize (" +
                    " measures A as a" +
                    " pattern (" + before + ")" +
                    " define" +
                    " A as A.theString like \"A%\"" +
                    ")";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            EPStatementSPI spi = (EPStatementSPI) epService.EPAdministrator.Create(model);
            StatementSpecCompiled spec = ((EPServiceProviderSPI) epService).StatementLifecycleSvc.GetStatementSpec(spi.StatementId);
            RowRegexExprNode expanded = RegexPatternExpandUtil.Expand(spec.MatchRecognizeSpec.Pattern);
            var writer = new StringWriter();
            expanded.ToEPL(writer, RowRegexExprNodePrecedenceEnum.MINIMUM);
            Assert.AreEqual(after, writer.ToString());
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
