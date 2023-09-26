///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.option
{
    /// <summary>
    ///     Implement this interface to provide values for substitution parameters.
    /// </summary>

    public delegate void  StatementSubstitutionParameterOption(StatementSubstitutionParameterContext value);

#if DEPRECATED_INTERFACE
    public interface StatementSubstitutionParameterOption
    {
        /// <summary>
        ///     Set statement substitution parameters.
        /// </summary>
        /// <param name="value">provides the setObject method and provides information about the statement</param>
        void SetStatementParameters(StatementSubstitutionParameterContext value);
    }
#endif
} // end of namespace