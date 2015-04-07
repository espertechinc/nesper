///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestPollExecStrategyDBQuery 
    {
        private PollExecStrategyDBQuery _dbPollExecStrategy;
    
        [SetUp]
        public void SetUp()
        {
            const string sql = "select myvarchar from mytesttable where mynumeric = ? order by mybigint asc";
    
            var databaseConnectionFactory = SupportDatabaseService.MakeService().GetConnectionFactory("mydb");
            var connectionCache = new ConnectionNoCacheImpl(databaseConnectionFactory, sql, null);
    
            var resultProperties = new Dictionary<String, Object>();
            resultProperties["myvarchar"] = typeof(string);
            var resultEventType = SupportEventAdapterService.Service.CreateAnonymousMapType("test", resultProperties);
    
            IDictionary<String, DBOutputTypeDesc> propertiesOut = new Dictionary<String, DBOutputTypeDesc>();
            propertiesOut["myvarchar"] = new DBOutputTypeDesc("TIME", typeof(string), null);
    
            _dbPollExecStrategy = new PollExecStrategyDBQuery(
                SupportEventAdapterService.Service,
                resultEventType,
                connectionCache,
                sql, 
                propertiesOut,
                null, 
                null);
        }
    
        [Test]
        public void TestPoll()
        {
            _dbPollExecStrategy.Start();
    
            var resultRows = new IList<EventBean>[3];
            resultRows[0] = _dbPollExecStrategy.Poll(new Object[] { -1 }, null);
            resultRows[1] = _dbPollExecStrategy.Poll(new Object[] { 500 }, null);
            resultRows[2] = _dbPollExecStrategy.Poll(new Object[] { 200 }, null);
    
            // should have joined to two rows
            Assert.AreEqual(0, resultRows[0].Count);
            Assert.AreEqual(2, resultRows[1].Count);
            Assert.AreEqual(1, resultRows[2].Count);
    
            var theEvent = resultRows[1][0];
            Assert.AreEqual("D", theEvent.Get("myvarchar"));
    
            theEvent = resultRows[1][1];
            Assert.AreEqual("E", theEvent.Get("myvarchar"));
    
            theEvent = resultRows[2][0];
            Assert.AreEqual("F", theEvent.Get("myvarchar"));
    
            _dbPollExecStrategy.Done();
            _dbPollExecStrategy.Dispose();
        }
    }
}
