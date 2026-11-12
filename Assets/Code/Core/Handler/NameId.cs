namespace Core.Handler
{
    public class NameId : IHandler {
        private string _id;

        public NameId(string id) {
            _id = id;
        }

        public NameId(IHandler id) : this(id.ToString()) { }

        public int CompareTo(IHandler another) {
            return _id.CompareTo(another.ToString());
        }

        public override string ToString() {
            return _id;
        }

        public bool EqualsIgnoreCase(IHandler anotherName) {
            return _id.ToLower().Equals(anotherName.ToString().ToLower());
        }

        public bool Equals(IHandler another) {
            return _id.Equals(another.ToString());
        }

        public override bool Equals(object obj) {
            if (obj is IHandler handler)
                return Equals(handler);
            return false;
        }

        public override int GetHashCode() {
            return _id.GetHashCode();
        }
    }

}