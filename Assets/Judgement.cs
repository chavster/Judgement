using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;
using KeepCoding;
using Rnd = UnityEngine.Random;

partial class Judgement : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable[] Keypad;
    public KMSelectable[] VerdictPad;
    public TextMesh DisplayText;
    public TextMesh NumberText;
    public MeshRenderer DisplayRend;

    static int _moduleIdCounter = 1;
    int _moduleId;

    private static readonly string[] Forenames = { "Aidan", "Chav", "Zoe", "Deaf", "Blan", "Ghost", "Hazel", "Goober", "Jimmy", "Homer", "Saul", "Walter", "Jeremiah", "Jams", "Jo", "Johnny", "Dwayne", "Cave", "Burger", "Jerma", "Sans", "Jon", "Garfield", "Mega", "Cruel", "Cyanix", "Tim", "Bomby", "Edgework", "Complicated", "Jason", "Freddy", "Gaga", "Barry", "Mordecai", "Rigby", "Jesus", "Seymour", "Superintendent", "Kevin", "dicey", "User", "Eltrick", "Juniper", "David", "MAXANGE", "Emik" };
    private static readonly string[] Surnames = { "Anas", "Salt", "Ster", "Blind", "Ante", "McBoatface", "McGooberson", "Neutron", "Simpleton", "Goodman", "White", "Clahkson", "Maie", "Hammock", "Ku", "Cage", "Johnson", "King", "Tron", "Serif", "Master", "Wi", "McBombface", "McEdgework", "Optimised", "Alfredo", "Voorhees", "Fazbear", "Oolala", "Benson", "Christ", "Skinner", "Lee", "Name", "Mitchell" };
    private static readonly string[] Crimes = { "Silliness", "Tax Fraud", "Dying", "Striking", "Solving", "Living", "Embezzlement", "Being Guilty", "Handling Salmon", "Minor Larceny", "{CRIME}", "Teleporting Bread" };
    private int ChosenForename;
    private int ChosenSurname;
    private int ChosenCrime;
    private int KeypadInput = -1;
    private Coroutine[] KeypadAnimCoroutines;
    private Coroutine[] VerdictAnimCoroutines;
    private int ForenameValue;
    private int SurnameValue;
    private int NameSum;
    private bool ModuleSolved;
    private bool VerdictAccessible = false;
    private bool CanPress;
    private bool StrikeChange = false;
    private int SolvedModules = 0;
    private int UnsolvedModules = 0;

    void Awake()
    {
        _moduleId = _moduleIdCounter++;
        KeypadAnimCoroutines = new Coroutine[Keypad.Length];
        VerdictAnimCoroutines = new Coroutine[VerdictPad.Length];

        Calculate();
        StartCoroutine(TheScanline());

        DisplayRend.material.color = Color.black;
        DisplayText.color = new Color32(102, 162, 38, 0);

        for (int i = 0; i < Keypad.Length; i++)
        {
            int x = i;
            Keypad[x].OnInteract += delegate { KeypadPress(x); return false; };
        }

        for (int i = 0; i < VerdictPad.Length; i++)
        {
            int x = i;
            VerdictPad[x].OnInteract += delegate { VerdictPress(x); return false; };
        }

        Module.OnActivate += delegate { DisplayRend.material.color = new Color32(2, 36, 0, 255); CanPress = true;
            StartCoroutine(GlitchText(NumberText.GetComponent<MeshRenderer>()));
            StartCoroutine(GlitchText(DisplayText.GetComponent<MeshRenderer>()));
            DisplayCase();
        };
    }

    void DisplayCase()
    {
        DisplayText.text = "The Court accuses\n" + Forenames[ChosenForename] + " " + Surnames[ChosenSurname] + "\nof " + Crimes[ChosenCrime];
        DisplayText.characterSize = (Forenames[ChosenForename].Length + Surnames[ChosenSurname].Length) >= 21 ? 1.0f : 1.35f;
        DisplayText.color = new Color32(102, 162, 38, (byte)Mathf.Ceil(DisplayText.color.a * 255));
    }

    void KeypadPress(int pos)
    {
        if (CanPress)
        {
            Audio.PlaySoundAtTransform("keypadPress", Keypad[pos].transform);
            if (KeypadAnimCoroutines[pos] != null)
                StopCoroutine(KeypadAnimCoroutines[pos]);
            KeypadAnimCoroutines[pos] = StartCoroutine(ButtonAnim(Keypad[pos].transform, 0, -0.005f));
            Keypad[pos].AddInteractionPunch();

            if (!ModuleSolved && !VerdictAccessible)
            {
                if (pos == 9) // Clear Key
                {
                    KeypadInput = -1;
                    NumberText.text = "";
                }
                else if (pos == 11) // Enter Key
                {
                    if (KeypadInput == NameSum)
                    {
                        VerdictAccessible = true;
                        DisplayRend.material.color = new Color32(30, 0, 0, 255);
                        DisplayText.text = "INNOCENT\nOR\nGUILTY?";
                        DisplayText.color = new Color32(164, 9, 9, (byte)Mathf.Ceil(DisplayText.color.a * 255));
                        DisplayText.characterSize = 1.35f;
                        NumberText.gameObject.SetActive(false);
                        StopCoroutine(ButtonAnim(Keypad[pos].transform, 0, -0.005f));
                        CrimeCalc();
                    }
                    else
                    {
                        Strike("You input " + KeypadInput + " which was incorrect.");
                        KeypadInput = -1;
                        NumberText.text = "";
                    }
                }
                else if (KeypadInput < 100)
                {
                    var correspondingNums = new[] {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9,
                -1,0,-1 };

                    var pressedNum = correspondingNums[pos];

                    if (KeypadInput == -1)
                        KeypadInput = pressedNum;
                    else
                        KeypadInput = (KeypadInput * 10) + pressedNum;
                    NumberText.text = KeypadInput.ToString();
                }
            }
        }
    }

    void VerdictPress(int pos)
    {
        if (CanPress)
        {
            Audio.PlaySoundAtTransform("keypadPress", VerdictPad[pos].transform);
            if (VerdictAnimCoroutines[pos] != null)
                StopCoroutine(VerdictAnimCoroutines[pos]);
            VerdictAnimCoroutines[pos] = StartCoroutine(ButtonAnim(VerdictPad[pos].transform, 0, -0.0075f));
            VerdictPad[pos].AddInteractionPunch();

            if (!ModuleSolved && VerdictAccessible)
            {
                if ((pos == 0 && !IsInnocent) || (pos == 1 && IsInnocent))
                {
                    StartCoroutine(Solve(pos));
                }
                else
                {
                    Strike("You pressed " + (pos == 1 ? "INNOCENT " : "GUILTY ") + "which was incorrect. The module has now reset.");
                    NumberText.gameObject.SetActive(true);
                    DisplayCase();
                }
            }
        }

    }

    private IEnumerator ButtonAnim(Transform target, float start, float end, float duration = 0.075f)
    {
        target.transform.localPosition = new Vector3(target.transform.localPosition.x, start, target.transform.localPosition.z);

        float timer = 0;
        while (timer < duration)
        {
            target.transform.localPosition = new Vector3(target.transform.localPosition.x, Mathf.Lerp(start, end, timer / duration), target.transform.localPosition.z);
            yield return null;
            timer += Time.deltaTime;
        }

        target.transform.localPosition = new Vector3(target.transform.localPosition.x, end, target.transform.localPosition.z);

        timer = 0;
        while (timer < duration)
        {
            target.transform.localPosition = new Vector3(target.transform.localPosition.x, Mathf.Lerp(end, start, timer / duration), target.transform.localPosition.z);
            yield return null;
            timer += Time.deltaTime;
        }

        target.transform.localPosition = new Vector3(target.transform.localPosition.x, start, target.transform.localPosition.z);
    }

    public void Log(string message)
    {
        Debug.LogFormat("[Judgement #{0}] {1}", _moduleId, message);
    }

    private IEnumerator Solve(int pos)
    {
        DisplayText.text = Forenames[ChosenForename] + " " + Surnames[ChosenSurname] + "\n" + "IS " + ((pos == 1 ? "INNOCENT" : "GUILTY"));
        DisplayText.color = new Color32(129, 0, 0, 1);
        Module.HandlePass();
        ModuleSolved = true;

        yield return new WaitForSeconds(5);
        DisplayText.fontSize = 250;
        DisplayText.text = "SOLVED";
        Audio.PlaySoundAtTransform("solveNoise", DisplayText.transform);

        yield return new WaitForSeconds(4.5f);

        float timer = 0;
        while (timer < 0.5f)
        {
            DisplayText.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(Rnd.Range(0, 1f), Rnd.Range(0, 1f)));
            yield return null;
            timer += Time.deltaTime;
        }

        DisplayText.gameObject.SetActive(false);
    }

    void Update()
    {
        SolvedModules = Bomb.GetSolvedModuleNames().Count();
        UnsolvedModules = Bomb.GetUnsolvedModuleNames().Count();
        ChangeStrikes(Strikes);
    }


    void Strike(string log)
    {
        if (!ModuleSolved)
        {
            VerdictAccessible = false;
            DisplayRend.material.color = new Color32(2, 36, 0, 255);
            Log(log);
            Module.HandleStrike();
            Calculate();
            NumberText.gameObject.SetActive(true);
            KeypadInput = -1;
            NumberText.text = "";
            DisplayCase();
        }

    }

    private IEnumerator GlitchText(MeshRenderer target)
    {
        var textMesh = target.GetComponent<TextMesh>();
        var misses = 0;
        while (true)
        {
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, Rnd.Range(0.7f, 0.9f));
            if (Rnd.Range(0, 30) == 0 || misses == 15)
            {
                misses = 0;
                target.material.SetTextureOffset("_MainTex", new Vector2(Rnd.Range(0, 1f), Rnd.Range(0, 1f)));
            }
            else
            {
                misses++;
                target.material.SetTextureOffset("_MainTex", Vector2.zero);
            }
            yield return new WaitForSeconds(Rnd.Range(0.075f, 0.125f));
        }
    }

    private IEnumerator TheScanline()
    {
        while (true)
        {
            yield return null;
            float offset = Time.time % 1;
            DisplayRend.material.SetTextureOffset("_MainTex", new Vector2(0, -offset));
        }
    }

    void Calculate()
    {
        ChosenForename = Rnd.Range(0, Forenames.Length);
        ChosenSurname = Rnd.Range(0, Surnames.Length);
        Log("The name is " + Forenames[ChosenForename] + " " + Surnames[ChosenSurname]);
        ChosenCrime = Rnd.Range(0, Crimes.Length);

        //Initialises letters into numbers
        ForenameValue = Forenames[ChosenForename].ToUpperInvariant().ToCharArray()
            .Select(x => x - '@')
            .Sum();
        SurnameValue = Surnames[ChosenSurname].ToUpperInvariant().ToCharArray()
            .Select(x => x - '@')
            .Sum();
        NameSum = ForenameValue + SurnameValue;
        Log("The name value is " + NameSum);

        NumberText.text = "";
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} input ### inputs up to three numbers to enter. || !{0} declare guilty/innocent presses either the guilty or innocent button.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        var keypadIx = "123456789C0E";

        yield return null;

        if ("INPUT".ContainsIgnoreCase(split[0]))
        {
            if (VerdictAccessible)
            {
                yield return "sendtochaterror The keypad is no longer accessible!";
                yield break;
            }

            if (split.Length == 1)
            {
                yield return "sendtochaterror Please specify what number to input!";
                yield break;
            }

            if (split.Length > 2)
                yield break;

            if (split[1].Length > 3)
            {
                yield return "sendtochaterror You cannot input more than 3 numbers!";
                yield break;
            }

            if (!split[1].All(char.IsDigit))
            {
                yield return "sendtochaterror Please do not input anything that isn't a number!";
                yield break;
            }

            if (KeypadInput != -1)
            {
                Keypad[9].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            foreach (var num in split[1])
            {
                Keypad[keypadIx.IndexOf(num)].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            Keypad[11].OnInteract();
            yield return new WaitForSeconds(0.1f);

            yield break;
        }

        if ("DECLARE".ContainsIgnoreCase(split[0]))
        {
            if (!VerdictAccessible)
            {
                yield return "sendtochaterror There is no verdict prompted yet!";
                yield break;
            }

            if (split.Length == 1)
            {
                yield return "sendtochaterror Please specify whether they are guilty or innocent!";
                yield break;
            }

            if (split.Length > 2)
                yield break;

            var verdicts = new[] { "GUILTY", "INNOCENT" };

            if (!verdicts.Any(x => x.ContainsIgnoreCase(split[1])))
            {
                yield return string.Format("{0} is not a valid verdict!", split[1]);
                yield break;
            }

            VerdictPad[Array.IndexOf(verdicts, split[1])].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;

        if (!VerdictAccessible)
        {
            if (KeypadInput != -1)
            {
                Keypad[9].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            var solutionIxes = NameSum.ToString().Select(x => "123456789C0E".IndexOf(x)).ToArray();

            foreach (var num in solutionIxes)
            {
                Keypad[num].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }

            Keypad[11].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        VerdictPad[IsInnocent ? 1 : 0].OnInteract();
        yield return new WaitForSeconds(0.1f);
    }
}
