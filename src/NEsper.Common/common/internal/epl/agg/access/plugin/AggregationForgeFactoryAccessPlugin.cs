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
        private readonly ExprPlugInMultiFunctionAggNode _parent;
        private readonly AggregationMultiFunctionHandler _handler;
        private EPChainableType _returnType;

        public AggregationForgeFactoryAccessPlugin(
            ExprPlugInMultiFunctionAggNode parent,
            AggregationMultiFunctionHandler handler)
        {
            this._parent = parent;
            this._handler = handler;
        }

        public override AggregationMultiFunctionStateKey GetAggregationStateKey(bool isMatchRecognize)
        {
            return _handler.AggregationStateUniqueKey;
        }

        public override AggregationStateFactoryForge GetAggregationStateFactory(bool isMatchRecognize)
        {
            AggregationMultiFunctionStateMode stateMode = _handler.StateMode;
            if (stateMode is AggregationMultiFunctionStateModeManaged) {
                AggregationMultiFunctionStateModeManaged managed = (AggregationMultiFunctionStateModeManaged) stateMode;
                return new AggregationStateFactoryForgePlugin(this, managed);
            }
            else {
                throw new IllegalStateException("Unrecognized state mode " + stateMode);
            }
        }

        public override AggregationAccessorForge AccessorForge {
            get {
                AggregationMultiFunctionAccessorMode accessorMode = _handler.AccessorMode;
                if (accessorMode is AggregationMultiFunctionAccessorModeManaged) {
                    AggregationMultiFunctionAccessorModeManaged managed =
                        (AggregationMultiFunctionAccessorModeManaged) accessorMode;
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
            AggregationMultiFunctionAgentMode agentMode = _handler.AgentMode;
            if (agentMode is AggregationMultiFunctionAgentModeManaged) {
                AggregationMultiFunctionAgentModeManaged managed = (AggregationMultiFunctionAgentModeManaged) agentMode;
                return new AggregationAgentForgePlugin(
                    this,
                    managed,
                    _parent.OptionalFilter == null ? null : _parent.OptionalFilter.Forge);
            }
            else {
                throw new IllegalStateException("Unrecognized accessor mode " + agentMode);
            }
        }

        public override Type ResultType {
            get {
                ObtainReturnType();
                return EPChainableTypeHelper.GetNormalizedClass(_returnType);
            }
        }

        public override ExprAggregateNodeBase AggregationExpression {
            get => _parent;
        }

        public override AggregationPortableValidation AggregationPortableValidation {
            get {
                var portable = new AggregationPortableValidationPluginMultiFunc();
                portable.Handler = _handler;
                portable.AggregationFunctionName = _parent.AggregationFunctionName;
                return portable;
            }
        }

        public EventType EventTypeCollection {
            get {
                ObtainReturnType();
                return EPChainableTypeHelper.GetEventTypeMultiValued(_returnType);
            }
        }

        public EventType EventTypeSingle {
            get {
                ObtainReturnType();
                return EPChainableTypeEventSingle.FromInputOrNull(_returnType);
            }
        }

        public Type ComponentTypeCollection {
            get {
                ObtainReturnType();
                return EPChainableTypeHelper.GetCollectionOrArrayComponentTypeOrNull(_returnType);
            }
        }

        private void ObtainReturnType()
        {
            if (_returnType == null) {
                _returnType = _handler.ReturnType;
            }
        }
    }
} // end of namespace