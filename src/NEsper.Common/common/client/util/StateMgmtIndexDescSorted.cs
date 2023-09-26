///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.client.util
{
    public class StateMgmtIndexDescSorted
    {
        private readonly string property;
        private readonly DataInputOutputSerdeForge serde;

        public StateMgmtIndexDescSorted(
            string property,
            DataInputOutputSerdeForge serde)
        {
            this.property = property;
            this.serde = serde;
        }

        public string Property => property;

        public DataInputOutputSerdeForge Serde => serde;
    }
} // end of namespace