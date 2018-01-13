using System.Collections.Generic;

namespace CodeBrix.Modularity
{
    public interface IModuleSettings
    {
        Dictionary<string, object> Settings { get; set; }
    }
}
