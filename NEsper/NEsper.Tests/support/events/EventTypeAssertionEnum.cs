///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.support.events
{
    public enum EventTypeAssertionEnum
    {
        NAME,
        TYPE,
        FRAGMENT_TYPE_NAME,
        FRAGMENT_IS_INDEXED
    }

    public delegate object EventTypeAssertionExtractor(EventPropertyDescriptor desc, EventType eventType);

    public static class EventTypeAssertionEnumExtensions
    {
        public static object ExtractPropertyName(EventPropertyDescriptor desc, EventType eventType)
        {
            return desc.PropertyName;
        }

        public static object ExtractPropertyType(EventPropertyDescriptor desc, EventType eventType)
        {
            return desc.PropertyType;
        }

        public static object ExtractFragmentTypeName(EventPropertyDescriptor desc, EventType eventType)
        {
            FragmentEventType fragType = eventType.GetFragmentType(desc.PropertyName);
            if (fragType == null)
            {
                return null;
            }
            return fragType.FragmentType.Name;
        }

        public static object ExtractFragmentTypeIsIndexed(EventPropertyDescriptor desc, EventType eventType)
        {
            FragmentEventType fragType = eventType.GetFragmentType(desc.PropertyName);
            if (fragType == null)
            {
                return null;
            }
            return fragType.IsIndexed;
        }

        public static EventTypeAssertionExtractor GetExtractor(this EventTypeAssertionEnum @enum)
        {
            switch (@enum)
            {
                case EventTypeAssertionEnum.NAME:
                    return ExtractPropertyName;
                case EventTypeAssertionEnum.TYPE:
                    return ExtractPropertyType;
                case EventTypeAssertionEnum.FRAGMENT_TYPE_NAME:
                    return ExtractFragmentTypeName;
                case EventTypeAssertionEnum.FRAGMENT_IS_INDEXED:
                    return ExtractFragmentTypeIsIndexed;
            }

            throw new ArgumentException();
        }
    
        public static EventTypeAssertionEnum[] GetSetWithFragment()
        {
            return new EventTypeAssertionEnum[]
            {
                EventTypeAssertionEnum.NAME, 
                EventTypeAssertionEnum.TYPE, 
                EventTypeAssertionEnum.FRAGMENT_TYPE_NAME, 
                EventTypeAssertionEnum.FRAGMENT_IS_INDEXED
            };
        }
    }
}
