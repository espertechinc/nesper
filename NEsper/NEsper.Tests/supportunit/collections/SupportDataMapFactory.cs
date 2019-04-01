///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportunit.collections
{
    public class SupportDataMapFactory
    {
        /// <summary>
        /// Creates a dictionary with a given key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SupportDataMap Create(string key, object value)
        {
            var supportDataMap = new SupportDataMap();
            supportDataMap[key] = value;
            return supportDataMap;
        }
    }
}
