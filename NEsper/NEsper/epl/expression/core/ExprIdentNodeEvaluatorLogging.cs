///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprIdentNodeEvaluatorLogging : ExprIdentNodeEvaluatorImpl
    {
        private readonly String _engineURI;
        private readonly String _propertyName;
        private readonly String _statementName;
    
        public ExprIdentNodeEvaluatorLogging(int streamNum, EventPropertyGetter propertyGetter, Type propertyType, ExprIdentNode identNode, String propertyName, String statementName, String engineURI)
                    : base(streamNum, propertyGetter, propertyType, identNode)
        {
            _propertyName = propertyName;
            _statementName = statementName;
            _engineURI = engineURI;
        }

        public override object Evaluate(EvaluateParams evaluateParams)
        {
            Object result = base.Evaluate(evaluateParams);
            if (AuditPath.IsInfoEnabled) {
                AuditPath.AuditLog(_engineURI, _statementName, AuditEnum.PROPERTY, _propertyName + " value " + result);
            }
            return result;
        }
    }
}
