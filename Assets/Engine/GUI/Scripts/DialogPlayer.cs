using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using DG.Tweening;
using DarkChariotStudios.Dialogs;

public class DialogPlayer : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    
    public TextMeshProUGUI PromptText;
    public Button Response1;
    public Button Response2;
    public Button Response3;
    public Button Response4;

    private List<Button> _responsesButtons = new List<Button>();

    public IEnumerator Close()
    {
        AdventureEngine.GetInstance().State.CurrentDialog.onDialogNodeChange -= OnDialogNodeChange;
        yield return GetComponent<CanvasGroup>().DOFade(0, 0.25f).WaitForCompletion();
        IsOpen = false;
    }

    public IEnumerator Open(string dialogId, string startingNodeId = null)
    {
        AdventureEngine.GetInstance().State.CurrentDialog.onDialogNodeChange -= OnDialogNodeChange;
        AdventureEngine.GetInstance().State.CurrentDialog.onDialogNodeChange += OnDialogNodeChange;
        
        OnDialogNodeChange(AdventureEngine.GetInstance().State.CurrentDialog.CurrentNode);

        yield return GetComponent<CanvasGroup>().DOFade(1, 0.25f).WaitForCompletion();
        IsOpen = true;
    }
    

    private void OnDialogNodeChange(DialogNode node)
    {
        PromptText.text = node.CurrentPrompt.Text;

        for (int i = 0; i < _responsesButtons.Count; i++)
        {
            if (i >= node.CurrentResponses.Count)
            {
                _responsesButtons[i].gameObject.SetActive(false);
                continue;
            }

            DialogResponse response = node.CurrentResponses[i];

            _responsesButtons[i].gameObject.SetActive(true);
            _responsesButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = response.Text;
        }
    }

    void Awake()
    {
        _responsesButtons.Add(Response1);
        _responsesButtons.Add(Response2);
        _responsesButtons.Add(Response3);
        _responsesButtons.Add(Response4);

        _responsesButtons[0].GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(OnResponse(0));
        });

        _responsesButtons[1].GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(OnResponse(1));
        });

        _responsesButtons[2].GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(OnResponse(2));
        });

        _responsesButtons[3].GetComponent<Button>().onClick.AddListener(() =>
        {
            StartCoroutine(OnResponse(3));
        });
    }

    IEnumerator OnResponse(int index)
    {
        yield return AdventureEngine.GetInstance().Dispatch(new DialogResponseAction(index));
    }

    void OnDestroy()
    {
    }
    
}



