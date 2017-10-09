///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.@base
{
    [TestFixture]
    public class TestJoinExecutionStrategyImpl 
    {
        private JoinExecutionStrategyImpl _join;
        private LinkedHashSet<MultiKey<EventBean>> _oldEvents;
        private LinkedHashSet<MultiKey<EventBean>> _newEvents;
        private SupportJoinSetProcessor _filter;
        private SupportJoinSetProcessor _indicator;
    
        [SetUp]
        public void SetUp()
        {
            _oldEvents = new LinkedHashSet<MultiKey<EventBean>>();
            _newEvents = new LinkedHashSet<MultiKey<EventBean>>();

            JoinSetComposer composer = new SupportJoinSetComposer(new UniformPair<ISet<MultiKey<EventBean>>>(_newEvents, _oldEvents));
            _filter = new SupportJoinSetProcessor();
            _indicator = new SupportJoinSetProcessor();
    
            _join = new JoinExecutionStrategyImpl(composer, _filter, _indicator, null);
        }
    
        [Test]
        public void TestJoin()
        {
            _join.Join(null, null);
    
            Assert.AreSame(_newEvents, _filter.LastNewEvents);
            Assert.AreSame(_oldEvents, _filter.LastOldEvents);
            Assert.IsNull(_indicator.LastNewEvents);
            Assert.IsNull(_indicator.LastOldEvents);
        }
    }
}
