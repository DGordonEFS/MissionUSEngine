using UnityEngine;
using System.Collections;

using DarkChariotStudios.Dialogs;
public class DialogPlayerTester : MonoBehaviour {

    public Dialog Dialog;

    public void Open()
    {
        StartCoroutine(OnOpen());
    }

    IEnumerator OnOpen()
    {
        yield return AdventureEngine.GetInstance().Dispatch(new OpenDialogAction(Dialog.Id));
        yield return new WaitUntil(() => AdventureEngine.GetInstance().State.CurrentDialog == null);
        yield return AdventureEngine.GetInstance().Dispatch(new SetVariableAction("is_working", true));
    }

    public void Close()
    {
        StartCoroutine(OnClose());
    }

    IEnumerator OnClose()
    {
        yield return AdventureEngine.GetInstance().Dispatch(new CloseDialogAction());
    }
}
