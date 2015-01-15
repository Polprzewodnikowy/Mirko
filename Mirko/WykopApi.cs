using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace korgeaux
{
    public sealed class WykopApi
    {
        private string appKey;
        private string appSecret;

        public WykopApi(string appKey, string appSecret)
        {
            this.appKey = appKey;
            this.appSecret = appSecret;
        }

        public T DoRequest<T>(RequestParams requestParams) where T : class
        {
            string mp = string.Join("/", requestParams.MethodParams);
            if (mp != "")
                mp += '/';

            string ap = string.Join(",", requestParams.ApiParams.Keys.Select(key => key + ',' + requestParams.ApiParams[key]));
            if (ap != "")
                ap += ',';

            string url =
                "http://a.wykop.pl/"
                + requestParams.Resource
                + '/'
                + mp
                + ap
                + "appkey,"
                + appKey
                + ",output,clear";

            string sign;

            using (MD5 md5 = MD5.Create())
            {
                var toHash = new StringBuilder(appSecret + url);
                toHash.Append(string.Join(",", requestParams.PostParams.Keys.OrderBy(s => s).Select(key => requestParams.PostParams[key]).ToList()));

                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(toHash.ToString()));

                var hash = new StringBuilder();
                foreach (byte b in byteHash)
                    hash.Append(b.ToString("x2"));

                sign = hash.ToString();
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Headers.Add("apisign", sign);
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/x-www-form-urlencoded";

            if (requestParams.PostParams.Count != 0)
            {
                using (Stream writer = req.GetRequestStream())
                {
                    byte[] postData = Encoding.UTF8.GetBytes(string.Join("&", requestParams.PostParams.Keys.Select(key => key + '=' + requestParams.PostParams[key])));
                    writer.Write(postData, 0, postData.Length);
                }
            }

            StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream());
            string result = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }

    public class RequestParams
    {
        public string Resource { get; private set; }
        public List<string> MethodParams { get; private set; }
        public SortedList<string, string> ApiParams { get; private set; }
        public SortedList<string, string> PostParams { get; private set; }

        public RequestParams(string resource)
        {
            this.Resource = resource;
            MethodParams = new List<string>();
            ApiParams = new SortedList<string, string>();
            PostParams = new SortedList<string, string>();
        }
    }

    [JsonObject]
    public class Profile
    {
        [JsonProperty("login")] public string Login { get; set; }
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("public_email")] public string PublicEmail { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("www")] public Uri WWW { get; set; }
        [JsonProperty("jabber")] public string Jabber { get; set; }
        [JsonProperty("gg")] public int? GG { get; set; }
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("about")] public string About { get; set; }
        [JsonProperty("author_group")] public AuthorGroup? AuthorGroup { get; set; }
        [JsonProperty("links_added")] public int? LinksAdded { get; set; }
        [JsonProperty("links_published")] public int? LinksPublished { get; set; }
        [JsonProperty("comments")] public int? Comments { get; set; }
        [JsonProperty("rank")] public int? Rank { get; set; }
        [JsonProperty("followers")] public int? Followers { get; set; }
        [JsonProperty("following")] public int? Following { get; set; }
        [JsonProperty("entries")] public int? Entries { get; set; }
        [JsonProperty("entries_comments")] public int? EntriesComments { get; set; }
        [JsonProperty("diggs")] public int? Diggs { get; set; }
        [JsonProperty("buries")] public int? Buries { get; set; }
        [JsonProperty("related_links")] public int? RelatedLinks { get; set; }
        [JsonProperty("signup_date")] public DateTime? SignupDate { get; set; }
        [JsonProperty("avatar")] public Uri Avatar { get; set; }
        [JsonProperty("avatar_big")] public Uri AvatarBig { get; set; }
        [JsonProperty("avatar_med")] public Uri AvatarMed { get; set; }
        [JsonProperty("avatar_lo")] public Uri AvatarLo { get; set; }
        [JsonProperty("is_observed")] public bool? IsObserved { get; set; }
        [JsonProperty("is_blocked")] public bool? IsBlocked { get; set; }
        [JsonProperty("sex")] public string Sex { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
    }

    [JsonObject]
    public class OwnProfile : Profile
    {
        [JsonProperty("userkey")] public string UserKey { get; set; }
    }

    [JsonObject]
    public class AuthorCollection
    {
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("author_group")] public AuthorGroup? AuthorGroup { get; set; }
        [JsonProperty("author_avatar")] public Uri AuthorAvatar { get; set; }
        [JsonProperty("author_avatar_big")] public Uri AuthorAvatarBig { get; set; }
        [JsonProperty("author_avatar_med")] public Uri AuthorAvatarMed { get; set; }
        [JsonProperty("author_avatar_lo")] public Uri AuthorAvatarLo { get; set; }
        [JsonProperty("author_sex")] public string AuthorSex { get; set; }
    }

    [JsonObject]
    public class Comment : AuthorCollection
    {
        [JsonProperty("id")] public int? Id { get; set; }
        [JsonProperty("date")] public DateTime? Date { get; set; }
        [JsonProperty("vote_count")] public int? VoteCount { get; set; }
        [JsonProperty("vote_count_plus")] public int? VoteCountPlus { get; set; }
        [JsonProperty("vote_count_minus")] public int? VoteCountMinus { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("source")] public Uri Source { get; set; }
        [JsonProperty("parent_id")] public int? ParentId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("can_vote")] public bool? CanVote { get; set; }
        [JsonProperty("user_vote")] public bool? UserVote { get; set; }
        [JsonProperty("blocked")] public bool? Blocked { get; set; }
        [JsonProperty("deleted")] public bool? Deleted { get; set; }
        [JsonProperty("embed")] public Embed Embed { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("app")] public string App { get; set; }
        [JsonProperty("user_favorite")] public bool? UserFavorite { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
        [JsonProperty("link")] public Link Link { get; set; }
    }

    [JsonObject]
    public class Dig : AuthorCollection
    {
        [JsonProperty("date")] public DateTime? Date { get; set; }
    }

    [JsonObject]
    public class Bury : Dig
    {
        [JsonProperty("reason")] public int? Reason { get; set; }
    }

    [JsonObject]
    public class Vote
    {
        [JsonProperty("vote")] public int? VoteCount { get; set; }
        [JsonProperty("voters")] public Dig[] Voters { get; set; }
    }

    [JsonObject]
    public class Link : AuthorCollection
    {
        [JsonProperty("id")] public int? Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("tags")] public string Tags { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("source_url")] public Uri SourceUrl { get; set; }
        [JsonProperty("vote_count")] public int? VoteCount { get; set; }
        [JsonProperty("comment_count")] public int? CommentCount { get; set; }
        [JsonProperty("report_count")] public int? ReportCount { get; set; }
        [JsonProperty("related_count")] public int? RelatedCount { get; set; }
        [JsonProperty("date")] public DateTime Date { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("group")] public string Group { get; set; }
        [JsonProperty("preview")] public Uri Preview { get; set; }
        [JsonProperty("user_vote")] public string UserVote { get; set; }
        [JsonProperty("user_favorite")] public bool? UserFavorite { get; set; }
        [JsonProperty("user_observe")] public bool? UserObserve { get; set; }
        [JsonProperty("user_list")] public int[] UserLists { get; set; }
        [JsonProperty("plus18")] public bool? Plus18 { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("can_vote")] public bool? CanVote { get; set; }
        [JsonProperty("has_own_content")] public bool? HasOwnContent { get; set; }
        [JsonProperty("is_hot")] public bool? IsHot { get; set; }
        [JsonProperty("category")] public string Category { get; set; }
        [JsonProperty("category_name")] public string CategoryName { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
        [JsonProperty("info")] public string Info { get; set; }
        [JsonProperty("app")] public string App { get; set; }
        [JsonProperty("own_content")] public string OwnContent { get; set; }
    }

    [JsonObject]
    public class RelatedLink : AuthorCollection
    {
        [JsonProperty("id")] public int? Id { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("entry_count")] public int? EntryCount { get; set; }
        [JsonProperty("vote_count")] public int? VoteCount { get; set; }
        [JsonProperty("user_vote")] public int? UserVote { get; set; }
        [JsonProperty("plus18")] public bool? Plus18 { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("link")] public Link Link { get; set; }
    }

    [JsonObject]
    public class MyWykop
    {
        [JsonProperty("type")] public string Type { get; set; }
    }

    [JsonObject]
    public class Entry : AuthorCollection, INotifyPropertyChanged
    {
        private int? voteCount;
        private int? userVote;
        private Dig[] voters;
        private EntryComment[] comments;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [JsonProperty("id")] public int? Id { get; set; }
        [JsonProperty("date")] public DateTime? Date { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("source")] public Uri Source { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }

        [JsonProperty("receiver")] public string Receiver { get; set; }
        [JsonProperty("receiver_avatar")] public Uri ReceiverAvatar { get; set; }
        [JsonProperty("receiver_avatar_big")] public Uri ReceiverAvatarBig { get; set; }
        [JsonProperty("receiver_avatar_med")] public Uri ReceiverAvatarMed { get; set; }
        [JsonProperty("receiver_avatar_lo")] public Uri ReceiverAvatarLo { get; set; }
        [JsonProperty("receiver_group")] public AuthorGroup? ReceiverGroup { get; set; }
        [JsonProperty("receiver_sex")] public string ReceiverSex { get; set; }

        [JsonProperty("comments")] public EntryComment[] Comments { get { return comments; } set { SetField(ref comments, value); } }
        [JsonProperty("blocked")] public bool? Blocked { get; set; }
        [JsonProperty("vote_count")] public int? VoteCount { get { return voteCount; } set { SetField(ref voteCount, value); } }
        [JsonProperty("user_vote")] public int? UserVote { get { return userVote; } set { SetField(ref userVote, value); } }
        [JsonProperty("user_favorite")] public bool? UserFavorite { get; set; }
        [JsonProperty("voters")] public Dig[] Voters { get { return voters; } set { SetField(ref voters, value); } }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("embed")] public Embed Embed { get; set; }
        [JsonProperty("deleted")] public bool? Deleted { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
        [JsonProperty("can_comment")] public bool? CanComment { get; set; }
        [JsonProperty("app")] public string App { get; set; }
        [JsonProperty("comment_count")] public int? CommentCount { get; set; }
    }

    [JsonObject]
    public class EntryComment : AuthorCollection, INotifyPropertyChanged
    {
        private int? voteCount;
        private int? userVote;
        private Dig[] voters;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if(EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [JsonProperty("id")] public int? Id { get; set; }
        [JsonProperty("date")] public DateTime? Date { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("source")] public Uri Source { get; set; }
        [JsonProperty("entry_id")] public int? EntryId { get; set; }
        [JsonProperty("blocked")] public bool? Blocked { get; set; }
        [JsonProperty("deleted")] public bool? Deleted { get; set; }
        [JsonProperty("vote_count")] public int? VoteCount { get { return voteCount; } set { SetField(ref voteCount, value); } }
        [JsonProperty("user_vote")] public int? UserVote { get { return userVote; } set { SetField(ref userVote, value); } }
        [JsonProperty("voters")] public Dig[] Voters { get { return voters; } set { SetField(ref voters, value); } }
        [JsonProperty("embed")] public Embed Embed { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("app")] public string App { get; set; }
        [JsonProperty("violation_url")] public Uri ViolationUrl { get; set; }
        [JsonProperty("entry")] public Entry Entry { get; set; }
    }

    [JsonObject]
    public class Notification : AuthorCollection
    {
        [JsonProperty("date")] public DateTime? Date { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("link")] public Link Link { get; set; }
        [JsonProperty("entry")] public Entry Entry { get; set; }
        [JsonProperty("comment")] public Comment Comment { get; set; }
        [JsonProperty("new")] public bool? New { get; set; }
    }

    [JsonObject]
    public class Embed
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("preview")] public Uri Preview { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("plus18")] public bool? Plus18 { get; set; }
        [JsonProperty("source")] public Uri Source { get; set; }
    }

    [JsonObject]
    public class ConversationsList : AuthorCollection
    {
        [JsonProperty("last_update")] public DateTime LastUpdate { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
    }

    [JsonObject]
    public class PmMessage : AuthorCollection
    {
        [JsonProperty("date")] public DateTime? Date { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("direction")] public string Direction { get; set; }
        [JsonProperty("embed")] public string Embed { get; set; }
        [JsonProperty("app")] public string App { get; set; }
    }

    public enum AuthorGroup
    {
        Green = 0,
        Orange = 1,
        Red = 2,
        Admin = 5,
        Banned = 1001,
        Deleted = 1002,
        Client = 2001,
    }
}
