using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;

public class OneLinksToAllScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public Text[] texts;

    private List<string> queryLinks = new List<string>();
    private string queryCheckBackURL = "http://en.wikipedia.org/w/api.php?action=query&format=json&prop=linkshere&lhprop=title&lhlimit=max&lhnamespace=0";
    private string queryGetRandomURL = "https://en.wikipedia.org/w/api.php?action=query&format=json&list=random&rnlimit=1&rnnamespace=0";
    private string queryLeadsToURL = "http://en.wikipedia.org/w/api.php?action=query&format=json&prop=links&pllimit=1&plnamespace=0";
    private string queryDisambigURL = "http://en.wikipedia.org/w/api.php?action=query&format=json&prop=pageprops&ppprop=disambiguation";
    private string title1 = "";
    private string title2 = "";
    private string contvar;
    private bool error = false;
    private bool activated = false;

    private List<string> addedArticles = new List<string>();
    private int curIndex = 0;

    private char[] keySet1 = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private char[] keySet2 = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
    private char[] keySet3 = new char[] { 'á', 'é', 'í', 'ó', 'ú', 'à', 'è', 'ì', 'ò', 'ù', 'ä', 'ë', 'ï', 'ö', 'ü', 'ā', 'ē', 'ī', 'ō', 'ū', 'ã', 'ñ', 'õ', ' ', ' ', ' ' };
    private char[] keySet4 = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '␣', '(', ')', '\'', '.', ',', '–', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
    private char[] keySetSolve = new char[] { 'C', 'O', 'N', 'G', 'R', 'A', 'T', 'U', 'L', 'A', 'T', 'I', 'O', 'N', 'S', 'Y', 'O', 'U', 'R', 'E', 'D', 'O', 'N', 'E', '!', ' ' };
    private int keyIndex = 0;

    private Coroutine load;
    private Coroutine loadlinks;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void OnActivate()
    {
        activated = true;
        load = StartCoroutine(Loading(0));
        StartCoroutine(QueryProcess());
    }

    void PressButton(KMSelectable pressed)
    {
        if ((moduleSolved != true && load == null && activated) || (pressed == buttons[4] && error))
        {
            if (pressed == buttons[0] && !texts[1].text.Equals(""))
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                addedArticles.Add(texts[1].text);
                curIndex++;
                texts[1].text = "";
                texts[3].text = (curIndex + 1).ToString();
            }
            else if (pressed == buttons[1] && curIndex != 0)
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = addedArticles[curIndex - 1];
                addedArticles.RemoveAt(curIndex - 1);
                curIndex--;
                texts[3].text = (curIndex + 1).ToString();
            }
            else if (pressed == buttons[2] && !texts[1].text.Equals(""))
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = texts[1].text.Substring(0, texts[1].text.Length-1);
            }
            else if (pressed == buttons[3] && !texts[1].text.Equals(""))
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                texts[1].text = "";
            }
            else if (pressed == buttons[4] && Valid())
            {
                pressed.AddInteractionPunch();
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                if (error)
                {
                    Debug.LogFormat("[One Links To All #{0}] Submit has been pressed, module disarmed!", moduleId);
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                    return;
                }
                Debug.LogFormat("[One Links To All #{0}] ==Submitted Path==", moduleId);
                texts[3].text = "";
                if (texts[1].text == "" && addedArticles.Count == 0)
                {
                    StartCoroutine(noSub());
                }
                else
                {
                    StartCoroutine(finalCheck());
                }
            }
            else if (pressed == buttons[31] && keyIndex != 0)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 0;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if(i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0011f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet1[i - 5].ToString();
                }
            }
            else if (pressed == buttons[32] && keyIndex != 1)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 1;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    else if (i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.3f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet2[i - 5].ToString();
                }
            }
            else if (pressed == buttons[33] && keyIndex != 2)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 2;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if(i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet3[i - 5].ToString();
                }
            }
            else if (pressed == buttons[34] && keyIndex != 3)
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                keyIndex = 3;
                for (int i = 5; i < 31; i++)
                {
                    if (i == 11)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    else if (i == 14)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 15)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if (i == 16)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 17)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                    }
                    else if (i == 20)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                    }
                    else if (i == 21)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.1f, 0.51f);
                    }
                    else if(i == 27)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                    }
                    else if (i == 29)
                    {
                        buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    }
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySet4[i - 5].ToString();
                }
            }
            else if (Array.IndexOf(buttons, pressed) > 4 && pressed != buttons[31] && pressed != buttons[32] && pressed != buttons[33] && pressed != buttons[34] && pressed.GetComponentInChildren<TextMesh>().text != " ")
            {
                if (keyIndex == 3 && pressed == buttons[15])
                {
                    if (texts[1].text.Trim() != "")
                    {
                        pressed.AddInteractionPunch(0.25f);
                        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                        texts[1].text += " ";
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    pressed.AddInteractionPunch(0.25f);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
                    texts[1].text += pressed.GetComponentInChildren<TextMesh>().text;
                }
            }
        }
    }

    private bool Valid()
    {
        if (error)
            return true;
        if (addedArticles.Count != 0 && texts[1].text != "")
        {
            return true;
        }
        else if (addedArticles.Count == 0 && texts[1].text != "")
        {
            return true;
        }
        else if (addedArticles.Count == 0 && texts[1].text == "")
        {
            return true;
        }
        else if (addedArticles.Count != 0 && texts[1].text == "")
        {
            return false;
        }
        return false;
    }

    private void DealWithError(int type)
    {
        if (type == 0)
        {
            texts[0].text = "Error: Failed the get a random article!";
            texts[2].text = "Error: Failed the get a random article!";
            Debug.LogFormat("[One Links To All #{0}] Error: Starting article query failed! Press submit to solve the module.", moduleId);
            error = true;
            StopAllCoroutines();
        }
        else if (type == 1)
        {
            texts[0].text = "Error: Failed to check if the path is valid!";
            texts[1].text = "";
            texts[2].text = "Error: Failed to check if the path is valid!";
            Debug.LogFormat("[One Links To All #{0}] Error: Link query failed! Press submit to solve the module.", moduleId);
            error = true;
            StopAllCoroutines();
        }
    }

    private IEnumerator QueryProcess()
    {
        Debug.LogFormat("<One Links To All #{0}> Starting query of starting article...", moduleId);
        while (title1.Equals(title2))
        {
            WWW www = new WWW(queryGetRandomURL);
            while (!www.isDone) { yield return null; };
            if (www.error == null)
            {
                var result = Newtonsoft.Json.Linq.JObject.Parse(www.text);
                title1 = result["query"]["random"][0]["title"].ToObject<string>();
                loadlinks = StartCoroutine(getLeadsToLink(title1));
                while (loadlinks != null) { yield return null; }
                if (queryLinks.Count == 0)
                    title1 = title2;
            }
            else
            {
                DealWithError(0);
            }
        }
        Debug.LogFormat("<One Links To All #{0}> Query of starting article successful! Found starting article: {1}", moduleId, title1);
        title2 = title1;
        Debug.LogFormat("<One Links To All #{0}> Starting query of finishing article...", moduleId);
        while (title1.Equals(title2))
        {
            WWW www = new WWW(queryGetRandomURL);
            while (!www.isDone) { yield return null; };
            if (www.error == null)
            {
                var result = Newtonsoft.Json.Linq.JObject.Parse(www.text);
                title2 = result["query"]["random"][0]["title"].ToObject<string>();
                loadlinks = StartCoroutine(getQueryLinks(title2, 0));
                while (loadlinks != null) { yield return null; }
                if (queryLinks.Count == 0)
                    title2 = title1;
                else
                {
                    WWW www2 = new WWW(queryDisambigURL + "&titles=" + title2);
                    while (!www2.isDone) { yield return null; };
                    if (www2.error == null)
                    {
                        if (www2.text.Contains("\"disambiguation\":"))
                        {
                            title2 = title1;
                        }
                    }
                    else
                    {
                        DealWithError(0);
                    }
                }
            }
            else
            {
                DealWithError(0);
            }
        }
        Debug.LogFormat("<One Links To All #{0}> Query of finishing article successful! Found finishing article: {1}", moduleId, title2);
        StopCoroutine(load);
        load = null;
        texts[0].text = title1;
        texts[2].text = title2;
        texts[3].text = 1.ToString();
        Debug.LogFormat("[One Links To All #{0}] The starting article is titled {1} and the ending article is titled {2}", moduleId, title1, title2);
    }

    private IEnumerator noSub()
    {
        load = StartCoroutine(Loading(1));
        bool valid = true;
        Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, title2);
        loadlinks = StartCoroutine(getQueryLinks(title2, 1));
        while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
        Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, title2);
        if (loadlinks != null)
        {
            StopCoroutine(loadlinks);
            loadlinks = null;
        }
        if (!queryLinks.Contains(title1))
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, title2);
            valid = false;
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, title2);
        }
        if (valid)
        {
            Debug.LogFormat("[One Links To All #{0}] Submitted path is valid, module disarmed!", moduleId);
            moduleSolved = true;
            audio.PlaySoundAtTransform("solve", transform);
            for (int i = 5; i < 31; i++)
            {
                if (i == 11)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 14)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 15)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 16)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 17)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 20)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 21)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 27)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 29)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                }
                if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = ":)";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySetSolve[i - 5].ToString();
                }
            }
            GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] Submitted path is invalid, Strike!", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        StopCoroutine(load);
        if (moduleSolved)
        {
            texts[0].text = "";
            texts[1].text = "GG";
            texts[2].text = "";
            texts[3].text = "";
        }
        else
        {
            texts[1].text = "";
            texts[3].text = 1.ToString();
        }
        load = null;
    }

    private IEnumerator finalCheck()
    {
        string temp = texts[1].text;
        load = StartCoroutine(Loading(1));
        bool valid = true;
        if (addedArticles.Count == 0)
        {
            Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, temp);
            loadlinks = StartCoroutine(getQueryLinks(temp, 1));
            while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
            Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, temp);
            if (loadlinks != null)
            {
                StopCoroutine(loadlinks);
                loadlinks = null;
            }
            if (!queryLinks.Contains(title1))
            {
                Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, temp);
                valid = false;
            }
            else
            {
                Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, temp);
            }
        }
        else
        {
            for (int i = 0; i <= addedArticles.Count; i++)
            {
                if (i == 0)
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, title1, addedArticles[0]);
                    loadlinks = StartCoroutine(getQueryLinks(addedArticles[0], 1));
                    while (loadlinks != null && !queryLinks.Contains(title1)) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, title1, addedArticles[0]);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(title1))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, title1, addedArticles[0]);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, title1, addedArticles[0]);
                    }
                }
                else if (i == addedArticles.Count)
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, addedArticles[i - 1], temp);
                    loadlinks = StartCoroutine(getQueryLinks(temp, 1));
                    while (loadlinks != null && !queryLinks.Contains(addedArticles[i - 1])) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, addedArticles[i - 1], temp);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(addedArticles[i - 1]))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, addedArticles[i - 1], temp);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, addedArticles[i - 1], temp);
                    }
                }
                else
                {
                    Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, addedArticles[i - 1], addedArticles[i]);
                    loadlinks = StartCoroutine(getQueryLinks(addedArticles[i], 1));
                    while (loadlinks != null && !queryLinks.Contains(addedArticles[i - 1])) { yield return null; }
                    Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, addedArticles[i - 1], addedArticles[i]);
                    if (loadlinks != null)
                    {
                        StopCoroutine(loadlinks);
                        loadlinks = null;
                    }
                    if (!queryLinks.Contains(addedArticles[i - 1]))
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, addedArticles[i - 1], addedArticles[i]);
                        valid = false;
                    }
                    else
                    {
                        Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, addedArticles[i - 1], addedArticles[i]);
                    }
                }
            }
        }
        Debug.LogFormat("<One Links To All #{0}> Starting query for {1} linking to {2}...", moduleId, temp, title2);
        loadlinks = StartCoroutine(getQueryLinks(title2, 1));
        while (loadlinks != null && !queryLinks.Contains(temp)) { yield return null; }
        Debug.LogFormat("<One Links To All #{0}> Query of {1} linking to {2} successful!", moduleId, temp, title2);
        if (loadlinks != null)
        {
            StopCoroutine(loadlinks);
            loadlinks = null;
        }
        if (!queryLinks.Contains(temp))
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (X)", moduleId, temp, title2);
            valid = false;
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] {1} -> {2} (✓)", moduleId, temp, title2);
        }
        if (valid)
        {
            Debug.LogFormat("[One Links To All #{0}] Submitted path is valid, module disarmed!", moduleId);
            moduleSolved = true;
            audio.PlaySoundAtTransform("solve", transform);
            for (int i = 5; i < 31; i++)
            {
                if (i == 11)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 14)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 15)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 16)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 17)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 20)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 21)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 27)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
                }
                else if (i == 29)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, 0f, 0.51f);
                }
                else if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localPosition = new Vector3(0f, -0.15f, 0.51f);
                    buttons[i].GetComponentInChildren<TextMesh>().gameObject.transform.localScale = new Vector3(0.0012f, 0.001f, 0.0012f);
                }
                if (i == 30)
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = ":)";
                }
                else
                {
                    buttons[i].GetComponentInChildren<TextMesh>().text = keySetSolve[i - 5].ToString();
                }
            }
            GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            Debug.LogFormat("[One Links To All #{0}] Submitted path is invalid, Strike!", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        StopCoroutine(load);
        if (moduleSolved)
        {
            texts[0].text = "";
            texts[1].text = "GG";
            texts[2].text = "";
            texts[3].text = "";
        }
        else
        {
            texts[1].text = temp;
            texts[3].text = (curIndex + 1).ToString();
        }
        load = null;
    }

    private IEnumerator getLeadsToLink(string title)
    {
        queryLinks.Clear();
        WWW www = new WWW(queryLeadsToURL + "&titles=" + title);
        while (!www.isDone) { yield return null; };
        if (www.error == null)
        {
            int index = www.text.IndexOf("pages") + 5;
            int ct = 0;
            string id = "";
            string newurl = "";
            while (ct < 2)
            {
                index++;
                if (www.text[index].Equals('\"'))
                {
                    ct++;
                }
                else if (ct == 1)
                {
                    id += www.text[index];
                }
            }
            id = "\"" + id + "\"";
            newurl = www.text.Replace(id, "\"id\"");
            var result = Newtonsoft.Json.Linq.JObject.Parse(newurl);
            try
            {
                queryLinks.Add(result["query"]["pages"]["id"]["links"][0]["title"].ToObject<string>());
            }
            catch (NullReferenceException)
            {
                // Here in case it has nothing it links to
            }
        }
        else
        {
            DealWithError(0);
        }
        loadlinks = null;
    }

    private IEnumerator getQueryLinks(string title, int type)
    {
        queryLinks.Clear();
        contvar = "temp";
        while (contvar != "")
        {
            string urledit = queryCheckBackURL;
            if (contvar != "temp")
                urledit += "&lhcontinue=" + contvar;
            string temp = "";
            if (type == 0)
            {
                temp = urledit + "&lhshow=!redirect&ppprops=!disambiguation&titles=" + title;
            }
            else
            {
                temp = urledit + "&titles=" + title;
            }
            WWW www = new WWW(temp);
            while (!www.isDone) { yield return null; };
            if (www.error == null)
            {
                int index = www.text.IndexOf("pages") + 5;
                int ct = 0;
                string id = "";
                string newurl = "";
                while (ct < 2)
                {
                    index++;
                    if (www.text[index].Equals('\"'))
                    {
                        ct++;
                    }
                    else if (ct == 1)
                    {
                        id += www.text[index];
                    }
                }
                id = "\"" + id + "\"";
                newurl = www.text.Replace(id, "\"id\"");
                newurl = newurl.Replace("\"continue\"", "\"cont\"");
                var result = Newtonsoft.Json.Linq.JObject.Parse(newurl);
                int count = 0;
                while (true)
                {
                    try
                    {
                        queryLinks.Add(result["query"]["pages"]["id"]["linkshere"][count]["title"].ToObject<string>());
                        count++;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Here in case it runs out of articles on a page to add
                        break;
                    }
                    catch (NullReferenceException)
                    {
                        // Here in case it is not a valid article
                        break;
                    }
                }
                if (newurl.Contains("\"cont\""))
                {
                    contvar = result["cont"]["lhcontinue"].ToObject<string>();
                }
                else
                {
                    contvar = "";
                }
            }
            else
            {
                DealWithError(type);
            }
        }
        loadlinks = null;
    }

    private IEnumerator Loading(int type)
    {
        int ct = 0;
        while (true)
        {
            if (ct > 3)
            {
                ct = 0;
            }
            if (ct == 0)
            {
                if (type == 0)
                {
                    texts[0].text = ".";
                    texts[2].text = ".";
                }
                else
                {
                    texts[1].text = ".";
                }
            }
            else if (ct == 1)
            {
                if (type == 0)
                {
                    texts[0].text = "..";
                    texts[2].text = "..";
                }
                else
                {
                    texts[1].text = "..";
                }
            }
            else if (ct == 2)
            {
                if (type == 0)
                {
                    texts[0].text = "...";
                    texts[2].text = "...";
                }
                else
                {
                    texts[1].text = "...";
                }
            }
            else if (ct == 3)
            {
                if (type == 0)
                {
                    texts[0].text = "";
                    texts[2].text = "";
                }
                else
                {
                    texts[1].text = "";
                }
            }
            ct++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} type <title> [Types 'title' using the keypad (Case sensative)] | !{0} add [Presses the add (+) button] | !{0} minus (#) [Presses the minus (-) button (optionally '#' times)] | !{0} clear [Presses the clear button] | !{0} delete (#) [Presses the delete button (optionally '#' times)] | !{0} submit [Presses the submit button]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (load != null || !activated)
        {
            yield return "sendtochaterror Buttons cannot be pressed right now as the module is currently generating the random articles!";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*add\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot add an empty article!";
                yield break;
            }
            buttons[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*minus\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curIndex == 0)
            {
                yield return "sendtochaterror Cannot remove anymore articles!";
                yield break;
            }
            buttons[1].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot clear text on an empty screen!";
                yield break;
            }
            buttons[3].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*delete\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (texts[1].text.Equals(""))
            {
                yield return "sendtochaterror Cannot delete text on an empty screen!";
                yield break;
            }
            buttons[2].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!Valid())
            {
                yield return "sendtochaterror Cannot press the submit button with at least 1 article added and having an empty screen!";
                yield break;
            }
            buttons[4].OnInteract();
            yield return "solve";
            yield return "strike";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*type\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length >= 2)
            {
                parameters[1] = command.Substring(5, command.Length - 5);
                parameters[1] = parameters[1].Replace("-", "–");
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    if (!keySet1.Contains(parameters[1][i]) && !keySet2.Contains(parameters[1][i]) && !keySet3.Contains(parameters[1][i]) && !keySet4.Contains(parameters[1][i]))
                    {
                        yield return "sendtochaterror The specified character to type '" + parameters[1][i] + "' is invalid!";
                        yield break;
                    }
                }
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    if (parameters[1][i].Equals(' '))
                    {
                        if (keyIndex != 3)
                        {
                            buttons[34].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        buttons[15].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else if (keySet1.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 0)
                        {
                            buttons[31].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        buttons[Array.IndexOf(keySet1, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else if (keySet2.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 1)
                        {
                            buttons[32].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        buttons[Array.IndexOf(keySet2, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else if (keySet3.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 2)
                        {
                            buttons[33].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        buttons[Array.IndexOf(keySet3, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                    else if (keySet4.Contains(parameters[1][i]))
                    {
                        if (keyIndex != 3)
                        {
                            buttons[34].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        buttons[Array.IndexOf(keySet4, parameters[1][i]) + 5].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify what to type!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*minus\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp > 0 && temp <= addedArticles.Count)
                    {
                        if (curIndex == 0)
                        {
                            yield return "sendtochaterror Cannot remove anymore articles!";
                            yield break;
                        }
                        for (int i = 0; i < temp; i++)
                        {
                            buttons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror The specified number of times to press the minus button is out of range 1-[# of articles - 1]!";
                    }
                }
                else
                {
                    yield return "sendtochaterror The specified number of times to press the minus button is invalid!";
                }
            }
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the number of times to press minus button!";
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*delete\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp = 0;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp > 0 && temp <= texts[1].text.Length)
                    {
                        if (texts[1].text.Equals(""))
                        {
                            yield return "sendtochaterror Cannot delete text on an empty screen!";
                            yield break;
                        }
                        for (int i = 0; i < temp; i++)
                        {
                            buttons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror The specified number of times to press the delete button is out of range 1-[length of text on screen]!";
                    }
                }
                else
                {
                    yield return "sendtochaterror The specified number of times to press the delete button is invalid!";
                }
            }
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the number of times to press delete button!";
            }
            yield break;
        }
    }
}
