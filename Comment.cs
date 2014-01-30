using System;
using System.Security.Policy;

namespace Capybara
{
    public class Comment
    {
        public Changeset Changeset { get; set; }
        public Author Author { get; set; }
        public string Content { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public override string ToString()
        {
            return string.Format("Author: {0}, Content: {1}, LastUpdatedDate: {2}", Author, Content, LastUpdatedDate.ToLocalTime());
        }
    }
}