///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.parser.delegates.array2dim;

namespace com.espertech.esper.common.@internal.@event.json.parser.core
{
    public class JsonDelegateEventObjectArray2Dim : JsonDelegateArray2DimBase
    {
        private readonly Type componentType;
        private readonly JsonDelegateFactory factory;

        public JsonDelegateEventObjectArray2Dim(
            JsonHandlerDelegator baseHandler,
            JsonDelegateBase parent,
            JsonDelegateFactory factory,
            Type componentType) : base(baseHandler, parent)
        {
            this.factory = factory;
            this.componentType = componentType;
        }

        public override object GetResult() => JsonDelegateEventObjectArray.CollectionToTypedArray(collection, componentType);

        public override JsonDelegateBase StartArrayInner()
        {
            return new JsonDelegateEventObjectArray(BaseHandler, this, factory, componentType.GetElementType());
        }
    }
} // end of namespace