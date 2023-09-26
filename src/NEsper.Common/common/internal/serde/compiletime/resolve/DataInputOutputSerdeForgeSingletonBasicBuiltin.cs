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
    public class DataInputOutputSerdeForgeSingletonBasicBuiltin : DataInputOutputSerdeForgeSingleton
    {
        private readonly Type basicBuiltin;

        public DataInputOutputSerdeForgeSingletonBasicBuiltin(
            Type serdeClass,
            Type basicBuiltin) : base(serdeClass)
        {
            this.basicBuiltin = basicBuiltin;
        }

        public Type BasicBuiltin => basicBuiltin;

        public override string ToString()
        {
            return "DataInputOutputSerdeForgeSingletonBasicBuiltin{" +
                   "basicBuiltin=" +
                   basicBuiltin +
                   '}';
        }
    }
} // end of namespace