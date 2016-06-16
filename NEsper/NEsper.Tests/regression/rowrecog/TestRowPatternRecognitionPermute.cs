///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
	public class TestRowPatternRecognitionPermute
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
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
	    public void TestPermute() {
	        RunPermute(false);
	        RunPermute(true);

	        RunDocSamples();

	        RunEquivalent(_epService, "mAtCh_Recognize_Permute(A)",
	                "(A)");
	        RunEquivalent(_epService, "match_recognize_permute(A,B)",
	                "(A B|B A)");
	        RunEquivalent(_epService, "match_recognize_permute(A,B,C)",
	                "(A B C|A C B|B A C|B C A|C A B|C B A)");
	        RunEquivalent(_epService, "match_recognize_permute(A,B,C,D)",
	                "(A B C D|A B D C|A C B D|A C D B|A D B C|A D C B|B A C D|B A D C|B C A D|B C D A|B D A C|B D C A|C A B D|C A D B|C B A D|C B D A|C D A B|C D B A|D A B C|D A C B|D B A C|D B C A|D C A B|D C B A)");

	        RunEquivalent(_epService, "match_recognize_permute((A B), C)",
	                "((A B) C|C (A B))");
	        RunEquivalent(_epService, "match_recognize_permute((A|B), (C D), E)",
	                "((A|B) (C D) E|(A|B) E (C D)|(C D) (A|B) E|(C D) E (A|B)|E (A|B) (C D)|E (C D) (A|B))");

	        RunEquivalent(_epService, "A match_recognize_permute(B,C) D",
	                "A (B C|C B) D");

	        RunEquivalent(_epService, "match_recognize_permute(A, match_recognize_permute(B, C))",
	                "(A (B C|C B)|(B C|C B) A)");
	    }

	    private void RunDocSamples() {
	        _epService.EPAdministrator.CreateEPL("create objectarray schema TemperatureSensorEvent(id string, device string, temp int)");

	        RunDocSampleUpToN();
	    }

	    private void RunDocSampleUpToN() {
	        string[] fields = "a_id,b_id".Split(',');
	        string epl = "select * from TemperatureSensorEvent\n" +
	                "match_recognize (\n" +
	                "  partition by device\n" +
	                "  measures A.id as a_id, B.id as b_id\n" +
	                "  pattern (match_recognize_permute(A, B))\n" +
	                "  define \n" +
	                "\tA as A.temp < 100, \n" +
	                "\tB as B.temp >= 100)";
	        Debug.WriteLine(epl);
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new object[] {"E1", "1", 99}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E2", "1", 100}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E2"});

	        _epService.EPRuntime.SendEvent(new object[] {"E3", "1", 100}, "TemperatureSensorEvent");
	        _epService.EPRuntime.SendEvent(new object[] {"E4", "1", 99}, "TemperatureSensorEvent");
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E4", "E3"});

	        _epService.EPRuntime.SendEvent(new object[] {"E5", "1", 98}, "TemperatureSensorEvent");
	        Assert.IsFalse(_listener.IsInvoked);

	        stmt.Dispose();
	    }

	    private void RunPermute(bool soda) {
	        TryPermute(soda, "(A B C)|(A C B)|(B A C)|(B C A)|(C A B)|(C B A)");
	        TryPermute(soda, "(match_recognize_permute(A,B,C))");
	    }

	    public void TryPermute(bool soda, string pattern)
	    {
	        string epl = "select * from SupportBean " +
	                "match_recognize (" +
	                " partition by intPrimitive" +
	                " measures A as a, B as b, C as c" +
	                " pattern (" + pattern + ")" +
	                " define" +
	                " A as A.theString like \"A%\"," +
	                " B as B.theString like \"B%\"," +
	                " C as C.theString like \"C%\"" +
	                ")";
	        EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl);
	        stmt.AddListener(_listener);

	        string[] prefixes = "A,B,C".Split(',');
	        string[] fields = "a,b,c".Split(',');

	        var e = PermutationEnumerator.Create(3);
	        int count = 0;

            foreach(int[] indexes in e) {
	            var expected = new object[3];
	            for (int i = 0; i < 3; i++) {
	                expected[indexes[i]] = SendEvent(prefixes[indexes[i]] + count.ToString(), count);
	            }
	            count++;

	            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
	        }

	        stmt.Dispose();
	    }

	    private static void RunEquivalent(EPServiceProvider epService, string before, string after) {
	        TestRowPatternRecognitionRepetition.RunEquivalent(epService, before, after);
	    }

	    private SupportBean SendEvent(string theString, int intPrimitive) {
	        SupportBean sb = new SupportBean(theString, intPrimitive);
	        _epService.EPRuntime.SendEvent(sb);
	        return sb;
	    }
	}
} // end of namespace
