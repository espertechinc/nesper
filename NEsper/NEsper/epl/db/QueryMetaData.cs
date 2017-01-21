///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Holder for query meta data information obtained from interrogating statements.
    /// </summary>
    public class QueryMetaData
    {
        private readonly IList<String> inputParameters;
        private readonly IDictionary<String, DBOutputTypeDesc> outputParameters;

        /// <summary>Ctor.</summary>
        /// <param name="inputParameters">is the input parameter names</param>
        /// <param name="outputParameters">is the output column names and types</param>
        public QueryMetaData(IList<String> inputParameters, IDictionary<String, DBOutputTypeDesc> outputParameters)
        {
            this.inputParameters = inputParameters;
            this.outputParameters = outputParameters;
        }

        /// <summary>Return the input parameters.</summary>
        /// <returns>input parameter names</returns>
        public IList<String> InputParameters
        {
            get { return inputParameters; }
        }

        /// <summary>Returns a map of output column name and type descriptor.</summary>
        /// <returns>column names and types</returns>
        public IDictionary<String, DBOutputTypeDesc> OutputParameters
        {
            get { return outputParameters; }
        }
    }
} // End of namespace
