using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DirectMessagesDeleter
{
    internal class Login
    {
        private string username = "", password = "";
        public string cookie = "";
        private CookieContainer CookieContainer;
        private CookieCollection Cookies;

        public Login(string username, string password)
        {
            this.CookieContainer = new CookieContainer();
            this.Cookies = new CookieCollection();
            this.username = username;  
            this.password = password;
        }
        public async Task<bool> StartWorker()
        {
            var LoginResponse = await RequestMaker("/accounts/login/", $"username={this.username}&password={this.password}&login_attempt_count=0&device_id={Guid.NewGuid()}", true);
            if (LoginResponse.Contains("logged_in_user"))
            {
                ContainerToString();
                return true;
            }
            else if (LoginResponse.Contains("challenge_required"))
            {
                var path = Regex.Match(LoginResponse, "api_path\":\"(.*?)\"").Groups[1].Value;
            InsertChoiceAgain:
                Console.Write("Enter Choice:\n0 For Sending Code Into Phone\n1 : For Sending Code Into Email\nThe Choice : ");
                var choice = Console.ReadLine();
                if ((choice != "0" && choice != "1"))
                {
                    Console.WriteLine("Enter a Valid Choice !!");
                    goto InsertChoiceAgain;
                }
                var PostChoiceResponse = await RequestMaker(path, $"choice={choice}");
                if (PostChoiceResponse.Contains("security_code"))
                {
                    Console.Write("Enter Code : ");
                    var code = Console.ReadLine();
                    var PostCodeResponse = await RequestMaker(path, $"security_code={code}", true);
                    if (PostCodeResponse.Contains("logged_in_user"))
                    {
                        
                        ContainerToString();
                        return true;
                    }
                }
                

            }
            return false;
        }

        private async Task<string> RequestMaker(string path, string PostData, bool AddCookiesToContainer = false)
        {
            byte[] Bytes = new System.Text.UTF8Encoding().GetBytes(PostData);
            System.Net.HttpWebRequest HttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create($"https://i.instagram.com/api/v1{path}");
            HttpWebRequest.Method = "POST";
            HttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            HttpWebRequest.Headers.Add("Accept-Language", "en-US");
            HttpWebRequest.UserAgent = "Instagram 100.1.0.29.135 Android";
            HttpWebRequest.CookieContainer = CookieContainer;
            HttpWebRequest.ContentLength = Bytes.Length;
            System.IO.Stream Stream = await HttpWebRequest.GetRequestStreamAsync();
            await Stream.WriteAsync(Bytes, 0, Bytes.Length);
            Stream.Dispose();
            Stream.Close();
            System.Net.HttpWebResponse Response;
            try
            {
                Response = (System.Net.HttpWebResponse) await HttpWebRequest.GetResponseAsync();
            }
            catch (System.Net.WebException ex)
            {
                Response = (System.Net.HttpWebResponse)ex.Response;
            }
            System.IO.StreamReader StreamReader = new System.IO.StreamReader(Response.GetResponseStream());
            string Result = (await StreamReader.ReadToEndAsync()).ToString();
            StreamReader.Dispose();
            StreamReader.Close();
            if (AddCookiesToContainer)
            {
                this.CookieContainer.Add(Response.Cookies);
                this.Cookies.Add(Response.Cookies) ;
            }
            return Result;
        }
        
        private void ContainerToString()
        {
            
            for (var i = 0; i < this.Cookies.Count; i++)
            {
                var t = this.Cookies[i];
                this.cookie += $"{t}";
                if (this.Cookies.Count != (i + 1))
                {
                    this.cookie += "; ";
                }
            }
        }

    }
}
