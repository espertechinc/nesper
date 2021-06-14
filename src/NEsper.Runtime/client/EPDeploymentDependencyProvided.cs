///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client.util;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	/// Provides information about EPL objects that a deployment provides to other deployments.
	/// </summary>
	public class EPDeploymentDependencyProvided
	{
	    private readonly ICollection<Item> _dependencies;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="dependencies">provision dependencies</param>
	    public EPDeploymentDependencyProvided(ICollection<Item> dependencies) {
	        _dependencies = dependencies;
	    }

	    /// <summary>
	    /// Returns the provision dependencies
	    /// </summary>
	    /// <value>items</value>
	    public ICollection<Item> Dependencies => _dependencies;

	    /// <summary>
	    /// Information about EPL Objects provided by the deployment
	    /// </summary>
	    public class Item
	    {
		    private readonly EPObjectType _objectType;
		    private readonly string _objectName;
		    private readonly ISet<string> _deploymentIds;

		    /// <summary>
		    /// Ctor.
		    /// </summary>
		    /// <param name="objectType">EPL object type</param>
		    /// <param name="objectName">EPL object name</param>
		    /// <param name="deploymentIds">deployment ids of consumers</param>
		    public Item(
			    EPObjectType objectType,
			    string objectName,
			    ISet<string> deploymentIds)
		    {
			    _objectType = objectType;
			    _objectName = objectName;
			    _deploymentIds = deploymentIds;
		    }

		    /// <summary>
		    /// Returns the EPL object type
		    /// </summary>
		    /// <value>object type</value>
		    public EPObjectType ObjectType => _objectType;

		    /// <summary>
		    /// Returns the EPL object name.
		    /// For scripts the object name is formatted as the script name followed by hash(#) and followed by the number of parameters.
		    /// For indexes the object name is formatted as "IndexName on named-window WindowName" or "IndexName on table TableName".
		    /// </summary>
		    /// <value>object name</value>
		    public string ObjectName => _objectName;

		    /// <summary>
		    /// Returns the deployment id of consuming deployments
		    /// </summary>
		    /// <value>deployment ids</value>
		    public ISet<string> DeploymentIds => _deploymentIds;

		    protected bool Equals(Item other)
		    {
			    return _objectType == other._objectType && 
			           _objectName == other._objectName &&
			           _deploymentIds.SetEquals(other._deploymentIds);
		    }

		    public override bool Equals(object obj)
		    {
			    if (ReferenceEquals(null, obj)) {
				    return false;
			    }

			    if (ReferenceEquals(this, obj)) {
				    return true;
			    }

			    if (obj.GetType() != this.GetType()) {
				    return false;
			    }

			    return Equals((Item) obj);
		    }

		    public override int GetHashCode()
		    {
			    unchecked {
				    var hashCode = (int) _objectType;
				    hashCode = (hashCode * 397) ^ (_objectName != null ? _objectName.GetHashCode() : 0);
				    hashCode = (hashCode * 397) ^ (_deploymentIds != null ? _deploymentIds.GetHashCode() : 0);
				    return hashCode;
			    }
		    }

		    public override string ToString()
		    {
			    return "Item{" +
			           "objectType=" +
			           _objectType +
			           ", objectName='" +
			           _objectName +
			           '\'' +
			           ", deploymentIds=" +
			           _deploymentIds +
			           '}';
		    }
	    }
	}
} // end of namespace
