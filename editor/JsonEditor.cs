namespace ParseidonJson.editor;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class JsonEditor : IJsonEditor
{
    private JToken _jsonToken;

    public void LoadJson(string json)
    {
        try
        {
            _jsonToken = JToken.Parse(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to load JSON.", ex);
        }
    }

    // Enhanced to support queries returning multiple tokens
    public JToken QueryJson(string query)
    {
        try
        {
            // Use SelectTokens for multiple matches and encapsulate them in a JArray if multiple results are found
            var tokens = _jsonToken.SelectTokens(query);
            if (tokens.Any())
            {
                // If only one token, return it directly; otherwise, return a JArray of all tokens
                return tokens.Count() == 1 ? tokens.First() : new JArray(tokens);
            }
            else
            {
                return null; // or JValue.CreateNull() to explicitly return a null JSON value
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Query failed: " + ex.Message, ex);
        }
    }

    public void UpdateJson(string path, JToken newValue)
    {
        try
        {
            var token = _jsonToken.SelectToken(path);
            if (token != null)
            {
                token.Replace(newValue);
            }
            else
            {
                throw new InvalidOperationException("Specified path not found in JSON.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update JSON.", ex);
        }
    }

    public string SaveJson()
    {
        return _jsonToken.ToString();
    }
}