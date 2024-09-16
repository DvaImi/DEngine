using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.LubanTable
{
    public interface ILubanModule : IGameModule
    {
        public Tables Tables { get; }

        ILubanModule Initialize();

        UniTask LoadAsync();
    }
}