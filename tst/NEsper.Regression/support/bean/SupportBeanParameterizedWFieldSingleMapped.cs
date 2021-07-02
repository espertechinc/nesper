///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanParameterizedWFieldSingleMapped<T>
    {
        public readonly IDictionary<string, T> mapField;

        public SupportBeanParameterizedWFieldSingleMapped(T value)
        {
            MapProperty = new Dictionary<string, T>();
            MapProperty.Put("key", value);
            mapField = new Dictionary<string, T>();
            mapField.Put("key", value);
        }

        public IDictionary<string, T> MapProperty { get; }

        public T MapKeyed(string key)
        {
            return MapProperty.Get(key);
        }
    }
} // end of namespace