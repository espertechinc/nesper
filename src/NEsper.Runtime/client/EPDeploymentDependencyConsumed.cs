///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
	/// Provides information about EPL objects that a deployment consumes (requires, depends on, refers to) from other deployments.
	/// </summary>
	public class EPDeploymentDependencyConsumed
	{
		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="dependencies">consumptions</param>
		public EPDeploymentDependencyConsumed(ICollection<Item> dependencies)
		{
			Dependencies = dependencies;
		}

		/// <summary>
		/// Returns the consumption dependencies
		/// </summary>
		/// <value>items</value>
		public ICollection<Item> Dependencies { get; }

		/// <summary>
		/// Information about EPL objects consumed by another deployment.
		/// </summary>
		public class Item
		{
			private readonly string _deploymentId;
			private readonly EPObjectType _objectType;
			private readonly string _objectName;

			/// <summary>
			/// Ctor.
			/// </summary>
			/// <param name="deploymentId">deployment id of the provider</param>
			/// <param name="objectType">EPL object type</param>
			/// <param name="objectName">EPL object name</param>
			public Item(
				string deploymentId,
				EPObjectType objectType,
				string objectName)
			{
				_deploymentId = deploymentId;
				_objectType = objectType;
				_objectName = objectName;
			}

			/// <summary>
			/// Returns the deployment id of the provider
			/// </summary>
			/// <value>deployment id</value>
			public string DeploymentId => _deploymentId;

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

			protected bool Equals(Item other)
			{
				return _deploymentId == other._deploymentId && _objectType == other._objectType && _objectName == other._objectName;
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
					var hashCode = (_deploymentId != null ? _deploymentId.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (int) _objectType;
					hashCode = (hashCode * 397) ^ (_objectName != null ? _objectName.GetHashCode() : 0);
					return hashCode;
				}
			}

			public override string ToString()
			{
				return "Item{" +
				       "deploymentId='" +
				       _deploymentId +
				       '\'' +
				       ", objectType=" +
				       _objectType +
				       ", objectName='" +
				       _objectName +
				       '\'' +
				       '}';
			}
		}
	}
} // end of namespace
