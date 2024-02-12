using Newtonsoft.Json.Linq;

namespace ParseidonJson.editor;

public interface IJsonQuery
{
    void LoadJson(string json);
    JToken QueryJson(string query);
}
