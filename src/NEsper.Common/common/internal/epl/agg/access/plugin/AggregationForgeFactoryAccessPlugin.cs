///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.access.plugin
{
    public class AggregationForgeFactoryAccessPlugin : AggregationForgeFactoryAccessBase
    {
        private readonly ExprPlugInMultiFunctionAggNode parent;
        private readonly AggregationMultiFunctionHandler handler;
        private EPChainableType returnType;

        public AggregationForgeFactoryAccessPlugin(
            ExprPlugInMultiFunctionAggNode parent,
            AggregationMultiFunctionHandler handler)
        {
            this.parent = parent;
            this.handler = handler;
        }

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return handler.AggregationStateUniqueKey;
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(
            bool isMatchRecognize,
            bool isJoin)
        {
            var stateMode = handler.StateMode;
            if (stateMode is AggregationMultiFunctionStateModeManaged managed) {
                return new AggregationStateFactoryForgePlugin(this, managed);
            }
            else {
                throw new IllegalStateException("Unrecognized state mode " + stateMode);
            }
        }

        public override AggregationAccessorForge AccessorForge {
            get {
                var accessorMode = handler.AccessorMode;
                if (accessorMode is AggregationMultiFunctionAccessorModeManaged managed) {
                    return new AggregationAccessorForgePlugin(this, managed);
                }
                else {
                    throw new IllegalStateException("Unrecognized accessor mode " + accessorMode);
                }
            }
        }

        public override AggregationAgentForge GetAggregationStateAgent(
            ImportService importService,
            string statementName)
        {
            var agentMode = handler.AgentMode;
            if (agentMode is AggregationMultiFunctionAgentModeManaged managed) {
                return new AggregationAgentForgePlugin(
                    this,
                    managed,
                    parent.OptionalFilter?.Forge);
            }
            else {
                throw new IllegalStateException("Unrecognized accessor mode " + agentMode);
            }
        }

        public override Type ResultType {
            get {
                ObtainReturnType();
                return returnType.GetNormalizedType();
            }
        }

        public override ExprAggregateNodeBase AggregationExpression => parent;

        public override AggregationPortableValidation AggregationPortableValidation {
            get {
                var portable = new AggregationPortableValidationPluginMultiFunc();
                portable.Handler = handler;
                portable.AggregationFunctionName = parent.AggregationFunctionName;
                return portable;
            }
        }

        public EventType EventTypeCollection {
            get {
                ObtainReturnType();
                return EPChainableTypeHelper.GetEventTypeMultiValued(returnType);
            }
        }

        public EventType EventTypeSingle {
            get {
                ObtainReturnType();
                return EPChainableTypeEventSingle.FromInputOrNull(returnType);
            }
        }

        public Type ComponentTypeCollection {
            get {
                ObtainReturnType();
                return EPChainableTypeHelper.GetCollectionOrArrayComponentTypeOrNull(returnType);
            }
        }

        private void ObtainReturnType()
        {
            if (returnType == null) {
                returnType = handler.ReturnType;
            }
        }
    }
} // end of namespace