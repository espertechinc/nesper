///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;

namespace NEsper.Avro.Core
{
    public class AvroFieldDescriptor
    {
        private readonly Field _field;
        private readonly bool _dynamic;
        private readonly bool _accessedByIndex;
        private readonly bool _accessedByKey;
    
        public AvroFieldDescriptor(Field field, bool dynamic, bool accessedByIndex, bool accessedByKey)
        {
            _field = field;
            _dynamic = dynamic;
            _accessedByIndex = accessedByIndex;
            _accessedByKey = accessedByKey;
        }

        public Field Field
        {
            get { return _field; }
        }

        public bool IsDynamic
        {
            get { return _dynamic; }
        }

        public bool IsAccessedByIndex
        {
            get { return _accessedByIndex; }
        }

        public bool IsAccessedByKey
        {
            get { return _accessedByKey; }
        }
    }
} // end of namespace
