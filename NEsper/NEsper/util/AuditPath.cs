///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Global bool for enabling and disable audit path reporting.
    /// </summary>
    public class AuditPath
    {
        /// <summary>Log destination for the query plan logging. </summary>
        public const string QUERYPLAN_LOG = "com.espertech.esper.queryplan"; 
    
        /// <summary>Log destination for the audit logging. </summary>
        public const string AUDIT_LOG = "com.espertech.esper.audit";
    
        /// <summary>Public access. </summary>
        public static bool IsAuditEnabled = false;

        private static readonly ILog AuditLogDestination;

        static AuditPath()
        {
            AuditLogDestination = LogManager.GetLogger(AUDIT_LOG);
        }
        
        private static String _auditPattern;

        public static string AuditPattern
        {
            set { _auditPattern = value; }
        }

        public static void AuditInsertInto(String engineURI, String statementName, EventBean theEvent)
        {
            AuditLog(engineURI, statementName, AuditEnum.INSERT, EventBeanUtility.Summarize(theEvent));
        }

        public static void AuditContextPartition(String engineURI, String statementName, bool allocate, int agentInstanceId)
        {
            var writer = new StringWriter();
            writer.Write(allocate ? "Allocate" : "Dispose");
            writer.Write(" cpid ");
            writer.Write(agentInstanceId.ToString());
            AuditLog(engineURI, statementName, AuditEnum.CONTEXTPARTITION, writer.ToString());
        }

        public static void AuditLog(String engineURI, String statementName, AuditEnum category, String message)
        {
            if (_auditPattern == null)
            {
                String text = AuditContext.DefaultFormat(statementName, category, message);
                AuditLogDestination.Info(text);
            }
            else
            {
                String result = _auditPattern
                    .Replace("%s", statementName)
                    .Replace("%u", engineURI)
                    .Replace("%c", category.GetValue())
                    .Replace("%m", message);
                AuditLogDestination.Info(result);
            }

            if (AuditCallback != null)
            {
                AuditCallback.Invoke(new AuditContext(engineURI, statementName, category, message));
            }
        }

        public static bool IsInfoEnabled
        {
            get { return AuditLogDestination.IsInfoEnabled && AuditCallback != null; }
        }

        public static event AuditCallback AuditCallback;

        //public static AuditCallback AuditCallback
        //{
        //    set { _auditCallback = value; }
        //    get { return _auditCallback; }
        //}
    }
}
