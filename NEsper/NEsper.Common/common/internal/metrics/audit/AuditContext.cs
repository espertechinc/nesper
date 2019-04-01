///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.common.client.annotation;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public class AuditContext
    {
        public AuditContext(string engineURI, string statementName, AuditEnum category, string message)
        {
            EngineURI = engineURI;
            StatementName = statementName;
            Category = category;
            Message = message;
        }

        public string EngineURI { get; }

        public string StatementName { get; }

        public AuditEnum Category { get; }

        public string Message { get; }

        public string Format()
        {
            return DefaultFormat(StatementName, Category, Message);
        }

        public static string DefaultFormat(string statementName, AuditEnum category, string message)
        {
            var buf = new StringBuilder();
            buf.Append("Statement ");
            buf.Append(statementName);
            buf.Append(" ");
            buf.Append(category.PrettyPrintText);
            buf.Append(" ");
            buf.Append(message);
            return buf.ToString();
        }
    }
}