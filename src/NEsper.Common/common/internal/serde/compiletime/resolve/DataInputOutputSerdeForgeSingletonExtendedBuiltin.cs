///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeSingletonExtendedBuiltin : DataInputOutputSerdeForgeSingleton
    {
        public DataInputOutputSerdeForgeSingletonExtendedBuiltin(
            Type serdeClass,
            Type extendedBuiltin) : base(serdeClass)
        {
            ExtendedBuiltin = extendedBuiltin;
        }

        public Type ExtendedBuiltin { get; }
    }
} // end of namespace