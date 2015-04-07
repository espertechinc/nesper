///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.events;

namespace com.espertech.esper.support.epl
{
    public class SupportStreamTypeSvc3Stream : StreamTypeService
    {
        private readonly StreamTypeService _impl;

        public SupportStreamTypeSvc3Stream()
        {
            _impl = new StreamTypeServiceImpl(EventTypes, StreamNames, new bool[10], "default", false);
        }

        public PropertyResolutionDescriptor ResolveByPropertyName(String propertyName, bool obtainFragment)
        {
            return _impl.ResolveByPropertyName(propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamName, String propertyName, bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropName(streamName, propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(String streamAndPropertyName, bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropName(streamAndPropertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByPropertyNameExplicitProps(String propertyName, bool obtainFragment)
        {
            return _impl.ResolveByPropertyNameExplicitProps(propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropNameExplicitProps(String streamName, String propertyName, bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropNameExplicitProps(streamName, propertyName, false);
        }

        public string[] StreamNames
        {
            get { return new String[] { "s0", "s1", "s2" }; }
        }

        public EventType[] EventTypes
        {
            get
            {
                EventType[] eventTypes = new EventType[]
                                         {
                                             SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                                             SupportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                                             SupportEventTypeFactory.CreateBeanType(typeof(SupportBeanComplexProps)),
                                         };
                return eventTypes;
            }
        }

        public string[] EventTypeNamees
        {
            get { return new String[] { "SupportBean", "SupportBean", "SupportBeanComplexProps" }; }
        }

        public static EventBean[] SampleEvents
        {
            get
            {
                return new EventBean[]
                       {
                           SupportEventBeanFactory.CreateObject(new SupportBean()),
                           SupportEventBeanFactory.CreateObject(new SupportBean()),
                           SupportEventBeanFactory.CreateObject(SupportBeanComplexProps.MakeDefaultBean()),
                       };
            }
        }

        public bool[] IsIStreamOnly
        {
            get { return new bool[10]; }
        }

        public int GetStreamNumForStreamName(String streamName)
        {
            return _impl.GetStreamNumForStreamName(streamName);
        }

        public bool IsOnDemandStreams
        {
            get { return _impl.IsOnDemandStreams; }
        }

        public string EngineURIQualifier
        {
            get { return "default"; }
        }

        public bool HasPropertyAgnosticType
        {
            get { return false; }
        }

        public bool HasTableTypes
        {
            get { return false; }
        }

        public bool IsStreamZeroUnambigous
        {
            get { return false; }
        }
    }
}
