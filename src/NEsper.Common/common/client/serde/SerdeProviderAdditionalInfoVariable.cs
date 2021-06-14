///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
	/// <summary>
	///     Information about the variable for which to obtain a serde.
	/// </summary>
	public class SerdeProviderAdditionalInfoVariable : SerdeProviderAdditionalInfo
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="raw">statement information</param>
	    /// <param name="variableName">variable name</param>
	    public SerdeProviderAdditionalInfoVariable(
            StatementRawInfo raw,
            string variableName) : base(raw)
        {
            VariableName = variableName;
        }

	    /// <summary>
	    ///     Returns the variable name
	    /// </summary>
	    /// <value>name</value>
	    public string VariableName { get; }

        public override string ToString()
        {
            return "variable '" + VariableName + "'";
        }
    }
} // end of namespace