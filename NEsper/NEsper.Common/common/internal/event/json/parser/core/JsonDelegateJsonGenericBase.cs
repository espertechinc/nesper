///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public abstract class JsonDelegateJsonGenericBase : JsonDelegateBase
    {
        public JsonDelegateJsonGenericBase(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent) : base(baseHandler, parent)
        {
        }

        public override JsonDelegateBase StartObject(string name)
        {
            return new JsonDelegateJsonGenericObject(BaseHandler, this);
        }

        public override JsonDelegateBase StartArray(string name)
        {
            return new JsonDelegateJsonGenericArray(BaseHandler, this);
        }
    }
} // end of namespace