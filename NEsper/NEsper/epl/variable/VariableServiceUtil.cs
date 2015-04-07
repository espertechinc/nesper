///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        public static String GetAssigmentExMessage(String variableName, Type variableType, Type initValueClass) {
            return "Variable '" + variableName
                    + "' of declared type " + variableType.GetTypeNameFullyQualPretty() +
                    " cannot be assigned a value of type " + initValueClass.GetTypeNameFullyQualPretty();
        }
    
        public static String CheckVariableContextName(String optionalStatementContextName, VariableMetaData variableMetaData) {
            if (optionalStatementContextName == null) {
                if (variableMetaData.ContextPartitionName != null) {
                    return "Variable '" + variableMetaData.VariableName + "' defined for use with context '" + variableMetaData.ContextPartitionName + "' can only be accessed within that context";
                }
            }
            else {
                if (variableMetaData.ContextPartitionName != null &&
                    !variableMetaData.ContextPartitionName.Equals(optionalStatementContextName)) {
                    return "Variable '" + variableMetaData.VariableName + "' defined for use with context '" + variableMetaData.ContextPartitionName + "' is not available for use with context '" + optionalStatementContextName + "'";
                }
            }
            return null;
        }
    
        public static String CheckVariableContextName(ContextDescriptor contextDescriptor, VariableMetaData variableMetaData) {
            if (contextDescriptor == null) {
                return CheckVariableContextName((String) null, variableMetaData);
            }
            return CheckVariableContextName(contextDescriptor.ContextName, variableMetaData);
        }

        public static void CheckAlreadyDeclaredVariable(String variableName, VariableService variableService) {
            if (variableService.GetVariableMetaData(variableName) != null) {
                throw new ExprValidationException(GetAlreadyDeclaredEx(variableName));
            }
        }

        public static void CheckAlreadyDeclaredTable(String variableName, TableService tableService) {
            if (tableService.GetTableMetadata(variableName) != null) {
                throw new ExprValidationException(GetAlreadyDeclaredEx(variableName));
            }
        }

        public static String GetAlreadyDeclaredEx(String variableName) {
            return "Variable by name '" + variableName + "' has already been created";
        }
    }
}
