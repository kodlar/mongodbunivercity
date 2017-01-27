using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest
            var filter = new BsonDocument();
            var recentPosts = await blogContext.Posts.Find(filter).Limit(10).SortByDescending(x => x.CreatedAtUtc).ToListAsync(); 
            
            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            List<string> tagList = new List<string>();
            string[] words = model.Tags.Split(',');
            if (words.Length > 0)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    tagList.Add(words[i]);
                }
            }

            
            // Insert the post into the posts collection

            Post post = new Post();
            post.Content = model.Content;
            post.CreatedAtUtc = DateTime.Now;
            post.Author = HttpContext.User.Identity.Name;

            List<Comment> commentList = new List<Comment>();
            post.Comments = commentList;
            post.Title = model.Title;

            post.Tags = tagList;

            await blogContext.Posts.InsertOneAsync(post);

            return RedirectToAction("Post", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier
            var filter = new BsonDocument(); 

            filter.Add("_id", new BsonObjectId(id));

            var post = await blogContext.Posts.Find(filter).FirstOrDefaultAsync();

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.
            var filter = new BsonDocument();
            
            if (!string.IsNullOrEmpty(tag))
            {
                filter.Add("Tags", tag);
            }

            var posts = await blogContext.Posts.Find(filter).ToListAsync();

            return View(posts);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"

            Comment commentItem = new Comment();
            commentItem.Content = model.Content;
            commentItem.Author = HttpContext.User.Identity.Name;
            commentItem.CreatedAtUtc = DateTime.Now;
            

            await blogContext.Posts.UpdateOneAsync(x=> x.Id == new ObjectId(model.PostId),
                                                    Builders<Post>.Update.PushEach(p => p.Comments,
                                                                    new List<Comment> { commentItem}));


            //await
            //    blogContext.Posts.UpdateOneAsync(x => x.Id == new ObjectId(model.PostId),
            //                                     Builders<Post>.Update.PushEach(p => p.Tags, new List<string>
            //                                                                                 {
            //                                                                                     "deneme",
            //                                                                                     "görelim",
            //                                                                                     "görmezsek şaşırırız...."
            //                                                                                 }));

            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}