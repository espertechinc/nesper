///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.util
{
    public class ValidationUtil
    {
        public static void ValidateRequiredPropString(String value, String operatorName, String propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw GetRequiredPropException(propertyName, operatorName);
            }
        }

        public static void ValidateRequiredProp(Object value, String operatorName, String propertyName)
        {
            if (value == null)
            {
                throw GetRequiredPropException(propertyName, operatorName);
            }
        }

        private static EPException GetRequiredPropException(String propertyName, String operatorName)
        {
            return new EPException("Required property '" + propertyName + "' for operator " + operatorName + "' is not provided");
        }
    }
}
