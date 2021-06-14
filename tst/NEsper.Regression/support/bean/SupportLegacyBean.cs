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

namespace com.espertech.esper.regressionlib.support.bean
{
    /// <summary>
    ///     Legacy Java class for testing non-JavaBean style accessor methods.
    /// </summary>
    [Serializable]
    public class SupportLegacyBean
    {
        private readonly string _legacyBeanVal;
        private readonly LegacyNested _legacyNested;
        private readonly IDictionary<string, string> _mapped;
        private readonly string[] _stringArray;

        public string fieldLegacyVal;
        public IDictionary<string, string> fieldMapped;
        public LegacyNested fieldNested;
        public string[] fieldStringArray;

        public SupportLegacyBean(string legacyBeanVal) : this(legacyBeanVal, null, null, null)
        {
        }

        public SupportLegacyBean(string[] stringArray) : this(null, stringArray, null, null)
        {
        }

        public SupportLegacyBean(
            string legacyBeanVal,
            string[] stringArray,
            IDictionary<string, string> mapped,
            string legacyNested)
        {
            _legacyBeanVal = legacyBeanVal;
            _stringArray = stringArray;
            _mapped = mapped;
            _legacyNested = new LegacyNested(legacyNested);

            fieldLegacyVal = legacyBeanVal;
            fieldStringArray = stringArray;
            fieldMapped = mapped;
            fieldNested = _legacyNested;
        }

        public string ReadLegacyBeanVal()
        {
            return _legacyBeanVal;
        }

        public string[] ReadStringArray()
        {
            return _stringArray;
        }

        public string ReadStringIndexed(int i)
        {
            return _stringArray[i];
        }

        public string ReadMapByKey(string key)
        {
            return _mapped.Get(key);
        }

        public IDictionary<string, string> ReadMap()
        {
            return _mapped;
        }

        public LegacyNested ReadLegacyNested()
        {
            return _legacyNested;
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