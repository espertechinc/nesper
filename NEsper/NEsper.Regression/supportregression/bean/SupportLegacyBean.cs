///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.supportregression.bean
{
    /// <summary>
    /// Legacy class for testing style accessor methods.
    /// </summary>

    [Serializable]
    public class SupportLegacyBean
    {
        private readonly String _legacyBeanVal;
        private readonly String[] _stringArray;
        private readonly IDictionary<String, String> _mapped;
        private readonly LegacyNested _legacyNested;

        public String fieldLegacyVal;
        public String[] fieldStringArray;
        public IDictionary<String, String> fieldMapped;
        public LegacyNested fieldNested;

        public SupportLegacyBean(String legacyBeanVal)
            : this(legacyBeanVal, null, null, null)
        {
        }

        public SupportLegacyBean(String[] stringArray)
            : this(null, stringArray, null, null)
        {
        }

        public SupportLegacyBean(String legacyBeanVal, String[] stringArray, IDictionary<String, String> mapped, String legacyNested)
        {
            this._legacyBeanVal = legacyBeanVal;
            this._stringArray = stringArray;
            this._mapped = mapped;
            this._legacyNested = new LegacyNested(legacyNested);

            this.fieldLegacyVal = legacyBeanVal;
            this.fieldStringArray = stringArray;
            this.fieldMapped = mapped;
            this.fieldNested = this._legacyNested;
        }

        public virtual String ReadLegacyBeanVal()
        {
            return _legacyBeanVal;
        }

        public virtual String[] ReadStringArray()
        {
            return _stringArray;
        }

        public virtual String ReadStringIndexed(int i)
        {
            return _stringArray[i];
        }

        public virtual String ReadMapByKey(String key)
        {
            return _mapped[key];
        }

        public virtual IDictionary<string,string> ReadMap()
        {
            return _mapped;
        }

        public virtual LegacyNested ReadLegacyNested()
        {
            return _legacyNested;
        }

        [Serializable]
        public class LegacyNested
        {
            public String fieldNestedValue;

            public LegacyNested(String nestedValue)
            {
                this.fieldNestedValue = nestedValue;
            }

            public virtual String ReadNestedValue()
            {
                return fieldNestedValue;
            }
        }
    }
}
