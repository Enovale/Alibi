using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alibi.Plugins.Fun
{
    public static class EightBall
    {
        public static string[] Responses;

        private static string[] _defaultResponses =
            new[]
            {
                "As I see it, yes.",
                "Ask again later.",
                "Better not tell you now.",
                "Cannot predict now.",
                "Concentrate and ask again.",
                "Don’t count on it.",
                "It is certain.",
                "It is decidedly so.",
                "Most likely.",
                "My reply is no.",
                "My sources say no.",
                "Outlook not so good.",
                "Outlook good.",
                "Reply hazy, try again.",
                "Signs point to yes.",
                "Very doubtful.",
                "Without a doubt.",
                "Yes.",
                "Yes – definitely.",
                "You may rely on it.",
            };

        static EightBall()
        {
            if (!File.Exists(FunPlugin.EightBallConfigPath) || new FileInfo(FunPlugin.EightBallConfigPath).Length <= 0)
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                File.WriteAllText(FunPlugin.EightBallConfigPath,
                    JsonSerializer.Serialize(_defaultResponses, options));
            }

            Responses = JsonSerializer.Deserialize<string[]>(File.ReadAllText(FunPlugin.EightBallConfigPath));
        }
    }
}