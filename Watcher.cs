using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Task = System.Threading.Tasks.Task;

namespace Capybara
{
    public class Watcher
    {
        private Mailer _mailer;
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public Watcher()
        {
            _mailer = new Mailer();
        }

        public void Watch()
        {
            ProcessProjects();
            Repeat.Interval(Configuration.PollingInterval, ProcessProjects);
        }

        private ICollection<TeamMember> GetTeamMembers(string project)
        {
            var teamId = GetProjectTeamId(project);

            var uri = new Uri(new Uri(Configuration.TfsUrl), String.Format(@"{0}/_api/_teams/members?__v=4&teamId={1}", Uri.EscapeUriString(project), teamId));
            var result = "";
            using (var webClient = CreateWebClient())
            {
                result = webClient.DownloadString(uri);
            }

            dynamic temp = JsonConvert.DeserializeObject(result);
            var teamMembers = new List<TeamMember>();
            foreach (var member in temp.members)
            {
                var teamMember = new TeamMember()
                {
                    Id = member.id,
                    DisplayName = member.displayName,
                    Email = member.email
                };

                teamMembers.Add(teamMember);
                Logger.Trace("Member: {0}", teamMember);
            }

            return teamMembers;
        }

        public string GetProjectTeamId(string project)
        {
            Logger.Trace("Getting team id for project {0}", project);

            var uri = new Uri(new Uri(Configuration.TfsUrl), Uri.EscapeUriString(project));
            var webpage = "";
            using (var webClient = CreateWebClient())
            {
                webpage = webClient.DownloadString(uri);
            }

            var prefix = "\"currentTeam\":{\"identity\":{\"id\":\"";
            var index = webpage.IndexOf(prefix, StringComparison.Ordinal);
            if (index == -1)
            {
                throw new Exception("Team ID not found.");
            }

            var id = webpage.Substring(index + prefix.Length, "f29529f4-8de1-4f00-8763-7db986c2d354".Length);
            Logger.Trace("Team id for project {0} is {1}", project, id);

            return id;
        }

        private void ProcessProjects()
        {
            Logger.Info("Processing projects ...");
            foreach (var project in Configuration.Projects)
            {
                try
                {
                    ProcessProject(project, Configuration.LastCheckDateTime);
                }
                catch (Exception error)
                {
                    Logger.ErrorException("Error processing project:\n", error);
                }
            }

            Configuration.LastCheckDateTime = DateTime.Now;
        }

        private void ProcessProject(string project, DateTime lastCheckDateTime)
        {
            Logger.Info("Processing project: {0}...", project);

            var changesets = GetLatestChangeset(project, 150);
            var commentedChangesets = GetCommentedChangesets(changesets);
            var teamMembers = GetTeamMembers(project);

            foreach (var changeset in commentedChangesets)
            {
                var comments = GetChangesetComments(changeset);
                var newComments = comments.Where(each => each.LastUpdatedDate.ToLocalTime() > lastCheckDateTime).ToList();
                if (newComments.Any())
                {
                    Logger.Debug("New comments found in changeset {0}", changeset);

                    var authors = comments.Select(each => each.Author).Distinct();
                    var members = new List<TeamMember>();
                    foreach (var author in authors)
                    {
                        var member = teamMembers.First(each => each.Id == author.Id);
                        if (member != null)
                        {
                            members.Add(member);
                        }
                    }

                    var owner = teamMembers.First(each => each.Id == changeset.OwnerId);
                    if (owner != null)
                    {
                        members.Add(owner);
                    }

                    SendMail(changeset, project, members.Distinct(), newComments);
                }
            }
        }

        private void SendMail(Changeset changeset, string project, IEnumerable<TeamMember> members, IEnumerable<Comment> newComments)
        {
            var subject = String.Format("[{0}]: New comments in '{1}'", project, changeset.Comment);
            var message = new StringBuilder();
            message.AppendFormat("New comments in changeset {0}: {1}<br>", changeset.Id, changeset.Comment);
            message.AppendFormat("{0}<br><br>", changeset.WebAccessUri);
            message.AppendLine();
            message.AppendLine();
            foreach (var comment in newComments)
            {
                message.AppendFormat("{0}:\n", comment.Author.DisplayName);
                message.AppendFormat("{0}", comment.Content);
                message.AppendLine("<br><br>");
            }

            Task.Factory.StartNew(() =>
            {
                Logger.Debug("Sending email...");
                _mailer.SendMail(subject, message.ToString(), members);
            });
        }

        private WebClient CreateWebClient()
        {
            return new WebClient
            {
                Credentials = new NetworkCredential(Configuration.TfsUsername, Configuration.TfsPassword),
                Encoding = Encoding.UTF8
            };
        }

        public ICollection<Comment> GetChangesetComments(Changeset changeset)
        {
            Logger.Trace("Processing comments for changeset {0}", changeset);
            var uri = new Uri(new Uri(Configuration.TfsUrl), String.Format(@"_apis/discussion/threads?artifactUri={0}", changeset.TfsUri));
            
            string result;
            using (var webClient = CreateWebClient())
            {
                result = webClient.DownloadString(uri);
            }

            var response = new List<Comment>();
            dynamic temp = JsonConvert.DeserializeObject(result);
            foreach (var each in temp.value)
            {
                
                foreach (var commentData in each.comments)
                {
                    if ((bool)commentData.isDeleted)
                    {
                        continue;
                    }

                    var author = new Author()
                    {
                        Id = commentData.author,
                        DisplayName = commentData.authorDisplayName
                    };

                    var comment = new Comment()
                    {
                        Changeset = changeset,
                        Author = author,
                        Content = commentData.content,
                        LastUpdatedDate = DateTime.Parse(commentData.lastUpdatedDate.ToString())
                    };

                    Logger.Trace("Comment: {0}", comment);
                    response.Add(comment);
                }
            }

            return response;
        }

        public ICollection<Changeset> GetCommentedChangesets(ICollection<Changeset> changesets)
        {
            var changesetUris = changesets.Select(each => each.TfsUri);
            var changesetUrisJson = JsonConvert.SerializeObject(changesetUris.ToArray());

            var uri = new Uri(new Uri(Configuration.TfsUrl), @"_apis/discussion/threadsBatch");

            string result;
            using (var webClient = CreateWebClient())
            {
                webClient.Headers.Add("Content-Type", @"application/json");
                result = webClient.UploadString(uri, changesetUrisJson);    
            }

            dynamic temp = JsonConvert.DeserializeObject(result);
            var uris = new List<string>();
            foreach (var each in temp.value)
            {
                uris.Add(each.Name.ToString());
            }

            var response = changesets.Where(each => uris.Contains(each.TfsUri)).ToList();
            foreach (var changeset in response)
            {
                Logger.Trace("Commented changeset: {0}", changeset);                
            }

            return response;
        }

        public ICollection<Changeset> GetLatestChangeset(string project, int count = 100)
        {
            var uri = new Uri(new Uri(Configuration.TfsUrl), String.Format(@"{0}/_api/_versioncontrol/history?__v=4", Uri.EscapeUriString(project)));

            var searchCriteria = String.Format(@"{{""itemPath"":""$/{0}"",""itemVersion"":"""",""top"":{1}}}", project, count);
            var values = new NameValueCollection
            {
                { "searchCriteria", searchCriteria }
            };

            byte[] result;
            using (var webClient = CreateWebClient())
            {
                result = webClient.UploadValues(uri, values);
            }

            var historyString = Encoding.UTF8.GetString(result);
            var ids = new List<Changeset>();

            dynamic history = JsonConvert.DeserializeObject(historyString);
            var results = history.results;
            foreach (var each in results)
            {
                var changeList = each.changeList;
                var changeset = new Changeset(Convert.ToInt32(changeList.changesetId))
                {
                    OwnerId = changeList.ownerId,
                    OwnerDisplayName = changeList.ownerDisplayName,
                    Comment = changeList.comment,
                    Project = project
                };

                Logger.Trace("Changeset: {0}", changeset);
                ids.Add(changeset);
            }

            return ids;
        }
    }
}
