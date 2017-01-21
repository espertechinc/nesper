///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableExprEvaluatorBase {
    
        protected readonly ExprNode exprNode;
        protected readonly string tableName;
        protected readonly string subpropName;
        protected readonly int streamNum;
        protected readonly Type returnType;
    
        public ExprTableExprEvaluatorBase(ExprNode exprNode, string tableName, string subpropName, int streamNum, Type returnType) {
            this.exprNode = exprNode;
            this.tableName = tableName;
            this.subpropName = subpropName;
            this.streamNum = streamNum;
            this.returnType = returnType;
        }
    }
}
