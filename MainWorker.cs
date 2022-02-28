using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace DirectMessagesDeleter
{
    internal class MainWorker
    {
        private string username = "", cookie = "", thread_id = "";
        private int counter = 1;
        public MainWorker(string cookie, string username)
        {
            Console.Clear();
            this.username = username;
            this.cookie = cookie;
        }

        public void StartWorker()
        {
            var user_id = Regex.Match(MakeGetRequest($"https://i.instagram.com/api/v1/users/{this.username}/usernameinfo/"), "pk\":(.*?),").Groups[1].Value;
            if (user_id != "")
            {
                this.thread_id = Regex.Match(MakePostRequest("https://i.instagram.com/api/v1/direct_v2/create_group_thread/", $"recipient_users=[\"{user_id}\"]"), "thread_id\":\"(.*?)\"").Groups[1].Value;
                var oldest_cursor = "";
                var has_older = true;
                while (has_older)
                {
                    var JsonText = MakeGetRequest($"https://i.instagram.com/api/v1/direct_v2/threads/{this.thread_id}/?cursor={oldest_cursor}"); // Get The Thread Last 20 Message
                    var JsonObjectThread = (JObject)JObject.Parse(JsonText)["thread"];

                    oldest_cursor = (string)JsonObjectThread["oldest_cursor"]; // Next ( Old ) 20 Message
                    has_older = (bool)JsonObjectThread["has_older"]; // Check If There Is An Old Cursor || 20 Or Least Old Messages

                    var items = ((JArray)JsonObjectThread["items"]).ToList(); // All Messages Stored in This List as JavaScript Object ( JSON )

                    foreach (var item in items) // Loop Through The Messages
                    {
                        if (((bool)item["is_sent_by_viewer"])) // Condition To Check If The Message Sent By Logged In Account 
                        {
                            var item_id = (string)item["item_id"];
                            var res = MakePostRequest($"https://i.instagram.com/api/v1/direct_v2/threads/{this.thread_id}/items/{item_id}/delete/", ""); // Delete Message Request
                            if (res.Contains("status\":\"ok\"")) // Means The Message Has Been Deleted Successfully
                            {
                                Console.CursorTop = 0;Console.CursorLeft = 0;
                                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[ + ] Users Length : "); Console.ForegroundColor = ConsoleColor.Magenta; Console.Write($"{Program.UsersLength}\n");
                                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[ + ] Username : "); Console.ForegroundColor = ConsoleColor.Magenta; Console.Write($"{this.username}\n");
                                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[ + ] Sleep - In Seconds - : "); Console.ForegroundColor = ConsoleColor.Magenta; Console.Write($"{Program.sleep}\n");
                                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[ + ] Messages Deleted : "); Console.ForegroundColor = ConsoleColor.Magenta; Console.Write($"{counter.ToString()}\n");
                                Console.ForegroundColor = ConsoleColor.Cyan; Console.Write("[ + ] Item ID : "); Console.ForegroundColor = ConsoleColor.Magenta; Console.Write($"{item_id}");
                            }
                            counter += 1;
                            System.Threading.Thread.Sleep((Program.sleep * 1000)); // Time Between Every Message Deletion
                        }
                    }
                }
            }
        }
        
        private string MakeGetRequest(string Url)
        {
            System.Net.HttpWebRequest HttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Url);
            HttpWebRequest.Method = "GET";
            HttpWebRequest.UserAgent = "Instagram 177.0.0.30.119 Android (25/7.1.2; 191dpi; 576x1024; Asus; ASUS_Z01QD; ASUS_Z01QD; intel; en_US; 276028020)";
            HttpWebRequest.Headers.Add("Cookie", this.cookie);

            System.Net.HttpWebResponse Response;
            try
            {
                Response = (System.Net.HttpWebResponse)HttpWebRequest.GetResponse();
            }
            catch (System.Net.WebException ex)
            {
                Response = (System.Net.HttpWebResponse)ex.Response;
            }
            System.IO.StreamReader StreamReader = new System.IO.StreamReader(Response.GetResponseStream());
            string Result = StreamReader.ReadToEnd().ToString();
            StreamReader.Dispose();
            StreamReader.Close();
            return Result;

        }
        private string MakePostRequest(string Url, string PostData)
        {
            byte[] Bytes = new System.Text.UTF8Encoding().GetBytes(PostData);
            System.Net.HttpWebRequest HttpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Url);
            HttpWebRequest.Method = "POST";
            HttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            HttpWebRequest.Headers.Add("Accept-Language", "en-US");
            HttpWebRequest.UserAgent = "Instagram 177.0.0.30.119 Android (25/7.1.2; 191dpi; 576x1024; Asus; ASUS_Z01QD; ASUS_Z01QD; intel; en_US; 276028020)";
            HttpWebRequest.Headers.Add("Cookie", this.cookie);
            HttpWebRequest.ContentLength = Bytes.Length;
            System.IO.Stream Stream = HttpWebRequest.GetRequestStream();
            Stream.Write(Bytes, 0, Bytes.Length);
            Stream.Dispose();
            Stream.Close();
            System.Net.HttpWebResponse Response;
            try
            {
                Response = (System.Net.HttpWebResponse)HttpWebRequest.GetResponse();
            }
            catch (System.Net.WebException ex)
            {
                Response = (System.Net.HttpWebResponse)ex.Response;
            }
            System.IO.StreamReader StreamReader = new System.IO.StreamReader(Response.GetResponseStream());
            string Result = (StreamReader.ReadToEnd()).ToString();
            StreamReader.Dispose();
            StreamReader.Close();
            return Result;
        }
    }
}
