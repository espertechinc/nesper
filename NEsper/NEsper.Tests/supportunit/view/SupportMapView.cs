///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.supportunit.events;

namespace com.espertech.esper.supportunit.view
{
    public class SupportMapView : SupportBaseView
    {
        public static readonly IList<SupportMapView> Instances = new List<SupportMapView>();
    
        public SupportMapView()
        {
            Instances.Add(this);
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            IsInvoked = true;
            LastNewData = newData;
            LastOldData = oldData;
    
            UpdateChildren(newData, oldData);
        }
    
        public SupportMapView(IDictionary<String, Object> eventTypeMap)
            : base(SupportEventTypeFactory.CreateMapType(eventTypeMap))
        {
            Instances.Add(this);
        }
    
        public static IList<SupportMapView> GetInstances()
        {
            return Instances;
        }
    }
}
