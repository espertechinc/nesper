///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateUnknown : JsonDelegateBase
    {
        public JsonDelegateUnknown(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent) : base(baseHandler, parent)
        {
        }

        public override object GetResult() => null;

        public override JsonDelegateBase StartObject(string name)
        {
            return new JsonDelegateUnknown(BaseHandler, this);
        }

        public override JsonDelegateBase StartArray(string name)
        {
            return new JsonDelegateUnknown(BaseHandler, this);
        }

        public override bool EndObjectValue(string name)
        {
            return false;
        }
    }
} // end of namespace