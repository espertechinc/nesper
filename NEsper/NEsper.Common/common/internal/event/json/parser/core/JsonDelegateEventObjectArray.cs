///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateEventObjectArray : JsonDelegateBase
    {
        private readonly Type componentType;
        private readonly List<object> events = new List<object>();
        private readonly JsonDelegateFactory factory;

        public JsonDelegateEventObjectArray(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent,
            JsonDelegateFactory factory,
            Type componentType) : base(baseHandler, parent)
        {
            this.factory = factory;
            this.componentType = componentType;
        }

        public override object GetResult() => CollectionToTypedArray(events, componentType);

        public override JsonDelegateBase StartObject(string name)
        {
            return factory.Make(BaseHandler, this);
        }

        public override JsonDelegateBase StartArray(string name)
        {
            return null;
        }

        public override bool EndObjectValue(string name)
        {
            return false;
        }

        public override void EndArrayValue(string name)
        {
            events.Add(ObjectValue);
        }

        public static object CollectionToTypedArray(
            ICollection<object> events,
            Type componentType)
        {
            var array = Array.CreateInstance(componentType, events.Count);
            for (var ii = 0; ii < events.Count; ii++) {
                array.SetValue(events[ii], ii);
            }

            return array;
        }
    }
} // end of namespace