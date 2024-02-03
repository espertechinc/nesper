///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.serde.compiletime.resolve;


namespace com.espertech.esper.common.client.util
{
    public class StateMgmtIndexDescInMulti
    {
        public StateMgmtIndexDescInMulti(
            string[] indexedProps,
            DataInputOutputSerdeForge[] serdes)
        {
            IndexedProps = indexedProps;
            Serdes = serdes;
        }

        public string[] IndexedProps { get; }

        public DataInputOutputSerdeForge[] Serdes { get; }
    }
} // end of namespace