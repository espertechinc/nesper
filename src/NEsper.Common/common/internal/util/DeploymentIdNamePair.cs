///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public class DeploymentIdNamePair
    {
        public DeploymentIdNamePair(
            string deploymentId,
            string name)
        {
            if (name == null) {
                throw new ArgumentException("Name is null");
            }

            DeploymentId = deploymentId;
            Name = name;
        }

        public string DeploymentId { get; }

        public string Name { get; }

        public override string ToString()
        {
            return "DeploymentIdNamePair{" +
                   "deploymentId='" +
                   DeploymentId +
                   '\'' +
                   ", name='" +
                   Name +
                   '\'' +
                   '}';
        }

        protected bool Equals(DeploymentIdNamePair other)
        {
            return string.Equals(DeploymentId, other.DeploymentId) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((DeploymentIdNamePair) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((DeploymentId != null ? DeploymentId.GetHashCode() : 0) * 397) ^
                       (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
} // end of namespace