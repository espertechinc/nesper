///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventTypeErasure
    {
        public SupportEventTypeErasure(
            string key,
            int subkey,
            IDictionary<string, SupportEventInnerTypeWGetIds> innerTypes,
            SupportEventInnerTypeWGetIds[] innerTypesArray)
        {
            Key = key;
            Subkey = subkey;
            InnerTypes = innerTypes;
            InnerTypesArray = innerTypesArray;
        }

        public IDictionary<string, SupportEventInnerTypeWGetIds> InnerTypes { get; }

        public string Key { get; }

        public int Subkey { get; }

        public SupportEventInnerTypeWGetIds[] InnerTypesArray { get; }
    }
} // end of namespace