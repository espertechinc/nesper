///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public abstract class TableColumnDesc
    {
        protected TableColumnDesc(
            int positionInDeclaration,
            string columnName)
        {
            PositionInDeclaration = positionInDeclaration;
            ColumnName = columnName;
        }

        public string ColumnName { get; private set; }

        public int PositionInDeclaration { get; private set; }
    }
}