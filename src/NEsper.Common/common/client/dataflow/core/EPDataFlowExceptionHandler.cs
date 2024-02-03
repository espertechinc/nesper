///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Handler for exceptions thrown by data flow operators.
    /// </summary>
    public interface EPDataFlowExceptionHandler
    {
        /// <summary>
        /// Handle exception.
        /// </summary>
        /// <param name="context">provides all exception information</param>
        void Handle(EPDataFlowExceptionContext context);
    }
}