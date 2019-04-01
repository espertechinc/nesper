///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.client
{
    public class SupportStatementStateListener
    {
        private readonly List<EPStatement> _createdEvents = new List<EPStatement>();
        private readonly List<EPStatement> _stateChangeEvents = new List<EPStatement>();
    
        public void OnStatementCreate(object sender, StatementStateEventArgs statementStateEventArgs)
        {
            _createdEvents.Add(statementStateEventArgs.Statement);
        }
    
        public void OnStatementStateChange(object sender, StatementStateEventArgs statementStateEventArgs)
        {
            _stateChangeEvents.Add(statementStateEventArgs.Statement);
        }
    
        public EPStatement AssertOneGetAndResetCreatedEvents()
        {
            Assert.AreEqual(1, _createdEvents.Count);
            EPStatement item = _createdEvents[0];
            _createdEvents.Clear();
            return item;
        }
    
        public EPStatement AssertOneGetAndResetStateChangeEvents()
        {
            Assert.AreEqual(1, _stateChangeEvents.Count);
            Assert.AreEqual(0, _createdEvents.Count);
            EPStatement item = _stateChangeEvents[0];
            _stateChangeEvents.Clear();
            return item;
        }
    
        public List<EPStatement> GetCreatedEvents()
        {
            return _createdEvents;
        }
    
        public List<EPStatement> GetStateChangeEvents()
        {
            return _stateChangeEvents;
        }
    }
}
