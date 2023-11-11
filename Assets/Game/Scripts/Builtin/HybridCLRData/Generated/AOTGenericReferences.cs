using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DEngine.Runtime.dll",
		"DEngine.dll",
		"System.dll",
		"UniTask.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.TaskPool<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>
	// DEngine.DEngineLinkedList.Enumerator<object>
	// DEngine.DEngineLinkedList<object>
	// DEngine.DataTable.IDataTable<object>
	// DEngine.Fsm.Fsm<object>
	// DEngine.Fsm.FsmState<object>
	// DEngine.Fsm.IFsm<object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<object>
	// System.EventHandler<object>
	// System.Func<int>
	// System.Nullable<int>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.ConditionalWeakTable.CreateValueCallback<object,object>
	// System.Runtime.CompilerServices.ConditionalWeakTable.Enumerator<object,object>
	// System.Runtime.CompilerServices.ConditionalWeakTable<object,object>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2>(Game.Update.UIExtension.<FadeToAlphaByUniTask>d__2&)
		// object DEngine.DEngineEntry.GetModule<object>()
		// System.Void DEngine.DEngineLog.Error<object,object,object>(string,object,object,object)
		// System.Void DEngine.DEngineLog.Error<object,object>(string,object,object)
		// System.Void DEngine.DEngineLog.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void DEngine.DEngineLog.Info<object,object>(string,object,object)
		// System.Void DEngine.DEngineLog.Info<object>(string,object)
		// System.Void DEngine.DEngineLog.Warning<object>(string,object)
		// DEngine.DataTable.IDataTable<object> DEngine.DataTable.IDataTableManager.GetDataTable<object>()
		// System.Void DEngine.Fsm.Fsm<object>.ChangeState<object>()
		// System.Void DEngine.Fsm.FsmState<object>.ChangeState<object>(DEngine.Fsm.IFsm<object>)
		// object DEngine.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void DEngine.Fsm.IFsm<object>.SetData<object>(string,object)
		// bool DEngine.Fsm.IFsmManager.DestroyFsm<object>()
		// System.Void DEngine.Procedure.IProcedureManager.StartProcedure<object>()
		// DEngine.DataTable.IDataTable<object> DEngine.Runtime.DataTableComponent.GetDataTable<object>()
		// bool DEngine.Runtime.FsmComponent.DestroyFsm<object>()
		// System.Void DEngine.Runtime.Log.Error<object,object,object>(string,object,object,object)
		// System.Void DEngine.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void DEngine.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void DEngine.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void DEngine.Runtime.Log.Info<object>(string,object)
		// System.Void DEngine.Runtime.Log.Warning<object>(string,object)
		// string DEngine.Utility.Text.Format<object,object,object,object>(string,object,object,object,object)
		// string DEngine.Utility.Text.Format<object,object,object>(string,object,object,object)
		// string DEngine.Utility.Text.Format<object,object>(string,object,object)
		// string DEngine.Utility.Text.Format<object>(string,object)
		// string DEngine.Utility.Text.ITextHelper.Format<object,object,object,object>(string,object,object,object,object)
		// string DEngine.Utility.Text.ITextHelper.Format<object,object,object>(string,object,object,object)
		// string DEngine.Utility.Text.ITextHelper.Format<object,object>(string,object,object)
		// string DEngine.Utility.Text.ITextHelper.Format<object>(string,object)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Game.Update.UpdateUGUIForm.<Close>d__25>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Game.Update.UpdateUGUIForm.<Close>d__25&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Game.Update.UpdateUGUIForm.<Close>d__25>(Game.Update.UpdateUGUIForm.<Close>d__25&)
		// object UnityEngine.Component.GetComponent<object>()
		// System.Void UnityEngine.Component.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// System.Void UnityEngine.GameObject.GetComponentsInChildren<object>(bool,System.Collections.Generic.List<object>)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
	}
}