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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.take
{
    public class ExprDotForgeTakeAndTakeLast : ExprDotForgeEnumMethodBase
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
            EPChainableType type;
            if (inputEventType != null) {
                type = EPChainableTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                type = EPChainableTypeHelper.CollectionOfSingleValue(collectionComponentType);
            }

            return new EnumForgeDescFactoryTake(enumMethod, type, inputEventType == null);
        }

        private class EnumForgeDescFactoryTake : EnumForgeDescFactory
        {
            private readonly EnumMethodEnum enumMethod;
            private readonly EPChainableType type;
            private readonly bool _isScalar;

            public EnumForgeDescFactoryTake(
                EnumMethodEnum _enumMethod,
                EPChainableType _type,
                bool isScalar)
            {
                enumMethod = _enumMethod;
                type = _type;
                _isScalar = isScalar;
            }

            public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum)
            {
                throw new IllegalStateException("No lambda expected");
            }

            public EnumForgeDesc MakeEnumForgeDesc(
                IList<ExprDotEvalParam> bodiesAndParameters,
                int streamCountIncoming,
                StatementCompileTimeServices services)
            {
                var sizeEval = bodiesAndParameters[0].BodyForge;
                EnumForge forge;
                if (enumMethod == EnumMethodEnum.TAKE) {
                    forge = new EnumTakeForge(sizeEval, streamCountIncoming, _isScalar);
                }
                else {
                    forge = new EnumTakeLastForge(sizeEval, streamCountIncoming, _isScalar);
                }

                return new EnumForgeDesc(type, forge);
            }
        }
    }
} // end of namespace