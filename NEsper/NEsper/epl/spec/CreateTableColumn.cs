///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class CreateTableColumn : MetaDefItem
    {
        public CreateTableColumn(string columnName, ExprNode optExpression, string optTypeName, bool? optTypeIsArray, bool? optTypeIsPrimitiveArray, IList<AnnotationDesc> annotations, bool? primaryKey)
        {
            ColumnName = columnName;
            OptExpression = optExpression;
            OptTypeName = optTypeName;
            OptTypeIsArray = optTypeIsArray;
            OptTypeIsPrimitiveArray = optTypeIsPrimitiveArray;
            Annotations = annotations;
            PrimaryKey = primaryKey;
        }

        public string ColumnName { get; private set; }

        public ExprNode OptExpression { get; private set; }

        public string OptTypeName { get; private set; }

        public bool? OptTypeIsArray { get; private set; }

        public IList<AnnotationDesc> Annotations { get; private set; }

        public bool? PrimaryKey { get; private set; }

        public bool? OptTypeIsPrimitiveArray { get; private set; }
    }
}
