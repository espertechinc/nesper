///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestInvalidSyntaxMsg 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestInvalidSyntax()
        {
            TryCompile("insert into 7event select * from " + typeof(SupportBeanReservedKeyword).FullName,
                       "Incorrect syntax near '7' at line 1 column 12 [insert into 7event select * from com.espertech.esper.support.bean.SupportBeanReservedKeyword]");
    
            TryCompile("select foo, create from " + typeof(SupportBeanReservedKeyword).FullName,
                       "Incorrect syntax near 'create' (a reserved keyword) at line 1 column 12, please check the select clause [select foo, create from com.espertech.esper.support.bean.SupportBeanReservedKeyword]");
    
            TryCompile("select * from pattern [",
                       "Unexpected end-of-input at line 1 column 23, please check the pattern expression within the from clause [select * from pattern []");
    
            TryCompile("select * from A, into",
                       "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 17, please check the from clause [select * from A, into]");
    
            TryCompile("select * from pattern[A -> B - C]",
                       "Incorrect syntax near '-' at line 1 column 29, please check the pattern expression within the from clause [select * from pattern[A -> B - C]]");
    
            TryCompile("insert into A (a",
                       "Unexpected end-of-input at line 1 column 16 [insert into A (a]");
    
            TryCompile("select case when 1>2 from A",
                       "Incorrect syntax near 'from' (a reserved keyword) expecting 'then' but found 'from' at line 1 column 21, please check the case expression within the select clause [select case when 1>2 from A]");
    
            TryCompile("select * from A full outer join B on A.field < B.field",
                       "Incorrect syntax near '<' expecting an equals '=' but found a lesser then '<' at line 1 column 45, please check the outer join within the from clause [select * from A full outer join B on A.field < B.field]");
    
            TryCompile("select a.b('aa\") from A",
                       "Unexpected end-of-input at line 1 column 23, please check the select clause [select a.b('aa\") from A]");
    
            TryCompile("select * from A, sql:mydb [\"",
                       "Unexpected end-of-input at line 1 column 28, please check the relational data join within the from clause [select * from A, sql:mydb [\"]");
    
            TryCompile("select * google",
                       "Incorrect syntax near 'google' at line 1 column 9");
    
            TryCompile("insert into into",
                       "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 12 [insert into into]");

            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SupportMessageAssertUtil.TryInvalid(
                _epService, "on SupportBean select 1",
                "Error starting statement: Required insert-into clause is not provided, the clause is required for split-stream syntax");
        }
    
        private void TryCompile(String expression, String expectedMsg)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, expectedMsg);
            }
        }
    }
}
