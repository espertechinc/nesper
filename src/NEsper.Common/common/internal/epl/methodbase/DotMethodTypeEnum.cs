///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public enum DotMethodTypeEnum
    {
        ENUM,
        DATETIME,
        AGGMETHOD
    };

    public static class DotMethodTypeEnumExtensions
    {
        public static string GetTypeName(this DotMethodTypeEnum value)
        {
            switch (value) {
                case DotMethodTypeEnum.ENUM:
                    return "enumeration";

                case DotMethodTypeEnum.DATETIME:
                    return "date-time";

                case DotMethodTypeEnum.AGGMETHOD:
                    return "aggregation";

                default:
                    throw new ArgumentException("invalid value", "value");
            }
        }
    }
}