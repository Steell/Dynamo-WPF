namespace Dynamo.UI.Models
{
    public abstract class WorkspaceState
    {
        public string Name { get; private set; }

        protected WorkspaceState(string name)
        {
            Name = name;
        }
    }

    public class CustomNodeWorkspaceState : WorkspaceState
    {
        public readonly string Category;
        public readonly string Description;

        public CustomNodeWorkspaceState(string name, string category, string description = "") : base(name)
        {
            Description = description;
            Category = category;
        }
    }

    public class HomeWorkspaceState : WorkspaceState
    {
        public HomeWorkspaceState(string name) : base(name) { }
    }
}