///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.common.@internal.util
{
    public class EnumValue
    {
        public EnumValue(
            Type enumClass,
            FieldInfo enumField)
        {
            EnumClass = enumClass;
            EnumField = enumField;
        }

        public Type EnumClass { get; }

        public FieldInfo EnumField { get; }
    }
} // end of namespace