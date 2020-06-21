///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

namespace com.espertech.esper.common.@internal.util
{
    public class ValueAndFieldDesc
    {
        public object Value { get; }
        public FieldInfo Field { get; }

        public ValueAndFieldDesc(
            object value,
            FieldInfo field)
        {
            Value = value;
            Field = field;
        }
    }
}