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
    public class AvroEventBeanGetterNestedPoly : EventPropertyGetter
    {
        private readonly AvroEventPropertyGetter[] _getters;
        private readonly Field _top;

        public AvroEventBeanGetterNestedPoly(Field top, AvroEventPropertyGetter[] getters)
        {
            _top = top;
            _getters = getters;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePoly(inner, _getters);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldValuePolyExists(inner, _getters);
        }

        public Object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            return AvroEventBeanGetterDynamicPoly.GetAvroFieldFragmentPoly(inner, _getters);
        }
    }
} // end of namespace