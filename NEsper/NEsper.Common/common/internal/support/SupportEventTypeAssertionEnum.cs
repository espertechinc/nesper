///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.support
{
    public enum SupportEventTypeAssertionEnum
    {
        NAME,
        TYPE,
        FRAGEMENT_TYPE_NAME,
        FRAGMENT_IS_INDEXED
    }

    public static class SupportEventTypeAssertionEnumExtensions
    {
        public static Extractor GetExtractor(
            this SupportEventTypeAssertionEnum enumValue)
        {
            switch (enumValue)
            {
                case SupportEventTypeAssertionEnum.NAME:
                    return (desc, eventType) => desc.PropertyName;
                case SupportEventTypeAssertionEnum.TYPE:
                    return (desc, eventType) => desc.PropertyType;
                case SupportEventTypeAssertionEnum.FRAGEMENT_TYPE_NAME:
                    return (desc, eventType) =>
                    {
                        var fragType = eventType.GetFragmentType(desc.PropertyName);
                        if (fragType == null)
                        {
                            return null;
                        }
                        return fragType.FragmentType.Name;
                    };
                case SupportEventTypeAssertionEnum.FRAGMENT_IS_INDEXED:
                    return (desc, eventType) =>
                    {
                        var fragType = eventType.GetFragmentType(desc.PropertyName);
                        if (fragType == null)
                        {
                            return null;
                        }
                        return fragType.IsIndexed;
                    };
            }

            throw new ArgumentException("value out of bounds", "enumValue");
        }

        public static SupportEventTypeAssertionEnum[] GetSetWithFragment()
        {
            return new SupportEventTypeAssertionEnum[]
            {
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE,
                SupportEventTypeAssertionEnum.FRAGEMENT_TYPE_NAME,
                SupportEventTypeAssertionEnum.FRAGMENT_IS_INDEXED
            };
        }
    }

    public delegate object Extractor(EventPropertyDescriptor desc, EventType eventType);
} // end of namespace