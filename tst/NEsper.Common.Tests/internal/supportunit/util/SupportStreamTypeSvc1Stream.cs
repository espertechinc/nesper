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
    public class SupportStreamTypeSvc1Stream : StreamTypeService
    {
        private readonly StreamTypeService _impl;
        private SupportEventTypeFactory _supportEventTypeFactory;

        public SupportStreamTypeSvc1Stream(SupportEventTypeFactory supportEventTypeFactory)
        {
            _supportEventTypeFactory = supportEventTypeFactory;
            _impl = new StreamTypeServiceImpl(EventTypes, StreamNames, new bool[10], false, false);
        }

        public string EngineURIQualifier => null;

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

        public string[] StreamNames => new[] { "s0" };

        public bool[] IStreamOnly => new bool[10];

        public int GetStreamNumForStreamName(string streamWildcard)
        {
            return _impl.GetStreamNumForStreamName(streamWildcard);
        }

        public bool IsOnDemandStreams => _impl.IsOnDemandStreams;

        public bool IsStreamZeroUnambigous => false;

        public bool IsOptionalStreams => false;

        public EventType[] EventTypes =>
            new EventType[] {
                _supportEventTypeFactory.CreateBeanType(typeof(SupportBean))
            };

        public bool HasPropertyAgnosticType => false;

        public bool HasTableTypes => false;
    }
} // end of namespace
