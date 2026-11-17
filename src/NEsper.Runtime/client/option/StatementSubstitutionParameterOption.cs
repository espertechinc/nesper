///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

} // end of namespace