///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    /// Implementation of a convertor for column results that renders the result as an object array itself.
    /// </summary>
    public class DeliveryConvertorObjectArr : DeliveryConvertor
    {
        internal static readonly DeliveryConvertorObjectArr INSTANCE = new DeliveryConvertorObjectArr();

        public Object[] ConvertRow(Object[] columns)
        {
            return new Object[]
            {
                columns
            };
        }
    }
}