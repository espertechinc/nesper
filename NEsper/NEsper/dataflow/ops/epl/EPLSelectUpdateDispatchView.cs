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
using com.espertech.esper.core.service;
using com.espertech.esper.view;


namespace com.espertech.esper.dataflow.ops.epl
{
    public class EPLSelectUpdateDispatchView : ViewSupport, UpdateDispatchView
    {
        private readonly Select _select;
    
        public EPLSelectUpdateDispatchView(Select select) {
            _select = select;
        }
    
        public void NewResult(UniformPair<EventBean[]> result) {
            _select.OutputOutputRateLimited(result);
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData) {
        }

        public override EventType EventType
        {
            get { return null; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }
    }
}
