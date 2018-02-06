///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternInvalid
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
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
        public void TestInvalid()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();

            var text = "select * from MyEvent " +
                       "match_recognize (" +
                       " measures A as a_array" +
                       " pattern (A+ B)" +
                       " define" +
                       " A as A.TheString = B.TheString)";
            TryInvalid(text, "Error starting statement: Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'A.TheString=B.TheString': Failed to find a stream named 'B' (did you mean 'A'?) [select * from MyEvent match_recognize ( measures A as a_array pattern (A+ B) define A as A.TheString = B.TheString)]");

            // invalid after syntax
            text = "select * from MyEvent#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a" +
                   "  AFTER MATCH SKIP TO OTHER ROW " +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalid(text, "Match-recognize AFTER clause must be either AFTER MATCH SKIP TO LAST ROW or AFTER MATCH SKIP TO NEXT ROW or AFTER MATCH SKIP TO CURRENT ROW [select * from MyEvent#keepall match_recognize (  measures A.TheString as a  AFTER MATCH SKIP TO OTHER ROW   pattern (A B*)   define     A as A.TheString like 'A%')]");

            // property cannot resolve
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a, D.TheString as x" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Failed to validate match-recognize measure expression 'D.TheString': Failed to resolve property 'D.TheString' to a stream or nested property in a stream [select * from MyEvent#keepall match_recognize (  measures A.TheString as a, D.TheString as x  pattern (A B*)   define     A as A.TheString like 'A%')]");
    
            // property not named
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString, A.TheString as xxx" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: The measures clause requires that each expression utilizes the AS keyword to assign a column name [select * from MyEvent#keepall match_recognize (  measures A.TheString, A.TheString as xxx  pattern (A B*)   define     A as A.TheString like 'A%')]");
    
            // grouped property not indexed
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures B.TheString as b1" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Failed to validate match-recognize measure expression 'B.TheString': Failed to resolve property 'B.TheString' (property 'B' is an indexed property and requires an index or enumeration method to access values) [select * from MyEvent#keepall match_recognize (  measures B.TheString as b1  pattern (A B*)   define     A as A.TheString like 'A%')]");
    
            // define twice
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'," +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Variable 'A' has already been defined [select * from MyEvent#keepall match_recognize (  measures A.TheString as a  pattern (A B*)   define     A as A.TheString like 'A%',    A as A.TheString like 'A%')]");
    
            // define for not used variable
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    X as X.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Variable 'X' does not occur in pattern [select * from MyEvent#keepall match_recognize (  measures A.TheString as a  pattern (A B*)   define     X as X.TheString like 'A%')]");
    
            // define mentions another variable
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as B.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'B.TheString like \"A%\"': Failed to find a stream named 'B' (did you mean 'A'?) [select * from MyEvent#keepall match_recognize (  measures A.TheString as a  pattern (A B*)   define     A as B.TheString like 'A%')]");
    
            // aggregation over multiple groups
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures sum(A.value+B.value) as mytotal" +
                    "  pattern (A* B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Aggregation functions in the measure-clause must only refer to properties of exactly one group variable returning multiple events [select * from MyEvent#keepall match_recognize (  measures sum(A.value+B.value) as mytotal  pattern (A* B*)   define     A as A.TheString like 'A%')]");
    
            // aggregation over no groups
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures sum(A.value) as mytotal" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Aggregation functions in the measure-clause must refer to one or more properties of exactly one group variable returning multiple events [select * from MyEvent#keepall match_recognize (  measures sum(A.value) as mytotal  pattern (A B*)   define     A as A.TheString like 'A%')]");
    
            // aggregation in define
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as astring" +
                    "  pattern (A B) " +
                    "  define " +
                    "    A as sum(A.Value + A.Value) > 3000" +
                    ")";
            TryInvalid(text, "Error starting statement: Failed to validate condition expression for variable 'A': An aggregate function may not appear in a DEFINE clause [select * from MyEvent#keepall match_recognize (  measures A.TheString as astring  pattern (A B)   define     A as sum(A.Value + A.Value) > 3000)]");
    
            // join disallowed
            text = "select * from MyEvent#keepall, MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.value as aval" +
                    "  pattern (A B*) " +
                    "  define " +
                    "    A as A.TheString like 'A%'" +
                    ")";
            TryInvalid(text, "Error starting statement: Joins are not allowed when using match-recognize [select * from MyEvent#keepall, MyEvent#keepall match_recognize (  measures A.value as aval  pattern (A B*)   define     A as A.TheString like 'A%')]");
        }
    
        private void TryInvalid(String epl, String error) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(error, ex.Message);
            }
        }
    }
}
