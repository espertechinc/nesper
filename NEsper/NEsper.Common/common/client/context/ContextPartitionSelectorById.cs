///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.context
{
    /// <summary>Selects a context partition by providing the context partition Id(s). </summary>
    public interface ContextPartitionSelectorById : ContextPartitionSelector
    {
        /// <summary>Return the context partition ids to select. </summary>
        /// <value>id set</value>
        ICollection<int> ContextPartitionIds { get; }
    }
}