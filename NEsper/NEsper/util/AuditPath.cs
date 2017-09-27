///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
        /// <summary>Logger destination for the query plan logging.</summary>
        public static readonly string QUERYPLAN_LOG = "com.espertech.esper.queryplan";

        /// <summary>Logger destination for the JDBC logging.</summary>
        public static readonly string JDBC_LOG = "com.espertech.esper.jdbc";

        /// <summary>Logger destination for the audit logging.</summary>
        public static readonly string AUDIT_LOG = "com.espertech.esper.audit";

        private static readonly ILog AUDIT_LOG_DESTINATION = LogManager.GetLogger(AuditPath.AUDIT_LOG);

        /// <summary>Public access.</summary>
        public static bool IsAuditEnabled = false;

        private static volatile AuditCallback _auditCallback;
        private static string _auditPattern;

        public static string AuditPattern
        {
            set { _auditPattern = value; }
        }

        public static void AuditInsertInto(string engineURI, string statementName, EventBean theEvent)
        {
            AuditLog(engineURI, statementName, AuditEnum.INSERT, EventBeanUtility.Summarize(theEvent));
        }

        public static void AuditContextPartition(
            string engineURI,
            string statementName,
            bool allocate,
            int agentInstanceId)
        {
            var writer = new StringWriter();
            writer.Write(allocate ? "Allocate" : "Destroy");
            writer.Write(" cpid ");
            writer.Write(agentInstanceId);
            AuditLog(engineURI, statementName, AuditEnum.CONTEXTPARTITION, writer.ToString());
        }

        public static void AuditLog(string engineURI, string statementName, AuditEnum category, string message)
        {
            if (_auditPattern == null)
            {
                string text = AuditContext.DefaultFormat(statementName, category, message);
                AUDIT_LOG_DESTINATION.Info(text);
            }
            else
            {
                string result =
                    _auditPattern.Replace("%s", statementName)
                        .Replace("%u", engineURI)
                        .Replace("%c", category.GetValue())
                        .Replace("%m", message);
                AUDIT_LOG_DESTINATION.Info(result);
            }
            if (_auditCallback != null)
            {
                _auditCallback.Invoke(new AuditContext(engineURI, statementName, category, message));
            }
        }

        public static bool IsInfoEnabled
        {
            get { return AUDIT_LOG_DESTINATION.IsInfoEnabled || _auditCallback != null; }
        }

        public static AuditCallback AuditCallback
        {
            get { return _auditCallback; }
            set { _auditCallback = value; }
        }
    }
} // end of namespace
