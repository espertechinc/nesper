///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    /// For use with high-availability and scale-out only, this class instructs the compiler that the serializer and de-serializer (serde)
    /// is available for a given component type and component type serde
    /// </summary>
    public class SerdeProvisionArrayOfNonPrimitive : SerdeProvision
    {
        private readonly Type componentType;
        private readonly SerdeProvision componentSerde;

        public SerdeProvisionArrayOfNonPrimitive(
            Type componentType,
            SerdeProvision componentSerde)
        {
            this.componentType = componentType;
            this.componentSerde = componentSerde;
        }

        public override DataInputOutputSerdeForge ToForge()
        {
            return new DIONullableObjectArraySerdeForge(componentType, componentSerde.ToForge());
        }
    }
} // end of namespace