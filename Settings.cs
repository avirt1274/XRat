using System;
using System.Diagnostics;
using System.Security.Policy;

namespace XRat
{
    public static class Settings
    {
        public static string token = "TOKEN";
        public static char prefix = '!';

        public static ulong guildId = 999999999999999;
        public static ulong logsChannelID = 199999999999991;


        public static bool debug = true; // Disable this before build release (Recomended)

        public static string startLog = "@everyone | XRat By @.avirt";

        public static string browserPath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        public static bool enable_startup = true;

        public static string banner = "[᲼](https://cdn.discordapp.com/attachments/1312175786861527116/1312470743358898206/standard.gif?ex=674c9d21&is=674b4ba1&hm=275299b820aa1b8ddc2ad5a798e49b45824d686ff7238135f7231657f21a6723&)";

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}