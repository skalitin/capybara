using System;
using System.Linq;

namespace Capybara
{
    public class Changeset
    {
        public static string TFS_CHANGESET_PREFIX = @"vstfs:///versioncontrol/changeset/";

        public Changeset()
        {
        }

        public Changeset(int id)
        {
            Id = id;
        }

        public Changeset(string tfsUrl)
        {
            var id = String.Concat(tfsUrl.Skip(TFS_CHANGESET_PREFIX.Length));
            Id = Convert.ToInt32(id);
        }

        public int Id { get; set; }
        public string Project { get; set; }
        public string Comment { get; set; }
        public string OwnerId { get; set; }
        public string OwnerDisplayName { get; set; }

        public string TfsUri
        {
            get
            {
                return String.Format(@"{0}{1}", TFS_CHANGESET_PREFIX, Id);
            }
        }

        public string WebAccessUri
        {
            get
            {
                return String.Format(@"{0}{1}/_versionControl/changeset/{2}", Configuration.TfsUrl, Uri.EscapeUriString(Project), Id);
            }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Comment: {1}, Uri: {2}", Id, Comment, WebAccessUri);
        }
    }
}