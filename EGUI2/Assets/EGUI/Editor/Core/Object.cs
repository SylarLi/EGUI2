using UnityEngine;

namespace EGUI
{
    [Persistence]
    public abstract class Object
    {
        [PersistentField]
        private string mName = "";

        public string name
        {
            get { return mName; }
            set { mName = value; }
        }
        
        [PersistentField]
        protected bool mDisposed;

        [PersistentField]
        protected bool mInternalDisposed;

        internal virtual void MarkInternalDisposed(bool value)
        {
            mInternalDisposed = value;
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            OnDestroy();
            mDisposed = true;
            Database.RemoveObject(this);
        }

        public virtual void OnDestroy()
        {
        }

        public Object()
        {
            Database.AddObject(this);
        }

        public static bool operator ==(Object obj1, Object obj2)
        {
            return ReferenceEquals(obj1, null) ? ReferenceEquals(obj2, null) : obj1.Equals(obj2);
        }

        public static bool operator !=(Object obj1, Object obj2)
        {
            return !(obj1 == obj2);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", name, GetType().Name);
        }

        public override bool Equals(object obj)
        {
            var o = (Object) obj;
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

        protected bool IsNull()
        {
            return mDisposed || mInternalDisposed;
        }
    }
}