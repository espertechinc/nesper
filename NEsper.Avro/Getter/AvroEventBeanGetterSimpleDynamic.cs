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
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterSimpleDynamic : AvroEventPropertyGetter
    {
        private readonly string _propertyName;

        public AvroEventBeanGetterSimpleDynamic(string propertyName)
        {
            _propertyName = propertyName;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            return record.Get(_propertyName);
        }

        public Object Get(EventBean theEvent)
        {
            return GetAvroFieldValue((GenericRecord) theEvent.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsPropertyAvro((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return record.Schema.GetField(_propertyName) != null;
        }

        public Object GetFragment(EventBean obj)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }
    }
} // end of namespace