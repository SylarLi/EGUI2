namespace EGUI
{
    public class CombinedCommand : Command
    {
        private Command[] mCommands;

        public CombinedCommand() { }

        public CombinedCommand(Command[] commands)
        {
            mCommands = commands;
        }

        public override void Execute()
        {
            for (int i = 0; i < mCommands.Length; i++)
                mCommands[i].Execute();
        }

        public override void Undo()
        {
            for (int i = mCommands.Length - 1; i >= 0; i--)
                mCommands[i].Execute();
        }
    }
}
