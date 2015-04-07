///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableColumnDescTyped : TableColumnDesc
    {
        public TableColumnDescTyped(int positionInDeclaration, string columnName, object unresolvedType, bool key)
            : base(positionInDeclaration, columnName)
        {
            UnresolvedType = unresolvedType;
            IsKey = key;
        }

        public object UnresolvedType { get; private set; }

        public bool IsKey { get; private set; }
    }
}
