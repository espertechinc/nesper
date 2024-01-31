///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.exception
{
    /// <summary>
    /// Indicates the phase during which and exception was encountered.
    /// </summary>
    public enum ExceptionHandlerExceptionType
    {
        /// <summary>
        /// Exception occurred during event processing.
        /// </summary>
        PROCESS,

        /// <summary>
        /// Exception occurred upon undeploy.
        /// </summary>
        UNDEPLOY,

        /// <summary>
        /// Stage-related exception.
        /// </summary>
        STAGE
    }
} // end of namespace