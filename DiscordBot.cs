using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using DSharpPlus.Entities;

namespace SimpleDiscordBot
{
    public class DiscordBot
    {
        #region FirstWorkingMethod

        public static DiscordClient discord; 
        private static HttpClient httpClient;
        private static bool isBotDUploaded;
       
        ///* We'll load the app config into this when we create it a little later. */
        private static IConfigurationRoot _config;

#pragma warning disable IDE0060 // Remove unused parameter
        public static async Task MainAsync(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            isBotDUploaded = false;
           
            try
            {
                httpClient = LocalHttpClient();

                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();

                //Load the config file(we'll create this shortly)
                Console.WriteLine("[info] Loading config file..");
                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();
                Console.WriteLine("[info] Done. The Bot is working!");
                discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = _config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot
                });
                
                discord.MessageCreated += async e =>
                {                   
                    isBotDUploaded = false;
                    if (e.Message.Content.ToLower().StartsWith("hi"))
                    {
                        await e.Message.RespondAsync("Good day! Can I help you?");
                    }
                    if (e.Message.Content.ToLower().StartsWith("get image"))
                    {
                        await UploadFile(e);
                        await WaitToCheckNewMessage(5000);
                    }
                    if (e.Message.Attachments.Count > 0 && !isBotDUploaded)
                    {
                        await DownloadFileAsync(e);
                    }
                };

                await discord.ConnectAsync();
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                // This will catch any exceptions that occur during the operation/setup of your bot.

                // Feel free to replace this with what ever logging solution you'd like to use.
                // I may do a guide later on the basic logger I implemented in my most recent bot.
                Console.Error.WriteLine(ex.ToString());
            }
        }

        public static HttpClient LocalHttpClient()
        {
            //// https://hidemyna.me/ru/proxy-list/?maxtime=250#list

            // Содержит параметры HTTP-прокси для System.Net.WebRequest класса.
            var proxy = new WebProxy()
            {
                Address = new Uri($"http://77.87.240.74:3128"),
                UseDefaultCredentials = false,
            };
            // Создает экземпляр класса System.Net.Http.HttpClientHandler.
            var httpClientHandler = new HttpClientHandler() { Proxy = proxy };
            // Предоставляет базовый класс для отправки HTTP-запросов и получения HTTP-ответов 
            // от ресурса с заданным URI.   
            HttpClient hc = new HttpClient(httpClientHandler);
            return hc;
        }


        public static async Task DownloadFileAsync(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {            
            if (!isBotDUploaded)
            {
                var fileUrl = e.Message.Attachments[0].Url;
                var fileName = e.Message.Attachments[0].FileName;
                var endDirectory = Directory.GetCurrentDirectory() + @"\Files\";

                using (httpClient = new HttpClient())
                {
                    using HttpResponseMessage response = await httpClient.GetAsync(fileUrl);
                    using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                    using FileStream DestinationStream = File.Create(endDirectory + fileName.ToString());
                    await streamToReadFrom.CopyToAsync(DestinationStream);
                }
                await e.Message.RespondAsync("Successfully Downloaded!");
                Console.WriteLine("[info] Downloaded");               
            }
            else
            {                
                Console.WriteLine("[info] It's mine");
            }
        }
        public static async Task UploadFile(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {            
            await e.Message.RespondAsync("Second..");
            var filePath = Directory.GetCurrentDirectory() + @"\made.png";            
            await e.Channel.SendFileAsync(filePath, "Complited");         
            await e.Message.RespondAsync("Good luck!");
            isBotDUploaded = true;           
        }
        public static async Task WaitToCheckNewMessage(int delay)
        {
            isBotDUploaded = false;
            await Task.Delay(delay);
        }
#endregion
    }
}
