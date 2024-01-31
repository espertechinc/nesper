///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Marker interface for use with view factories that create data window views only.
    ///     <para />
    ///     Please <seealso cref="DataWindowView" /> for details on views that meet data window requirements.
    /// </summary>
    public interface DataWindowViewFactory : ViewFactory
    {
    }
} // end of namespace