///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class PopulateFieldWValueDescriptor
    {
        public PopulateFieldWValueDescriptor(
            string propertyName,
            Type fieldType,
            Type containerType,
            PopulateFieldValueSetter setter,
            bool forceNumeric)
        {
            PropertyName = propertyName;
            FieldType = fieldType;
            ContainerType = containerType;
            Setter = setter;
            IsForceNumeric = forceNumeric;
        }

        public string PropertyName { get; }

        public Type FieldType { get; }

        public Type ContainerType { get; }

        public PopulateFieldValueSetter Setter { get; }

        public bool IsForceNumeric { get; }
    }
}