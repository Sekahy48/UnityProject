namespace Handler
{
    public class NameId : IHandler {
        private string id;

        public NameId(string id) {
            this.id = id;
        }

        public NameId(IHandler id) : this(id.ToString()) { }

        public int CompareTo(IHandler another) {
            return id.CompareTo(another.ToString());
        }

        public override string ToString() {
            return id;
        }

        public bool EqualsIgnoreCase(IHandler anotherName) {
            return id.ToLower().Equals(anotherName.ToString().ToLower());
        }

        public bool Equals(IHandler another) {
            return id.Equals(another.ToString());
        }

        public override bool Equals(object obj) {
            if (obj is IHandler handler)
                return Equals(handler);
            return false;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }
    }

}