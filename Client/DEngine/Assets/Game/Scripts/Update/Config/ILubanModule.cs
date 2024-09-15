using Cysharp.Threading.Tasks;
using Game.Config;

namespace Game.Config
{
    public interface ILubanModule : IGameModule
    {
        public Tables Tables { get; }

        UniTask LoadAsync();
    }
}