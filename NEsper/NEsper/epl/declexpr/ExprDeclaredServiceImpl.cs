///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.declexpr
{
    public class ExprDeclaredServiceImpl : ExprDeclaredService
    {
        private readonly ILockable _iLock;
        private readonly IDictionary<String, ExpressionDeclItem> _globalExpressions;
        private readonly IDictionary<String, List<ExpressionScriptProvided>> _globalScripts;

        public ExprDeclaredServiceImpl(ILockManager lockManager)
        {
            _iLock = lockManager.CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
            _globalExpressions = new Dictionary<String, ExpressionDeclItem>();
            _globalScripts = new Dictionary<String, List<ExpressionScriptProvided>>();
        }

        #region ExprDeclaredService Members

        public String AddExpressionOrScript(CreateExpressionDesc expressionDesc)
        {
            using (_iLock.Acquire())
            {
                if (expressionDesc.Expression != null)
                {
                    ExpressionDeclItem expression = expressionDesc.Expression;
                    String name = expression.Name;
                    if (_globalExpressions.ContainsKey(name))
                    {
                        throw new ExprValidationException("Expression '" + name + "' has already been declared");
                    }
                    _globalExpressions.Put(name, expression);
                    return name;
                }
                else
                {
                    ExpressionScriptProvided newScript = expressionDesc.Script;
                    String name = newScript.Name;

                    List<ExpressionScriptProvided> scripts = _globalScripts.Get(name);
                    if (scripts != null)
                    {
                        foreach (ExpressionScriptProvided script in scripts)
                        {
                            if (script.ParameterNames.Count == newScript.ParameterNames.Count)
                            {
                                throw new ExprValidationException(
                                    "Script '" + name +
                                    "' that takes the same number of parameters has already been declared");
                            }
                        }
                    }
                    else
                    {
                        scripts = new List<ExpressionScriptProvided>(2);
                        _globalScripts.Put(name, scripts);
                    }
                    scripts.Add(newScript);

                    return name;
                }
            }
        }

        public ExpressionDeclItem GetExpression(String name)
        {
            return _globalExpressions.Get(name);
        }

        public IList<ExpressionScriptProvided> GetScriptsByName(String name)
        {
            return _globalScripts.Get(name);
        }

        public void DestroyedExpression(CreateExpressionDesc expressionDesc)
        {
            using (_iLock.Acquire())
            {
                if (expressionDesc.Expression != null)
                {
                    _globalExpressions.Remove(expressionDesc.Expression.Name);
                }
                else
                {
                    _globalScripts.Remove(expressionDesc.Script.Name);
                }
            }
        }

        public void Dispose()
        {
            _globalExpressions.Clear();
        }

        #endregion
    }
}