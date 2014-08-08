namespace Dynamo.UI.Models
{
    public class NewCustomNodeEventArgs
    {
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        public NewCustomNodeEventArgs(string name, string category, string description = "")
        {
            Description = description;
            Category = category;
            Name = name;
        }
    }
}