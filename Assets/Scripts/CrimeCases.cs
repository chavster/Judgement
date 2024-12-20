﻿using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using KModkit;
using UnityEngine.XR.WSA.Input;

partial class Judgement
{
    private int NameLength;
    private bool IsInnocent = true;
    private int Strikes;
    void CrimeCalc(int pos)
    {
        switch (ChosenCrime)
        {
            case 0:
                IsInnocent = NameSum > 150;
                break;

            case 1:
                IsInnocent = ForenameValue > SurnameValue;
                break;

            case 2:
                IsInnocent = SurnameValue > ForenameValue;
                break;

            case 3:
                
                break;

            case 4:
                IsInnocent = SolvedModules > UnsolvedModules;
                Log("The number of solved modules is " + SolvedModules + ". Press " + (IsInnocent ? "INNOCENT" : "GUILTY"));
                break;

            case 5:

                IsInnocent = 115 < (NameSum - Forenames[ChosenForename].Length - Surnames[ChosenSurname].Length);

                break;

            case 6:

                IsInnocent = 2000 < (Bomb.GetPortCount() * NameSum);

                break;

            case 7:
                
                string s = NameSum.ToString();
                
                IsInnocent = s.Any(x => "97531".Contains(x));
                break;

            case 8:

                break;

            case 9:

                break;

            case 10:

                break;

            case 11:

                break;

            case 12:

                break;

            case 13:

                break;

            case 14:

                break;

            case 15:

                break;
            case 16:

                break;
            case 17:

                break;
            case 18:

                break;
            case 19:

                break;
            case 20:

                break;
            case 21:

                break;
            case 22:

                break;
            case 23:

                break;
            case 24:

                break;
            case 25:

                break;
            case 26:

                break;
            case 27:

                break;
            case 28:

                break;
            case 29:

                break;
            case 30:

                break;
            case 31:

                break;
            case 32:

                break;

        }
    }
}