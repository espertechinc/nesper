///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Revision event bean for the overlayed scheme.
    /// </summary>
    public class RevisionEventBeanDeclared : EventBean
    {
        /// <summary>Ctor. </summary>
        /// <param name="eventType">revision event type</param>
        /// <param name="underlying">event wrapped</param>
        public RevisionEventBeanDeclared(RevisionEventType eventType, EventBean underlying)
        {
            RevisionEventType = eventType;
            UnderlyingFullOrDelta = underlying;
        }

        /// <summary>Returns true if latest event, or false if not. </summary>
        /// <value>indicator if latest</value>
        public bool IsLatest { get; set; }

        /// <summary>Sets versions. </summary>
        /// <value>versions</value>
        public RevisionBeanHolder[] Holders { private get; set; }

        /// <summary>Returns last base event. </summary>
        /// <value>base event</value>
        public EventBean LastBaseEvent { get; set; }

        /// <summary>Returns wrapped event. </summary>
        /// <value>wrapped event</value>
        public EventBean UnderlyingFullOrDelta { get; private set; }

        /// <summary>Returns the key. </summary>
        /// <value>key</value>
        public object Key { get; set; }

        /// <summary>Returns the revision event type. </summary>
        /// <value>type</value>
        public RevisionEventType RevisionEventType { get; private set; }

        public EventType EventType
        {
            get { return RevisionEventType; }
        }

        public Object Get(String property)
        {
            EventPropertyGetter getter = RevisionEventType.GetGetter(property);
            if (getter == null)
            {
                return null;
            }
            return getter.Get(this);
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public object Underlying
        {
            get { return typeof (RevisionEventBeanDeclared); }
        }

        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = RevisionEventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + propertyExpression + "' is not a valid property name for this type");
            }
            return getter.GetFragment(this);
        }
    
        /// <summary>Returns a versioned value. </summary>
        /// <param name="parameters">getter parameters</param>
        /// <returns>value</returns>
        public Object GetVersionedValue(RevisionGetterParameters parameters)
        {
            RevisionBeanHolder holderMostRecent = null;
    
            if (Holders != null)
            {
                foreach (int numSet in parameters.PropertyGroups)
                {
                    RevisionBeanHolder holder = Holders[numSet];
                    if (holder != null)
                    {
                        if (holderMostRecent == null)
                        {
                            holderMostRecent = holder;
                        }
                        else
                        {
                            if (holder.Version > holderMostRecent.Version)
                            {
                                holderMostRecent = holder;
                            }
                        }
                    }
                }
            }
    
            // none found, use last full event
            if (holderMostRecent == null)
            {
                if (LastBaseEvent == null) {
                    return null;
                }
                return parameters.BaseGetter.Get(LastBaseEvent);
            }
    
            return holderMostRecent.GetValueForProperty(parameters.PropertyNumber);
        }
    }
}
