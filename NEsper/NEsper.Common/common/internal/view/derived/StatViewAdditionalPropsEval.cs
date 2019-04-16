///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    public class StatViewAdditionalPropsEval
    {
        private readonly string[] additionalProps;
        private readonly ExprEvaluator[] additionalEvals;
        private readonly Type[] additionalTypes;

        public StatViewAdditionalPropsEval(
            string[] additionalProps,
            ExprEvaluator[] additionalEvals,
            Type[] additionalTypes)
        {
            this.additionalProps = additionalProps;
            this.additionalEvals = additionalEvals;
            this.additionalTypes = additionalTypes;
        }

        public string[] GetAdditionalProps()
        {
            return additionalProps;
        }

        public ExprEvaluator[] GetAdditionalEvals()
        {
            return additionalEvals;
        }

        public Type[] GetAdditionalTypes()
        {
            return additionalTypes;
        }

        public void AddProperties(
            IDictionary<string, object> newDataMap,
            object[] lastValuesEventNew)
        {
            if (lastValuesEventNew != null) {
                for (int i = 0; i < additionalProps.Length; i++) {
                    newDataMap.Put(additionalProps[i], lastValuesEventNew[i]);
                }
            }
        }
    }
} // end of namespace