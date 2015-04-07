///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Prepared statement implementation that stores the statement object model and a list
    /// of substitution parameters, to be mapped into an internal representation upon creation.
    /// </summary>
    [Serializable]
    public class EPPreparedStatementImpl : EPPreparedStatement
    {
        private readonly EPStatementObjectModel _model;
        private readonly IList<SubstitutionParameterExpressionBase> _subParams;
        private readonly string _optionalEPL;
        private bool _initialized;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="model">is the statement object model</param>
        /// <param name="subParams">is the substitution parameter list</param>
        /// <param name="optionalEPL">The optional epl.</param>
        public EPPreparedStatementImpl(EPStatementObjectModel model, IList<SubstitutionParameterExpressionBase> subParams, String optionalEPL)
        {
            _model = model;
            _subParams = subParams;
            _optionalEPL = optionalEPL;
        }

        public void SetObject(String parameterName, Object value)
        {
            ValidateNonEmpty();
            if (_subParams[0] is SubstitutionParameterExpressionIndexed) {
                throw new ArgumentException("Substitution parameters are unnamed, please use setObject(index,...) instead");
            }
            var found = false;
            foreach (SubstitutionParameterExpressionBase subs in _subParams) {
                if (((SubstitutionParameterExpressionNamed) subs).Name == parameterName) {
                    found = true;
                    subs.Constant = value;
                }
            }
            if (!found) {
                throw new ArgumentException("Invalid substitution parameter name of '" + parameterName + "' supplied, failed to find the name");
            }
        }

        public void SetObject(int parameterIndex, Object value)
        {
            ValidateNonEmpty();
            if (_subParams[0] is SubstitutionParameterExpressionNamed) {
                throw new ArgumentException("Substitution parameters are named, please use setObject(name,...) instead");
            }
            if (parameterIndex < 1) {
                throw new ArgumentException("Substitution parameter index starts at 1");
            }
            var found = false;
            foreach (SubstitutionParameterExpressionBase subs in _subParams) {
                if (((SubstitutionParameterExpressionIndexed) subs).Index == parameterIndex) {
                    found = true;
                    subs.Constant = value;
                }
            }
            if (!found) {
                throw new ArgumentException("Invalid substitution parameter index of " + parameterIndex + " supplied, the maximum for this statement is " + _subParams.Count);
            }
        }

        /// <summary>
        /// Gets the optional epl.
        /// </summary>
        /// <value>
        /// The optional epl.
        /// </value>
        public string OptionalEPL
        {
            get { return _optionalEPL; }
        }

        /// <summary>Returns the statement object model for the prepared statement </summary>
        /// <value>object model</value>
        public EPStatementObjectModel Model
        {
            get { return _model; }
        }

        /// <summary>Returns the substitution parameters. </summary>
        public IList<SubstitutionParameterExpressionBase> SubParams
        {
            get { return _subParams; }
        }

        private void ValidateNonEmpty()
        {
            if (_subParams.Count == 0)
            {
                throw new ArgumentException("Statement does not have substitution parameters indicated by the '?' character");
            }
        }
    }
}
