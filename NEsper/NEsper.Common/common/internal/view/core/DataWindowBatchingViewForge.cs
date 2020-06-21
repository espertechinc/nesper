///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    /// Tag interface for data window view factories that express a batch expiry policy.
    /// <para />Such data windows allow iteration through the currently batched events,
    /// and such data windows post insert stream events only when batching conditions have been met and
    /// the batch is released.
    /// </summary>
    public interface DataWindowBatchingViewForge
    {
    }
} // end of namespace