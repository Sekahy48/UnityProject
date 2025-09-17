namespace Handler
{

    using System;

    public interface IHandler : IComparable<IHandler>
    {
        public string ToString();
        public bool EqualsIgnoreCase(IHandler another);
        public bool Equals(IHandler another);

        
    }
}


