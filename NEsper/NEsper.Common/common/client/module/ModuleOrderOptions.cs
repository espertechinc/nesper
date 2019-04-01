///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.module
{
	/// <summary>
	/// Options class passed to #getModuleOrder(java.util.Collection, ModuleOrderOptions)} for controlling the behavior of ordering and dependency checking logic.
	/// </summary>
	[Serializable]
	public class ModuleOrderOptions 
	{

	    private bool checkCircularDependency = true;
	    private bool checkUses = true;

	    /// <summary>
	    /// Returns true (the default) to indicate that the algorithm checks for circular dependencies among the uses-dependency graph,
	    /// or false to not perform this check.
	    /// </summary>
	    /// <returns>indicator.</returns>
	    public bool IsCheckCircularDependency() {
	        return checkCircularDependency;
	    }

	    /// <summary>
	    /// Set this indicator to true (the default) to indicate that the algorithm checks for circular dependencies among the uses-dependency graph,
	    /// or false to not perform this check.
	    /// </summary>
	    /// <param name="checkCircularDependency">indicator.</param>
	    public void SetCheckCircularDependency(bool checkCircularDependency) {
	        this.checkCircularDependency = checkCircularDependency;
	    }

	    /// <summary>
	    /// Returns true (the default) to cause the algorithm to check uses-dependencies ensuring all dependencies are satisfied i.e.
	    /// all dependent modules are either deployed or are part of the modules passed in, or false to not perform the checking.
	    /// </summary>
	    /// <returns>indicator</returns>
	    public bool IsCheckUses() {
	        return checkUses;
	    }

	    /// <summary>
	    /// Set this indicator to true (the default) to cause the algorithm to check uses-dependencies ensuring all dependencies are satisfied i.e.
	    /// all dependent modules are either deployed or are part of the modules passed in, or false to not perform the checking.
	    /// </summary>
	    /// <param name="checkUses">indicator</param>
	    public void SetCheckUses(bool checkUses) {
	        this.checkUses = checkUses;
	    }
	}
} // end of namespace