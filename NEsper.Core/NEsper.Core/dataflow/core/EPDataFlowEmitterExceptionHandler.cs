///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.core
{
    public class EPDataFlowEmitterExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EPDataFlowEmitterExceptionHandler(String engineURI,
                                                 String statementName,
                                                 bool audit,
                                                 String dataFlowName,
                                                 String operatorName,
                                                 int operatorNumber,
                                                 String operatorPrettyPrint,
                                                 EPDataFlowExceptionHandler optionalExceptionHandler)
        {
            EngineURI = engineURI;
            StatementName = statementName;
            Audit = audit;
            DataFlowName = dataFlowName;
            OperatorName = operatorName;
            OperatorNumber = operatorNumber;
            OperatorPrettyPrint = operatorPrettyPrint;
            OptionalExceptionHandler = optionalExceptionHandler;
        }

        public void HandleException(Object targetObject, FastMethod fastMethod, Exception ex, Object[] parameters)
        {
            Log.Error("Exception encountered: " + ex.Message, ex);

            if (OptionalExceptionHandler != null)
            {
                OptionalExceptionHandler.Handle(
                    new EPDataFlowExceptionContext(
                        DataFlowName, OperatorName, OperatorNumber, OperatorPrettyPrint, ex));
            }
        }

        public bool Audit { get; private set; }

        public string EngineURI { get; private set; }

        public string StatementName { get; private set; }

        public string DataFlowName { get; private set; }

        public string OperatorName { get; private set; }

        public int OperatorNumber { get; private set; }

        public string OperatorPrettyPrint { get; private set; }

        public EPDataFlowExceptionHandler OptionalExceptionHandler { get; private set; }

        public void HandleAudit(Object targetObject, Object[] parameters)
        {
            if (Audit)
            {
                AuditPath.AuditLog(
                    EngineURI, StatementName, AuditEnum.DATAFLOW_OP,
                    "dataflow " + DataFlowName + " operator " + OperatorName + "(" + OperatorNumber + ") parameters " +
                    parameters.Render());
            }
        }
    }
}