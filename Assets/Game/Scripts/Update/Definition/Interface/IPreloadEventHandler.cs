namespace Game.Update
{
	/// <summary>
	/// 预加载后处理事件
	/// </summary>
	/// <typeparam name="T">预加载完成的类型</typeparam>
	public interface IPreloadEventHandler
	{
		void Run();
	} 
}
