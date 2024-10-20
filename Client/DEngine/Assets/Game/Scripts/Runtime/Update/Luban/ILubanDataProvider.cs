namespace Game.Update.DataTable
{
    public interface ILubanDataProvider : IGameModule
    {
        public Tables Tables { get; }
    }
}