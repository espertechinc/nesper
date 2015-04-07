///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view.stat
{
    public class StatViewAdditionalProps
    {
        private readonly String[] _additionalProps;
        private readonly ExprEvaluator[] _additionalExpr;
    
        private StatViewAdditionalProps(String[] additionalProps, ExprEvaluator[] additionalExpr)
        {
            _additionalProps = additionalProps;
            _additionalExpr = additionalExpr;
        }

        public string[] AdditionalProps
        {
            get { return _additionalProps; }
        }

        public ExprEvaluator[] AdditionalExpr
        {
            get { return _additionalExpr; }
        }

        public static StatViewAdditionalProps Make(ExprNode[] validated, int startIndex, EventType parentEventType)
        {
            if (validated.Length <= startIndex)
            {
                return null;
            }
    
            IList<String> additionalProps = new List<String>();
            IList<ExprEvaluator> lastValueExpr = new List<ExprEvaluator>();
            var copyAllProperties = false;
    
            for (var i = startIndex; i < validated.Length; i++)
            {
                if (validated[i] is ExprWildcard)
                {
                    copyAllProperties = true;
                }

                additionalProps.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validated[i]));
                lastValueExpr.Add(validated[i].ExprEvaluator);
            }
    
            if (copyAllProperties) {
                foreach (var propertyDescriptor in parentEventType.PropertyDescriptors) {
                    if (propertyDescriptor.IsFragment) {
                        continue;
                    }
                    additionalProps.Add(propertyDescriptor.PropertyName);
                    var getter = parentEventType.GetGetter(propertyDescriptor.PropertyName);
                    var type = propertyDescriptor.PropertyType;
                    ExprEvaluator exprEvaluator = new ProxyExprEvaluator {
                        ProcEvaluate = evaluateParams => getter.Get(evaluateParams.EventsPerStream[0]),
                        ReturnType = type
                    };
                    lastValueExpr.Add(exprEvaluator);
                }
            }
    
            var addPropsArr = additionalProps.ToArray();
            var valueExprArr = lastValueExpr.ToArray();
            return new StatViewAdditionalProps(addPropsArr, valueExprArr);
        }
    
        public void AddProperties(IDictionary<String, Object> newDataMap, Object[] lastValuesEventNew)
        {
            if (lastValuesEventNew != null) {
                for (var i = 0; i < _additionalProps.Length; i++) {
                    newDataMap.Put(_additionalProps[i], lastValuesEventNew[i]);
                }
            }
        }
    
        public static void AddCheckDupProperties(IDictionary<String, Object> target, StatViewAdditionalProps addProps, params ViewFieldEnum[] builtin)
        {
            if (addProps == null)
            {
                return;
            }
    
            for (var i = 0; i < addProps.AdditionalProps.Length; i++) {
                var name = addProps.AdditionalProps[i];
                for (var j = 0; j < builtin.Length; j++) {
                    if (string.Equals(name, builtin[j].GetName(), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("The property by name '" + name + "' overlaps the property name that the view provides");
                    }
                }
                target.Put(name, addProps.AdditionalExpr[i].ReturnType);
            }
        }
    }
}
