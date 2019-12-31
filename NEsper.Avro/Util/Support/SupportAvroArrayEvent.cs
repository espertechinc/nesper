///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

namespace NEsper.Avro.Util.Support
{
    public class SupportAvroArrayEvent
    {
        public SupportAvroArrayEvent(GenericRecord[] someAvroArray)
        {
            SomeAvroArray = someAvroArray;
        }

        public GenericRecord[] SomeAvroArray { get; }
    }
}