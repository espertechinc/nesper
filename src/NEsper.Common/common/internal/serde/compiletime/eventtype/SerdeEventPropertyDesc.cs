///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
    public class SerdeEventPropertyDesc
    {
        public SerdeEventPropertyDesc(
            DataInputOutputSerdeForge forge,
            ISet<EventType> nestedTypes)
        {
            Forge = forge;
            NestedTypes = nestedTypes;
        }

        public DataInputOutputSerdeForge Forge { get; }

        public ISet<EventType> NestedTypes { get; }
    }
} // end of namespace