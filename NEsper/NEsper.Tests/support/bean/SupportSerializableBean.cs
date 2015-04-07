///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.support.bean
{
	[Serializable]
	public class SupportSerializableBean
	{
	    private String id;

	    public SupportSerializableBean(String id)
	    {
	        this.id = id;
	    }

	    public String Id
	    {
	    	get { return id; }
	    }

	    public override bool Equals(Object obj)
	    {
	        if (!(obj is SupportSerializableBean))
	        {
	            return false;
	        }
	        SupportSerializableBean other = (SupportSerializableBean) obj;
	        return other.id.Equals(id);
	    }

	    public override int GetHashCode()
	    {
	        return id.GetHashCode();
	    }

	    public override String ToString()
	    {
	        return this.GetType().FullName + " id=" + id;
	    }
	}
} // End of namespace
