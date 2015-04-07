///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    public class PopulateFieldWValueDescriptor
    {
        public PopulateFieldWValueDescriptor(string propertyName, Type fieldType, Type containerType, PopulateFieldValueSetter setter, bool forceNumeric)
        {
            PropertyName = propertyName;
            FieldType = fieldType;
            ContainerType = containerType;
            Setter = setter;
            IsForceNumeric = forceNumeric;
        }

        public string PropertyName { get; private set; }

        public Type FieldType { get; private set; }

        public Type ContainerType { get; private set; }

        public PopulateFieldValueSetter Setter { get; private set; }

        public bool IsForceNumeric { get; private set; }
    }
}
