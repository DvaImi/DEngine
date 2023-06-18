using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DEngine.Runtime.dll",
		"DEngine.dll",
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DEngine.DataTable.IDataTable<object>
	// DEngine.Fsm.FsmState<object>
	// DEngine.Fsm.IFsm<object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.List<object>
	// System.EventHandler<object>
	// System.Nullable<int>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,Game.Hotfix.UIExtension.<FadeToAlphaByUniTask>d__2>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,Game.Hotfix.UIExtension.<FadeToAlphaByUniTask>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Hotfix.UIExtension.<FadeToAlphaByUniTask>d__2>(Game.Hotfix.UIExtension.<FadeToAlphaByUniTask>d__2&)
		// object DEngine.DEngineEntry.GetModule<object>()
		// System.Void DEngine.Fsm.FsmState<object>.ChangeState<object>(DEngine.Fsm.IFsm<object>)
		// object DEngine.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void DEngine.Fsm.IFsm<object>.SetData<object>(string,object)
		// System.Void DEngine.Procedure.IProcedureManager.StartProcedure<object>()
		// DEngine.DataTable.IDataTable<object> DEngine.Runtime.DataTableComponent.GetDataTable<object>()
		// bool DEngine.Runtime.FsmComponent.DestroyFsm<object>()
		// System.Void DEngine.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void DEngine.Runtime.Log.Error<object,object,object>(string,object,object,object)
		// System.Void DEngine.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void DEngine.Runtime.Log.Info<object>(string,object)
		// System.Void DEngine.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void DEngine.Runtime.Log.Warning<object>(string,object)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Game.Hotfix.HotfixUGUIForm.<Close>d__25>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Game.Hotfix.HotfixUGUIForm.<Close>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Hotfix.HotfixUGUIForm.<Close>d__25>(Game.Hotfix.HotfixUGUIForm.<Close>d__25&)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
	}
}