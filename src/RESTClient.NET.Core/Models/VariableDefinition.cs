namespace RESTClient.NET.Core.Models
{
    /// <summary>
    /// Represents a variable definition in an HTTP file
    /// </summary>
    public class VariableDefinition
    {
        /// <summary>
        /// Gets or sets the variable name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the variable value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the variable type
        /// </summary>
        public VariableType Type { get; set; }

        /// <summary>
        /// Gets or sets the line number where this variable is defined
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Initializes a new instance of the VariableDefinition class
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="value">The variable value</param>
        /// <param name="type">The variable type</param>
        public VariableDefinition(string name, string value, VariableType type = VariableType.File)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} = {Value} ({Type})";
        }
    }

    /// <summary>
    /// Defines the types of variables in HTTP files
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// File-level variable defined in the HTTP file
        /// </summary>
        File,

        /// <summary>
        /// Environment variable
        /// </summary>
        Environment,

        /// <summary>
        /// System-generated variable
        /// </summary>
        System,

        /// <summary>
        /// Variable referencing another request's response
        /// </summary>
        Request
    }
}
