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
    public class JsonDeserializerEventObjectArray2Dim : JsonDeserializerArray2DimBase
    {
        private readonly Type componentType;
        private readonly JsonDelegateFactory factory;

        public JsonDeserializerEventObjectArray2Dim(
            JsonDeserializerBase parent,
            JsonDelegateFactory factory,
            Type componentType) : base(parent)
        {
            this.factory = factory;
            this.componentType = componentType;
        }

        public override object GetResult() => JsonDeserializerEventObjectArray.CollectionToTypedArray(collection, componentType);

        public override JsonDeserializerBase StartArrayInner()
        {
            return new JsonDeserializerEventObjectArray(this, factory, componentType.GetElementType());
        }
    }
} // end of namespace