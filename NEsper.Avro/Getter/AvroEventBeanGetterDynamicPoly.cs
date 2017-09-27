///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.client;

using NEsper.Avro.Core;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterDynamicPoly : AvroEventPropertyGetter
    {
        private readonly AvroEventPropertyGetter[] _getters;

        public AvroEventBeanGetterDynamicPoly(AvroEventPropertyGetter[] getters)
        {
            this._getters = getters;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            return GetAvroFieldValuePoly(record, _getters);
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFieldValue(record);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return GetAvroFieldValuePolyExists(record, _getters);
        }

        internal static bool GetAvroFieldValuePolyExists(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return false;
            }
            record = NavigatePoly(record, getters);
            return record != null && getters[getters.Length - 1].IsExistsPropertyAvro(record);
        }

        internal static Object GetAvroFieldValuePoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return null;
            }
            record = NavigatePoly(record, getters);
            if (record == null)
            {
                return null;
            }
            return getters[getters.Length - 1].GetAvroFieldValue(record);
        }

        internal static Object GetAvroFieldFragmentPoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            if (record == null)
            {
                return null;
            }
            record = NavigatePoly(record, getters);
            if (record == null)
            {
                return null;
            }
            return getters[getters.Length - 1].GetAvroFragment(record);
        }

        private static GenericRecord NavigatePoly(GenericRecord record, AvroEventPropertyGetter[] getters)
        {
            for (var i = 0; i < getters.Length - 1; i++)
            {
                var value = getters[i].GetAvroFieldValue(record);
                if (!(value is GenericRecord))
                {
                    return null;
                }
                record = (GenericRecord) value;
            }
            return record;
        }
    }
} // end of namespace
