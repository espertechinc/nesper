///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Hold a raw SQL-statements parameter information that were specified in the form ${name}.
    /// </summary>

    public class SQLParameterDesc
	{
	    private readonly IList<String> parameters;
	    private readonly IList<String> builtinIdentifiers;

	    /// <summary>Ctor.</summary>
	    /// <param name="parameters">is the name of parameters</param>
	    /// <param name="builtinIdentifiers">is the names of built-in predefined values</param>
	    public SQLParameterDesc(IList<String> parameters, IList<String> builtinIdentifiers)
	    {
	        this.parameters = parameters;
	        this.builtinIdentifiers = builtinIdentifiers;
	    }

	    /// <summary>Returns parameter names.</summary>
	    /// <returns>parameter names</returns>
	    public IList<String> Parameters
	    {
	        get {return parameters;}
	    }

	    /// <summary>Returns built-in identifiers.</summary>
	    /// <returns>built-in identifiers</returns>
	    public IList<String> BuiltinIdentifiers
	    {
	        get {return builtinIdentifiers;}
	    }

	    public override String ToString()
	    {
	        return "params=" + parameters.Render() +
	               " builtin=" + builtinIdentifiers.Render();
	    }
	}
} // End of namespace
