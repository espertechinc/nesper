///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.util
{
    public class EPLValidationUtil {
    
        public static void ValidateTableExists(TableService tableService, string name) {
            if (tableService.GetTableMetadata(name) != null) {
                throw new ExprValidationException("A table by name '" + name + "' already exists");
            }
        }
    
        public static void ValidateContextName(bool table, string tableOrNamedWindowName, string tableOrNamedWindowContextName, string optionalContextName, bool mustMatchContext)
                
        {
            if (tableOrNamedWindowContextName != null) {
                if (optionalContextName == null || !optionalContextName.Equals(tableOrNamedWindowContextName)) {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
            else {
                if (mustMatchContext && optionalContextName != null) {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
        }
    
        private static ExprValidationException GetCtxMessage(bool table, string tableOrNamedWindowName, string tableOrNamedWindowContextName) {
            string prefix = table ? "Table": "Named window";
            return new ExprValidationException(prefix + " by name '" + tableOrNamedWindowName + "' has been declared for context '" + tableOrNamedWindowContextName + "' and can only be used within the same context");
        }
    }
}
