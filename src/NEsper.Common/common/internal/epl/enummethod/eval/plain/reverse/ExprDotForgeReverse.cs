///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class ExprDotForgeReverse : ExprDotForgeEnumMethodBase
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
            EPType type;
            if (inputEventType != null) {
                type = EPTypeHelper.CollectionOfEvents(inputEventType);
            }
            else {
                type = EPTypeHelper.CollectionOfSingleValue(collectionComponentType, null);
            }

            return new EnumForgeDescFactoryReverse(type, inputEventType == null);
        }

        private class EnumForgeDescFactoryReverse : EnumForgeDescFactory {
            private readonly EPType _type;
            private readonly bool _isScalar;

            public EnumForgeDescFactoryReverse(EPType type, bool isScalar)
            {
                _type = type;
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
                EnumForge forge = new EnumReverseForge(streamCountIncoming, _isScalar);
                return new EnumForgeDesc(_type, forge);
            }
        }  
    }
} // end of namespace