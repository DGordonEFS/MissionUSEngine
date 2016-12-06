using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public static class ExtensionMethods
{
    public static void SetInteractable(this Toggle toggle, bool value)
    {
        toggle.interactable = value;

        CanvasGroup canvasGroup = toggle.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = canvasGroup.interactable = value;
    }

    public static string ToProperName(this string target)
    {
        string name = "";
        string[] words = target.Split(new string[] { " " }, StringSplitOptions.None);

        for (int i = 0; i < words.Length; i++)
        {
            if (i > 0)
                name += " ";

            string word = words[i];
            word = word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1).ToLower();
            name += word;
        }

        return name;
    }

    public static T Random<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default(T);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static List<T> RandomExclusive<T>(this List<T> list, int count)
    {
        if (list.Count == 0)
            return new List<T>();

        List<T> rList = new List<T>();

        for (int i = 0; i < list.Count; i++)
        {
            rList.Add(list[i]);
        }

        List<T> finalList = new List<T>();

        for (int i = 0; i < count; i++)
        {
            T item = rList.Random();
            rList.Remove(item);
            finalList.Add(item);
        }

        return finalList;
    }
}
