///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     For use with high-availability and scale-out only, this class instructs the compiler that the serializer and de-serializer (serde)
    ///     is available via a singleton-pattern-style static field named "INSTANCE" (preferred) or by has a default constructor.
    /// </summary>
    public class SerdeProvisionByClass : SerdeProvision
    {
        /// <summary>
        ///     Class of the serde.
        /// </summary>
        /// <param name="serdeClass">serde class</param>
        public SerdeProvisionByClass(Type serdeClass)
        {
            SerdeClass = serdeClass;
        }

        /// <summary>
        ///     Returns the class of the serde
        /// </summary>
        /// <value>serde class</value>
        public Type SerdeClass { get; }

        public override DataInputOutputSerdeForge ToForge()
        {
            var field = SerdeClass.GetField("INSTANCE");
            if (field != null) {
                return new DataInputOutputSerdeForgeSingleton(SerdeClass);
            }

            try {
                MethodResolver.ResolveCtor(SerdeClass, Type.EmptyTypes);
                return new DataInputOutputSerdeForgeEmptyCtor(SerdeClass);
            }
            catch (MethodResolverNoSuchCtorException) {
            }

            throw new EPException(
                "Serde class '" +
                SerdeClass.Name +
                "' does not have a singleton-style INSTANCE field or default constructor");
        }
    }
} // end of namespace