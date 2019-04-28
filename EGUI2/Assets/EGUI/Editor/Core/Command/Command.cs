using System;
using System.Collections.Generic;

namespace EGUI
{
    public abstract class Command
    {
        public delegate void OnCommandPushed(Command command);

        public delegate void OnUndoPerformed(Command command);

        public delegate void OnRedoPerformed(Command command);

        public static OnCommandPushed onCommandPushed = command => { };

        public static OnUndoPerformed onUndoPerformed = command => { };

        public static OnRedoPerformed onRedoPerformed = command => { };

        private static int BreakDurationTicks = 5000000;

        private static List<Command> stack = new List<Command>();

        private static int index = -1;

        private static int anchor = -1;

        public long createTime = DateTime.Now.Ticks;

        public static void Execute(Command command)
        {
            command.Execute();
            if (command.UndoRedoable)
                Push(command);
        }

        public static bool Undoable()
        {
            return index >= 0;
        }

        public static void PerformUndo()
        {
            if (Undoable())
            {
                var command = Pop();
                command.Undo();
                onUndoPerformed(command);
            }
        }

        public static bool Redoable()
        {
            return stack.Count > 0 && index < stack.Count - 1;
        }

        public static void PerformRedo()
        {
            if (Redoable())
            {
                index += 1;
                var command = stack[index];
                command.Execute();
                onRedoPerformed(command);
            }
        }

        private static void Push(Command command)
        {
            if (index >= -1 && index < stack.Count - 1)
                stack.RemoveRange(index + 1, stack.Count - 1 - index);
            var last = index >= 0 && index <= stack.Count - 1 ? stack[index] : null;
            if (last != null && last.GetType() == command.GetType() &&
                command.createTime - last.createTime > 0 &&
                command.createTime - last.createTime < BreakDurationTicks &&
                last.Merge(command))
            {
                return;
            }

            stack.Add(command);
            index = stack.Count - 1;
            onCommandPushed(command);
        }

        private static Command Pop()
        {
            var command = stack[index];
            index -= 1;
            return command;
        }

        public static void Clear()
        {
            stack.Clear();
            index = -1;
            anchor = -1;
        }

        public static void Anchor()
        {
            anchor = index;
        }

        public static bool Floating()
        {
            return anchor != index;
        }

        public virtual bool UndoRedoable
        {
            get { return true; }
        }

        public virtual void Execute()
        {
        }

        public virtual void Undo()
        {
        }

        public virtual bool Merge(Command command, bool checkOnly = false)
        {
            return false;
        }
    }
}