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

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedDynamicSimple : EventPropertyGetter
    {
        private readonly Field _posTop;
        private readonly string _propertyName;

        public AvroEventBeanGetterNestedDynamicSimple(Field posTop, string propertyName)
        {
            _posTop = posTop;
            _propertyName = propertyName;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null)
            {
                return null;
            }
            return inner.Get(_propertyName);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null)
            {
                return false;
            }
            return inner.Schema.GetField(_propertyName) != null;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
} // end of namespace