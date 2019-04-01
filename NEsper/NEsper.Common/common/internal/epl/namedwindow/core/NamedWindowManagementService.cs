///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     Service to manage named windows on an runtime level.
    /// </summary>
    public interface NamedWindowManagementService
    {
        void AddNamedWindow(string namedWindowName, NamedWindowMetaData desc, EPStatementInitServices services);

        NamedWindow GetNamedWindow(string deploymentId, string namedWindowName);

        int DeploymentCount { get; }

        void DestroyNamedWindow(string deploymentId, string namedWindowName);
    }

    public class NamedWindowManagementServiceConstants
    {
        /// <summary>
        ///     Error message for data windows required.
        /// </summary>
        public const string ERROR_MSG_DATAWINDOWS =
            "Named windows require one or more child views that are data window views";

        /// <summary>
        ///     Error message for no data window allowed.
        /// </summary>
        public const string ERROR_MSG_NO_DATAWINDOW_ALLOWED =
            "Consuming statements to a named window cannot declare a data window view onto the named window";
    }
} // end of namespace