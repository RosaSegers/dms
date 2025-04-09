namespace Document.Api.Domain.Entities
{
    public class Document
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Document() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public Document(string name, string description, string file)
        {
            this.Id = Guid.NewGuid();
            Name = name;
            Description = description;
            File = file;
        }

        public Document(Guid id, string name, string description, string file)
        {
            Id = id;
            Name = name;
            Description = description;
            File = file;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string File { get; set; }

    }
}
