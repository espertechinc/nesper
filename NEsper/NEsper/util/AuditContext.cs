///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.client.annotation;

namespace com.espertech.esper.util
{
    public class AuditContext {
    
        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly AuditEnum _category;
        private readonly String _message;
    
        public AuditContext(String engineURI, String statementName, AuditEnum category, String message)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _category = category;
            _message = message;
        }

        public string EngineURI
        {
            get { return _engineURI; }
        }

        public string StatementName
        {
            get { return _statementName; }
        }

        public AuditEnum Category
        {
            get { return _category; }
        }

        public string Message
        {
            get { return _message; }
        }

        public String Format()
        {
            return DefaultFormat(_statementName, _category, _message);
        }
    
        public static String DefaultFormat(String statementName, AuditEnum category, String message)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("Statement ");
            buf.Append(statementName);
            buf.Append(" ");
            buf.Append(category.GetPrettyPrintText());
            buf.Append(" ");
            buf.Append(message);
            return buf.ToString();
        }
    }
}
