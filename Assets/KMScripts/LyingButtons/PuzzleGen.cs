using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleGen 
{
    //static Dictionary<string, List<List<int>>> storedPermutations = new Dictionary<string, List<List<int>>>();
    private int NUM_TOTAL_RETRIES = 1000;

    public Button[] generatePuzzle(Button[] buttons, List<int> possNumLiars, int numColors)
    {
        List<ClueObj> allPossClues = getAllPossibleClueIds(buttons, possNumLiars, numColors);
        //Debug.LogFormat("Number of clues: {0}", allPossClues.Count);
        List<ClueObj> allTrueClues = new List<ClueObj>(), allFalseClues = new List<ClueObj>();
        ClueTester clueTester = new ClueTester();
        foreach (ClueObj clue in allPossClues)
        {
            if (clueTester.testClue(clue, buttons))
                allTrueClues.Add(clue);
            else
                allFalseClues.Add(clue);
        }
        /*
        Debug.LogFormat("All true clues: {0}", allTrueClues.Count);
        foreach(ClueObj clue in allTrueClues)
            Debug.LogFormat("{0}", clue.toString());
        Debug.LogFormat("All false clues: {0}", allFalseClues.Count);
        foreach (ClueObj clue in allFalseClues)
            Debug.LogFormat("{0}", clue.toString());
        */
        /*
        List<List<ClueObj>> trueClues = new List<List<ClueObj>>(), falseClues = new List<List<ClueObj>>();
        for(int i = 0; i < buttons.Length; i++)
        {
            if(buttons[i].isTruth)
            {
                trueClues.Add(new List<ClueObj>());
                trueClues[trueClues.Count - 1].AddRange(allTrueClues);
                trueClues[trueClues.Count - 1].Shuffle();
            }
            else
            {
                falseClues.Add(new List<ClueObj>());
                falseClues[falseClues.Count - 1].AddRange(allFalseClues);
                falseClues[falseClues.Count - 1].Shuffle();
            }
        }
        */
        return GetValidPuzzle(buttons, clueTester, possNumLiars, allTrueClues, allFalseClues);
    }
	private List<ClueObj> getAllPossibleClueIds(Button[] buttons, List<int> possNumLiars, int numColors)
    {
        possNumLiars.Sort();
        int maxLiars = possNumLiars[possNumLiars.Count - 1];
        string[] locIds = { "RA", "RB", "RC", "CA", "CB", "CC" };
        string[] locNames = { "top row", "middle row", "bottom row", "left column", "middle column", "right column" };
        List<ClueObj> clues = new List<ClueObj>();

        
        List<ClueObj> locClues = new List<ClueObj>();
        ClueTester clueTester = new ClueTester();
        for (int i = 0; i <= maxLiars && i <= 3; i++)
        {
            for(int j = 0; j < locIds.Length; j++)
                locClues.Add(new CEqual("The " + locNames[j] + " contains exactly " + i + " liars", locIds[j], "LLL", i));
        }
        for(int i = 1; i < maxLiars && i < 3; i++)
        {
            for (int j = 0; j < locIds.Length; j++)
            {
                locClues.Add(new CLeast("The " + locNames[j] + " contains at least " + i + " liar(s)", locIds[j], "LLL", i));
                locClues.Add(new CFewer("The " + locNames[j] + " contains " + i + " or fewer liars", locIds[j], "LLL", i));
            }
        }
        for(int i = 0; i < locIds.Length; i++)
        {
            for (int j = i + 1; j < locIds.Length; j++)
                locClues.Add(new CEqual("The " + locNames[i] + " contains an equal number of liars as the " + locNames[j], locIds[i], "LLL", locIds[j], "LLL"));
        }
        for (int i = 0; i < locIds.Length; i++)
        {
            for (int j = 0; j < locIds.Length; j++)
            {
                if(i != j)
                    locClues.Add(new CGreater("The " + locNames[i] + " contains more liars than the " + locNames[j], locIds[i], "LLL", locIds[j], "LLL"));
            }
        }
        List<ClueObj> trueLocClues = new List<ClueObj>(), falseLocClues = new List<ClueObj>();
        foreach(ClueObj clue in locClues)
        {
            if (clueTester.testClue(clue, buttons))
                trueLocClues.Add(clue);
            else
                falseLocClues.Add(clue);
        }
        trueLocClues = trueLocClues.Shuffle();
        falseLocClues = falseLocClues.Shuffle();
        while (trueLocClues.Count > 9)
            trueLocClues.RemoveAt(0);
        while (falseLocClues.Count > 9)
            falseLocClues.RemoveAt(0);
        clues.AddRange(trueLocClues);
        clues.AddRange(falseLocClues);

        for (int i = 1; i <= maxLiars && i <= 3; i++)
        {
            clues.Add(new CEqual("The number of rows that contains a liar is exactly " + i, "GD", "DRL", i));
            clues.Add(new CEqual("The number of columns that contains a liar is exactly " + i, "GD", "DCL", i));
        }

        foreach(int i in possNumLiars)
            clues.Add(new CEqual(string.Format("There {1} exactly {0} liar{2}", i, i == 1 ? "is" : "are", i == 1 ? "" : "s"), "GD", "LLL", i));
        
        for(int i = 0; i <= maxLiars; i++)
        {
            if(i != 1)
                clues.Add(new CEqual("There are exactly " + i + " liars that are orthogonally adjacent to one another", "GD", "AJL", i));
        }
        
        locIds = new string[] { "A1", "B1", "C1", "A2", "B2", "C2", "A3", "B3", "C3" };
        locNames = new string[] { "top left", "top middle", "top right", "middle left", "center", "middle right", "bottom left", "bottom middle", "bottom right" };
        for (int i = 0; i < 9; i++)
        {
            clues.Add(new CEqual("The " + locNames[i] + " button is not a liar", locIds[i], "LLL", 0));
            clues.Add(new CEqual("The " + locNames[i] + " button is a liar", locIds[i], "LLL", 1));
        }

        /*
        int[] maxNumAdj = { 2, 3, 2, 3, 4, 3, 2, 3, 2 };
        for (int i = 0; i < 9; i++)
        {
            for(int j = 0; j <= maxNumAdj[i] && j <= maxLiars; j++)
                clues.Add(new CEqual("There is exactly " + j + " liars that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
        }
        for (int i = 0; i < 9; i++)
        {
            for (int j = 1; j < maxNumAdj[i] && j < maxLiars; j++)
            {
                clues.Add(new CLeast("There is at least " + j + " liar(s) that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
                clues.Add(new CFewer("There are " + j + " or fewer liars that are orthogonally adjacent to the " + locNames[i] + " button", "J" + (i + 1), "LLL", j));
            }  
        }
        */
        
        int[] colorSums = new int[numColors];
        foreach (Button button in buttons)
            colorSums[(int)button.buttonColor]++;
        List<ClueObj> colorClues = new List<ClueObj>();
        for(int i = 0; i < numColors; i++)
        {
            if(colorSums[i] > 1)
            {
                for(int j = 0; j < numColors; j++)
                {
                    if (i != j)
                        colorClues.Add(new CGreater("The " + getColorName((ButtonColor)i) + " buttons has more liars than the " + getColorName((ButtonColor)j) + " buttons", "GD", "N" + getColorCode((ButtonColor)i) + "L", "N" + getColorCode((ButtonColor)j) + "L"));
                }
            }
        }
        for (int i = 0; i < numColors; i++)
        {
            if (colorSums[i] > 0)
            {
                for (int j = i + 1; j < numColors; j++)
                {
                    if (colorSums[j] > 0)
                        colorClues.Add(new CEqual("The number of liars in the " + getColorName((ButtonColor)i) + " buttons are equal to the number of liars in the " + getColorName((ButtonColor)j) + " buttons", "GD", "N" + getColorCode((ButtonColor)i) + "L", "GD", "N" + getColorCode((ButtonColor)j) + "L"));
                }
            }
        }
        for(int i = 0; i < numColors; i++)
        {
            if(colorSums[i] > 0)
            {
                for(int j = 0; j <= colorSums[i] && j <= maxLiars; j++)
                    colorClues.Add(new CEqual("The " + getColorName((ButtonColor)i) + " buttons contains exactly " + j + " liar(s)", "GD", "N" + getColorCode((ButtonColor)i) + "L", j));
            }
        }
        for (int i = 0; i < numColors; i++)
        {
            for (int j = 1; j < colorSums[i] && j < maxLiars; j++)
            {
                colorClues.Add(new CLeast("The " + getColorName((ButtonColor)i) + " buttons contains at least " + j + " liar(s)", "GD", "N" + getColorCode((ButtonColor)i) + "L", j));
                colorClues.Add(new CFewer("The " + getColorName((ButtonColor)i) + " buttons contains " + j + " or fewer liars", "GD", "N" + getColorCode((ButtonColor)i) + "L", j));
            }
        }
        List<ClueObj> trueColorClues = new List<ClueObj>(), falseColorClues = new List<ClueObj>();
        foreach(ClueObj clue in colorClues)
        {
            if (clueTester.testClue(clue, buttons))
                trueColorClues.Add(clue);
            else
                falseColorClues.Add(clue);
        }
        trueColorClues = trueColorClues.Shuffle();
        while (trueColorClues.Count > 9)
            trueColorClues.RemoveAt(0);
        falseColorClues = falseColorClues.Shuffle();
        while (falseColorClues.Count > 9)
            falseColorClues.RemoveAt(0);
        clues.AddRange(trueColorClues);
        clues.AddRange(falseColorClues);
        int dc = 0;
        foreach(int colorSum in colorSums)
        {
            if (colorSum > 0)
                dc++;
        }
        if(dc > 1)
        {
            for (int i = 1; i <= maxLiars && i <= dc; i++)
                clues.Add(new CEqual("The number of colors that contains a liar is exactly " + i, "GD", "CDL", i));
        }
        
        return clues;
    }
    private Button[] GetValidPuzzle(Button[] buttons, ClueTester clueTester, List<int> possNumLiars, List<ClueObj> trueClues, List<ClueObj> falseClues)
    {
        for(int z = 0; z < NUM_TOTAL_RETRIES; z++)
        {
            for (var x = 0; x < buttons.Length; x++)
                buttons[x].clue = buttons[x].isTruth ? trueClues.PickRandom() : falseClues.PickRandom();
            if (CanSolve(buttons: buttons, clueTester: clueTester, possNumLiars: possNumLiars))
                return buttons;
        }
        return null;
    }
    private bool CanSolve(Button[] buttons, ClueTester clueTester, List<int> possNumLiars)
    {
        int sum = 0;
        var cntButtons = buttons.Length;
        var possibleCombinations = possNumLiars.SelectMany(amount => GeneratePermutations(cntButtons, amount)).ToList();
        for (var x = 0; x < possibleCombinations.Count && sum < 2; x++)
        {
            var curCombination = possibleCombinations[x];
            var copiedButtons = Copy(buttons);
            for (var idx = 0; idx < cntButtons; idx++)
                copiedButtons[idx].isTruth = !curCombination.Contains(idx);
            if (copiedButtons.All(aButton => clueTester.testClue(aButton.clue, copiedButtons) == aButton.isTruth))
                sum++;
        }
        return sum == 1;
    }

    private static List<List<int>> GeneratePermutations(int itemCount, int itemsPicked = 0)
    {
        var output = new List<List<int>> { new List<int>() };
        for (var curLoopIdx = 0; curLoopIdx < itemsPicked; curLoopIdx++)
        {
            var nextOutput = new List<List<int>>();
            foreach (var item in output)
            {
                var idxesAllow = Enumerable.Range(0, itemCount).Where(a => !item.Any() || a > item.Max()).ToArray();
                foreach (var idxCur in idxesAllow)
                    nextOutput.Add(item.Concat(new[] { idxCur }).ToList());
            }
            output = nextOutput;
        }
        return output;
    }

    private string getColorCode(ButtonColor color)
    {
        switch(color)
        {
            case ButtonColor.RED:
                return "R";
            case ButtonColor.YELLOW:
                return "Y";
            case ButtonColor.BLUE:
                return "B";
        }
        return null;
    }
    private string getColorName(ButtonColor color)
    {
        switch (color)
        {
            case ButtonColor.RED:
                return "red";
            case ButtonColor.YELLOW:
                return "yellow";
            case ButtonColor.BLUE:
                return "blue";
        }
        return null;
    }
    private Button[] Copy(Button[] buttons)
    {
        Button[] copy = new Button[buttons.Length];
        for(int i = 0; i < buttons.Length; i++)
            copy[i] = new Button(buttons[i].buttonColor, buttons[i].isTruth, buttons[i].coord, buttons[i].clue);
        return copy;
    }
}
