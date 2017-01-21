///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.scopetest;

using NUnit.Framework;


namespace com.espertech.esper.epl.view
{
    [TestFixture]
    public class TestOutputConditionNull 
    {
    	private OutputConditionNull _condition;	
        private SupportUpdateListener _listener;
    	private OutputCallback _callback;
    
        [SetUp]
        public void SetUp()
        { 
        	_listener = new SupportUpdateListener();

            _callback = new OutputCallback(
                (doOutput, forceUpdate) => _listener.Update(null, null));

            _condition = new OutputConditionNull(_callback);    	
        }
        
        [Test]
        public void TestUpdateCondition()
        {
        	// the callback should be made regardles of the Update
        	_condition.UpdateOutputCondition(1,1);
        	Assert.IsTrue(_listener.GetAndClearIsInvoked());
        	_condition.UpdateOutputCondition(1,0);
        	Assert.IsTrue(_listener.GetAndClearIsInvoked());
        	_condition.UpdateOutputCondition(0,1);
        	Assert.IsTrue(_listener.GetAndClearIsInvoked());
        	_condition.UpdateOutputCondition(0,0);
        	Assert.IsTrue(_listener.GetAndClearIsInvoked());
        }
    }
}
