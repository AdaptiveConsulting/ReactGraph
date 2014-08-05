using System.Collections.Generic;
using System.Text;

namespace ReactGraph.Internals.Visualisation
{
    public class VertexVisualProperties
    {
        readonly Dictionary<string, string> customProperties = new Dictionary<string, string>();

        public VertexVisualProperties(string id)
        {
            Id = id;
        }

        public string Id { get; set; }

        public string Label { get; set; }

        public string Color { get; set; }

        public void AddCustomProperty(string key, string value)
        {
            customProperties[key] = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("     {0} [label=\"{1}\"", Id, Label);

            if (!string.IsNullOrEmpty(Color))
            {
                sb.AppendFormat(", fillcolor=\"{0}\"", Color);
            }

            foreach (var customProperty in customProperties)
            {
                sb.AppendFormat(", {0}=\"{1}\"", customProperty.Key, customProperty.Value);
            }

            sb.Append("];");
            return sb.ToString();
        }
    }
}