///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportStreamTypeSvc3Stream : StreamTypeService
    {
        private readonly StreamTypeService _impl;
        private SupportEventTypeFactory _supportEventTypeFactory;

        public SupportStreamTypeSvc3Stream(SupportEventTypeFactory supportEventTypeFactory)
        {
            _supportEventTypeFactory = supportEventTypeFactory;
            _impl = new StreamTypeServiceImpl(EventTypes, StreamNames, new bool[10], false, false);
        }

        public string[] eventTypeNamees => new[] { "SupportBean", "SupportBean", "SupportBeanComplexProps" };

        public EventBean[] SampleEvents =>
            new[] {
                SupportEventBeanFactory.CreateObject(_supportEventTypeFactory, new SupportBean()),
                SupportEventBeanFactory.CreateObject(_supportEventTypeFactory, new SupportBean()),
                SupportEventBeanFactory.CreateObject(_supportEventTypeFactory, SupportBeanComplexProps.MakeDefaultBean())
            };

        public string EngineURIQualifier => "default";

        public PropertyResolutionDescriptor ResolveByPropertyName(
            string propertyName,
            bool obtainFragment)
        {
            return _impl.ResolveByPropertyName(propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(
            string streamName,
            string propertyName,
            bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropName(streamName, propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropName(
            string streamAndPropertyName,
            bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropName(streamAndPropertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByPropertyNameExplicitProps(
            string propertyName,
            bool obtainFragment)
        {
            return _impl.ResolveByPropertyNameExplicitProps(propertyName, false);
        }

        public PropertyResolutionDescriptor ResolveByStreamAndPropNameExplicitProps(
            string streamName,
            string propertyName,
            bool obtainFragment)
        {
            return _impl.ResolveByStreamAndPropNameExplicitProps(streamName, propertyName, false);
        }

        public string[] StreamNames => new[] { "s0", "s1", "s2" };

        public EventType[] EventTypes
        {
            get {
                EventType[] eventTypes = {
                    _supportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                    _supportEventTypeFactory.CreateBeanType(typeof(SupportBean)),
                    _supportEventTypeFactory.CreateBeanType(typeof(SupportBeanComplexProps))
                };
                return eventTypes;
            }
        }

        public bool[] IStreamOnly => new bool[10];

        public int GetStreamNumForStreamName(string streamName)
        {
            return _impl.GetStreamNumForStreamName(streamName);
        }

        public bool IsOnDemandStreams => _impl.IsOnDemandStreams;

        public bool HasPropertyAgnosticType => false;

        public bool IsStreamZeroUnambigous => false;

        public bool IsOptionalStreams => false;

        public bool HasTableTypes => false;
    }
} // end of namespace
