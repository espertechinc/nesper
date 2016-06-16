///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.rowregex;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
	public class TestRowPatternRecognitionRepetition
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestRepeat()
	    {
	        RunAssertionRepeat(false);
	        RunAssertionRepeat(true);

	        RunAssertionPrev();

	        RunInvalid();

	        RunDocSamples();
	    }

	    private void RunDocSamples() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema TemperatureSensorEvent(id string, device string, temp int)");

	        RunDocSampleExactlyN();
	        RunDocSampleNOrMore_and_BetweenNandM("A{2,} B");
	        RunDocSampleNOrMore_and_BetweenNandM("A{2,3} B");
	        RunDocSampleUpToN();
	    }

	    private void RunDocSampleUpToN() {
	        var fields = "a0_id,a1_id,b_id".Split(',');
	        var epl = "select * from TemperatureSensorEvent\n" +
	                "match_recognize (\n" +
	                "  partition by device\n" +
	                "  measures A[0].id as a0_id, A[1].id as a1_id, B.id as b_id\n" +
	                "  pattern (A{,2} B)\n" +
	                "  define \n" +
	                "\tA as A.temp >= 100,\n" +
	                "\tB as B.temp >= 102)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "1", 99}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E2", "1", 100}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E3", "1", 100}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E4", "1", 101}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E5", "1", 102}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", "E4", "E5"});

	        stmt.Dispose();
	    }

	    private void RunDocSampleNOrMore_and_BetweenNandM(string pattern) {
	        var fields = "a0_id,a1_id,a2_id,b_id".Split(',');
	        var epl = "select * from TemperatureSensorEvent\n" +
	                "match_recognize (\n" +
	                "  partition by device\n" +
	                "  measures A[0].id as a0_id, A[1].id as a1_id, A[2].id as a2_id, B.id as b_id\n" +
	                "  pattern (" + pattern + ")\n" +
	                "  define \n" +
	                "\tA as A.temp >= 100,\n" +
	                "\tB as B.temp >= 102)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "1", 99}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E2", "1", 100}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E3", "1", 100}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E4", "1", 101}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E5", "1", 102}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E3", "E4", "E5"});

	        stmt.Dispose();
	    }

	    private void RunDocSampleExactlyN() {
	        var fields = "a0_id,a1_id".Split(',');
	        var epl = "select * from TemperatureSensorEvent\n" +
	                "match_recognize (\n" +
	                "  partition by device\n" +
	                "  measures A[0].id as a0_id, A[1].id as a1_id\n" +
	                "  pattern (A{2})\n" +
	                "  define \n" +
	                "\tA as A.temp >= 100)";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "1", 99}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E2", "1", 100}, "TemperatureSensorEvent");

	        _epService.EPRuntime.SendEvent(new object[] {"E3", "1", 100}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E2", "E3"});

	        _epService.EPRuntime.SendEvent(new object[]{"E4", "1", 101}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E5", "1", 102}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "E5"});

	        stmt.Dispose();
	    }

	    private void RunInvalid() {
	        var template = "select * from SupportBean " +
	                "match_recognize (" +
	                "  measures A as a" +
	                "  pattern (REPLACE) " +
	                ")";
	        _epService.EPAdministrator.CreateEPL("create variable int myvariable = 0");

            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{}"),
	                "Invalid match-recognize quantifier '{}', expecting an expression");
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{null}"),
	                "Error starting statement: pattern quantifier 'null' must return an integer-type value");
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{myvariable}"),
	                "Error starting statement: pattern quantifier 'myvariable' must return a constant value");
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{prev(A)}"),
	                "Error starting statement: Invalid match-recognize pattern expression 'pattern quantifier");

	        var expected = "Error starting statement: Invalid pattern quantifier value -1, expecting a minimum of 1";
	        SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{-1}"), expected);
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{,-1}"), expected);
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{-1,10}"), expected);
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{-1,}"), expected);
            SupportMessageAssertUtil.TryInvalid(_epService, template.RegexReplaceAll("REPLACE", "A{5,3}"),
	                "Error starting statement: Invalid pattern quantifier value 5, expecting a minimum of 1 and maximum of 3");
	    }

	    private void RunAssertionPrev() {
	        var text = "select * from SupportBean " +
	                "match_recognize (" +
	                "  measures A as a" +
	                "  pattern (A{3}) " +
	                "  define " +
	                "    A as A.intPrimitive > prev(A.intPrimitive)" +
	                ")";

	        var stmt = _epService.EPAdministrator.CreateEPL(text);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        SendEvent("A1", 1);
	        SendEvent("A2", 4);
	        SendEvent("A3", 2);
	        SendEvent("A4", 6);
	        SendEvent("A5", 5);
	        var b6 = SendEvent("A6", 6);
	        var b7 = SendEvent("A7", 7);
	        var b8 = SendEvent("A9", 8);
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new object[] {new object[] {b6,b7,b8}});
	    }

	    private void RunAssertionRepeat(bool soda) {
	        // Atom Assertions
	        //
	        //

	        // single-bound assertions
	        RunAssertionRepeatSingleBound(soda);

	        // defined-range assertions
	        RunAssertionsRepeatRange(soda);

	        // lower-bounds assertions
	        RunAssertionsUpTo(soda);

	        // upper-bounds assertions
	        RunAssertionsAtLeast(soda);

	        // Nested Assertions
	        //
	        //

	        // single-bound nested assertions
	        RunAssertionNestedRepeatSingle(soda);

	        // defined-range nested assertions
	        RunAssertionNestedRepeatRange(soda);

	        // lower-bounds nested assertions
	        RunAssertionsNestedUpTo(soda);

	        // upper-bounds nested assertions
	        RunAssertionsNestedAtLeast(soda);
	    }

        [Test]
	    public void TestEquivalent() {
	        //
	        // Single-bounds Repeat.
	        //
	        RunEquivalent(_epService, "A{1}", "A");
	        RunEquivalent(_epService, "A{2}", "A A");
	        RunEquivalent(_epService, "A{3}", "A A A");
	        RunEquivalent(_epService, "A{1} B{2}", "A B B");
	        RunEquivalent(_epService, "A{1} B{2} C{3}", "A B B C C C");
	        RunEquivalent(_epService, "(A{2})", "(A A)");
	        RunEquivalent(_epService, "A?{2}", "A? A?");
	        RunEquivalent(_epService, "A*{2}", "A* A*");
	        RunEquivalent(_epService, "A+{2}", "A+ A+");
	        RunEquivalent(_epService, "A??{2}", "A?? A??");
	        RunEquivalent(_epService, "A*?{2}", "A*? A*?");
	        RunEquivalent(_epService, "A+?{2}", "A+? A+?");
	        RunEquivalent(_epService, "(A B){1}", "(A B)");
	        RunEquivalent(_epService, "(A B){2}", "(A B) (A B)");
	        RunEquivalent(_epService, "(A B)?{2}", "(A B)? (A B)?");
	        RunEquivalent(_epService, "(A B)*{2}", "(A B)* (A B)*");
	        RunEquivalent(_epService, "(A B)+{2}", "(A B)+ (A B)+");

	        RunEquivalent(_epService, "A B{2} C", "A B B C");
	        RunEquivalent(_epService, "A (B{2}) C", "A (B B) C");
	        RunEquivalent(_epService, "(A{2}) C", "(A A) C");
	        RunEquivalent(_epService, "A (B{2}|C{2})", "A (B B|C C)");
	        RunEquivalent(_epService, "A{2} B{2} C{2}", "A A B B C C");
	        RunEquivalent(_epService, "A{2} B C{2}", "A A B C C");
	        RunEquivalent(_epService, "A B{2} C{2}", "A B B C C");

	        // range bounds
	        RunEquivalent(_epService, "A{1, 3}", "A A? A?");
	        RunEquivalent(_epService, "A{2, 4}", "A A A? A?");
	        RunEquivalent(_epService, "A?{1, 3}", "A? A? A?");
	        RunEquivalent(_epService, "A*{1, 3}", "A* A* A*");
	        RunEquivalent(_epService, "A+{1, 3}", "A+ A* A*");
	        RunEquivalent(_epService, "A??{1, 3}", "A?? A?? A??");
	        RunEquivalent(_epService, "A*?{1, 3}", "A*? A*? A*?");
	        RunEquivalent(_epService, "A+?{1, 3}", "A+? A*? A*?");
	        RunEquivalent(_epService, "(A B)?{1, 3}", "(A B)? (A B)? (A B)?");
	        RunEquivalent(_epService, "(A B)*{1, 3}", "(A B)* (A B)* (A B)*");
	        RunEquivalent(_epService, "(A B)+{1, 3}", "(A B)+ (A B)* (A B)*");

	        // lower-only bounds
	        RunEquivalent(_epService, "A{2,}", "A A A*");
	        RunEquivalent(_epService, "A?{2,}", "A? A? A*");
	        RunEquivalent(_epService, "A*{2,}", "A* A* A*");
	        RunEquivalent(_epService, "A+{2,}", "A+ A+ A*");
	        RunEquivalent(_epService, "A??{2,}", "A?? A?? A*?");
	        RunEquivalent(_epService, "A*?{2,}", "A*? A*? A*?");
	        RunEquivalent(_epService, "A+?{2,}", "A+? A+? A*?");
	        RunEquivalent(_epService, "(A B)?{2,}", "(A B)? (A B)? (A B)*");
	        RunEquivalent(_epService, "(A B)*{2,}", "(A B)* (A B)* (A B)*");
	        RunEquivalent(_epService, "(A B)+{2,}", "(A B)+ (A B)+ (A B)*");

	        // upper-only bounds
	        RunEquivalent(_epService, "A{,2}", "A? A?");
	        RunEquivalent(_epService, "A?{,2}", "A? A?");
	        RunEquivalent(_epService, "A*{,2}", "A* A*");
	        RunEquivalent(_epService, "A+{,2}", "A* A*");
	        RunEquivalent(_epService, "A??{,2}", "A?? A??");
	        RunEquivalent(_epService, "A*?{,2}", "A*? A*?");
	        RunEquivalent(_epService, "A+?{,2}", "A*? A*?");
	        RunEquivalent(_epService, "(A B){,2}", "(A B)? (A B)?");
	        RunEquivalent(_epService, "(A B)?{,2}", "(A B)? (A B)?");
	        RunEquivalent(_epService, "(A B)*{,2}", "(A B)* (A B)*");
	        RunEquivalent(_epService, "(A B)+{,2}", "(A B)* (A B)*");

	        //
	        // Nested Repeat.
	        //
	        RunEquivalent(_epService, "(A B){2}", "(A B) (A B)");
	        RunEquivalent(_epService, "(A){2}", "A A");
	        RunEquivalent(_epService, "(A B C){3}", "(A B C) (A B C) (A B C)");
	        RunEquivalent(_epService, "(A B){2} (C D){2}", "(A B) (A B) (C D) (C D)");
	        RunEquivalent(_epService, "((A B){2} C){2}", "((A B) (A B) C) ((A B) (A B) C)");
	        RunEquivalent(_epService, "((A|B){2} (C|D){2}){2}", "((A|B) (A|B) (C|D) (C|D)) ((A|B) (A|B) (C|D) (C|D))");
	    }

	    private void RunAssertionNestedRepeatSingle(bool soda) {
	        RunTwiceAB(soda, "(A B) (A B)");
	        RunTwiceAB(soda, "(A B){2}");

	        RunAThenTwiceBC(soda, "A (B C) (B C)");
	        RunAThenTwiceBC(soda, "A (B C){2}");
	    }

	    private void RunAssertionNestedRepeatRange(bool soda) {
	        RunOnceOrTwiceABThenC(soda, "(A B) (A B)? C");
	        RunOnceOrTwiceABThenC(soda, "(A B){1,2} C");
	    }

	    private void RunAssertionsAtLeast(bool soda) {
	        RunAtLeast2AThenB(soda, "A A A* B");
	        RunAtLeast2AThenB(soda, "A{2,} B");
	        RunAtLeast2AThenB(soda, "A{2,4} B");
	    }

	    private void RunAssertionsUpTo(bool soda) {
	        RunUpTo2AThenB(soda, "A? A? B");
	        RunUpTo2AThenB(soda, "A{,2} B");
	    }

	    private void RunAssertionsRepeatRange(bool soda) {
	        Run2To3AThenB(soda, "A A A? B");
	        Run2To3AThenB(soda, "A{2,3} B");
	    }

	    private void RunAssertionsNestedUpTo(bool soda) {
	        RunUpTo2ABThenC(soda, "(A B)? (A B)? C");
	        RunUpTo2ABThenC(soda, "(A B){,2} C");
	    }

	    private void RunAssertionsNestedAtLeast(bool soda) {
	        RunAtLeast2ABThenC(soda, "(A B) (A B) (A B)* C");
	        RunAtLeast2ABThenC(soda, "(A B){2,} C");
	    }

	    private void RunAssertionRepeatSingleBound(bool soda) {
	        RunExactly2A(soda, "A A");
	        RunExactly2A(soda, "A{2}");
	        RunExactly2A(soda, "(A{2})");

	        // concatenation
	        RunAThen2BThenC(soda, "A B B C");
	        RunAThen2BThenC(soda, "A B{2} C");

	        // nested
	        RunAThen2BThenC(false, "A (B B) C");
	        RunAThen2BThenC(false, "A (B{2}) C");

	        // alteration
	        RunAThen2BOr2C(soda, "A (B B|C C)");
	        RunAThen2BOr2C(soda, "A (B{2}|C{2})");

	        // multiple
	        Run2AThen2B(soda, "A A B B");
	        Run2AThen2B(soda, "A{2} B{2}");
	    }

	    private void RunAtLeast2ABThenC(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {true, true, false}, new string[] {
	                "A1,B1,A2,B2,C1",
	                "A1,B1,A2,B2,A3,B3,C1"
	        }, new string[] {"A1,B1,C1", "A1,B1,A2,C1", "B1,A1,B2,C1"});
	    }

	    private void RunOnceOrTwiceABThenC(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {true, true, false}, new string[] {
	                "A1,B1,C1",
	                "A1,B1,A2,B2,C1"
	        }, new string[] {"C1", "A1,A2,C2", "B1,A1,C1"});
	    }

	    private void RunAtLeast2AThenB(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b", new bool[] {true, false}, new string[] {
	                "A1,A2,B1",
	                "A1,A2,A3,B1"
	        }, new string[] {"A1,B1", "B1"});
	    }

	    private void RunUpTo2AThenB(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b", new bool[] {true, false}, new string[] {
	                "B1",
	                "A1,B1",
	                "A1,A2,B1"
	        }, new string[] {"A1"});
	    }

	    private void Run2AThen2B(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b", new bool[] {true, true}, new string[] {
	                "A1,A2,B1,B2",
	        }, new string[] {"A1,A2,B1", "B1,B2,A1,A2", "A1,B1,A2,B2"});
	    }

	    private void RunUpTo2ABThenC(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {true, true, false}, new string[] {
	                "C1",
	                "A1,B1,C1",
	                "A1,B1,A2,B2,C1",
	        }, new string[] {"A1,B1,A2,B2", "A1,A2"});
	    }

	    private void Run2To3AThenB(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b", new bool[] {true, false}, new string[] {
	                "A1,A2,A3,B1",
	                "A1,A2,B1",
	        }, new string[] {"A1,B1", "A1,A2", "B1"});
	    }

	    private void RunAThen2BOr2C(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {false, true, true}, new string[] {
	                "A1,C1,C2",
	                "A2,B1,B2",
	        }, new string[] {"B1,B2", "C1,C2", "A1,B1,C1", "A1,C1,B1"});
	    }

	    private void RunTwiceAB(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b", new bool[] {true, true}, new string[] {
	                "A1,B1,A2,B2",
	        }, new string[] {"A1,A2,B1", "A1,A2,B1,B2", "A1,B1,B2,A2"});
	    }

	    private void RunAThenTwiceBC(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {false, true, true}, new string[] {
	                "A1,B1,C1,B2,C2",
	        }, new string[] {"A1,B1,C1,B2", "A1,B1,C1,C2", "A1,B1,B2,C1,C2"});
	    }

	    private void RunAThen2BThenC(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a,b,c", new bool[] {false, true, false}, new string[] {
	                "A1,B1,B2,C1",
	        }, new string[] {"B1,B2,C1", "A1,B1,C1", "A1,B1,B2"});
	    }

	    private void RunExactly2A(bool soda, string pattern) {
	        RunAssertion(soda, pattern, "a", new bool[] {true}, new string[]{
	                "A1,A2",
	                "A3,A4",
	        }, new string[] {"A5"});
	    }

	    private void RunAssertion(bool soda, string pattern, string propertyNames, bool[] arrayProp,
	                              string[] sequencesWithMatch,
	                              string[] sequencesNoMatch) {
	        var props = propertyNames.Split(',');
	        var measures = MakeMeasures(props);
	        var defines = MakeDefines(props);

	        var text = "select * from SupportBean " +
	                "match_recognize (" +
	                " partition by intPrimitive" +
	                " measures " + measures +
	                " pattern (" + pattern + ")" +
	                " define " + defines +
	                ")";
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, text).AddListener(_listener);

	        var sequenceNum = 0;
	        for (var i = 0; i < sequencesWithMatch.Length; i++) {
	            RunAssertionSequence(true, props, arrayProp, sequenceNum, sequencesWithMatch[i]);
	            sequenceNum++;
	        }

	        for (var i = 0; i < sequencesNoMatch.Length; i++) {
	            RunAssertionSequence(false, props, arrayProp, sequenceNum, sequencesNoMatch[i]);
	            sequenceNum++;
	        }

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionSequence(bool match, string[] propertyNames, bool[] arrayProp, int sequenceNum, string sequence) {

	        // send events
	        var events = sequence.Split(',');
	        IDictionary<string, IList<SupportBean>> sent = new Dictionary<string, IList<SupportBean>>();
	        foreach (var anEvent in events) {
	            var type = new string(new char[]{anEvent[0]});
	            var bean = SendEvent(anEvent, sequenceNum);
	            var propName = type.ToLower();
	            if (!sent.ContainsKey(propName)) {
	                sent.Put(propName, new List<SupportBean>());
	            }
	            sent.Get(propName).Add(bean);
	        }

	        // prepare expected
	        var expected = new object[propertyNames.Length];
	        for (var i = 0; i < propertyNames.Length; i++) {
	            var sentForType = sent.Get(propertyNames[i]);
	            if (arrayProp[i]) {
	                expected[i] = sentForType == null ? null : sentForType.ToArray();
	            }
	            else {
	                if (match) {
	                    Assert.IsTrue(sentForType.Count == 1);
	                    expected[i] = sentForType[0];
	                }
	            }
	        }

	        if (match) {
	            var @event = _listener.AssertOneGetNewAndReset();
	            EPAssertionUtil.AssertProps(@event, propertyNames, expected);
	        }
	        else {
                Assert.IsFalse(_listener.IsInvoked, "Failed at " + sequence);
	        }
	    }

	    private string MakeDefines(string[] props) {
	        var delimiter = "";
	        var buf = new StringWriter();
	        foreach (var prop in props) {
	            buf.Write(delimiter);
	            delimiter = ", ";
                buf.Write(prop.ToUpper());
                buf.Write(" as ");
                buf.Write(prop.ToUpper());
                buf.Write(".theString like \"");
                buf.Write(prop.ToUpper());
                buf.Write("%\"");
	        }
	        return buf.ToString();
	    }

	    private string MakeMeasures(string[] props) {
	        var delimiter = "";
	        var buf = new StringWriter();
	        foreach (var prop in props) {
                buf.Write(delimiter);
	            delimiter = ", ";
                buf.Write(prop.ToUpper());
                buf.Write(" as ");
                buf.Write(prop);
	        }
	        return buf.ToString();
	    }

	    private SupportBean SendEvent(string theString, int intPrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        _epService.EPRuntime.SendEvent(sb);
	        return sb;
	    }

	    internal static void RunEquivalent(EPServiceProvider epService, string before, string after) {
	        var epl = "select * from SupportBean.win:keepall() " +
	                "match_recognize (" +
	                " measures A as a" +
	                " pattern (" + before + ")" +
	                " define" +
	                " A as A.theString like \"A%\"" +
	                ")";
	        var model = epService.EPAdministrator.CompileEPL(epl);
	        var spi = (EPStatementSPI) epService.EPAdministrator.Create(model);
	        var spec = ((EPServiceProviderSPI) (epService)).StatementLifecycleSvc.GetStatementSpec(spi.StatementId);
	        var expanded = RegexPatternExpandUtil.Expand(spec.MatchRecognizeSpec.Pattern);
	        var writer = new StringWriter();
	        expanded.ToEPL(writer, RowRegexExprNodePrecedenceEnum.MINIMUM);
	        Assert.AreEqual(after, writer.ToString());
	        epService.EPAdministrator.DestroyAllStatements();
	    }
	}
} // end of namespace
