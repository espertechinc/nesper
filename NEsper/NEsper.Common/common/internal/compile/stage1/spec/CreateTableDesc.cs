///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Descriptor for create-table statements.
    /// </summary>
    [Serializable]
    public class CreateTableDesc
    {
        public CreateTableDesc(
            string tableName,
            IList<CreateTableColumn> columns)
        {
            TableName = tableName;
            Columns = columns;
        }

        public string TableName { get; private set; }

        public IList<CreateTableColumn> Columns { get; private set; }
    }
}