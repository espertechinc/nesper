///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTForge : AggregationMultiFunctionForge
    {
        public SupportAggMFMultiRTForge()
        {
            Forges.Add(this);
        }

        public static ISet<SupportAggMFMultiRTForge> Forges { get; } = new HashSet<SupportAggMFMultiRTForge>();

        public static IList<AggregationMultiFunctionDeclarationContext> FunctionDeclContexts { get; } =
            new List<AggregationMultiFunctionDeclarationContext>();

        public static IList<AggregationMultiFunctionValidationContext> FunctionHandlerValidationContexts { get; } =
            new List<AggregationMultiFunctionValidationContext>();

        public void AddAggregationFunction(AggregationMultiFunctionDeclarationContext declarationContext)
        {
            Forges.Add(this);
            FunctionDeclContexts.Add(declarationContext);
        }

        public AggregationMultiFunctionHandler ValidateGetHandler(
            AggregationMultiFunctionValidationContext validationContext)
        {
            FunctionHandlerValidationContexts.Add(validationContext);
            return new SupportAggMFMultiRTHandler(validationContext);
        }

        public static void Reset()
        {
            Forges.Clear();
            FunctionDeclContexts.Clear();
            FunctionHandlerValidationContexts.Clear();
        }
    }
} // end of namespace