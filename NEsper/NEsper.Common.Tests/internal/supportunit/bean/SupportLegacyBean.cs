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

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    /// <summary>
    ///     Legacy Java class for testing non-JavaBean style accessor methods.
    /// </summary>
    [Serializable]
    public class SupportLegacyBean
    {
        public string fieldLegacyVal;
        public IDictionary<string, string> fieldMapped;
        public LegacyNested fieldNested;
        public string[] fieldStringArray;
        private readonly string legacyBeanVal;
        private readonly LegacyNested legacyNested;
        private readonly IDictionary<string, string> mapped;
        private readonly string[] stringArray;

        public SupportLegacyBean(string legacyBeanVal)
            : this(legacyBeanVal, null, null, null)
        {
        }

        public SupportLegacyBean(string[] stringArray)
            : this(null, stringArray, null, null)
        {
        }

        public SupportLegacyBean(
            string legacyBeanVal,
            string[] stringArray,
            IDictionary<string, string> mapped,
            string legacyNested)
        {
            this.legacyBeanVal = legacyBeanVal;
            this.stringArray = stringArray;
            this.mapped = mapped;
            this.legacyNested = new LegacyNested(legacyNested);

            fieldLegacyVal = legacyBeanVal;
            fieldStringArray = stringArray;
            fieldMapped = mapped;
            fieldNested = this.legacyNested;
        }

        public string ReadLegacyBeanVal()
        {
            return legacyBeanVal;
        }

        public string[] ReadStringArray()
        {
            return stringArray;
        }

        public string ReadStringIndexed(int i)
        {
            return stringArray[i];
        }

        public string ReadMapByKey(string key)
        {
            return mapped.Get(key);
        }

        public IDictionary<string, string> ReadMap()
        {
            return mapped;
        }

        public LegacyNested ReadLegacyNested()
        {
            return legacyNested;
        }

        [Serializable]
        public class LegacyNested
        {
            public string fieldNestedValue;

            public LegacyNested(string nestedValue)
            {
                fieldNestedValue = nestedValue;
            }

            public string ReadNestedValue()
            {
                return fieldNestedValue;
            }
        }
    }
} // end of namespace