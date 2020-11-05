///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportReferenceCountedMapForge : AggregationMultiFunctionForge
    {
        private static readonly AggregationMultiFunctionStateKey SHARED_STATE_KEY =
            new InertAggregationMultiFunctionStateKey();

        public void AddAggregationFunction(AggregationMultiFunctionDeclarationContext declarationContext)
        {
        }

        public AggregationMultiFunctionHandler ValidateGetHandler(
            AggregationMultiFunctionValidationContext validationContext)
        {
            if (validationContext.FunctionName.Equals("referenceCountedMap")) {
                return new SupportReferenceCountedMapRCMFunctionHandler(
                    SHARED_STATE_KEY,
                    validationContext.ParameterExpressions);
            }

            if (validationContext.FunctionName.Equals("referenceCountLookup")) {
                var eval = validationContext.ParameterExpressions[0];
                return new SupportReferenceCountedMapRCLFunctionHandler(eval);
            }

            throw new ArgumentException("Unexpected function name '" + validationContext.FunctionName);
        }
    }
} // end of namespace