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
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.view
{
    public class SupportBeanClassView : SupportBaseView, CloneableView
    {
        private readonly Type _clazz;
    
        public SupportBeanClassView()
        {
            Instances.Add(this);
        }

        public SupportBeanClassView(Type clazz)
            : base(SupportEventTypeFactory.CreateBeanType(clazz))
        {
            this._clazz = clazz;
            Instances.Add(this);
        }

        static SupportBeanClassView()
        {
            Instances = new List<SupportBeanClassView>();
        }

        public View CloneView()
        {
            return new SupportBeanClassView(_clazz);
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            IsInvoked = true;
            LastNewData = newData;
            LastOldData = oldData;
    
            UpdateChildren(newData, oldData);
        }

        public static List<SupportBeanClassView> Instances { get; private set; }
    }
}
