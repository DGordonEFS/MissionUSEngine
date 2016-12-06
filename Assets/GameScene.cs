using UnityEngine;
using System.Collections;

using DarkChariotStudios.Dialogs;

public class GameScene : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        //CreateDialog();
        StartCoroutine(OnStart());
    }

    IEnumerator OnStart()
    {
        yield return null;
    }

    public void CreateDialog()
    {
        /*
        var dialog = Resources.Load<Dialog>("dialog_test");

        dialog.DefaultNodeId = "n01";

        var node = dialog.CreateNode("n01");
        node.AddPrompt("Hello World.").Done()
            .AddResponse("Who am I?", "n02").Done()
            .AddResponse("Where am I?", "n02").Done()
            .AddResponse("Why am I?", "n02").Done()
            .AddResponse("I have a headache.", "n02").Done();

        node = dialog.CreateNode("n02");
        node.AddPrompt("P2.").Done()
            .AddResponse("R1").Done()
            .AddResponse("R2").Done()
            .AddResponse("R3").Done()
            .AddResponse("R4").Done();
            */
    }
    
	
	// Update is called once per frame
	void Update () {
	
	}
}
