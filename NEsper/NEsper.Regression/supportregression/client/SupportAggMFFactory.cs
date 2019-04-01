///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFFactory : PlugInAggregationMultiFunctionFactory
    {
        private static readonly ICollection<SupportAggMFFactory> factories = 
            new HashSet<SupportAggMFFactory>();
        private static readonly IList<PlugInAggregationMultiFunctionDeclarationContext> functionDeclContexts =
            new List<PlugInAggregationMultiFunctionDeclarationContext>();
        private static readonly IList<PlugInAggregationMultiFunctionValidationContext> functionHandlerValidationContexts =
            new List<PlugInAggregationMultiFunctionValidationContext>();
    
        public static void Reset()
        {
            factories.Clear();
            functionDeclContexts.Clear();
            functionHandlerValidationContexts.Clear();
        }

        public static ICollection<SupportAggMFFactory> Factories
        {
            get { return factories; }
        }

        public static IList<PlugInAggregationMultiFunctionDeclarationContext> FunctionDeclContexts
        {
            get { return functionDeclContexts; }
        }

        public static IList<PlugInAggregationMultiFunctionValidationContext> FunctionHandlerValidationContexts
        {
            get { return functionHandlerValidationContexts; }
        }

        public void AddAggregationFunction(PlugInAggregationMultiFunctionDeclarationContext declarationContext)
        {
            factories.Add(this);
            functionDeclContexts.Add(declarationContext);
        }
    
        public SupportAggMFFactory()
        {
            factories.Add(this);
        }
    
        public PlugInAggregationMultiFunctionHandler ValidateGetHandler(PlugInAggregationMultiFunctionValidationContext validationContext)
        {
            functionHandlerValidationContexts.Add(validationContext);
            return new SupportAggMFHandler(validationContext);
        }
    }
}
