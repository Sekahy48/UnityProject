namespace Core.Handler
{
    public class EntityId : IHandler {
    private int _id;

    public EntityId(int id) {
        _id = id;
    }

    public EntityId(IHandler other) {
        _id = int.Parse(other.ToString());
    }

    public int CompareTo(IHandler another) {
        return _id.ToString().CompareTo(another.ToString());
    }

    public override string ToString() {
        return _id.ToString();
    }

    public int ToInt() {
        return _id;
    }

    public bool EqualsIgnoreCase(IHandler anotherName) {
        return Equals(anotherName);
    }

    public bool Equals(IHandler another) {
        return _id.ToString() == another.ToString();
    }

    public override bool Equals(object obj) {
        if (obj is IHandler handler)
            return Equals(handler);
        return false;
    }

    public override int GetHashCode() {
        return _id.GetHashCode();
    }

    public static bool operator ==(EntityId left, IHandler right) {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(EntityId left, IHandler right) {
        return !(left == right);
    }
}

}