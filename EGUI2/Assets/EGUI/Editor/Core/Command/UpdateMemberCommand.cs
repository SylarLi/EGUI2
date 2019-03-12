using System.Reflection;
using UnityEngine;

namespace EGUI
{
    /// <summary>
    /// Update member's value for object's fields and properties.
    /// </summary>
    public class UpdateMemberCommand : Command
    {
        private object mObj;

        private string mMemberName;

        private MemberInfo[] mMemberPath;

        private object mMemberValue;

        private object mMemberRaw;

        public UpdateMemberCommand() { }

        public UpdateMemberCommand(object obj, string name, object value)
        {
            mObj = obj;
            mMemberValue = value;
            mMemberName = name;
            mMemberPath = CoreUtil.FindMemberPath(obj.GetType(), name);
            mMemberRaw = CoreUtil.GetMemberValue(mMemberPath, obj);
        }

        public override void Execute()
        {
            CoreUtil.SetMemberValue(mMemberPath, mObj, mMemberValue);
        }

        public override void Undo()
        {
            CoreUtil.SetMemberValue(mMemberPath, mObj, mMemberRaw);
        }

        public override bool Merge(Command command)
        {
            var cmd = command as UpdateMemberCommand;
            if (mObj == cmd.mObj && mMemberName == cmd.mMemberName)
            {
                mMemberValue = cmd.mMemberValue;
                return true;
            }
            return false;
        }
    }
}
