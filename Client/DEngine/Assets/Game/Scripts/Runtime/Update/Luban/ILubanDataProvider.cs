namespace Game.Update.DataTable
{
    public interface ILubanDataProvider : IDataProvider
    {
        public Tables Tables { get; }
    }
}