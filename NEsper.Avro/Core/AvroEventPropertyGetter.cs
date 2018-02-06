///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.events;

namespace NEsper.Avro.Core
{
    public interface AvroEventPropertyGetter : EventPropertyGetterSPI
    {
        Object GetAvroFieldValue(GenericRecord record);
    
        Object GetAvroFragment(GenericRecord record);
    
        bool IsExistsPropertyAvro(GenericRecord record);
    }
} // end of namespace
