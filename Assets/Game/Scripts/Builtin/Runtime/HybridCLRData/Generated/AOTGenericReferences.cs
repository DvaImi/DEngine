public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// Game.dll
	// GameFramework.dll
	// UniTask.dll
	// UnityEngine.CoreModule.dll
	// UnityGameFramework.Runtime.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// GameFramework.DataTable.IDataTable<object>
	// GameFramework.Fsm.FsmState<object>
	// GameFramework.Fsm.IFsm<object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.List<object>
	// System.EventHandler<object>
	// System.Nullable<int>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityEvent<int>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Hotfix.ProcedurePreload.<PreloadResources>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Hotfix.ProcedurePreload.<PreloadResources>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Game.Hotfix.HotfixLauncher.<UnLoadLauncher>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Game.Hotfix.HotfixLauncher.<UnLoadLauncher>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Hotfix.ProcedurePreload.<PreloadResources>d__4>(Game.Hotfix.ProcedurePreload.<PreloadResources>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Hotfix.HotfixLauncher.<UnLoadLauncher>d__2>(Game.Hotfix.HotfixLauncher.<UnLoadLauncher>d__2&)
		// Cysharp.Threading.Tasks.UniTask<object> Game.AwaitableUtility.LoadAssetAsync<object>(UnityGameFramework.Runtime.ResourceComponent,string)
		// System.Void GameFramework.Fsm.FsmState<object>.ChangeState<object>(GameFramework.Fsm.IFsm<object>)
		// object GameFramework.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void GameFramework.Fsm.IFsm<object>.SetData<object>(string,object)
		// object GameFramework.GameFrameworkEntry.GetModule<object>()
		// System.Void GameFramework.Procedure.IProcedureManager.StartProcedure<object>()
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Hotfix.WelcomeForm.<DownLoadTest>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Hotfix.WelcomeForm.<DownLoadTest>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Hotfix.WelcomeForm.<WebRequestTest>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Hotfix.WelcomeForm.<WebRequestTest>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Game.Hotfix.WelcomeForm.<ChangeLanguage>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Game.Hotfix.WelcomeForm.<ChangeLanguage>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Hotfix.WelcomeForm.<DownLoadTest>d__6>(Game.Hotfix.WelcomeForm.<DownLoadTest>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Hotfix.WelcomeForm.<WebRequestTest>d__7>(Game.Hotfix.WelcomeForm.<WebRequestTest>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Hotfix.WelcomeForm.<ChangeLanguage>d__8>(Game.Hotfix.WelcomeForm.<ChangeLanguage>d__8&)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// GameFramework.DataTable.IDataTable<object> UnityGameFramework.Runtime.DataTableComponent.GetDataTable<object>()
		// bool UnityGameFramework.Runtime.FsmComponent.DestroyFsm<object>()
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object,object>(string,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Warning<object>(string,object)
	}
}