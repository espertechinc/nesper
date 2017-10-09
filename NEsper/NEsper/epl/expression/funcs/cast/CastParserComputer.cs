///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.funcs.cast
{
    /// <summary>Casting and parsing computer.</summary>
    public interface CasterParserComputer
    {
        /// <summary>
        /// Compute an result performing casting and parsing.
        /// </summary>
        /// <param name="input">to process</param>
        /// <param name="evaluateParams">The evaluate parameters.</param>
        /// <returns>
        /// cast or parse result
        /// </returns>
        Object Compute(Object input, EvaluateParams evaluateParams);

        bool IsConstantForConstInput { get; }
    }
}
