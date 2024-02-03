///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For use with <seealso cref="SQLColumnTypeConversion" />, context of parameter conversion.
    /// </summary>
    public class SQLInputParameterContext
    {
        /// <summary>Ctor. </summary>
        public SQLInputParameterContext()
        {
        }

        /// <summary>Returns the parameter number. </summary>
        /// <returns>number of parameter</returns>
        public int ParameterNumber { get; set; }

        /// <summary>Returns the parameter value. </summary>
        /// <returns>parameter value</returns>
        public object ParameterValue { get; set; }
    }
}