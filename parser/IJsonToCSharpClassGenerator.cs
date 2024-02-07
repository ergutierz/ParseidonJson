namespace ParseidonJson.parser;

public interface IJsonToCSharpClassGenerator
{
    string GenerateCSharpClasses(string json);
    
    double LastOperationElapsedTimeMs { get; }
}
