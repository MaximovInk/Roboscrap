using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager instance;
        public InputField fileName;
        public Transform loadButtonsParent,saveButtonsParent;
        public Button buttonPrefab;
        
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Application.dataPath + "/saves/"))
            {
                Directory.CreateDirectory(Application.dataPath + "/saves");
            }

            string path = Application.dataPath + "/saves/" + fileName.name + ".xyz";
            
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
            {
                bf.Serialize(fs,ChunkManager.instance.map);
            }
        }

        public void LoadSaveFiles()
        {
            if (!Directory.Exists(Application.dataPath + "/saves/"))
            {
                Directory.CreateDirectory(Application.dataPath + "/saves");
            }

            
            
            var files = Directory.EnumerateFiles(Application.dataPath+"/saves", "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".xyz") ).ToList();

            for (int i = 0; i < loadButtonsParent.childCount; i++)
            {
                Destroy(loadButtonsParent.GetChild(i).gameObject);
            }
            for (int i = 0; i < saveButtonsParent.childCount; i++)
            {
                Destroy(saveButtonsParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < files.Count; i++)
            {
                var bload = Instantiate(buttonPrefab, loadButtonsParent);
                var bsave = Instantiate(buttonPrefab, saveButtonsParent);
                var bi = i;

                var name = Path.GetFileName(files[i]).Split('.')[0];
                
                bload.GetComponentInChildren<Text>().text = name;
                bload.onClick.AddListener(() => { LoadFile(files[bi]); });
                bsave.GetComponentInChildren<Text>().text = name;
                bsave.onClick.AddListener(() => { fileName.text = name; });
            }
        }

        public void LoadFile(string path)
        {
            
            
            GameManager.Instance.LoadScene(1);
        }
    }
}