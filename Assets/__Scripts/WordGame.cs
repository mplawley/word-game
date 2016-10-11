using UnityEngine;
using System.Collections;
using System.Collections.Generic; //We'll be using List<> & Dictionary<>
using System.Linq;

public enum GameMode
{
	preGame, //Before the game starts
	loading, //The word list is loading and being parsed
	makeLevel, //The individual WordLevel is being created
	levelPrep, //The level visuals are Instantiated
	inLevel //The level is in progress
}

public class WordGame : MonoBehaviour
{
	public static WordGame S; //Singleton

	[Header("------------")]

	public GameMode mode = GameMode.preGame;
	public WordLevel currLevel;

	void Awake()
	{
		S = this; //Assign the singleton
	}

	// Use this for initialization
	void Start()
	{
		mode = GameMode.loading;

		//Tells WordList.S to start parsing all the words
		WordList.S.Init();
	}

	//Called by the SendMessage() command from WordList

	public void WordListParseComplete()
	{
		mode = GameMode.makeLevel;

		//Make a level and assign it to currLevel, the current WordLevel
		currLevel = MakeWordLevel();
	}

	//With the default value of -1, this method will generate a level from a random word
	public WordLevel MakeWordLevel(int levelNum = -1)
	{
		WordLevel level = new WordLevel();
		if (levelNum == -1)
		{
			//Pick a random level
			level.longWordIndex = Random.Range(0, WordList.S.longWordCount);
		}
		else
		{
			//This can be added later
		}
		level.levelNum = levelNum;
		level.word = WordList.S.GetLongWord(level.longWordIndex);
		level.charDict = WordLevel.MakeCharDict(level.word);

		//Call a coroutine to check all the words in the WordList and see whether
		//each word can be spelled by the chars in level.charDict
		StartCoroutine(FindSubWordsCoroutine(level));

		//This returns the level before the coroutine finishes, so
		//SubWordSearchComplete() is called when the coroutine is done
		return level;
	}

	//A coroutine that finds words that can be spelled in this level
	public IEnumerator FindSubWordsCoroutine(WordLevel level)
	{
		level.subWords = new List<string>();
		string str;

		List<string> words = WordList.S.GetWords(); //This is very fast because List<string> is passed by reference

		//Iterate through all the words in the WordList
		for (int i = 0; i < WordList.S.wordCount; i++)
		{
			str = words[i];

			//Check whether each one can be spelled using level.charDict
			if (WordLevel.CheckWordInLevel(str, level))
			{
				level.subWords.Add(str);
			}

			//Yield if we've parsed a lot of words this frame
			if (i % WordList.S.numToParseBeforeYield == 0)
			{
				//yield until the next frame
				yield return null;
			}
		}

		//List<string>.Sort() sorts alphabetically by default
		level.subWords.Sort();

		//Now sort by length to have words grouped by number of letters
		level.subWords = SortWordsByLength(level.subWords).ToList();

		//The courtine is complete, so call SubWordSearchComplete()
		SubWordSearchComplete();
	}

	public static IEnumerable<string> SortWordsByLength(IEnumerable<string> e)
	{
		//Use LINQ to sort the array received and return a copy.
		//The LINQ syntax is different from regular C#
		var sorted = from s in e
			orderby s.Length ascending
				select s;
		return sorted;
	}

	public void SubWordSearchComplete()
	{
		mode = GameMode.levelPrep;
	}
}
