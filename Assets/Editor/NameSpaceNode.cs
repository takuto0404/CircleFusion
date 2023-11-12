using System.Collections.Generic;

namespace Plugins.Editor.MermaidMaker
{
    public class NameSpaceNode
    {
        public NameSpaceNode(int id,string nameSpaceName)
        {
            Id = id;
            NameSpaceName = nameSpaceName;
            Children = new List<NameSpaceNode>();
        }

        public readonly int Id;
        public readonly string NameSpaceName;
        public bool WasSelected;
        public readonly List<NameSpaceNode> Children;
    }
}