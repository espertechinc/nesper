///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterMappedRuntimeKeyed : EventPropertyGetterMapped
    {
        private readonly Field _pos;

        public AvroEventBeanGetterMappedRuntimeKeyed(Field pos)
        {
            _pos = pos;
        }

        public Object Get(EventBean eventBean, string mapKey)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_pos);
            return AvroEventBeanGetterMapped.GetMappedValue(values, mapKey);
        }
    }
} // end of namespace