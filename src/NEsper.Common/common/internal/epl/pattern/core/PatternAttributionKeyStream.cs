///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public class PatternAttributionKeyStream : PatternAttributionKey
    {
        private readonly int streamNum;

        public PatternAttributionKeyStream(int streamNum)
        {
            this.streamNum = streamNum;
        }

        public int StreamNum => streamNum;

        public T Accept<T>(
            PatternAttributionKeyVisitor<T> visitor,
            short factoryNodeId)
        {
            return visitor.Visit(this, factoryNodeId);
        }
    }
} // end of namespace