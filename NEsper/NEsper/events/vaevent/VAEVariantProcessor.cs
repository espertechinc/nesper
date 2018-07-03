///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.view;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Represents a variant event stream, allowing events of disparate event types to be treated 
    /// polymophically.
    /// </summary>
    public class VAEVariantProcessor : ValueAddEventProcessor
    {
        /// <summary>Specification for the variant stream. </summary>
        protected readonly VariantSpec VariantSpec;

        /// <summary>The event type representing the variant stream. </summary>
        protected VariantEventType VariantEventType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="variantSpec">specifies how to handle the disparate events</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        /// <param name="config">The config.</param>
        /// <param name="lockManager">The lock manager.</param>
        public VAEVariantProcessor(
            EventAdapterService eventAdapterService,
            VariantSpec variantSpec,
            EventTypeIdGenerator eventTypeIdGenerator,
            ConfigurationVariantStream config,
            ILockManager lockManager)
        {
            VariantSpec = variantSpec;

            VariantPropResolutionStrategy strategy;
            if (variantSpec.TypeVariance == TypeVarianceEnum.ANY)
            {
                strategy = new VariantPropResolutionStrategyAny(lockManager, variantSpec);
            }
            else
            {
                strategy = new VariantPropResolutionStrategyDefault(lockManager, variantSpec);
            }

            EventTypeMetadata metadata = EventTypeMetadata.CreateValueAdd(variantSpec.VariantStreamName, TypeClass.VARIANT);
            VariantEventType = new VariantEventType(eventAdapterService, metadata, eventTypeIdGenerator.GetTypeId(variantSpec.VariantStreamName), variantSpec, strategy, config);
        }

        public EventType ValueAddEventType
        {
            get { return VariantEventType; }
        }

        public void ValidateEventType(EventType eventType)
        {
            if (VariantSpec.TypeVariance == TypeVarianceEnum.ANY)
            {
                return;
            }

            if (eventType == null)
            {
                throw new ExprValidationException(GetMessage());
            }

            // try each permitted type
            if (VariantSpec.EventTypes.Any(variant => variant == eventType))
            {
                return;
            }

            // test if any of the supertypes of the eventtype is a variant type
            foreach (EventType variant in VariantSpec.EventTypes)
            {
                // Check all the supertypes to see if one of the matches the full or delta types
                IEnumerable<EventType> deepSupers = eventType.DeepSuperTypes;
                if (deepSupers == null)
                {
                    continue;
                }

                foreach (var superType in deepSupers)
                {
                    if (superType == variant)
                    {
                        return;
                    }
                }
            }

            throw new ExprValidationException(GetMessage());
        }

        public EventBean GetValueAddEventBean(EventBean theEvent)
        {
            return new VariantEventBean(VariantEventType, theEvent);
        }

        public void OnUpdate(EventBean[] newData, EventBean[] oldData, NamedWindowRootViewInstance namedWindowRootView, EventTableIndexRepository indexRepository)
        {
            throw new UnsupportedOperationException();
        }

        public ICollection<EventBean> GetSnapshot(EPStatementAgentInstanceHandle createWindowStmtHandle, Viewable parent)
        {
            throw new UnsupportedOperationException();
        }

        public virtual void RemoveOldData(EventBean[] oldData, EventTableIndexRepository indexRepository, AgentInstanceContext agentInstanceContext)
        {
            throw new UnsupportedOperationException();
        }

        private String GetMessage()
        {
            return "Selected event type is not a valid event type of the variant stream '" + VariantSpec.VariantStreamName + "'";
        }
    }
}
