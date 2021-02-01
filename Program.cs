using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AuthGG_DiscordBot.Modules;
using System.Collections.Specialized;
using System.Net.NetworkInformation;

namespace zDiscordBot
{
    //Please check Top of Commands.cs :)
    //C# AuthGG Discord Bot by xo1337 :)
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();
        

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string TokenKey = "DISCORD_BOT_TOKEN"; // PUT YOUR DISCORD BOT TOKEN HERE
            _client.Log += client_Log;
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, TokenKey);
            
            await _client.StartAsync();

            await Task.Delay(-1);

        }

      
        private Task client_Log(LogMessage arg)
        {     
            Console.WriteLine(arg);       
            return Task.CompletedTask;         
        }
        
        public async Task RegisterCommandsAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += ReadyTask;
            _client.UserJoined += AnnounceUserJoined;
            _client.UserLeft += AnnounceUserLeft;
           
                                    
        }

        private async Task ReadyTask()
        {
            await _client.SetGameAsync("https://github.com/xo1337", null, ActivityType.Watching);        
        }
        
        private async Task AnnounceUserJoined(IGuildUser user)
        {
            Console.WriteLine($"{user.Username} joined {user.Guild.Name}");

            if (user.Guild.Id == Servers.PrivateGuild)
            {                                 
                try
                {
                    if (!Helper.IsCustomer(user.Username.ToString())) // Not Customer
                    {
                        await user.KickAsync("You're not a customer lmao");
                    }                     
                    else if (Helper.IsCustomer(user.Username.ToString())) // Customer
                    {
                        /* Please uncomment out this code if you'd like a welcome message :) Also make sure to change the string from "welcome" to your welcome channel name.
                        IReadOnlyCollection<SocketGuildChannel> channels = _client.GetGuild(user.Guild.Id).Channels;
         
                        foreach (SocketGuildChannel channel in channels)
                        if (channel.Name.Contains("welcome"))
                        {
                             Random Rand = new Random();
                            await _client.GetGuild(user.Guild.Id).GetTextChannel(channel.Id).SendMessageAsync(Rand.Next(0, 100) > 50 ? $"Welcome to moms basement {user.Mention}!" : $"Welcome {user.Mention}!, Welcome to our customer discord :heart: Glad to have you here :)", false);
                            break;
                        }
                        */
                        await user.AddRoleAsync(_client.GetGuild(Servers.PrivateGuild).Roles.FirstOrDefault(x => x.Name == "Customer")); //Replace Customer Text With Your Role Name!
                    }

                    else if (Helper.IsAdmin(user.Username.ToString()) || _client.GetGuild(Servers.PrivateGuild).OwnerId == user.Id || user.IsBot) // Admin Checks
                    {
                        Console.WriteLine($"Skipped Checks Admin For {user.Username}.");
                        return;
                    }
                                   
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception from buyer discord: {ex.Message}");
                    return;
                }
                
               
            }
            else if (user.Guild.Id == Servers.PublicGuild)
            {
                try
                {
                    /*
                    IReadOnlyCollection<SocketGuildChannel> channels = _client.GetGuild(user.Guild.Id).Channels;

                    foreach (SocketGuildChannel channel in channels)
                    if (channel.Name.Contains("welcome")) // Put Your Welcome Channel Name Here!
                    {
                        Random Rand = new Random();
                        await _client.GetGuild(user.Guild.Id).GetTextChannel(channel.Id).SendMessageAsync($"Welcome {user.Mention} To Server!");
                        break;
                    }
                    */ //If you want a welcome message when the user joins, please uncomment this part of the code and change the channel name to your welcome channel name :).

                    if (Helper.IsCustomer(user.Username.ToString()))
                    {      
                        //Uncomment this code below if you want exists customers who join / rejoin the public server to get their customer role :)
                        //await user.AddRoleAsync(_client.GetGuild(Servers.PrivateGuild).Roles.FirstOrDefault(x => x.Name == "Gang")); //Change the text "gang" to your customer role name.
                    }
                }
                catch(Exception exception)
                {
                    Console.WriteLine("Exception from public server: " + exception.Message);
                }
            }

            await Task.Delay(0);
        }

        private async Task AnnounceUserLeft(IGuildUser user)
        {
            Console.WriteLine($"{user.Username} Has left {user.Guild.Name} :(");

           
            await Task.Delay(0);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                var context = new SocketCommandContext(_client, message);

                List<string> blacklistedWords = new List<string>()
                {
                    "cheat",
                };

                foreach (string str in blacklistedWords)
                {
                    var user = (arg.Author as IGuildUser);
                    try
                    {
                        if (arg.Content.ToLower().Contains(str) && !user.GuildPermissions.Administrator)
                        {
                            await arg.DeleteAsync();
                            return;
                        }
                        if (arg.Content.ToLower().Contains(str) && user.GuildPermissions.Administrator)                       
                            return;
                  
                    }
                    catch
                    {
                        string exception = "Unhandled exception: " + CommandError.Exception;
                        Console.WriteLine(exception);
                    }
                }

                if (message.Author.IsBot)
                {
                    return;
                }

                int argPos = 0;
                string BotPrefix = "?"; //Change this to what ever.
                if (message.HasStringPrefix(BotPrefix, ref argPos)) 
                {
                    var r = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!r.IsSuccess)
                    {
                        Console.WriteLine(r.ErrorReason);
                    }
                }
            }                 
        }
    }
}
