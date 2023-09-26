///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeSingletonMKArray : DataInputOutputSerdeForgeSingleton
    {
        public DataInputOutputSerdeForgeSingletonMKArray(
            Type serdeClass,
            string simpleNameComponent) : base(serdeClass)
        {
            SimpleNameComponent = simpleNameComponent;
        }

        public string SimpleNameComponent { get; }
    }
} // end of namespace