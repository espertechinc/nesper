///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.@base
{
    public abstract class ExprDotForgeTwoLambda : ExprDotForgeEnumMethodBase
    {
        protected abstract TwoLambdaThreeFormEventPlainFactory.ForgeFunction TwoParamEventPlain();
        protected abstract TwoLambdaThreeFormEventPlusFactory.ForgeFunction TwoParamEventPlus();
        protected abstract TwoLambdaThreeFormScalarFactory.ForgeFunction TwoParamScalar();

        public override EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext)
        {
            if (parameters.Count < 2) {
                throw new IllegalStateException();
            }

            var lambdaFirst = (ExprLambdaGoesNode)parameters[0];
            var lambdaSecond = (ExprLambdaGoesNode)parameters[1];
            if (lambdaFirst.GoesToNames.Count != lambdaSecond.GoesToNames.Count) {
                throw new ExprValidationException(
                    "Enumeration method '" +
                    enumMethodUsedName +
                    "' expected the same number of parameters for both the key and the value expression");
            }

            var numParameters = lambdaFirst.GoesToNames.Count;

            if (inputEventType != null) {
                var streamNameFirst = lambdaFirst.GoesToNames[0];
                var streamNameSecond = lambdaSecond.GoesToNames[0];
                if (numParameters == 1) {
                    return new TwoLambdaThreeFormEventPlainFactory(
                        inputEventType,
                        streamNameFirst,
                        streamNameSecond,
                        TwoParamEventPlain());
                }

                IDictionary<string, object> fieldsFirstX = new LinkedHashMap<string, object>();
                IDictionary<string, object> fieldsSecondX = new LinkedHashMap<string, object>();
                fieldsFirstX.Put(lambdaFirst.GoesToNames[1], typeof(int?));
                fieldsSecondX.Put(lambdaSecond.GoesToNames[1], typeof(int?));
                if (numParameters > 2) {
                    fieldsFirstX.Put(lambdaFirst.GoesToNames[2], typeof(int?));
                    fieldsSecondX.Put(lambdaSecond.GoesToNames[2], typeof(int?));
                }

                var typeFirstX = ExprDotNodeUtility.MakeTransientOAType(
                    enumMethodUsedName,
                    fieldsFirstX,
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
                var typeSecondX = ExprDotNodeUtility.MakeTransientOAType(
                    enumMethodUsedName,
                    fieldsSecondX,
                    validationContext.StatementRawInfo,
                    validationContext.StatementCompileTimeService);
                return new TwoLambdaThreeFormEventPlusFactory(
                    inputEventType,
                    streamNameFirst,
                    streamNameSecond,
                    typeFirstX,
                    typeSecondX,
                    lambdaFirst.GoesToNames.Count,
                    TwoParamEventPlus());
            }

            IDictionary<string, object> fieldsFirst = new LinkedHashMap<string, object>();
            IDictionary<string, object> fieldsSecond = new LinkedHashMap<string, object>();
            fieldsFirst.Put(lambdaFirst.GoesToNames[0], collectionComponentType);
            fieldsSecond.Put(lambdaSecond.GoesToNames[0], collectionComponentType);
            if (numParameters > 1) {
                fieldsFirst.Put(lambdaFirst.GoesToNames[1], typeof(int?));
                fieldsSecond.Put(lambdaSecond.GoesToNames[1], typeof(int?));
            }

            if (numParameters > 2) {
                fieldsFirst.Put(lambdaFirst.GoesToNames[2], typeof(int?));
                fieldsSecond.Put(lambdaSecond.GoesToNames[2], typeof(int?));
            }

            var typeFirst = ExprDotNodeUtility.MakeTransientOAType(
                enumMethodUsedName,
                fieldsFirst,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
            var typeSecond = ExprDotNodeUtility.MakeTransientOAType(
                enumMethodUsedName,
                fieldsSecond,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
            return new TwoLambdaThreeFormScalarFactory(
                typeFirst,
                typeSecond,
                lambdaFirst.GoesToNames.Count,
                TwoParamScalar());
        }
    }
} // end of namespace