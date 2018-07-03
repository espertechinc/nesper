///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events.arr
{
    using Map = IDictionary<string, object>;

    public class ObjectArrayEventBeanPropertyWriterMapProp 
        : ObjectArrayEventBeanPropertyWriter
    {
        private readonly String _key;
    
        public ObjectArrayEventBeanPropertyWriterMapProp(int propertyIndex, String key)
            : base(propertyIndex)
        {
            _key = key;
        }
    
        public override void Write(Object value, Object[] array)
        {
            var mapEntryRaw = array[Index];
            if (mapEntryRaw != null) {
                if (mapEntryRaw is Map mapEntry) {
                    mapEntry.Put(_key, value);
                } else if (mapEntryRaw.GetType().IsGenericStringDictionary()) {
                    mapEntryRaw.AsStringDictionary().Put(_key, value);
                }
            }
        }
    }
}
