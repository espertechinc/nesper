///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.json.parser.core;

namespace com.espertech.esper.common.@internal.@event.json.parser.delegates.array2dim
{
    public abstract class JsonDelegateArray2DimBase : JsonDelegateBase
    {
        protected ICollection<object> collection = new List<object>();

        public JsonDelegateArray2DimBase(
            JsonHandlerDelegator BaseHandler,
            JsonDelegateBase parent) : base(BaseHandler, parent)
        {
        }

        public abstract JsonDelegateBase StartArrayInner();

        public override JsonDelegateBase StartObject(string name)
        {
            return null;
        }

        public override JsonDelegateBase StartArray(string name)
        {
            return StartArrayInner();
        }

        public override bool EndObjectValue(string name)
        {
            return false;
        }

        public override void EndArrayValue(string name)
        {
            collection.Add(ObjectValue);
        }
    }
} // end of namespace