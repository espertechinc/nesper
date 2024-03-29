///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    /// Interface for a root pattern node for removing matches.
    /// </summary>
    public interface EvalRootMatchRemover
    {
        void RemoveMatch(ISet<EventBean> matchEvent);
    }
} // end of namespace