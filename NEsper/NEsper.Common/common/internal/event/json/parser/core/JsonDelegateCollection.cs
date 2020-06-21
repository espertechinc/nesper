///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateCollection : JsonDelegateBase
    {
        private readonly List<object> events = new List<object>();
        private readonly JsonDelegateFactory factory;

        public JsonDelegateCollection(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent,
            JsonDelegateFactory factory) : base(baseHandler, parent)
        {
            this.factory = factory;
        }

        public override object GetResult() => events;

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
    }
} // end of namespace