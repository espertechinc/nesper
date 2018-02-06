///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedGetterCRC32Serialized : EventPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _statementName;
        private readonly ExprEvaluator[] _evaluators;
        private readonly Serializer[] _serializers;
        private readonly int _granularity;

        public ContextControllerHashedGetterCRC32Serialized(String statementName, IList<ExprNode> nodes, int granularity)
        {
            _statementName = statementName;
            _evaluators = new ExprEvaluator[nodes.Count];

            var returnTypes = new Type[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                _evaluators[i] = nodes[i].ExprEvaluator;
                returnTypes[i] = _evaluators[i].ReturnType;
            }

            _serializers = SerializerFactory.GetSerializers(returnTypes);
            _granularity = granularity;
        }

        public Object Get(EventBean eventBean)
        {
            var events = new[] { eventBean };

            var @params = new Object[_evaluators.Length];
            for (int i = 0; i < _serializers.Length; i++)
            {
                @params[i] = _evaluators[i].Evaluate(new EvaluateParams(events, true, null));
            }

            byte[] bytes;
            try
            {
                bytes = SerializerFactory.Serialize(_serializers, @params);
            }
            catch (IOException e)
            {
                Log.Error("Exception serializing parameters for computing consistent hash for statement '" + _statementName + "': " + e.Message, e);
                bytes = new byte[0];
            }

            var value = (int) (bytes.GetCrc32() % _granularity);
            var result = value;
            if (result >= 0)
            {
                return result;
            }

            return -result;
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
