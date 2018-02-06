using System;

namespace NEsper.Scripting.ClearScript
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ExposeToClearScriptAttributeAttribute : Attribute
    {
        public string Name { get; set; }

        public ExposeToClearScriptAttributeAttribute(string name = null)
        {
            this.Name = name;
        }
    }
}
