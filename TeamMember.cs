namespace Capybara
{
    public class TeamMember
    {
        protected bool Equals(TeamMember other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((TeamMember)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(TeamMember left, TeamMember right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TeamMember left, TeamMember right)
        {
            return !Equals(left, right);
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, DisplayName: {1}, Email: {2}", Id, DisplayName, Email);
        }
    }
}