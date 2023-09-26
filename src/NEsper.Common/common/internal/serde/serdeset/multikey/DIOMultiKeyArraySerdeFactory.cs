///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public class DIOMultiKeyArraySerdeFactory
    {
        private static readonly IDictionary<Type, DIOMultiKeyArraySerde> INDEX_BY_TYPE =
            new Dictionary<Type, DIOMultiKeyArraySerde>();

        private static IDictionary<string, DIOMultiKeyArraySerde> byPrettyName = null;

        static DIOMultiKeyArraySerdeFactory()
        {
            Add(DIOMultiKeyArrayCharSerde.INSTANCE);
            Add(DIOMultiKeyArrayBooleanSerde.INSTANCE);
            Add(DIOMultiKeyArrayByteSerde.INSTANCE);
            Add(DIOMultiKeyArrayShortSerde.INSTANCE);
            Add(DIOMultiKeyArrayIntSerde.INSTANCE);
            Add(DIOMultiKeyArrayLongSerde.INSTANCE);
            Add(DIOMultiKeyArrayFloatSerde.INSTANCE);
            Add(DIOMultiKeyArrayDoubleSerde.INSTANCE);
            Add(DIOMultiKeyArrayObjectSerde.INSTANCE);
        }

        private static void Add(DIOMultiKeyArraySerde serde)
        {
            INDEX_BY_TYPE.Put(serde.ComponentType, serde);
        }

        public static DIOMultiKeyArraySerde GetSerde(Type componentType)
        {
            return INDEX_BY_TYPE.Get(componentType);
        }

        public static DIOMultiKeyArraySerde GetSerde(string classNamePretty)
        {
            if (byPrettyName == null) {
                byPrettyName = new Dictionary<string,DIOMultiKeyArraySerde>();
                foreach (var serde in INDEX_BY_TYPE) {
                    byPrettyName.Put(serde.Key.Name, serde.Value);
                }
            }

            return byPrettyName.Get(classNamePretty);
        }
    }
} // end of namespace