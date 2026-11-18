using System.IO;
using System.Xml;
using UnityEditor.Android;

public class AndroidLanManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 10;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string file = Path.Combine(path, "src/main/AndroidManifest.xml");
        var doc = new XmlDocument(); doc.Load(file);
        const string android = "http://schemas.android.com/apk/res/android";
        foreach (string permission in new[] { "INTERNET", "ACCESS_NETWORK_STATE", "ACCESS_WIFI_STATE", "CHANGE_WIFI_MULTICAST_STATE" })
        {
            string name = "android.permission." + permission;
            bool found = false;
            foreach(XmlElement element in doc.GetElementsByTagName("uses-permission")) if(element.GetAttribute("name",android)==name)found=true;
            if(found)continue;
            var p = doc.CreateElement("uses-permission"); p.SetAttribute("name",android,name); doc.DocumentElement.AppendChild(p);
        }
        doc.Save(file);
    }
}
