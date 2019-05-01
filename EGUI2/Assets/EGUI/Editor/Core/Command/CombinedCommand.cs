using System.Linq;
using UnityEngine;

namespace EGUI
{
    public class CombinedCommand : Command
    {
        private Command[] mCommands;

        public Command[] commands
        {
            get { return mCommands; }
        }

        public CombinedCommand()
        {
        }

        public CombinedCommand(Command[] commands)
        {
            mCommands = commands;
        }

        public override void Execute()
        {
            for (var i = 0; i < mCommands.Length; i++)
                mCommands[i].Execute();
        }

        public override void Undo()
        {
            for (var i = mCommands.Length - 1; i >= 0; i--)
                mCommands[i].Undo();
        }

        public override bool Merge(Command command, bool checkOnly = false)
        {
            var cmd = command as CombinedCommand;
            for (var i = 0; i < commands.Length; i++)
            {
                if (cmd.commands[i].GetType() != commands[i].GetType())
                    return false;
            }

            for (var i = 0; i < commands.Length; i++)
            {
                if (!cmd.commands[i].Merge(commands[i], true))
                    return false;
            }

            if (!checkOnly)
            {
                for (var i = 0; i < commands.Length; i++)
                {
                    cmd.commands[i].Merge(commands[i]);
                }
            }

            return true;
        }
    }
}