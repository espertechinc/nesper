///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    /// Exception indicating a problem in a variable declaration.
    /// </summary>
    public class VariableDeclarationException : Exception
    {
        /// <summary>Ctor.</summary>
        /// <param name="msg">the exception message.</param>
        public VariableDeclarationException(String msg)
            : base(msg)
        {
        }
    }
} // End of namespace