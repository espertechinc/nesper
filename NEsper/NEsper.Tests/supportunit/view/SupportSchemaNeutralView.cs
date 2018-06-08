///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.view;


namespace com.espertech.esper.supportunit.view
{
    public class SupportSchemaNeutralView : SupportBaseView, UpdateDispatchView
    {
        public SupportSchemaNeutralView()
        {
        }
    
        public SupportSchemaNeutralView(String viewName)
        {
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            LastNewData = newData;
            LastOldData = oldData;
            UpdateChildren(newData, oldData);
        }

        public override Viewable Parent
        {
            set
            {
                base.Parent = value;
                SetEventType(value != null ? value.EventType : null);
            }
        }

        public void NewResult(UniformPair<EventBean[]> result)
        {
        }
    }
}
