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
    /// <summary>Selects context partitions of a nested context by providing selector according to the nested contexts. </summary>
    public interface ContextPartitionSelectorNested : ContextPartitionSelector
    {
        /// <summary>Selectors for each level of the nested context. </summary>
        /// <value>selectors</value>
        IList<ContextPartitionSelector[]> Selectors { get; }
    }
}