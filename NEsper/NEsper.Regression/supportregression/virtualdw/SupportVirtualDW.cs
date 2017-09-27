///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.virtualdw
{
    public class SupportVirtualDW : VirtualDataWindow
    {
        private static ISet<object> initializationData;

        public static readonly String ITERATE = "iterate";

        public SupportVirtualDW(VirtualDataWindowContext context)
        {
            Context = context;
            Data = initializationData;
            Events = new List<VirtualDataWindowEvent>();
        }

        public static ISet<object> InitializationData
        {
            set { initializationData = value; }
        }

        public VirtualDataWindowContext Context { get; private set; }

        public bool IsDestroyed { get; private set; }

        public ICollection<object> Data { get; set; }

        public VirtualDataWindowLookupContext LastRequestedIndex { get; private set; }

        public VirtualDataWindowLookup GetLookup(VirtualDataWindowLookupContext desc)
        {
            LastRequestedIndex = desc;
            return new SupportVirtualDWIndex(this, Context);
        }

        public void Dispose()
        {
            IsDestroyed = true;
        }

        public object[] LastKeys
        {
            set { LastAccessKeys = value; }
            get { return LastAccessKeys; }
        }

        public object[] LastAccessKeys { get; private set; }

        public EventBean[] LastAccessEvents { get; set; }

        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            LastUpdateNew = newData;
            LastUpdateOld = oldData;
            Context.OutputStream.Update(newData, oldData);
        }

        public EventBean[] LastUpdateNew { get; private set; }

        public EventBean[] LastUpdateOld { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            if (Context.CustomConfiguration != null && Context.CustomConfiguration.Equals(ITERATE))
            {
                var events = Data.Select(item => Context.EventFactory.Wrap(item)).ToList();
                return events.GetEnumerator();
            }
            return Collections.GetEmptyList<EventBean>().GetEnumerator();
        }

        public void HandleEvent(VirtualDataWindowEvent theEvent)
        {
            Events.Add(theEvent);
        }

        public List<VirtualDataWindowEvent> Events { get; private set; }
    }
}
