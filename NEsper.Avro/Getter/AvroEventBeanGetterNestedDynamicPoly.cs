///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicPoly : EventPropertyGetter
    {
        private readonly string _fieldTop;
        private readonly AvroEventPropertyGetter _getter;

        public AvroEventBeanGetterNestedDynamicPoly(string fieldTop, AvroEventPropertyGetter getter)
        {
            _fieldTop = fieldTop;
            _getter = getter;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_fieldTop);
            return inner == null ? null : _getter.GetAvroFieldValue(inner);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var field = record.Schema.GetField(_fieldTop);
            if (field == null)
            {
                return false;
            }
            Object inner = record.Get(_fieldTop);
            if (!(inner is GenericRecord))
            {
                return false;
            }
            return _getter.IsExistsPropertyAvro((GenericRecord) inner);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
} // end of namespace