///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
    /// <summary>Service provider interface for filter service. </summary>
    public interface FilterServiceSPI : FilterService
    {
        bool IsSupportsTakeApply { get; }

        /// <summary>Take a set of statements of out the active filters, returning a save-set of filters. </summary>
        /// <param name="statementId">statement ids to remove</param>
        /// <returns>filters</returns>
        FilterSet Take(ICollection<int> statementId);
    
        /// <summary>Apply a set of previously taken filters. </summary>
        /// <param name="filterSet">to apply</param>
        void Apply(FilterSet filterSet);
    
        /// <summary>Add activity listener. </summary>
        /// <param name="filterServiceListener">to add</param>
        void AddFilterServiceListener(FilterServiceListener filterServiceListener);
    
        /// <summary>Remove activity listener. </summary>
        /// <param name="filterServiceListener">to remove</param>
        void RemoveFilterServiceListener(FilterServiceListener filterServiceListener);

        int FilterCountApprox { get; }

        int CountTypes { get; }

        ILockable WriteLock { get; }

        /// <summary>
        /// Initialization is optional and provides a chance to preload things after statements are available.
        /// </summary>
        void Init();
    }
}
