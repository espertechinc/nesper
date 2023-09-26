///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    public class ASTAggregationHelper
    {
        public static ExprNode TryResolveAsAggregation(
            ImportServiceCompileTime importService,
            bool distinct,
            string functionName,
            LazyAllocatedMap<HashableMultiKey, AggregationMultiFunctionForge> plugInAggregations,
            ClassProvidedExtension classProvidedExtension)
        {
            try {
                var aggregationFactory = importService.ResolveAggregationFunction(functionName, classProvidedExtension);
                return new ExprPlugInAggNode(distinct, aggregationFactory, functionName);
            }
            catch (ImportUndefinedException) {
                // Not an aggregation function
            }
            catch (ImportException e) {
                throw new ValidationException("Error resolving aggregation: " + e.Message, e);
            }

            // try plug-in aggregation multi-function
            var configPair = importService
                .ResolveAggregationMultiFunction(functionName, classProvidedExtension);
            if (configPair != null) {
                var multiKey = new HashableMultiKey(configPair.First.FunctionNames);
                var factory = plugInAggregations.Map.Get(multiKey);
                if (factory == null) {
                    if (configPair.Second != null) {
                        factory = TypeHelper.Instantiate<AggregationMultiFunctionForge>(
                            configPair.Second);
                    }
                    else {
                        factory = TypeHelper.Instantiate<AggregationMultiFunctionForge>(
                            configPair.First.MultiFunctionForgeClassName,
                            importService.TypeResolver);
                    }

                    plugInAggregations.Map[multiKey] = factory;
                }

                factory.AddAggregationFunction(
                    new AggregationMultiFunctionDeclarationContext(
                        functionName.ToLowerInvariant(),
                        distinct,
                        configPair.First));
                return new ExprPlugInMultiFunctionAggNode(distinct, configPair.First, factory, functionName);
            }

            // try built-in expanded set of aggregation functions
            return importService.ResolveAggExtendedBuiltin(functionName, distinct);
        }
    }
} // end of namespace