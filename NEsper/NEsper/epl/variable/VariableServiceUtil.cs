///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.variable
{
    public class VariableServiceUtil
    {
        public static string GetAssigmentExMessage(string variableName, Type variableType, Type initValueClass)
        {
            return string.Format(
                "Variable '{0}' of declared type {1} cannot be assigned a value of type {2}",
                variableName,
                variableType.GetCleanName(),
                initValueClass.GetCleanName());
        }

        public static string CheckVariableContextName(
            string optionalStatementContextName,
            VariableMetaData variableMetaData)
        {
            if (optionalStatementContextName == null)
            {
                if (variableMetaData.ContextPartitionName != null)
                {
                    return "Variable '" + variableMetaData.VariableName + "' defined for use with context '" +
                           variableMetaData.ContextPartitionName + "' can only be accessed within that context";
                }
            }
            else
            {
                if (variableMetaData.ContextPartitionName != null &&
                    !variableMetaData.ContextPartitionName.Equals(optionalStatementContextName))
                {
                    return "Variable '" + variableMetaData.VariableName + "' defined for use with context '" +
                           variableMetaData.ContextPartitionName + "' is not available for use with context '" +
                           optionalStatementContextName + "'";
                }
            }
            return null;
        }

        public static string CheckVariableContextName(
            ContextDescriptor contextDescriptor,
            VariableMetaData variableMetaData)
        {
            if (contextDescriptor == null)
            {
                return CheckVariableContextName((string) null, variableMetaData);
            }
            return CheckVariableContextName(contextDescriptor.ContextName, variableMetaData);
        }

        public static void CheckAlreadyDeclaredVariable(string variableName, VariableService variableService)

        {
            if (variableService.GetVariableMetaData(variableName) != null)
            {
                throw new ExprValidationException(GetAlreadyDeclaredEx(variableName, false));
            }
        }

        public static void CheckAlreadyDeclaredTable(string tableName, TableService tableService)

        {
            if (tableService.GetTableMetadata(tableName) != null)
            {
                throw new ExprValidationException(GetAlreadyDeclaredEx(tableName, true));
            }
        }

        public static string GetAlreadyDeclaredEx(string variableOrTableName, bool isTable)
        {
            if (isTable)
            {
                return "Table by name '" + variableOrTableName + "' has already been created";
            }
            else
            {
                return "Variable by name '" + variableOrTableName + "' has already been created";
            }
        }
    }
} // end of namespace
