using System.Text;
using UnityEngine;

public static class OpenAIResponseTextExtractor
{
    public static string Extract(string responseJson)
    {
        if (string.IsNullOrEmpty(responseJson))
        {
            return "";
        }

        try
        {
            OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseJson);
            string text = ExtractFromStructuredResponse(response);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }
        catch
        {
            // Si Unity no puede parsear el JSON, usamos el fallback manual.
        }

        return ExtractTextFallback(responseJson);
    }

    private static string ExtractFromStructuredResponse(OpenAIResponse response)
    {
        if (response == null)
        {
            return "";
        }

        if (!string.IsNullOrWhiteSpace(response.output_text))
        {
            return response.output_text;
        }

        if (response.output == null)
        {
            return "";
        }

        StringBuilder builder = new StringBuilder();

        foreach (OpenAIOutput output in response.output)
        {
            if (output == null || output.content == null)
            {
                continue;
            }

            foreach (OpenAIContent content in output.content)
            {
                if (content == null)
                {
                    continue;
                }

                string text = !string.IsNullOrWhiteSpace(content.text) ? content.text : content.refusal;
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(text);
            }
        }

        return builder.ToString();
    }

    private static string ExtractTextFallback(string json)
    {
        int outputTextIndex = json.IndexOf("output_text", System.StringComparison.Ordinal);
        if (outputTextIndex >= 0)
        {
            string text = ExtractJsonString(json, "text", outputTextIndex);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return ExtractJsonString(json, "text", 0);
    }

    private static string ExtractJsonString(string json, string key, int startIndex)
    {
        string pattern = "\"" + key + "\"";
        int keyIndex = json.IndexOf(pattern, Mathf.Clamp(startIndex, 0, json.Length), System.StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return "";
        }

        int colonIndex = json.IndexOf(':', keyIndex + pattern.Length);
        int quoteIndex = colonIndex >= 0 ? json.IndexOf('"', colonIndex + 1) : -1;
        if (quoteIndex < 0)
        {
            return "";
        }

        StringBuilder builder = new StringBuilder();
        bool escaping = false;

        for (int i = quoteIndex + 1; i < json.Length; i++)
        {
            char c = json[i];

            if (escaping)
            {
                AppendEscapedCharacter(builder, c, json, ref i);
                escaping = false;
                continue;
            }

            if (c == '\\')
            {
                escaping = true;
                continue;
            }

            if (c == '"')
            {
                break;
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    private static void AppendEscapedCharacter(StringBuilder builder, char c, string json, ref int index)
    {
        switch (c)
        {
            case '"': builder.Append('"'); break;
            case '\\': builder.Append('\\'); break;
            case '/': builder.Append('/'); break;
            case 'b': builder.Append('\b'); break;
            case 'f': builder.Append('\f'); break;
            case 'n': builder.Append('\n'); break;
            case 'r': builder.Append('\r'); break;
            case 't': builder.Append('\t'); break;
            case 'u':
                AppendUnicodeCharacter(builder, json, ref index);
                break;
            default:
                builder.Append(c);
                break;
        }
    }

    private static void AppendUnicodeCharacter(StringBuilder builder, string json, ref int index)
    {
        if (index + 4 >= json.Length)
        {
            return;
        }

        string hex = json.Substring(index + 1, 4);
        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
        {
            builder.Append((char)code);
            index += 4;
        }
    }

    [System.Serializable]
    private class OpenAIResponse
    {
        public string output_text;
        public OpenAIOutput[] output;
    }

    [System.Serializable]
    private class OpenAIOutput
    {
        public OpenAIContent[] content;
    }

    [System.Serializable]
    private class OpenAIContent
    {
        public string text;
        public string refusal;
    }
}
