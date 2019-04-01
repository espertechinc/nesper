///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.context
{
    /// <summary>Selects context partitions by receiving a context partition identifier for interrogation. </summary>
    public interface ContextPartitionSelectorFiltered : ContextPartitionSelector
    {
        /// <summary>
        /// Filter function should return true or false to indicate interest in this context partition.
        /// <para />
        /// Do not hold on to ContextIdentifier instance between calls. The engine may reused an reassing values to this object.
        /// </summary>
        /// <param name="contextPartitionIdentifier">provides context partition information, may</param>
        /// <returns>true to pass filter, false to reject</returns>
        Boolean Filter(ContextPartitionIdentifier contextPartitionIdentifier);
    }
}