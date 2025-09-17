namespace Handler
{
    public class EntityId : IHandler {
    private int id;

    public EntityId(int id) {
        this.id = id;
    }

    public EntityId(IHandler other) {
        this.id = int.Parse(other.ToString());
    }

    public int CompareTo(IHandler another) {
        return id.ToString().CompareTo(another.ToString());
    }

    public override string ToString() {
        return id.ToString();
    }

    public int ToInt() {
        return id;
    }

    public bool EqualsIgnoreCase(IHandler anotherName) {
        return Equals(anotherName);
    }

    public bool Equals(IHandler another) {
        return id.ToString() == another.ToString();
    }

    public override bool Equals(object obj) {
        if (obj is IHandler handler)
            return Equals(handler);
        return false;
    }

    public override int GetHashCode() {
        return id.GetHashCode();
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