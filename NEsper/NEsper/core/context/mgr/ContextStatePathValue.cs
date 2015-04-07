///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.context;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextStatePathValue
    {
        public ContextStatePathValue(int? optionalContextPartitionId, byte[] blob, ContextPartitionState state)
        {
            OptionalContextPartitionId = optionalContextPartitionId;
            Blob = blob;
            State = state;
        }

        public int? OptionalContextPartitionId { get; private set; }

        public byte[] Blob { get; private set; }

        public ContextPartitionState State { get; set; }

        public override String ToString()
        {
            return "ContextStatePathValue{" +
                   "optionalContextPartitionId=" + OptionalContextPartitionId +
                   ", state=" + State +
                   '}';
        }
    }
}