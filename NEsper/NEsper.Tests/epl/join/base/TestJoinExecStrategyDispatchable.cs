///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.events;
using com.espertech.esper.view.internals;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.@base
{
    [TestFixture]
    public class TestJoinExecStrategyDispatchable 
    {
        private JoinExecStrategyDispatchable _dispatchable;
        private BufferView _bufferViewOne;
        private BufferView _bufferViewTwo;
        private SupportJoinExecutionStrategy _joinExecutionStrategy;
    
        [SetUp]
        public void SetUp()
        {
            _bufferViewOne = new BufferView(0);
            _bufferViewTwo = new BufferView(1);
    
            _joinExecutionStrategy = new SupportJoinExecutionStrategy();
    
            _dispatchable = new JoinExecStrategyDispatchable(
                _joinExecutionStrategy, 2);
    
            _bufferViewOne.Observer = _dispatchable;
            _bufferViewTwo.Observer = _dispatchable;
        }
    
        [Test]
        public void TestFlow()
        {
            EventBean[] oldDataOne = SupportEventBeanFactory.MakeEvents(new String[] {"a"});
            EventBean[] newDataOne = SupportEventBeanFactory.MakeEvents(new String[] {"b"});
            EventBean[] oldDataTwo = SupportEventBeanFactory.MakeEvents(new String[] {"c"});
            EventBean[] newDataTwo = SupportEventBeanFactory.MakeEvents(new String[] {"d"});
    
            _bufferViewOne.Update(newDataOne, oldDataOne);
            _dispatchable.Execute();
            Assert.AreEqual(1, _joinExecutionStrategy.GetLastNewDataPerStream()[0].Length);
            Assert.AreSame(newDataOne[0], _joinExecutionStrategy.GetLastNewDataPerStream()[0][0]);
            Assert.AreSame(oldDataOne[0], _joinExecutionStrategy.GetLastOldDataPerStream()[0][0]);
            Assert.IsNull(_joinExecutionStrategy.GetLastNewDataPerStream()[1]);
            Assert.IsNull(_joinExecutionStrategy.GetLastOldDataPerStream()[1]);
    
            _bufferViewOne.Update(newDataTwo, oldDataTwo);
            _bufferViewTwo.Update(newDataOne, oldDataOne);
            _dispatchable.Execute();
            Assert.AreEqual(1, _joinExecutionStrategy.GetLastNewDataPerStream()[0].Length);
            Assert.AreEqual(1, _joinExecutionStrategy.GetLastNewDataPerStream()[1].Length);
            Assert.AreSame(newDataTwo[0], _joinExecutionStrategy.GetLastNewDataPerStream()[0][0]);
            Assert.AreSame(oldDataTwo[0], _joinExecutionStrategy.GetLastOldDataPerStream()[0][0]);
            Assert.AreSame(newDataOne[0], _joinExecutionStrategy.GetLastNewDataPerStream()[1][0]);
            Assert.AreSame(oldDataOne[0], _joinExecutionStrategy.GetLastOldDataPerStream()[1][0]);
        }
    }
}
