///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.core
{
    /// <summary>
    /// Simple implementation of the directory.
    /// </summary>

	public class SimpleServiceDirectory : Directory
	{
		private IDictionary<string,object> m_dataTable ;
        private ILockable m_dataLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleServiceDirectory"/> class.
        /// </summary>
		public SimpleServiceDirectory()
		{
			m_dataTable = new Dictionary<string,object>() ;
            m_dataLock = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		}

        /// <summary>
        /// Lookup an object by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public object Lookup(string name)
		{
			using(m_dataLock.Acquire())
			{
				return m_dataTable.Get( name ) ;
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
			using(m_dataLock.Acquire())
			{
#if true
			    m_dataTable[name] = obj;
#else
                if ( m_dataTable.ContainsKey( name ) )
				{
					throw new DirectoryException( "Value '" + name + "' was already bound" ) ;
				}
				
				m_dataTable[name] = obj ;
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
			using(m_dataLock.Acquire())
			{
				m_dataTable[name] = obj;
			}
		}

        /// <summary>
        /// Unbind the object at the given name.
        /// </summary>
        /// <param name="name"></param>
		public void Unbind(string name)
		{
			using(m_dataLock.Acquire())
			{
				m_dataTable.Remove( name ) ;
			}
		}

        /// <summary>
        /// Rename the object at oldName with newName.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
		public void Rename(string oldName, string newName)
		{
			using(m_dataLock.Acquire())
			{
				object tempObj = m_dataTable.Get( oldName );
				if ( tempObj == null )
				{
					throw new DirectoryException( "Value '" + oldName + "' was not found" ) ;
				}
				
				if ( m_dataTable.ContainsKey( newName ) )
				{
					throw new DirectoryException( "Value '" + newName + "' was already bound" ) ;
				}
				
				m_dataTable.Remove(oldName) ;
				m_dataTable[newName] = tempObj;				
			}
		}

        /// <summary>
        /// Enumerates the names bound in the named context.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public IEnumerator<string> List(string name)
		{
		    return m_dataTable.Keys.GetEnumerator();
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
		public void Dispose()
		{
			m_dataTable = null ;
		}
	}
}
