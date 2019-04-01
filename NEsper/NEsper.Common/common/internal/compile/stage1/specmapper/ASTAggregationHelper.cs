///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
	public class ASTAggregationHelper {
	    public static ExprNode TryResolveAsAggregation(ImportServiceCompileTime importService,
	                                                   bool distinct,
	                                                   string functionName,
	                                                   LazyAllocatedMap<ConfigurationCompilerPlugInAggregationMultiFunction, AggregationMultiFunctionForge> plugInAggregations) {
	        try {
	            AggregationFunctionForge aggregationFactory = importService.ResolveAggregationFunction(functionName);
	            return new ExprPlugInAggNode(distinct, aggregationFactory, functionName);
	        } catch (ImportUndefinedException e) {
	            // Not an aggregation function
	        } catch (ImportException e) {
	            throw new ValidationException("Error resolving aggregation: " + e.Message, e);
	        }

	        // try plug-in aggregation multi-function
	        ConfigurationCompilerPlugInAggregationMultiFunction config = importService.ResolveAggregationMultiFunction(functionName);
	        if (config != null) {
	            AggregationMultiFunctionForge factory = plugInAggregations.Map.Get(config);
	            if (factory == null) {
	                factory = (AggregationMultiFunctionForge) TypeHelper.Instantiate(typeof(AggregationMultiFunctionForge), config.MultiFunctionForgeClassName, importService.ClassForNameProvider);
	                plugInAggregations.Map.Put(config, factory);
	            }
	            factory.AddAggregationFunction(new AggregationMultiFunctionDeclarationContext(functionName.ToLowerInvariant(), distinct, config));
	            return new ExprPlugInMultiFunctionAggNode(distinct, config, factory, functionName);
	        }

	        // try built-in expanded set of aggregation functions
	        return importService.ResolveAggExtendedBuiltin(functionName, distinct);
	    }
	}
} // end of namespace