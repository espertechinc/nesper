///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     A simple node for the creation of a tree, intended in this case to mirror an XML model.
    /// </summary>
    public class ElementPathNode
    {
        private IList<ElementPathNode> _children;

        public ElementPathNode(
            ElementPathNode parent,
            XmlQualifiedName name)
        {
            Name = name;
            Parent = parent;
        }

        public XmlQualifiedName Name { get; }

        public ElementPathNode Parent { get; }

        public ElementPathNode AddChild(XmlQualifiedName name)
        {
            if (_children == null) {
                _children = new List<ElementPathNode>();
            }

            var newChild = new ElementPathNode(this, name);
            _children.Add(newChild);
            return newChild;
        }

        public bool DoesNameAlreadyExistInHierarchy()
        {
            return DoesNameAlreadyExistInHierarchy(Name);
        }

        private bool DoesNameAlreadyExistInHierarchy(XmlQualifiedName nameToFind)
        {
            var doesNameAlreadyExistInHierarchy = false;
            if (Parent != null) {
                if (Parent.Name.Equals(nameToFind)) {
                    doesNameAlreadyExistInHierarchy = true;
                }
                else {
                    return Parent.DoesNameAlreadyExistInHierarchy(nameToFind);
                }
            }

            return doesNameAlreadyExistInHierarchy;
        }
    }
} // end of namespace