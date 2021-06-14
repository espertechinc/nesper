///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.client
{
    public class MyAnnotationValueEnumAttribute : Attribute
    {
        public SupportEnum SupportEnum { get; set; }
        [DefaultValue(SupportEnum.ENUM_VALUE_2)]
        public SupportEnum SupportEnumDef { get; set; }
    }
} // end of namespace