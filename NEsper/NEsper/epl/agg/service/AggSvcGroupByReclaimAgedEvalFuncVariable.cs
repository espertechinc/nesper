///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedEvalFuncVariable : AggSvcGroupByReclaimAgedEvalFunc
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly VariableReader _variableReader;
    
        public AggSvcGroupByReclaimAgedEvalFuncVariable(VariableReader variableReader)
        {
            _variableReader = variableReader;
        }

        public double? LongValue
        {
            get
            {
                var val = _variableReader.Value;
                if (val.IsNumber())
                {
                    return val.AsDouble();
                }

                Log.Warn("Variable '{0} returned a null value, using last valid value", _variableReader.VariableMetaData.VariableName);
                return null;
            }
        }
    }
}
