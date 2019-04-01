///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.plugin;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFHandler : PlugInAggregationMultiFunctionHandler
    {
        private readonly PlugInAggregationMultiFunctionValidationContext _validationContext;
    
        public SupportAggMFHandler(PlugInAggregationMultiFunctionValidationContext validationContext)
        {
            _validationContext = validationContext;
        }

        static SupportAggMFHandler()
        {
            ProviderFactories = new List<PlugInAggregationMultiFunctionStateFactory>();
            Accessors = new List<AggregationAccessor>();
            ProviderKeys = new List<AggregationStateKey>();
        }

        public static void Reset()
        {
            ProviderKeys.Clear();
            Accessors.Clear();
            ProviderFactories.Clear();
        }

        public static IList<AggregationStateKey> ProviderKeys { get; set; }

        public static IList<AggregationAccessor> Accessors { get; set; }

        public static IList<PlugInAggregationMultiFunctionStateFactory> ProviderFactories { get; set; }

        public AggregationStateKey AggregationStateUniqueKey
        {
            get
            {
                // we share single-event stuff
                if (SupportAggMFFuncExtensions.IsSingleEvent(_validationContext.FunctionName))
                {
                    AggregationStateKey key = new SupportAggregationStateKey("A1");
                    ProviderKeys.Add(key);
                    return key;
                }
                // never share anything else
                return new ProxyAggregationStateKey();
            }
        }

        public PlugInAggregationMultiFunctionStateFactory StateFactory
        {
            get
            {
                // for single-event tracking factories for assertions
                if (SupportAggMFFuncExtensions.IsSingleEvent(_validationContext.FunctionName))
                {
                    var factory = new SupportAggMFFactorySingleEvent();
                    ProviderFactories.Add(factory);
                    return factory;
                }
                return SupportAggMFFuncExtensions.FromFunctionName(_validationContext.FunctionName).GetStateFactory(_validationContext);
            }
        }

        public AggregationAccessor Accessor
        {
            get
            {
                // for single-event tracking accessors for assertions
                if (SupportAggMFFuncExtensions.IsSingleEvent(_validationContext.FunctionName))
                {
                    var accessorEvent = new SupportAggMFAccessorSingleEvent();
                    Accessors.Add(accessorEvent);
                    return accessorEvent;
                }
                return SupportAggMFFuncExtensions.FromFunctionName(_validationContext.FunctionName).GetAccessor();
            }
        }

        public EPType ReturnType
        {
            get
            {
                return SupportAggMFFuncExtensions.FromFunctionName(_validationContext.FunctionName).
                    GetReturnType(_validationContext.EventTypes[0], _validationContext.ParameterExpressions);
            }
        }

        public AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext)
        {
            return null;
        }

        public class SupportAggregationStateKey : AggregationStateKey
        {
            private readonly String _id;
    
            public SupportAggregationStateKey(String id)
            {
                _id = id;
            }
    
            public override bool Equals(Object o)
            {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;
    
                var that = (SupportAggregationStateKey) o;
    
                if (_id != null ? !_id.Equals(that._id) : that._id != null) return false;
    
                return true;
            }
    
            public override int GetHashCode() {
                return _id != null ? _id.GetHashCode() : 0;
            }
        }
    }
}
