using System;
using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    public class LanguageManager : MonoBehaviour
    {
        public TextAsset CSVLanguageFile;
        
        public static LanguageManager instance;
        public bool isEnglish
        {
            get { return is_english; }
            set
            {
                var update = value != is_english;
                is_english = value;
                if (update)
                {
                    onLanguageChanged?.Invoke();
                }
            }
        }

        public string[] russian, english , keys;
        
        public delegate void OnLanguageChanged();

        public event OnLanguageChanged onLanguageChanged;
            
        
        private bool is_english = true;

        public string GetValueByKey(string key)
        {
            key = key.ToLower();
            switch (key)
            {
                case "_vsync":
                    return GetValueByKey("vsync")+": " + (
                               QualitySettings.vSyncCount == 0 ? "Off" :
                               QualitySettings.vSyncCount == 1 ? "60fps" :
                               QualitySettings.vSyncCount == 2 ? "30fps" :
                               QualitySettings.vSyncCount.ToString());
                case "_aa":
                    return GetValueByKey("aa")+": " + QualitySettings.antiAliasing + "x";
                case "_language":
                    return GetValueByKey("language") +": " + (is_english ? "English" : "Русский");
                case "_loading":
                    return GetValueByKey("loading") + " " + GameManager.loadingPercent + "%"; 
            }

            if (!keys.Contains(key))
            {
                Debug.LogError("Not contains key:" + key);
                return key;
            }

            return is_english ? english[Array.IndexOf(keys, key)] : russian[Array.IndexOf(keys, key)];
        }


        private void Awake()
        {

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                LoadKeys();
                DontDestroyOnLoad(instance);
            }
        }

        public void LoadKeys()
        {
            var grid = SplitCsvGrid(CSVLanguageFile.text);
            
            keys = new string[grid.GetLength(1)];
            russian = new string[grid.GetLength(1)];
            english = new string[grid.GetLength(1)];

            for (var i = 0; i < grid.GetLength(1); i++)
            {
                keys[i] = grid[0, i];
                russian[i] = grid[1, i];
                english[i] = grid[2, i];
            }
        }

        private static string[,] SplitCsvGrid(string csvText)
        {
            
            
            var lines = csvText.Split("\n"[0]);

            var width = lines.Select(SplitCsvLine).Aggregate(0, (current, row) => Mathf.Max(current, row.Length));

            var outputGrid = new string[width +1, lines.Length +1];
            for (var y = 0; y < lines.Length; y++)
            {
                var row = SplitCsvLine(lines[y]);
                for (var x = 0; x < row.Length; x++)
                {
                    outputGrid[x, y] = row[x];
                    outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
                }
            }

            return outputGrid;
        }

        private static string[] SplitCsvLine(string line)
        {
            return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
                    @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
                    System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
        }
    }
}