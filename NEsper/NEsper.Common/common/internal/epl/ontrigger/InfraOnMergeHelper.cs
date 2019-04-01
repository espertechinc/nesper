///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     Factory for handles for updates/inserts/deletes/select
    /// </summary>
    public class InfraOnMergeHelper
    {
        public InfraOnMergeHelper(
            InfraOnMergeActionIns insertUnmatched, IList<InfraOnMergeMatch> matched, IList<InfraOnMergeMatch> unmatched,
            bool requiresTableWriteLock)
        {
            InsertUnmatched = insertUnmatched;
            Matched = matched;
            Unmatched = unmatched;
            IsRequiresTableWriteLock = requiresTableWriteLock;
        }

        public InfraOnMergeActionIns InsertUnmatched { get; }

        public IList<InfraOnMergeMatch> Matched { get; }

        public IList<InfraOnMergeMatch> Unmatched { get; }

        public bool IsRequiresTableWriteLock { get; }
    }
} // end of namespace