///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
			    injectionStrategy = functionName switch {
				    "ss" => new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTPlainScalarStateFactory))
					    .AddExpression("param", validationContext.AllParameterExpressions[0]),
				    "sa" => new InjectionStrategyClassNewInstance(
						    typeof(SupportAggMFMultiRTArrayCollScalarStateFactory))
					    .AddExpression("evaluator", validationContext.AllParameterExpressions[0])
					    .AddConstant(
						    "evaluationType",
						    validationContext.AllParameterExpressions[0].Forge.EvaluationType),
				    "sc" => new InjectionStrategyClassNewInstance(
						    typeof(SupportAggMFMultiRTArrayCollScalarStateFactory))
					    .AddExpression("evaluator", validationContext.AllParameterExpressions[0])
					    .AddConstant(
						    "evaluationType",
						    validationContext.AllParameterExpressions[0].Forge.EvaluationType),
				    "se1" => new InjectionStrategyClassNewInstance(typeof(SupportAggMFMultiRTSingleEventStateFactory)),
				    "ee" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTEnumerableEventsStateFactory)),
				    _ => throw new UnsupportedOperationException("Unknown function '" + functionName + "'")
			    };

			    var mode = new AggregationMultiFunctionStateModeManaged()
				    .WithInjectionStrategyAggregationStateFactory(injectionStrategy);
			    stateFactoryModes.Add(mode);
			    return mode;
		    }
	    }

	    public AggregationMultiFunctionAccessorMode AccessorMode {
		    get {
			    var functionName = validationContext.FunctionName;
			    InjectionStrategy injectionStrategy = functionName switch {
				    "ss" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTPlainScalarAccessorFactory)),
				    "sa" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTArrayScalarAccessorFactory)),
				    "sc" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTCollScalarAccessorFactory)),
				    "se1" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTSingleEventAccessorFactory)),
				    "se2" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTSingleEventAccessorFactory)),
				    "ee" => new InjectionStrategyClassNewInstance(
					    typeof(SupportAggMFMultiRTEnumerableEventsAccessorFactory)),
				    _ => throw new IllegalStateException("Unrecognized function name '" + functionName + "'")
			    };

			    var mode = new AggregationMultiFunctionAccessorModeManaged()
				    .WithInjectionStrategyAggregationAccessorFactory(injectionStrategy);
			    accessorModes.Add(mode);
			    return mode;
		    }
	    }

	    public EPChainableType ReturnType {
		    get {
			    var functionName = validationContext.FunctionName;
			    return functionName switch {
				    "ss" => EPChainableTypeHelper.SingleValue(
					    validationContext.AllParameterExpressions[0].Forge.EvaluationType),
				    "sa" => EPChainableTypeHelper.Array(
					    validationContext.AllParameterExpressions[0].Forge.EvaluationType),
				    "sc" => EPChainableTypeHelper.CollectionOfSingleValue(
					    validationContext.AllParameterExpressions[0].Forge.EvaluationType),
				    "se1" => EPChainableTypeHelper.SingleEvent(
					    validationContext.EventTypes[0]),
				    "se2" => EPChainableTypeHelper.SingleEvent(
					    validationContext.EventTypes[0]),
				    "ee" => EPChainableTypeHelper.CollectionOfEvents(
					    validationContext.EventTypes[0]),
				    _ => throw new IllegalStateException("Unrecognized function name '" + functionName + "'")
			    };
		    }
	    }

	    public AggregationMultiFunctionAgentMode AgentMode => throw new UnsupportedOperationException("This implementation does not support tables");

	    public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx) {
	        return null; // not implemented
	    }
	}
} // end of namespace
