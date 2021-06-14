///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Holder for query meta data information obtained from interrogating statements.
    /// </summary>
    public class QueryMetaData
    {
        /// <summary>Ctor.</summary>
        /// <param name="inputParameters">is the input parameter names</param>
        /// <param name="outputParameters">is the output column names and types</param>
        public QueryMetaData(
            IList<string> inputParameters,
            IDictionary<string, DBOutputTypeDesc> outputParameters)
        {
            InputParameters = inputParameters;
            OutputParameters = outputParameters;
        }

        /// <summary>Return the input parameters.</summary>
        /// <returns>input parameter names</returns>
        public IList<string> InputParameters { get; }

        /// <summary>Returns a map of output column name and type descriptor.</summary>
        /// <returns>column names and types</returns>
        public IDictionary<string, DBOutputTypeDesc> OutputParameters { get; }
    }
} // End of namespace