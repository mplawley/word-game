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
	public GameObject prefabLetter;
	public Rect wordArea = new Rect(-24, 19, 48, 28);
	public float letterSize = 1.5f;
	public bool showAllWyrds = true;
	public float bigLetterSize = 4f;
	public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
	public Color bigColorSelected = Color.white;
	public Vector3 bigLetterCenter = new Vector3(0, -16, 0);

	[Header("------------")]

	public GameMode mode = GameMode.preGame;
	public WordLevel currLevel;
	public List<Wyrd> wyrds;
	public List<Letter> bigLetters;
	public List<Letter> bigLettersActive;

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
		Layout();
	}

	void Layout()
	{
		//Place the letters for each subword of currLevel on screen
		wyrds = new List<Wyrd>();

		//Variables that will be used in this method
		GameObject go;
		Letter lett;
		string word;
		Vector3 pos;
		float left = 0;
		float columnWidth = 3;
		char c;
		Color col;
		Wyrd wyrd;

		//Determine how many rows of Letters will fit on screen
		int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

		//Make a Wyrd of each level.subWord
		for (int i = 0; i < currLevel.subWords.Count; i++)
		{
			wyrd = new Wyrd();
			word = currLevel.subWords[i];

			//If the word is longer than columnWidth, expand it
			columnWidth = Mathf.Max(columnWidth, word.Length);

			//Instantiate a PrefabLetter for each letter of the word
			for (int j = 0; j < word.Length; j++)
			{
				c = word[j]; //Grab the jth char of the word
				go = Instantiate(prefabLetter) as GameObject;
				lett = go.GetComponent<Letter>();
				lett.c = c; //Set the c of the Letter

				//Position the Letter
				pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

				//The % here makes multiple columns line up
				pos.y -= (i % numRows) * letterSize;
				lett.pos = pos;
				go.transform.localScale = Vector3.one * letterSize;
				wyrd.Add(lett);
			}

			if (showAllWyrds)
			{
				wyrd.visible = true; //Line for testing
			}

			wyrds.Add(wyrd);

			//If we've gotten to the numRows(th) row, start a new column
			if (i % numRows == numRows - 1)
			{
				left += (columnWidth + 0.5f) * letterSize;
			}
		}

		//Place the big letters
		//Initialize the List<>s for big Letters
		bigLetters = new List<Letter>();
		bigLettersActive = new List<Letter>();

		//Create a big Letter for each letter in the target word
		for (int i = 0; i < currLevel.word.Length; i++)
		{
			//This is similar to the process for a normal Letter
			c = currLevel.word[i];
			go = Instantiate(prefabLetter) as GameObject;
			lett = go.GetComponent<Letter>();
			lett.c = c;
			go.transform.localScale = Vector3.one * bigLetterSize;

			//Set the initial position of the big Letters below the screen
			pos = new Vector3 (0, -100, 0);
			lett.pos = pos;

			col = bigColorDim;
			lett.color = col;
			lett.visible = true; //This is always true for big letters
			lett.big = true;
			bigLetters.Add(lett);
		}

		//Shuffle the big letters
		bigLetters = ShuffleLetters(bigLetters);

		//Arrange them on screen
		ArrangeBigLetters();

		//Set the mode to be in-game
		mode = GameMode.inLevel;
	}

	//This shuffles a List<Letter> randomly and returns the result
	List<Letter> ShuffleLetters(List<Letter> letts)
	{
		List<Letter> newL = new List<Letter>();
		int ndx;
		while (letts.Count > 0)
		{
			ndx = Random.Range(0, letts.Count);
			newL.Add(letts[ndx]);
			letts.RemoveAt(ndx);
		}
		return newL;
	}

	//This arranges the big Letters on screen
	void ArrangeBigLetters()
	{
		//The halfWidth allows the big Letters to be centered
		float halfWidth = ((float) bigLetters.Count) / 2f - 0.5f;
		Vector3 pos;
		for (int i = 0; i < bigLetters.Count; i++)
		{
			pos = bigLetterCenter;
			pos.x += (i - halfWidth) * bigLetterSize;
			bigLetters[i].pos = pos;
		}

		//bigLettersActive
		halfWidth = ((float) bigLettersActive.Count) / 2f - 0.5f;
		for (int i = 0; i < bigLettersActive.Count; i++)
		{
			pos = bigLetterCenter;
			pos.x += (i - halfWidth) * bigLetterSize;
			pos.y += bigLetterSize * 1.25f;
			bigLettersActive[i].pos = pos;
		}
	}
}
