using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ideal.Config;

namespace ideal.Helper
{
    public static class TelegramHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        public static async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                var botToken = AppConfig.Instance.TelegramBotToken;
                var chatIds = new[]
                {
                    AppConfig.Instance.TelegramChatId1,
                    AppConfig.Instance.TelegramChatId2
                };

                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                foreach (var chatId in chatIds)
                {
                    var content = new StringContent(
                        $"{{\"chat_id\":\"{chatId}\",\"text\":\"{EscapeForJson(message)}\",\"parse_mode\":\"Markdown\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    await httpClient.PostAsync(url, content);
                }
            }
            catch
            {
                //Telegram gönderimi başarısız olursa sessizce geçiyoruz
            }
        }

        //JSON içinde özel karakterleri kaçırmak için
        private static string EscapeForJson(string text)
        {
            return text.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r");
        }
    }
}
