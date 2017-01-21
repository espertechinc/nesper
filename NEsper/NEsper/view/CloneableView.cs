///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.view
{
    /// <summary>
    /// Views that can work under a group-by must be able to duplicate and are required 
    /// to implement this interface.
    /// </summary>
    public interface CloneableView
    {
        /// <summary>
        /// Duplicates the view.
        /// <para /> Expected to return a same view in initialized state for grouping. 
        /// </summary>
        View CloneView();
    }
}
