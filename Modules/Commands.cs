using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace AuthGG_DiscordBot.Modules
{
    /*
     * 
     * C# AuthGG Discord Bot By xo1337
     * 
     * Please first look through the entire code and make sure everything is set to your needs. Thank you.
     * Also Please goto the redeem key command and make sure you have the roles setup. What i mean, is that when a person it redeems a key, so just make
     * sure you specifiy the name of the role in the string, otherwise the bot will not work and throw exceptions at you.
     * 
     * Please be sure to check out auth.gg's api so you can do more stuff / commands.
     * Also make sure you're using the right methods like POST, GET, etc. Thanks
     * 
     * AuthGG API Documentation
     * https://setup.auth.gg/
     * 
     * 
     * xo1337 - Out.
     */

    public class AuthGG
    {
        
        public static string Authorization   = "AUTHORIZATION";   //12                              
        public static string APIKEY          = "AUTHGG_API_KEY";
        public static string SECRET          = "AUTH_GG_APPLICATION_SECRET";
        public static string AID             = "AUTH_GG_AID";
    }


    public class Helper
    {
        public static bool IsAdmin(string username)
        {   

            dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(username)}"));

            if (authdata["rank"] == "4" || authdata["variable"] == "admin")
                return true;
            else 
                return false;
        }

        public static bool IsBlacklisted(string username)
        {
            dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(username)}"));

            if (authdata["variable"] == "blacklisted" || authdata["variable"] == "banned")
                return true;
            else
                return false;
        }

        public static bool IsCustomer(string username)
        {
            dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(username)}"));
            if (authdata["info"] == "No user found" || authdata["status"] == "failed")
                return false;
            else
                return true;
        }

    }

    public class HttpRequest
    {
        public static string Post(string url, NameValueCollection pairs)
        {
            try
            {
                byte[] response = null;
                using (WebClient client = new WebClient())
                    response = client.UploadValues(url, pairs);
                return Encoding.UTF8.GetString(response, 0, response.Length);
            }
            catch (WebException)
            {
                return "0";
            }
            catch (Exception)
            {
                return "0";
            }
        }

        public static string Get(string url)
        {
            try
            {
                byte[] response = null;
                using (WebClient client = new WebClient())
                    response = client.DownloadData(url);
                return Encoding.UTF8.GetString(response, 0, response.Length);
            }

            catch (WebException)
            {
                return "0";
            }
            catch (Exception)
            {
                return "0";
            }
        }
    }


    public class Servers
    { 
        public static ulong PublicGuild = 0000; //Set Your Public Server Guild, Also Check Program.cs
        public static ulong PrivateGuild = 0000; //Set Your Private Server Guild, Also Check Program.cs
    }

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("addperm")]

        public async Task AddPerms(SocketGuildUser user , string type = null)
        {
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
            }
            else
            {
                if (type == null)
                {
                    await ReplyAsync($"{Context.User.Mention}, Provide a permission.");
                    return;
                }
          

                if (type == "none")
                {
                    const int none_rank = 1;
                    dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=editrank&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}&rank={none_rank.ToString()}"));
                    if (authdata["status"] == "success")
                    {
                        await ReplyAsync($"{Context.User.Mention}, Successfully set permission **{type}** to {user.Mention}.");
                        return;
                    }
                }
                if (type == "admin")
                {
                    const int admin_rank = 4;                  
                    dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=editrank&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}&rank={admin_rank.ToString()}"));
                    if (authdata["status"] == "success")
                    {
                        await ReplyAsync($"{Context.User.Mention}, Successfully set permission **{type}** to {user.Mention}.");
                        return;
                    }
                                    
                }    
                
            }
        }



        [Command("users")]
        public async Task GetAllUsers()
        {
  
            dynamic response = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=count&authorization={AuthGG.Authorization}"));
            await ReplyAsync($"{Context.User.Mention}, There are {response["value"]} In The AuthGG Application!");
        }

        [Command("blacklist")]

        public async Task blacklist(SocketGuildUser user, [RemainderAttribute]string reason = null)
        {
            if (!Helper.IsAdmin(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
            }
            dynamic response = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=editvar&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}&value={reason}"));
            string result = null;
            if (response["status"] == "sucess" || response["info"] == "User variable has been updated")
            {
               
                result = $"{Context.User.Mention}, Blacklisted **{Uri.EscapeDataString(user.Username.ToString())}**, Reason: {reason}";
            }
            await ReplyAsync(result);

        }
        
        [Command("unblacklist")]

        public async Task Unblacklist(SocketGuildUser user)
        {
           if (!Helper.IsAdmin(Uri.EscapeDataString(Context.User.Username.ToString())))
           {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
           }
          
            dynamic response = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=editvar&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}&value=\"unblacklisted\""));
            if (response["status"] == "success")
            {
                await ReplyAsync($"{Context.User.Mention}, Unblacklisted **{Uri.EscapeDataString(user.Username.ToString())}**.");
                return;
            }     
        }

        [Command("genkeys")]
        public async Task genkeys(string type = null, int amount = 0)
        {
            
            if (!Helper.IsAdmin(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
            }

            if (type == null)
            {
                await ReplyAsync($"{Context.User.Mention}, You need to provide a type.");
                return;
            }
            else if (amount <= 0 || amount >= 5)
            {
                await ReplyAsync($"{Context.User.Mention}, You can't generate " + amount.ToString() + " Keys.");
                return;
            }
            else if (type == "week")
            {
                Console.WriteLine(Context.User + " Generated a " + type + " & " + amount + " of them.");
                string result = HttpRequest.Get($"https://developers.auth.gg/LICENSES/?type=generate&days=7&amount={amount}&level=20&authorization={AuthGG.Authorization}&format=2");
                await ReplyAsync($"```{result.Replace("{\"0\":\"", "").Replace("\"}", "")}```");
            }
            else if (type == "month")
            {
                Console.WriteLine(Context.User + " Generated a " + type + " & " + amount + " of them.");
                string result = HttpRequest.Get($"https://developers.auth.gg/LICENSES/?type=generate&days=30&amount={amount}&level=20&authorization={AuthGG.Authorization}&format=2");
                await ReplyAsync($"```{result.Replace("{\"0\":\"", "").Replace("\"}", "")}```");
            }
            else if (type == "lifetime")
            {
               Console.WriteLine(Context.User + " Generated a " + type + " & " + amount + " of them.");
               string result = HttpRequest.Get($"https://developers.auth.gg/LICENSES/?type=generate&days=9998&amount={amount}&level=20&authorization={AuthGG.Authorization}&format=2");                
               await ReplyAsync($"```{result.Replace("{\"0\":\"", "").Replace("\"}", "")}```");
                
            }
        }

        [Command("delkey")]

        public async Task DeleteKey(string key = null)
        {
            if (!Helper.IsCustomer(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have a subscription.");
                return;
            }
            else if (!Helper.IsAdmin(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
            }

            dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/LICENSES/?type=delete&license={key}&authorization={AuthGG.Authorization}"));

            if (authdata["info"] == "No license found")
            {
                await ReplyAsync($"{Context.User.Mention}, No key exists.");
                return;
            }
            else if (authdata["info"] == "License has been deleted")
            {
                await ReplyAsync($"{Context.User.Mention}, Key Successfully deleted.");
            }
        }


        [Command("redeem")]
        public async Task redeem(string key = null)
        {
            if (Helper.IsBlacklisted(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You're blacklisted from AuthGG Application.");
                return;

            }

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[25];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var randomString = new String(stringChars);
            string resultPass = randomString;

            if (key == null)
            {
                await ReplyAsync($"{Context.User.Mention}, usage: `?redeem key-example`");
                return;
            }
           
            var username = Context.Guild.GetUser(Context.User.Id).Username;
            dynamic response = JsonConvert.DeserializeObject(HttpRequest.Post("https://api.auth.gg/v1/", new NameValueCollection() {
                { "type", "register"},
                { "aid", AuthGG.AID},
                { "apikey", AuthGG.APIKEY},
                { "secret", AuthGG.SECRET},
                { "username",  Uri.EscapeDataString(Context.User.Username.ToString())},
                { "password", resultPass },
                { "email", "ID: " + Context.User.Id.ToString()},
                { "license", key },
                { "hwid","no_hwid_set" }              
            }));

            if (response["result"] == "success")
            {
               dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));
               dynamic reset_hwid_result = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/HWID/?type=reset&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));

               if (authdata["status"] == "success" && reset_hwid_result["status"] == "success")
               {
                    SocketGuildUser user = Context.Guild.CurrentUser;
                    if (user.Guild.Id == Servers.PrivateGuild)
                    {
                        
                        var CustomerRole = Context.Guild.Roles.FirstOrDefault(x => (x.Name.ToString().Contains("customer"))); //CHANGE "customer" to the customer role name!
                        await ((SocketGuildUser)Context.User).AddRoleAsync(CustomerRole);
                        await ReplyAsync($"{Context.User.Mention}, Key Successfully Redeemed, Expiration: `{authdata["expiry"]}`");

                        await Context.User.SendMessageAsync($"{Context.User.Mention}, Your Account Details:\n \n**Username:** ** {Uri.EscapeDataString(Context.User.Username.ToString())}**\n**Password:  ** **{resultPass}**\n**Expiration:** **{authdata["expiry"]}**");
                        return;
                    }
                    else if (user.Guild.Id == Servers.PublicGuild)
                    {
                                          
                        var CustomerRole = Context.Guild.Roles.FirstOrDefault(x => (x.Name.ToString().Contains("Gang")));
                        await ((SocketGuildUser)Context.User).AddRoleAsync(CustomerRole);
                        await ReplyAsync($"{Context.User.Mention}, Key Successfully Redeemed, Expiration: `{authdata["expiry"]}`");
                        await Context.User.SendMessageAsync($"{Context.User.Mention}, Your Details:\n \n**Username:** ** {Uri.EscapeDataString(Context.User.Username.ToString())}**\n**Password:  ** **{resultPass}**\n**Expiration:** **{authdata["expiry"]}**");
                        return;
                    }                       
               }
               else
               {
                    await ReplyAsync($"{Context.User.Mention}, We couldn't add you to the customer group, Please contact support.");
                    return;
               }
               
 
            }
            else if (response["result"] == "invalid_license")
            {

                await ReplyAsync($"{Context.User.Mention}, Key is invalid or already redeemed.");
                return;

            }
            else if (response["result"] == "email_used")
            {

                await ReplyAsync($"{Context.User.Mention}, Something went wrong trying to make your account. Contact Staff.");
                return;

            }
            else if (response["result"] == "invalid_username")
            {

               await ReplyAsync($"{Context.User.Mention}, Your username is already taken, contact staff.");
               return;

            }
                                                          
        }

        [Command("reset")]
        public async Task Reset(string type = null)
        {
            if (!Helper.IsCustomer(Context.User.Username.ToString()))
            {
                await ReplyAsync($"{Context.User.Mention}, You're not a customer.");
                return;
            }
  
            if (type == "hwid")
            {
                dynamic response = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/HWID/?type=reset&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));
                if (response["status"] == "success")
                {
                    await ReplyAsync($"{Context.User.Mention}, Your HWID has been reset."); 
                    return;
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention}, Something went wrong. Contact the developer.");
                    return;        
                }
            }
        }

        [Command("extend")]

        public async Task extend(string type = null, string password = null, string key = null)
        {

            if (type != "key" || type == null)
            {
                await ReplyAsync($"{Context.User.Mention}, Invalid type provided");
                return;
            }
            else
            {
                dynamic response_extend_sub = JsonConvert.DeserializeObject(HttpRequest.Post("https://api.auth.gg/v1/", new NameValueCollection()
                {
                    {"type","extend"},
                    {"aid", AuthGG.AID},
                    {"apikey", AuthGG.APIKEY},
                    {"secret", AuthGG.SECRET },
                    {"username", Uri.EscapeDataString(Context.User.Username.ToString()) },
                    {"password", password},
                    {"license", key}
                }));

                if (response_extend_sub["result"] == "invalid_details")
                {            
                    await ReplyAsync($"{Context.User.Mention}, Your password is wrong.");
                    return;

                }
                else if (response_extend_sub["result"] == "invalid_license")
                {
                    await ReplyAsync($"{Context.User.Mention}, Invalid key.");
                    return;
                }
                else if (response_extend_sub["result"] == "success")
                {
                    dynamic NewAuthData = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));
                    await ReplyAsync($"{Context.User.Mention}, Success! Your subscription has been extended to: `{NewAuthData["expiry"]}`");
                    return;
                }
                
            }
        }

        [Command("download")]
        public async Task Download()
        {
            string status = null;
            string message = null;
            dynamic response = JsonConvert.DeserializeObject(HttpRequest.Post("https://api.auth.gg/v1/", new NameValueCollection(){
                {"type","info" },
                {"aid", AuthGG.AID },
                {"apikey", AuthGG.APIKEY },
                {"secret" , AuthGG.SECRET},
                {"username","Put your username from your application" },
                {"password","Put your password from your application" },
                {"hwid","no_hwid_set" }

            }));

            if (response["status"] == "Enabled")
            {
                status = "Online :white_check_mark:";
                message = response["downloadlink"];
            }

            else if (response["status"] == "Disabled")
            {
                status = "Offline :x:";
                message = "Wait for an update...";
            }

            await Context.User.SendMessageAsync($"**Status : {status}**\n**Version : {response["version"]}**\n**Download: {message}**");

        }

        [Command("expires")]
        
        public async Task expires()
        {
            dynamic data = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));
            await ReplyAsync($"{Context.User.Mention}, Your subscription expires: `{data["expiry"]}`");
            
        }


        [Command("myinfo")]

        public async Task myInfo()
        {
            dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(Context.User.Username.ToString())}"));
            string result = null;
            string lastlogin = null;
            string last_hwid = null;

            if (authdata["lastlogin"] == null || authdata["lastlogin"] == "")
            {
                lastlogin = "No last login was found.";
            }              
            else
            {
                lastlogin = authdata["lastlogin"];
            }

            if (authdata["hwid"] == "" || authdata["hwid"] == "no_hwid_set")
            {
                last_hwid = "No hwid was found.";
            }           
            else
            {
                last_hwid = authdata["hwid"];
            }

            if (authdata["status"] == "failed" && authdata["info"] == "No user found")
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have a subscription.");
                return;
            }

            if (authdata["rank"] == null || authdata["rank"] == "0")
            {
                result = "None";
            }
            else if (authdata["rank"] == "4")
            {
                result = "Admin";
            }

            await ReplyAsync($"{Context.User.Mention}, Check your dms.");
            await Context.User.SendMessageAsync($"{Context.User.Mention}, Your account details.\n```Username: {authdata["username"]}\nExpiration: {authdata["expiry"]}\nLast Login: {lastlogin}\nHWID: {last_hwid}\nPermissons: {result}```");


        }

        [Command("get")]
        public async Task CheckData(string type = null, SocketGuildUser user = null)
        {
            if (!Helper.IsAdmin(Uri.EscapeDataString(Context.User.Username.ToString())))
            {
                await ReplyAsync($"{Context.User.Mention}, You do not have permissions to use this command.");
                return;
            }
            if (type == "data") 
            {
                string lastlogin = null;
                string lastip = null;
                string lasthwid = null;
                string rank = null;
                string discord_nickname = null;
                dynamic authdata = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/USERS/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}"));
                

                if (user.Nickname == null || user.Nickname == "")
                    discord_nickname = "User has no nickname.";

                if (authdata["username"] == null || authdata["username"] == "")
                {
                    await Context.User.SendMessageAsync($"{Context.User.Mention}, Auth couldn't retrive the specified users data.");
                    return;
                }

                if (authdata["rank"] == null || authdata["rank"] == "0")
                    rank = $"None";

                if (authdata["rank"] == "4")
                    rank = $"Admin";

                if (authdata["hwid"] == null || authdata["hwid"] == "no_hwid_set")
                    lasthwid = "No hwid has been set.";

                if (authdata["lastlogin"] == null || authdata["lastlogin"] == "")
                    lastlogin = "No last login found.";

                if (authdata["lastip"] == null || authdata["lastip"] == "")
                    lastip = "No last IP found.";

                if (authdata["status"] == "failed" && authdata["info"] == "No user found")
                {
                    await ReplyAsync($"{Context.User.Mention}, No user was found.");
                    return;
                }

                await Context.User.SendMessageAsync($"`{Uri.EscapeDataString(user.Username.ToString())}'s Account Information`\n ```\nUsername: {authdata["username"]}\nExpiration: {authdata["expiry"]}\nLast Login: {lastlogin}\nLast IP: {lastip}\nHWID: {lasthwid}\nPermissions: {rank}\n```\n```Discord Information```\n```Discord Username: {user.Username}\nDiscord ID: {user.Id}\nDiscord Nickname: {discord_nickname}\nAccount Created: {user.CreatedAt}```");
                return;
            }
            else if (type == "hwid")
            {
                string data = null;
                dynamic response = JsonConvert.DeserializeObject(HttpRequest.Get($"https://developers.auth.gg/HWID/?type=fetch&authorization={AuthGG.Authorization}&user={Uri.EscapeDataString(user.Username.ToString())}"));

                if (response["info"] == "Not set")
                    data = "None Set";
                else
                    data = response["info"];

                await ReplyAsync($"{Context.User.Mention}, **{Uri.EscapeDataString(user.Username.ToString())}'s** hwid: `{data}`");
                return;
            }            
        }
    }  
}
