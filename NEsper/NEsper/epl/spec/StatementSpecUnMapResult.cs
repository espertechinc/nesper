///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.soda;

namespace com.espertech.esper.epl.spec
{
	/// <summary>
	/// Return result for unmap operators unmapping an intermal statement representation to the SODA object model.
	/// </summary>
	public class StatementSpecUnMapResult
	{
	    private readonly EPStatementObjectModel _objectModel;
        private readonly IList<SubstitutionParameterExpressionBase> _substitutionParams;

	    /// <summary>Ctor.</summary>
	    /// <param name="objectModel">of the statement</param>
	    /// <param name="substitutionParams">a map of parameter index and parameter</param>
        public StatementSpecUnMapResult(EPStatementObjectModel objectModel, IList<SubstitutionParameterExpressionBase> substitutionParams)
	    {
	        _objectModel = objectModel;
	        _substitutionParams = substitutionParams;
	    }

	    /// <summary>Returns the object model.</summary>
	    /// <returns>object model</returns>
	    public EPStatementObjectModel ObjectModel
	    {
	    	get { return _objectModel; }
	    }

	    /// <summary>
	    /// Returns the substitution paremeters keyed by the parameter's index.
	    /// </summary>
	    /// <returns>map of index and parameter</returns>
        public IList<SubstitutionParameterExpressionBase> SubstitutionParams
	    {
	    	get { return _substitutionParams; }
	    }
	}
} // End of namespace
