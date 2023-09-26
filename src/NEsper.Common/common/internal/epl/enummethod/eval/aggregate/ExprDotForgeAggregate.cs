///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.aggregate
{
    public partial class ExprDotForgeAggregate : ExprDotForgeEnumMethodBase
    {
        public override EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext)
        {
            var goesNode = (ExprLambdaGoesNode)parameters[1];
            var numParameters = goesNode.GoesToNames.Count;
            var firstName = goesNode.GoesToNames[0];
            var secondName = goesNode.GoesToNames[1];

            IDictionary<string, object> fields = new LinkedHashMap<string, object>();
            var initializationType = parameters[0].Forge.EvaluationType;
            if (initializationType == null) {
                throw new ExprValidationException("Initialization value is null-typed");
            }

            fields.Put(firstName, initializationType);
            if (inputEventType == null) {
                fields.Put(secondName, collectionComponentType);
            }

            if (numParameters > 2) {
                fields.Put(goesNode.GoesToNames[2], typeof(int));
                if (numParameters > 3) {
                    fields.Put(goesNode.GoesToNames[3], typeof(int));
                }
            }

            var evalEventType = ExprDotNodeUtility.MakeTransientOAType(
                enumMethodUsedName,
                fields,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
            if (inputEventType == null) {
                return new EnumForgeDescFactoryAggregateScalar(evalEventType);
            }

            return new EnumForgeDescFactoryAggregateEvent(evalEventType, inputEventType, secondName, numParameters);
        }
    }
} // end of namespace