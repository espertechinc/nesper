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
    /// Extends the <seealso cref="ExceptionHandler" /> with a method to, for the inbound-pool threading configuration,
    /// handle exceptions that are not associated to a specific statement i.e. sharable-filter processing exceptions.
    /// <para>
    /// For use with inbound-thread-pool only, when the engine evaluates events as shared filters
    /// and not associated to any statements, the engine passes the exception to this method.
    /// </para>
    /// </summary>
    /// <param name="context">the exception information</param>
    public delegate void ExceptionHandlerInboundPool(ExceptionHandlerContextUnassociated context);
} // end of namespace
