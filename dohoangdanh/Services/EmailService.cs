using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace dohoangdanh.Services
{
    public class EmailService
    {
        private readonly IConfiguration _cfg;
        public EmailService(IConfiguration cfg) { _cfg = cfg; }

        public async Task SendAsync(string to, string subject, string body)
        {
            var key = _cfg["Resend:ApiKey"];
            var from = $"{_cfg["Resend:FromName"]} <{_cfg["Resend:FromEmail"]}>";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", key);

            var payload = JsonSerializer.Serialize(new
            {
                from,
                to = new[] { to },
                subject,
                html = $"<p>{body}</p>"
            });

            var res = await http.PostAsync("https://api.resend.com/emails",
                new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }
    }
}