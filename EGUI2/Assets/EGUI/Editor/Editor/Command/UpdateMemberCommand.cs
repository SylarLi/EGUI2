using System;
using System.Reflection;

namespace EGUI
{
    public class UpdateMemberCommand : Command
    {
        private Type mType;

        private object mObj;

        private string mMemberName;

        private MemberInfo[] mMemberPath;

        private object mMemberValue;

        private object mMemberRaw;

        public UpdateMemberCommand() { }

        public UpdateMemberCommand(object obj, string name, object value)
        {
            mType = obj is Type ? obj as Type : obj.GetType();
            mObj = obj is Type ? null : obj;
            mMemberValue = value;
            mMemberName = name;
            mMemberPath = CoreUtil.FindMemberPath(mType, name);
            mMemberRaw = CoreUtil.GetMemberValue(mMemberPath, mObj);
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
            if (mType == cmd.mType && 
                mObj == cmd.mObj && 
                mMemberName == cmd.mMemberName)
            {
                mMemberValue = cmd.mMemberValue;
                return true;
            }
            return false;
        }
    }
}
