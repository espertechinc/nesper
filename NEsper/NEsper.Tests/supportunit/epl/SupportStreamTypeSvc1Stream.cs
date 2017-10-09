///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportStreamTypeSvc1Stream : StreamTypeService
    {
        private readonly StreamTypeService _impl;

        public SupportStreamTypeSvc1Stream()
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
            get { return new String[] { "s0" }; }
        }

        public EventType[] EventTypes
        {
            get
            {
                return new EventType[]
                {
                    SupportEventTypeFactory.CreateBeanType(typeof(SupportBean))
                };
            }
        }

        public bool[] IsIStreamOnly
        {
            get { return new bool[10]; }
        }

        public int GetStreamNumForStreamName(String streamWildcard)
        {
            return _impl.GetStreamNumForStreamName(streamWildcard);
        }

        public bool IsOnDemandStreams
        {
            get { return _impl.IsOnDemandStreams; }
        }

        public string EngineURIQualifier
        {
            get { return null; }
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