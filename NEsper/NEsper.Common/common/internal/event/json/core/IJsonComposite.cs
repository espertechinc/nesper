using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.@event.json.core
{
    public interface IJsonComposite
    {
        /// <summary>
        /// Returns the set of property names that are visible to the caller.
        /// </summary>
        public ICollection<string> PropertyNames { get; }
        
        /// <summary>
        /// Allows the caller to get or set a value in the composite structure.
        /// </summary>
        /// <param name="name"></param>
        public object this[string name] { get; set; }
    }
}