///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.compat.collections;

namespace NEsper.Avro.Core
{
    public class AvroEventTypeFragmentTypeCache
    {
        private IDictionary<string, AvroSchemaEventType> _cacheByRecordSchemaName;

        public AvroSchemaEventType Get(string recordSchemaName)
        {
            if (_cacheByRecordSchemaName == null)
            {
                _cacheByRecordSchemaName = new Dictionary<string, AvroSchemaEventType>();
            }
            return _cacheByRecordSchemaName.Get(recordSchemaName);
        }

        public void Add(string recordSchemaName, AvroSchemaEventType fragmentType)
        {
            if (_cacheByRecordSchemaName == null)
            {
                _cacheByRecordSchemaName = new Dictionary<string, AvroSchemaEventType>();
            }
            _cacheByRecordSchemaName.Put(recordSchemaName, fragmentType);
        }
    }
} // end of namespace