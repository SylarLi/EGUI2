namespace EGUI
{
    [Persistence]
    public abstract class Object
    {
        private bool mDisposed;

        public bool disposed { get { return mDisposed; } }

        public virtual void Update() { }

        public virtual void Dispose() { OnDestroy(); mDisposed = true; }

        public virtual void OnDestroy() { }

        public static bool operator ==(Object obj1, Object obj2)
        {
            if (ReferenceEquals(obj1, null))
            {
                return ReferenceEquals(obj2, null);
            }
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Object obj1, Object obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            Object o = (Object)obj;
            if (ReferenceEquals(o, null) || o.IsNull())
            {
                return IsNull();
            }
            return ReferenceEquals(this, o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private bool IsNull()
        {
            return disposed;
        }
    }
}
