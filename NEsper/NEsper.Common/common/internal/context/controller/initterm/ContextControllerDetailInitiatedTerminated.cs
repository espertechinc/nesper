///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerDetailInitiatedTerminated : ContextControllerDetail
    {
        private ContextConditionDescriptor startCondition;
        private ContextConditionDescriptor endCondition;
        private bool overlapping;
        private ExprEvaluator distinctEval;
        private Type[] distinctTypes;

        public ContextConditionDescriptor StartCondition {
            get => startCondition;
        }

        public void SetStartCondition(ContextConditionDescriptor startCondition)
        {
            this.startCondition = startCondition;
        }

        public ContextConditionDescriptor EndCondition {
            get => endCondition;
        }

        public void SetEndCondition(ContextConditionDescriptor endCondition)
        {
            this.endCondition = endCondition;
        }

        public bool IsOverlapping {
            get { return overlapping; }
        }

        public void SetOverlapping(bool overlapping)
        {
            this.overlapping = overlapping;
        }

        public ExprEvaluator DistinctEval {
            get => distinctEval;
        }

        public void SetDistinctEval(ExprEvaluator distinctEval)
        {
            this.distinctEval = distinctEval;
        }

        public Type[] GetDistinctTypes()
        {
            return distinctTypes;
        }

        public void SetDistinctTypes(Type[] distinctTypes)
        {
            this.distinctTypes = distinctTypes;
        }
    }
} // end of namespace