using Cysharp.Threading.Tasks;

namespace Game.Config
{
    public interface IConfig
    {
        UniTask LoadAsync();
    }
}
