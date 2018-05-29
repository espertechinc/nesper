///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Interface for an exception handler.
    /// <para />
    /// When the engine encounters an unchecked exception processing a continous-query
    /// statement it allows any exception handler that is registered with the engine to
    /// handle the exception, in the order any handlers are registered.
    /// <para />
    /// On-demand queries as well as any exceptions thrown by static method invocations
    /// or event method invocations or the API other then the sendEvent method are not
    /// provided to an exception handler.
    /// <para />
    /// An application may throw a runtime exception in the @handle method to cancel further
    /// processing of an event against
    /// statements.
    /// <para />
    /// Handle the exception as contained in the context object passed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ExceptionHandlerEventArgs"/> instance containing the event data.</param>
    public delegate void ExceptionHandler(object sender, ExceptionHandlerEventArgs args);
}
