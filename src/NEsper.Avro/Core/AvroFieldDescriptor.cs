///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        public AvroFieldDescriptor(
            Field field,
            bool dynamic,
            bool accessedByIndex,
            bool accessedByKey)
        {
            Field = field;
            IsDynamic = dynamic;
            IsAccessedByIndex = accessedByIndex;
            IsAccessedByKey = accessedByKey;
        }

        public bool IsDynamic { get; }

        public bool IsAccessedByIndex { get; }

        public bool IsAccessedByKey { get; }

        public Field Field { get; }
    }
} // end of namespace