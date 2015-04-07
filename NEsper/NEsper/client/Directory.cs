///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
	public interface Directory : IDisposable
	{
		/// <summary>
		/// Lookup an object by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		
		Object Lookup(string name) ;

		/// <summary>
		/// Bind an object to a name.  Throws an exception if
		/// the name is already bound.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		
		void Bind(string name, Object obj);
		
		/// <summary>
		/// Bind an object to a name.  If the object is already
		/// bound, rebind it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		
		void Rebind(string name, Object obj);
		
		/// <summary>
		/// Unbind the object at the given name.
		/// </summary>
		/// <param name="name"></param>
		
		void Unbind(string name) ;

		/// <summary>
		/// Rename the object at oldName with newName.
		/// </summary>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		
	    void Rename(String oldName, String newName);
	    
	    /// <summary>
	    /// Enumerates the names bound in the named context.
	    /// </summary>
	    /// <param name="name"></param>
	    /// <returns></returns>
	    
	    IEnumerator<string> List(string name);
	}
}
