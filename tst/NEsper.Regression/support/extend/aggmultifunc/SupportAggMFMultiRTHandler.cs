///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
	public partial class SupportAggMFMultiRTHandler : AggregationMultiFunctionHandler {
	    private readonly AggregationMultiFunctionValidationContext validationContext;

	    public static IList<AggregationMultiFunctionStateKey> providerKeys = new List<AggregationMultiFunctionStateKey>();
	    public static IList<AggregationMultiFunctionStateMode> stateFactoryModes = new List<AggregationMultiFunctionStateMode>();
	    public static IList<AggregationMultiFunctionAccessorMode> accessorModes = new List<AggregationMultiFunctionAccessorMode>();

	    public static void Reset() {
	        providerKeys.Clear();
	        stateFactoryModes.Clear();
	        accessorModes.Clear();
	    }

	    public static IList<AggregationMultiFunctionStateKey> ProviderKeys => providerKeys;

	    public static IList<AggregationMultiFunctionStateMode> StateFactoryModes => stateFactoryModes;

	    public static IList<AggregationMultiFunctionAccessorMode> AccessorModes => accessorModes;

	    public SupportAggMFMultiRTHandler(AggregationMultiFunctionValidationContext validationContext) {
	        this.validationContext = validationContext;
	    }

	    public AggregationMultiFunctionStateKey AggregationStateUniqueKey {
		    get {
			    // we share single-event stuff
			    var functionName = validationContext.FunctionName;
			    if (functionName.Equals("se1") || functionName.Equals("se2")) {
				    AggregationMultiFunctionStateKey key = new SupportAggregationStateKey("A1");
				    providerKeys.Add(key);
				    return key;
			    }

			    // never share anything else
			    return new InertAggregationMultiFunctionStateKey();
		    }
	    }

	    public AggregationMultiFunctionStateMode StateMode {
		    get {
			    InjectionStrategy injectionStrategy;
			    var functionName = validationContext.FunctionName;
			    if (functionName.Equals("ss")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTPlainScalarStateFactory))
						    .AddExpression("param", validationContext.AllParameterExpressions[0]);
			    }
			    else if (functionName.Equals("sa") || functionName.Equals("sc")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTArrayCollScalarStateFactory))
						    .AddExpression("evaluator", validationContext.AllParameterExpressions[0])
						    .AddConstant("evaluationType", validationContext.AllParameterExpressions[0].Forge.EvaluationType);
			    }
			    else if (functionName.Equals("se1")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTSingleEventStateFactory));
			    }
			    else if (functionName.Equals("ee")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTEnumerableEventsStateFactory));
			    }
			    else {
				    throw new UnsupportedOperationException("Unknown function '" + functionName + "'");
			    }

			    var mode = new AggregationMultiFunctionStateModeManaged()
				    .WithInjectionStrategyAggregationStateFactory(injectionStrategy);
			    stateFactoryModes.Add(mode);
			    return mode;
		    }
	    }

	    public AggregationMultiFunctionAccessorMode AccessorMode {
		    get {
			    var functionName = validationContext.FunctionName;
			    InjectionStrategy injectionStrategy;
			    if (functionName.Equals("ss")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTPlainScalarAccessorFactory));
			    }
			    else if (functionName.Equals("sa")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTArrayScalarAccessorFactory));
			    }
			    else if (functionName.Equals("sc")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTCollScalarAccessorFactory));
			    }
			    else if (functionName.Equals("se1") || functionName.Equals("se2")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTSingleEventAccessorFactory));
			    }
			    else if (functionName.Equals("ee")) {
				    injectionStrategy =
					    new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTEnumerableEventsAccessorFactory));
			    }
			    else {
				    throw new IllegalStateException("Unrecognized function name '" + functionName + "'");
			    }

			    var mode = new AggregationMultiFunctionAccessorModeManaged()
				    .WithInjectionStrategyAggregationAccessorFactory(injectionStrategy);
			    accessorModes.Add(mode);
			    return mode;
		    }
	    }

	    public EPChainableType ReturnType {
		    get {
			    var functionName = validationContext.FunctionName;
			    if (functionName.Equals("ss")) {
				    return EPChainableTypeHelper.SingleValue(validationContext.AllParameterExpressions[0].Forge.EvaluationType);
			    }
			    else if (functionName.Equals("sa")) {
				    return EPChainableTypeHelper.Array(validationContext.AllParameterExpressions[0].Forge.EvaluationType);
			    }
			    else if (functionName.Equals("sc")) {
				    return EPChainableTypeHelper.CollectionOfSingleValue(
					    validationContext.AllParameterExpressions[0].Forge.EvaluationType);
			    }
			    else if (functionName.Equals("se1") || functionName.Equals("se2")) {
				    return EPChainableTypeHelper.SingleEvent(validationContext.EventTypes[0]);
			    }
			    else if (functionName.Equals("ee")) {
				    return EPChainableTypeHelper.CollectionOfEvents(validationContext.EventTypes[0]);
			    }
			    else {
				    throw new IllegalStateException("Unrecognized function name '" + functionName + "'");
			    }
		    }
	    }

	    public AggregationMultiFunctionAgentMode AgentMode => throw new UnsupportedOperationException("This implementation does not support tables");

	    public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {
	        return null; // not implemented
	    }
	}
} // end of namespace
