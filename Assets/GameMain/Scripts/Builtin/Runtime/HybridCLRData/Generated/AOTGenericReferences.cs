public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
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
	// System.Collections.Generic.List.Enumerator<object>
	// System.EventHandler<object>
	// System.Nullable<int>
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
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GeminiLion.Hotfix.WelcomeForm.<WebRequestTest>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GeminiLion.Hotfix.WelcomeForm.<WebRequestTest>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<GeminiLion.Hotfix.WelcomeForm.<WebRequestTest>d__4>(GeminiLion.Hotfix.WelcomeForm.<WebRequestTest>d__4&)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// System.Void UnityGameFramework.Runtime.DataNodeComponent.SetData<object>(string,object)
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