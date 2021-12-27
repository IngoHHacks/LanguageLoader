using System.Collections.Generic;

namespace LanguageLoader
{
    public class LanguageInfo
    {
        public string fileName;
        public string id;
        public string name;
        public string author;
        public string special;
        public string confirmText;
        public string translations;

        public LanguageInfo(string name, string meta, string translations)
        {
            fileName = name;
            this.translations = translations;
            Dictionary<string, string> metaMap = new Dictionary<string, string>();
            foreach (string line in meta.Replace("\r", "").Split('\n'))
            {
                if (line.Contains(":"))
                {
                    string[] arr = line.Replace(": ", ":").Split(new char[] { ':' }, 2);
                    metaMap.Add(arr[0], arr[1]);
                }
            }
            metaMap.TryGetValue("id", out id);
            metaMap.TryGetValue("name", out this.name);
            if (this.name == null) this.name = name;
            metaMap.TryGetValue("author", out author);
            metaMap.TryGetValue("special", out special);
        }
    }
}