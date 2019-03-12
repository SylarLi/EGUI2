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
            foreach (var command in mCommands)
            {
                command.Execute();
            }
        }

        public override void Undo()
        {
            foreach (var command in mCommands)
            {
                command.Undo();
            }
        }
    }
}
