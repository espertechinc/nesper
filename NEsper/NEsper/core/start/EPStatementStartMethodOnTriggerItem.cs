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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.property;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodOnTriggerItem {
        private readonly ExprNode whereClause;
        private readonly bool isNamedWindowInsert;
        private readonly string insertIntoTableNames;
        private readonly ResultSetProcessorFactoryDesc factoryDesc;
        private readonly PropertyEvaluator propertyEvaluator;
    
        public EPStatementStartMethodOnTriggerItem(ExprNode whereClause, bool isNamedWindowInsert, string insertIntoTableNames, ResultSetProcessorFactoryDesc factoryDesc, PropertyEvaluator propertyEvaluator) {
            this.whereClause = whereClause;
            this.isNamedWindowInsert = isNamedWindowInsert;
            this.insertIntoTableNames = insertIntoTableNames;
            this.factoryDesc = factoryDesc;
            this.propertyEvaluator = propertyEvaluator;
        }
    
        public ExprNode GetWhereClause() {
            return whereClause;
        }
    
        public bool IsNamedWindowInsert() {
            return isNamedWindowInsert;
        }
    
        public string GetInsertIntoTableNames() {
            return insertIntoTableNames;
        }
    
        public ResultSetProcessorFactoryDesc GetFactoryDesc() {
            return factoryDesc;
        }
    
        public PropertyEvaluator GetPropertyEvaluator() {
            return propertyEvaluator;
        }
    }
} // end of namespace
