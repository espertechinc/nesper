///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedEvalFuncVariable : AggSvcGroupByReclaimAgedEvalFunc
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AggSvcGroupByReclaimAgedEvalFuncVariable));

        private readonly VariableReader _variableReader;

        public AggSvcGroupByReclaimAgedEvalFuncVariable(VariableReader variableReader)
        {
            this._variableReader = variableReader;
        }

        public double? LongValue {
            get {
                var val = _variableReader.Value;
                if (val != null && val.IsNumber()) {
                    return val.AsDouble();
                }

                Log.Warn(
                    "Variable '" +
                    _variableReader.MetaData.VariableName +
                    " returned a null value, using last valid value");
                return null;
            }
        }
    }
} // end of namespace