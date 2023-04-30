public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// GameFramework.dll
	// UnityEngine.CoreModule.dll
	// UnityGameFramework.Runtime.dll
	// mscorlib.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// GameFramework.DataTable.IDataTable<object>
	// GameFramework.Fsm.FsmState<object>
	// GameFramework.Fsm.IFsm<object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.EventHandler<object>
	// System.Nullable<int>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskCompletionSource<byte>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityEvent<int>
	// }}

	public void RefMethods()
	{
		// System.Void GameFramework.Fsm.FsmState<object>.ChangeState<object>(GameFramework.Fsm.IFsm<object>)
		// object GameFramework.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void GameFramework.Fsm.IFsm<object>.SetData<object>(string,object)
		// object GameFramework.GameFrameworkEntry.GetModule<object>()
		// System.Void GameFramework.Procedure.IProcedureManager.StartProcedure<object>()
		// string GameFramework.Utility.Text.Format<object>(string,object)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAssets>d__23>(System.Runtime.CompilerServices.TaskAwaiter<object>&,GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAssets>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAsset>d__24<object>>(GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAsset>d__24<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAssets>d__23>(GeminiLion.Hotfix.AwaitUtility.<AwaitLoadAssets>d__23&)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// GameFramework.DataTable.IDataTable<object> UnityGameFramework.Runtime.DataTableComponent.GetDataTable<object>()
		// bool UnityGameFramework.Runtime.FsmComponent.DestroyFsm<object>()
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object,object>(string,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Warning<object>(string,object)
	}
}