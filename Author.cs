namespace Capybara
{
    public class Author
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }

        protected bool Equals(Author other)
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
            return Equals((Author)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(Author left, Author right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Author left, Author right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, DisplayName: {1}", Id, DisplayName);
        }
    }
}