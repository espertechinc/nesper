///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.client
{
    public class MyAnnotationValueEnumAttribute : Attribute
    {
        public MyAnnotationValueEnumAttribute()
        {
            SupportEnumDef = SupportEnum.ENUM_VALUE_2;
        }

        public SupportEnum SupportEnum { get; set; }
        public SupportEnum SupportEnumDef { get; set; }
    }
}
