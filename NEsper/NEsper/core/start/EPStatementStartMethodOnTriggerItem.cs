///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.property;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodOnTriggerItem
    {
        public EPStatementStartMethodOnTriggerItem(
            ExprNode whereClause,
            bool isNamedWindowInsert,
            string insertIntoTableNames,
            ResultSetProcessorFactoryDesc factoryDesc,
            PropertyEvaluator propertyEvaluator)
        {
            WhereClause = whereClause;
            IsNamedWindowInsert = isNamedWindowInsert;
            InsertIntoTableNames = insertIntoTableNames;
            FactoryDesc = factoryDesc;
            PropertyEvaluator = propertyEvaluator;
        }

        public ExprNode WhereClause { get; private set; }

        public bool IsNamedWindowInsert { get; private set; }

        public string InsertIntoTableNames { get; private set; }

        public ResultSetProcessorFactoryDesc FactoryDesc { get; private set; }

        public PropertyEvaluator PropertyEvaluator { get; private set; }
    }
} // end of namespace