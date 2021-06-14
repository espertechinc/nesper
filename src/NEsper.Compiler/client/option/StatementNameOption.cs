///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Implement this interface to provide a custom user object at compile-time for the statements when they are compiled.
    /// </summary>

    public delegate string StatementNameOption(StatementNameContext env);

#if DEPRECATED_INTERFACE
    public interface StatementNameOption
    {
        /// <summary>
        ///     Returns the statement name to assign to a statement at compilation-time.
        ///     <para />
        ///     Implementations would typically interrogate the context object EPL expression
        ///     or module and module item information and determine the right statement name to assign.
        /// </summary>
        /// <param name="env">the statement's compile context</param>
        /// <returns>statement name or null if none needs to be assigned</returns>
        string GetValue(StatementNameContext env);
    }
#endif
} // end of namespace