using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class KeystrokesScr : MonoBehaviour {
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio SFX;
    public KMSelectable[] KeyboardKeys;
    public KMSelectable Spacebar;
    public Material[] KeyMats;

    static readonly List<List<string>> _categories = new List<List<string>>
        { new List<string> {"Wires","The Button","Keypad","Maze","Memory","Simon Says","Password","Who's On First","Morse Code","Complicated Wires","Wire Sequence","Capacitator Discharge","Venting Gas","Knob","Batteries","Indicators","Ports"},
          new List<string> {"Alfa","Bravo","Charlie","Delta","Echo","Foxtrot","Golf","Hotel","India","Juliett","Kilo","Lima","Mike","November","Oscar","Papa","Quebec","Romeo","Sierra","Tango","Uniform","Victor","Whiskey","X-Ray","Yankee","Zulu"},
          new List<string> {"Aries","Taurus","Gemini","Cancer","Leo","Virgo","Libra","Scoripo","Sagittarius","Capricorn","Aquarius","Pisces","Sun","Moon","Mercury","Venus","Mars","Jupiter","Saturn","Uranus","Neptune","Pluto","Fire","Water","Earth","Air"},
          new List<string> {"Miss Scarlett","Professor Plum","Mrs. Peacock","Reverend Green","Colonel Mustard","Mrs. White","Candlestick","Dagger","Lead Pipe","Revolver","Rope","Spanner","Dining Room","Library","Lounge","Kitchen","Study","Conservatory","Hall","Billard Room","Ballroom"},
          new List<string> {"Buhar","Lanaluff","Bob","Mountoise","Nibs","Aluga","Lugirit","Caadarim","Vellarim","Flaurim","Gloorim","Melbor","Clondar","Docsplode","Magmy","Pouse","Ukkens","Asteran","Violan","Zenlad","Zapra","Myrchat","Percy","Cutie Pie"},
          new List<string> {"Colour Flash","Piano Keys","Semaphore","Math","Emoji Math","Lights Out","Switches","Word Scramble", "Anagrams","Combination Lock","Filibuster","Motion Sense","Round Keypad","Listening","Foreign Exchange Rates","Answering Questions","Orientation Cube","Morsematics","Connection Check","Letter Keys","Forget Me Not","Rotary Phone","Astrology"},
          new List<string> {"Ansuz","Berkana","Kenaz","Dagaz","Ehwaz","Fehu","Gebo","Hagalaz","Isa","Jera","Eihwaz","Laguz","Mannaz","Othila","Perthro","Algiz","Raido","Sowulo","Teiwaz","Uruz","Wunjo","Thurisaz"},
          new List<string> {"Buenos Aires", "Brisbane", "Sydney", "Sao Paulo", "Bujumbura","Praia","Whitehorse","Beijing","Quito","Papeete","Tbilisi","Tokyo","Berlin","Tarawa","Managua","Alofi","Lahore","Moscow","Omsk","Edinburgh","Bangkok","Denver","Unalaska"},
          new List<string> {"Taxi Dispatch","Cow","Exctractor Fan","Train Station","Arcade","Casino","Supermarket","Soccer Match","Tawny Owl","Sewing Machine","Thrush Nightingale","Car Engine","Oboe","Saxophone","Tuba","Marimba","Phone Ringing","Tibetan Nuns","Throat Singing","Dial-up Internet","Police Radio Scanner","Censorship Bleep","Medieval Weapons","Door Closing","Chainsaw","Compressed Air","Servo Motor","Waterfall","Taearing Fabric","Zipper","Vacuum Cleaner","Ballpoint Pen Writng","Rattling Iron Chain","Book Page Turning","Table Tennis","Squeaky Toy", "Helicopter", "Firework Exploding", "Glass Shattering"}
        };
    static readonly string _keyboard = "QWERTYUIOPASDFGHJKLZXCVBNM";

    private List<string> _displayedWords = new List<string>();
    private List<int> _solutionKeys = new List<int>();
    private int _liarIndex = 0;
    private int _liarDisplayIndex;

    private int _curDisplayIndex;
    private bool _submitting;

    private int _loggingId;
    static private int _loggingIdCounter = 1;
    private bool _solved;

    void Start() {
        _loggingId = _loggingIdCounter++;
        Spacebar.OnInteract += HandleSpacebar();
        for (int i = 0; i < 26; i++)
        {
            int key = i;
            KeyboardKeys[i].OnInteract += HandleKey(key);
        }
        GeneratePuzzle();
    }

    void GeneratePuzzle()
    {
        string[] categoryLoggingNames = new string[] { "Vanilla Modules + Widgets", "NATO Phonetic Alphabet letters", "Astrology Symbols", "Murder Suspects + Weapons + Rooms", "Monsplodes", "First 23 Modded Modules", "Elder Futhark Runes", "Timezones Cities", "Listening Sounds" };
        int majorityCategoryIndex = Rnd.Range(0, _categories.Count());
        int oddOneOutIndex = (majorityCategoryIndex + Rnd.Range(1, _categories.Count())) % _categories.Count();
        if (majorityCategoryIndex / 3 == oddOneOutIndex / 3)
        {
            _liarIndex += 3 * (majorityCategoryIndex / 3);
        }
        else
        {
            _liarIndex += 3 * Enumerable.Range(0, 3).ToList().Where(x => x != majorityCategoryIndex / 3 && x != oddOneOutIndex / 3).ToList()[0];
        }
        if (majorityCategoryIndex % 3 == oddOneOutIndex % 3)
        {
            _liarIndex += majorityCategoryIndex % 3;
        }
        else
        {
            _liarIndex += Enumerable.Range(0, 3).ToList().Where(x => x != majorityCategoryIndex % 3 && x != oddOneOutIndex % 3).ToList()[0];
        }
        Debug.LogFormat("[Keystrokes #{0}] The category for three of the words is {1}, and the odd one out's category is {2}, making the solution display's category {3}", _loggingId, categoryLoggingNames[majorityCategoryIndex], categoryLoggingNames[oddOneOutIndex], categoryLoggingNames[_liarIndex]);
        _displayedWords.AddRange(_categories[majorityCategoryIndex].Shuffle().Take(3));
        _displayedWords.Add(_categories[oddOneOutIndex][Rnd.Range(0, _categories[oddOneOutIndex].Count)]);
        _displayedWords.Shuffle();
        _liarDisplayIndex = Rnd.Range(0, 5);
        _displayedWords.Insert(_liarDisplayIndex, GenerateLiar());
        Debug.LogFormat("[Keystrokes #{0}] The chosen words are {1}.", _loggingId, _displayedWords.Join(", "));
        _displayedWords = _displayedWords.Select(x => x.ToUpperInvariant().Where(y => _keyboard.Contains(y)).Join("")).ToList();
        UpdateKeys();
    }

    string GenerateLiar()
    {
        string originalWord = _categories[_liarIndex][Rnd.Range(0, _categories[_liarIndex].Count)];
        List<char> manipulableWord = originalWord.Select(x => char.ToUpperInvariant(x)).Where(y => _keyboard.Contains(y)).Distinct().ToList();
        List<char> wordNonLetters;
        int numChanges = Rnd.Range(1, 6);
        int numRemovals = Rnd.Range(0, Math.Min(numChanges, manipulableWord.Count - 1));
        for (int i = 0; i < numRemovals; i++)
        {
            int removalIndex = Rnd.Range(0, manipulableWord.Count);
            _solutionKeys.Add(_keyboard.IndexOf(manipulableWord[removalIndex]));
            manipulableWord.RemoveAt(removalIndex);
        }
        for (int i = 0; i < numChanges - numRemovals; i++)
        {
            wordNonLetters = _keyboard.Where(x => !manipulableWord.Contains(x)).ToList();
            int additionIndex = Rnd.Range(0, wordNonLetters.Count);
            _solutionKeys.Add(_keyboard.IndexOf(wordNonLetters[additionIndex]));
            manipulableWord.Add(wordNonLetters[additionIndex]);
        }
        foreach (string i in _categories[_liarIndex])
        {
            if (i.Equals(originalWord))
            {
                continue;
            }
            if (_keyboard.Select(x => i.ToUpperInvariant().Where(y => _keyboard.Contains(y)).Contains(x) == manipulableWord.Contains(x)).Count(x => !x) <= 5)
            {
                _solutionKeys.Clear();
                return GenerateLiar();               
            }
        } 
        Debug.LogFormat("[Keystrokes #{0}] The word used to generate the solution display is {1}.", _loggingId, originalWord);
        return manipulableWord.Join("");
    }

    KMSelectable.OnInteractHandler HandleSpacebar() {
        return delegate ()
        {
            if (!_submitting && !_solved)
            {
                Spacebar.AddInteractionPunch(0.5f);
                SFX.PlaySoundAtTransform("Key Click", transform);
                _curDisplayIndex++;
                if (_curDisplayIndex == 5)
                    _curDisplayIndex = 0;
                UpdateKeys();
            }
            return false;
        };
    }
    void UpdateKeys()
    {
        for (int i = 0; i < 26; i++)
        {
            if (_displayedWords[_curDisplayIndex].Contains(_keyboard[i]))
            {
                KeyboardKeys[i].GetComponent<MeshRenderer>().material = KeyMats[1];
            }
            else
            {
                KeyboardKeys[i].GetComponent<MeshRenderer>().material = KeyMats[0];
            }
        }
    }
    KMSelectable.OnInteractHandler HandleKey (int key)
    {
        return delegate ()
        {
            if (!_solved)
            {
                KeyboardKeys[key].AddInteractionPunch(0.5f);
                SFX.PlaySoundAtTransform("Key Click", transform);
                if (_curDisplayIndex != _liarDisplayIndex)
                {
                    Module.HandleStrike();
                    Debug.LogFormat("[Keystrokes #{0}] Attempted to submit display {1}, which was not the solution display. Strike!", _loggingId, _displayedWords[_curDisplayIndex]);
                }
                else
                {
                    Debug.LogFormat("[Keystrokes #{0}] Pressed key {1}.", _loggingId, _keyboard[key]);
                    _submitting = true;
                    if (_solutionKeys.Contains(key))
                    {
                        Debug.LogFormat("[Keystrokes #{0}] That was correct.", _loggingId);
                        _solutionKeys.Remove(key);
                        if (!_displayedWords[_curDisplayIndex].Contains(_keyboard[key]))
                        {
                            KeyboardKeys[key].GetComponent<MeshRenderer>().material = KeyMats[1];
                        }
                        else
                        {
                            KeyboardKeys[key].GetComponent<MeshRenderer>().material = KeyMats[0];
                        }
                        if (_solutionKeys.Count == 0)
                        {
                            Debug.LogFormat("[Keystrokes #{0}] Module solved.", _loggingId);
                            SFX.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                            Module.HandlePass();
                            _solved = true;
                        }
                    }
                    else
                    {
                        Debug.LogFormat("[Keystrokes #{0}] That was incorrect. Strike!", _loggingId);
                        Module.HandleStrike();
                    }                
                }
            }
            return false;
        };
    }
    string TwitchHelpMessage = "Use '!{0} cycle' to cycle the full list of displays. Use '!{0} toggle N' to to move X displays forward (default if no number is specified is 1). Use '!{0} submit ABC' while on the correct display to submit an answer.";
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (command == "cycle")
        {
            yield return null;
            int cycleNum = 0;
            while (cycleNum < 5)
            {
                yield return new WaitForSeconds(2.5f);
                Spacebar.OnInteract();
                cycleNum++;
            }
        }
        if (command == "toggle")
        {
            yield return null;
            yield return new WaitForSeconds(2.5f);
            Spacebar.OnInteract();
        }
        else if (command.StartsWith("toggle "))
        {
            string[] cmdArray = command.Split(' ');
            int toggleNum;
            if (cmdArray.Length < 2)
            {
                yield return "sendtochaterror Too many paramaters!";
                yield break;
            }
            if (!int.TryParse(cmdArray[1], out toggleNum))
            {
                yield return string.Format("sendtochaterror Parameter '{0}' not a number!", cmdArray[1]);
                yield break;
            }
            int step = 0;
            yield return null;
            while (step < toggleNum)
            {
                Spacebar.OnInteract();
                yield return new WaitForSeconds(0.2f);
                step++;
            }
        }
        else if (command.StartsWith("submit "))
        {
            string[] cmdArray = command.Split(' ');
            if (cmdArray.Length < 2)
            {
                yield return "sendtochaterror Too many paramaters!";
                yield break;
            }
            if (cmdArray.Length > 2)
            {
                yield return "sendtochaterror Specify a submission!";
                yield break;
            }
            string submission = cmdArray[1].ToUpperInvariant();
            foreach (char i in submission)
            {
                if (!_keyboard.Contains(i))
                {
                    yield return string.Format("sendtochaterror Character '{0}' not alpha!", i);
                    yield break;
                }
            }
            yield return null;
            foreach (char i in submission)
            {
                yield return new WaitForSeconds(0.2f);
                if (!_solutionKeys.Contains(_keyboard.IndexOf(i)))
                {
                    KeyboardKeys[_keyboard.IndexOf(i)].OnInteract();
                    yield break;
                }
                KeyboardKeys[_keyboard.IndexOf(i)].OnInteract();
            }
        }
        else
        {
            yield return "sendtochaterror Unrecognized command!";
            yield break;
        }
    }
}
