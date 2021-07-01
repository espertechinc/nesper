///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.common.@internal.filterspec
{
    public sealed partial class FilterSpecParamInForge
    {
        public class InValueAdderMap : FilterSpecParamInAdder
        {
            public static readonly InValueAdderMap INSTANCE = new InValueAdderMap();

            private InValueAdderMap()
            {
            }

            public void Add(
                ICollection<object> constants,
                object value)
            {
                var map = value.AsObjectDictionary(MagicMarker.SingletonInstance);
                constants.AddAll(map.Keys);
            }
            
            public void ValueToString(StringBuilder @out)
            {
                @out.Append("map keys");
            }
        }
    }
}