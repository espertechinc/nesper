///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class CreateTableColumn
    {
        public CreateTableColumn(
            string columnName,
            ExprNode optExpression,
            ClassDescriptor optType,
            IList<AnnotationDesc> annotations,
            bool? primaryKey)
        {
            ColumnName = columnName;
            OptExpression = optExpression;
            OptType = optType;
            Annotations = annotations;
            PrimaryKey = primaryKey;
        }

        public ClassDescriptor OptType { get; set; }

        public string ColumnName { get; private set; }

        public ExprNode OptExpression { get; private set; }

        public IList<AnnotationDesc> Annotations { get; private set; }

        public bool? PrimaryKey { get; private set; }
    }
}