///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    public class StatViewAdditionalPropsEval
    {
        public StatViewAdditionalPropsEval(
            string[] additionalProps,
            ExprEvaluator[] additionalEvals,
            Type[] additionalTypes,
            DataInputOutputSerde[] additionalSerdes)
        {
            AdditionalProps = additionalProps;
            AdditionalEvals = additionalEvals;
            AdditionalTypes = additionalTypes;
            AdditionalSerdes = additionalSerdes;
        }

        public string[] AdditionalProps { get; }

        public DataInputOutputSerde[] AdditionalSerdes { get; }

        public ExprEvaluator[] AdditionalEvals { get; }

        public Type[] AdditionalTypes { get; }

        public void AddProperties(
            IDictionary<string, object> newDataMap,
            object[] lastValuesEventNew)
        {
            if (lastValuesEventNew != null) {
                for (var i = 0; i < AdditionalProps.Length; i++) {
                    newDataMap.Put(AdditionalProps[i], lastValuesEventNew[i]);
                }
            }
        }
    }
} // end of namespace