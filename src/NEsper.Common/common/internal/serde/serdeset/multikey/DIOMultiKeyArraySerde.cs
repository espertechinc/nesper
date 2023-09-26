///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;

namespace com.espertech.esper.common.@internal.serde.serdeset.multikey
{
    public interface DIOMultiKeyArraySerde
    {
        Type ComponentType { get; }
    }
    
    public interface DIOMultiKeyArraySerde<T> : DIOMultiKeyArraySerde, DataInputOutputSerde<T>
    {
    }
} // end of namespace