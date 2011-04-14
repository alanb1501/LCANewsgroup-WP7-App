using System;
using System.Net;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LCANewsgroup.ViewModels
{
    public class MainPostsViewModel : ViewModelBase
    {
        //todo: have user enter this shit...
        private string passwordhash = @"";
        private string username = "";

        private string allPostUrl = @"https://api.greenpride.com/Service.svc/Posts?UserName={0}&Password={1}&format=xml"; 

        public class Post
        {
            public DateTime Posted { get; set; }
            public User Author { get; set; }
            public string Subject { get; set; }
            public int ReplyCount { get { return Replies.Count; } }
            public string Description { get;set; }
            public int PostId { get; set; }
            public int ParentId { get; set; }
            public List<Post> Replies { get; set;}
        }

        public class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string NickName { get; set; }
            public int UserId { get; set; }
            public string Username { get; set; }

            public override string ToString()
            {
                return FullName;
                //return String.Format("{0} \"{1}\" {2}",FirstName,NickName,LastName);
            }
        }

        public ObservableCollection<Post> Posts { get; private set; }
               
        public MainPostsViewModel()
        {
            Posts = new ObservableCollection<Post>();

            string url = String.Format(allPostUrl, username, passwordhash);

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {

                WebClient webClient = new WebClient();
                webClient.DownloadStringAsync(new Uri(url));

                webClient.DownloadStringCompleted += (o, e) =>
                    {
                        ParsePosts(e.Result);
                    };
            }
            else
            {
                Posts.Add(new Post()
                {
                    Author = new User() { FullName = "John Q. Stallion" },
                    Description = "Imma thinking aboot upgrading my internets [currently 6 Mbps DSL] and looks like uverse can has 24 Mbps for $50/month [introductory]. &nbsp;Quadruple the speed of Comcast at the same price (I could write &quot;price point&quot; but doesn&#39;t that sound retarded when people use that term in casual conversation?).",
                    ParentId = 10,
                    PostId = 10,
                    Posted = DateTime.Now.Subtract(new TimeSpan(0, 10, 0)),
                    Subject = "I like dogs",
                    Replies = new List<Post>()
                });
            }
        }

        private void ParsePosts(string xmlPostData)
        {

            XDocument doc = XDocument.Parse(xmlPostData);

            //nasty hack for now.
            foreach (var e in doc.Root.DescendantsAndSelf())
            {
                if(e.Name.Namespace  != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }
            }

            var allPosts = (from post in doc.Root.Elements()
                            select new Post()
                            {
                                Author = new User()
                                {
                                    FullName = post.Element("AuthorName").Value,
                                    UserId = Int32.Parse(post.Element("AuthorID").Value)
                                },
                                Replies = new List<Post>(),
                                Description = post.Element("Description").Value,
                                Subject = post.Element("Subject").Value,
                                Posted = DateTime.Parse(post.Element("Date").Value),
                                ParentId = Int32.Parse(post.Element("ParentID").Value),
                                PostId = Int32.Parse(post.Element("PostID").Value)
                            }).ToDictionary(a => a.PostId);

            foreach (var post in allPosts.Values)
            {
                if (post.ParentId == post.PostId)
                {
                    continue;
                }

                var p = allPosts[post.ParentId];

                if (p != null)
                {
                    p.Replies.Add(post);
                }
            }

            //nasty! XD
            (from post in allPosts.Values
             where post.ParentId == post.PostId
             select post).ToList().ForEach((a) => { Posts.Add(a); });

            RaisePropertyChangedEvent("Posts");
        }
    }
}
