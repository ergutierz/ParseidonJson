using Newtonsoft.Json.Linq;

namespace ParseidonJson.editor;

public interface IJsonEditor
{
    void LoadJson(string json);
    JToken QueryJson(string query);
    void UpdateJson(string path, JToken newValue);
    string SaveJson();
}
