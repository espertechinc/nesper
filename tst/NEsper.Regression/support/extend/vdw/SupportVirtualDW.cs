///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDW : VirtualDataWindow
    {
        public const string ITERATE = "iterate";
        private static ISet<object> initializationData;

        public SupportVirtualDW(VirtualDataWindowContext context)
        {
            Context = context;
            Data = initializationData;
        }

        public VirtualDataWindowContext Context { get; }

        public bool IsDestroyed { get; private set; }

        public ISet<object> Data { get; set; }

        public VirtualDataWindowLookupContext LastRequestedLookup => RequestedLookups[0];

        public IList<VirtualDataWindowLookupContext> RequestedLookups { get; } =
            new List<VirtualDataWindowLookupContext>();

        public object[] LastKeys {
            set => LastAccessKeys = value;
        }

        public object[] LastAccessKeys { get; private set; }

        public EventBean[] LastAccessEvents { get; set; }

        public EventBean[] LastUpdateNew { get; private set; }

        public EventBean[] LastUpdateOld { get; private set; }

        public IList<VirtualDataWindowEvent> Events { get; } = new List<VirtualDataWindowEvent>();

        public static ISet<object> InitializationData {
            set => initializationData = value;
        }

        public VirtualDataWindowLookup GetLookup(VirtualDataWindowLookupContext desc)
        {
            RequestedLookups.Insert(0, desc);
            return new SupportVirtualDWIndex(this, Context);
        }

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            LastUpdateNew = newData;
            LastUpdateOld = oldData;
            Context.OutputStream.Update(newData, oldData);
        }

        public void HandleEvent(VirtualDataWindowEvent theEvent)
        {
            Events.Add(theEvent);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var factory = (SupportVirtualDWFactory) Context.Factory.Factory;
            var compileTimeConfiguration = factory.InitializeContext.CustomConfiguration;
            if (compileTimeConfiguration != null && compileTimeConfiguration.Equals(ITERATE)) {
                IList<EventBean> events = new List<EventBean>();
                foreach (var item in Data) {
                    events.Add(Context.EventFactory.Wrap(item));
                }

                return events.GetEnumerator();
            }

            return Collections.GetEmptyList<EventBean>().GetEnumerator();
        }

        public void Dispose()
        {
            IsDestroyed = true;
        }
    }
} // end of namespace