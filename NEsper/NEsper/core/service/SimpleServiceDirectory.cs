///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.core
{
    /// <summary>
    /// Simple implementation of the directory.
    /// </summary>

	public class SimpleServiceDirectory : Directory
	{
		private IDictionary<string,object> _dataTable ;
        private readonly ILockable _dataLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleServiceDirectory"/> class.
        /// </summary>
		public SimpleServiceDirectory(ILockManager lockManager)
		{
			_dataTable = new Dictionary<string,object>() ;
            _dataLock = lockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		}

        /// <summary>
        /// Lookup an object by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public object Lookup(string name)
		{
			using(_dataLock.Acquire())
			{
				return _dataTable.Get( name ) ;
			}
		}

        /// <summary>
        /// Bind an object to a name.  Throws an exception if
        /// the name is already bound.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
		public void Bind(string name, object obj)
		{
			using(_dataLock.Acquire())
			{
#if true
			    _dataTable[name] = obj;
#else
                if ( _dataTable.ContainsKey( name ) )
				{
					throw new DirectoryException( "Value '" + name + "' was already bound" ) ;
				}
				
				_dataTable[name] = obj ;
#endif
			}
		}

        /// <summary>
        /// Bind an object to a name.  If the object is already
        /// bound, rebind it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
		public void Rebind(string name, object obj)
		{
			using(_dataLock.Acquire())
			{
				_dataTable[name] = obj;
			}
		}

        /// <summary>
        /// Unbind the object at the given name.
        /// </summary>
        /// <param name="name"></param>
		public void Unbind(string name)
		{
			using(_dataLock.Acquire())
			{
				_dataTable.Remove( name ) ;
			}
		}

        /// <summary>
        /// Rename the object at oldName with newName.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
		public void Rename(string oldName, string newName)
		{
			using(_dataLock.Acquire())
			{
				object tempObj = _dataTable.Get( oldName );
				if ( tempObj == null )
				{
					throw new DirectoryException( "Value '" + oldName + "' was not found" ) ;
				}
				
				if ( _dataTable.ContainsKey( newName ) )
				{
					throw new DirectoryException( "Value '" + newName + "' was already bound" ) ;
				}
				
				_dataTable.Remove(oldName) ;
				_dataTable[newName] = tempObj;				
			}
		}

        /// <summary>
        /// Enumerates the names bound in the named context.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public IEnumerator<string> List(string name)
		{
		    return _dataTable.Keys.GetEnumerator();
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
		public void Dispose()
		{
			_dataTable = null ;
		}
	}
}
