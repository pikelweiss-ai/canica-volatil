using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Art original editable per codi. Cap imatge externa ni captura de la UI.
public static class AlphaArtworkBuilder
{
    const string Dir="Assets/Artwork/Alpha";
    static Texture2D image;
    static Color C(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c);return c; }
    static void Begin(int w,int h,Color color) { image=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color[w*h];for(int i=0;i<pixels.Length;i++)pixels[i]=color;image.SetPixels(pixels); }
    static void Rect(int x,int y,int w,int h,string color) { var c=C(color);for(int j=Math.Max(0,y);j<Math.Min(image.height,y+h);j++)for(int i=Math.Max(0,x);i<Math.Min(image.width,x+w);i++)image.SetPixel(i,j,c); }
    static void Disc(int x,int y,int rx,int ry,string color) { var c=C(color);for(int j=Math.Max(0,y-ry);j<Math.Min(image.height,y+ry);j++)for(int i=Math.Max(0,x-rx);i<Math.Min(image.width,x+rx);i++)if((float)(i-x)*(i-x)/(rx*rx)+(float)(j-y)*(j-y)/(ry*ry)<=1)image.SetPixel(i,j,c); }
    static void Poly(string color,params Vector2Int[] points)
    {
        var xs=new List<int>();int min=image.height,max=0;foreach(var p in points){min=Math.Min(min,p.y);max=Math.Max(max,p.y);}
        for(int y=Math.Max(0,min);y<=Math.Min(image.height-1,max);y++)
        {
            xs.Clear();for(int i=0,j=points.Length-1;i<points.Length;j=i++)
                if((points[i].y>y)!=(points[j].y>y))xs.Add(points[i].x+(y-points[i].y)*(points[j].x-points[i].x)/(points[j].y-points[i].y));
            xs.Sort();for(int i=0;i+1<xs.Count;i+=2)Rect(xs[i],y,xs[i+1]-xs[i]+1,1,color);
        }
    }
    public static void Build()
    {
        Directory.CreateDirectory(Dir);Logo();Cavern();Hero();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach(string path in Directory.GetFiles(Dir,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=32;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
    }
    static void Save(string name) { image.Apply();File.WriteAllBytes(Dir+"/"+name+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image); }
    public static Sprite Load(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Dir+"/"+name+".png");
    static readonly Dictionary<char,string> Glyphs=new Dictionary<char,string> {
        {'C',"01111/11000/11000/11000/11000/11000/01111"}, {'A',"01110/11011/11011/11111/11011/11011/11011"},
        {'N',"10001/11001/11001/10101/10011/10011/10001"}, {'I',"11111/00100/00100/00100/00100/00100/11111"},
        {'V',"11011/11011/11011/11011/11011/01110/00100"}, {'O',"01110/11011/11011/11011/11011/11011/01110"},
        {'L',"11000/11000/11000/11000/11000/11000/11111"}, {'T',"11111/00100/00100/00100/00100/00100/00100"} };
    static void Word(string text,int y,int dx,int dy,string color)
    {
        for(int i=0;i<text.Length;i++) { var rows=Glyphs[text[i]].Split('/');for(int r=0;r<7;r++)for(int x=0;x<5;x++)if(rows[r][x]=='1')Rect(28+i*48+x*8+dx,y+(6-r)*8+dy,8,8,color); }
    }
    static void Logo()
    {
        Begin(416,184,Color.clear);
        foreach(var line in new[] { ("CANICA",104),("VOLATIL",22) })
        {
            for(int oy=-3;oy<=3;oy++)for(int ox=-3;ox<=3;ox++)Word(line.Item1,line.Item2,ox+3,oy-5,"091522");
            Word(line.Item1,line.Item2,0,-4,"a56b2c");Word(line.Item1,line.Item2,0,0,"f3c85b");
            // Brillantor superior de cada pixel de lletra.
            var rows=line.Item1;for(int i=0;i<rows.Length;i++){var bits=Glyphs[rows[i]].Split('/');for(int r=0;r<7;r++)for(int x=0;x<5;x++)if(bits[r][x]=='1' && (r==0 || bits[r-1][x]=='0'))Rect(28+i*48+x*8,line.Item2+(6-r)*8+6,8,2,"fff0ae");}
        }
        Rect(178,84,8,5,"091522");Rect(175,87,8,5,"f3c85b");Rect(169,92,8,5,"fff0ae");
        Marble(365,126,25);Save("Logo");
    }
    static void Marble(int x,int y,int r)
    {
        Disc(x+3,y-4,r+2,r+2,"07131e");Disc(x,y,r,r,"b97c24");Disc(x,y+2,r-2,r-2,"efbe30");Disc(x-3,y+5,r-5,r-6,"ffda49");
        Rect(x-r/2,y+1,r/5,r/3,"131c22");Rect(x+r/4,y+1,r/5,r/3,"131c22");Rect(x-r/2,y-r/2,r,r/5,"131c22");Rect(x-r/3,y-r/2-3,2*r/3,4,"131c22");Rect(x-r/2,y+r/2,5,4,"fff0a8");
    }
    static void Cavern()
    {
        Begin(640,360,C("0c1722"));
        // Masses de roca continues, vores irregulars i centre tranquil.
        var rng=new System.Random(4719);
        for(int layer=0;layer<3;layer++)
        {
            string color=layer==0?"142938":layer==1?"10222f":"0a141e";
            for(int x=-30;x<680;x+=rng.Next(25,65))
            {
                int depth=rng.Next(15,65)+layer*12;
                Poly(color,new Vector2Int(x,360),new Vector2Int(x+70,360),new Vector2Int(x+45,360-depth),new Vector2Int(x+20,360-depth-12));
                Poly(color,new Vector2Int(x,0),new Vector2Int(x+70,0),new Vector2Int(x+50,depth),new Vector2Int(x+20,depth+10));
            }
        }
        for(int x=15;x<640;x+=120)
        {
            int h=38+rng.Next(35);Rect(x,25,18,h,"182d38");Rect(x+2,25,3,h,"24434c");Rect(x-4,25+h,26,5,"2b4750");
        }
        for(int i=0;i<95;i++){int x=rng.Next(640),y=rng.Next(360);if(y<70 || y>290)Rect(x,y,2+rng.Next(5),1,"26414a");}
        Save("CavernBackdrop");
    }
    static void Hero()
    {
        Begin(448,496,C("0b1c29"));
        // Portal de ruines, profunditat, passarel·la i un tresor per descobrir.
        Poly("163441",new Vector2Int(0,390),new Vector2Int(80,485),new Vector2Int(300,495),new Vector2Int(448,360),new Vector2Int(448,250),new Vector2Int(0,270));
        Poly("204652",new Vector2Int(25,275),new Vector2Int(100,425),new Vector2Int(315,450),new Vector2Int(414,300));
        Disc(255,325,82,112,"101c29");Disc(255,325,60,92,"29616b");Disc(255,325,45,78,"478e88");Disc(255,325,28,68,"7bb6a0");
        for(int y=260;y<431;y+=25) { Rect(164,y,20,22,"536568");Rect(167,y+17,15,3,"8a9890");Rect(325,y-10,20,22,"3a4e57"); }
        for(int x=183;x<325;x+=25){Rect(x,421,23,21,"617774");Rect(x+2,438,18,3,"a2aa8b");}
        Poly("0a2833",new Vector2Int(0,0),new Vector2Int(0,190),new Vector2Int(190,130),new Vector2Int(448,200),new Vector2Int(448,0));
        for(int i=0;i<35;i++)Rect((i*71)%440,35+(i*29)%105,8+(i%4)*5,1,"20515a");
        Poly("27303b",new Vector2Int(27,202),new Vector2Int(210,280),new Vector2Int(408,205),new Vector2Int(221,104));
        Poly("53664e",new Vector2Int(27,222),new Vector2Int(210,300),new Vector2Int(408,225),new Vector2Int(221,131));
        Poly("829265",new Vector2Int(27,222),new Vector2Int(210,300),new Vector2Int(408,225),new Vector2Int(221,151));
        Poly("506853",new Vector2Int(66,225),new Vector2Int(210,281),new Vector2Int(366,225),new Vector2Int(219,170));
        for(int i=0;i<35;i++){int x=100+(i*41)%250,y=190+(i*13)%53;Rect(x,y,3,2,i%2==0?"91a169":"344f48");}
        for(int i=0;i<5;i++)
        {
            int x=209+i*7,y=224+i*10;
            Poly("9b9c7a",new Vector2Int(x-18,y),new Vector2Int(x,y+8),new Vector2Int(x+19,y),new Vector2Int(x,y-7));
            Rect(x-8,y+3,10,1,"c5bea0");
        }
        Poly("6a503d",new Vector2Int(188,156),new Vector2Int(244,156),new Vector2Int(293,0),new Vector2Int(220,0));
        for(int y=8;y<149;y+=15){Rect(218-y/5,y,68-y/6,5,"ac8860");Rect(220-y/5,y+4,64-y/6,2,"d0ab74");}
        Rect(310,235,44,25,"543e30");Rect(310,256,44,9,"a4773e");Rect(310,252,44,4,"e0b45d");Rect(327,244,8,12,"edcb72");
        Rect(315,237,3,26,"9f884f");Rect(346,237,3,26,"9f884f");Rect(330,245,2,4,"473929");Rect(322,257,16,2,"c79853");
        Disc(152,209,42,12,"293a35");Marble(152,242,35);
        foreach(var p in new[] { new Vector2Int(67,285),new Vector2Int(374,310) })
        {
            Rect(p.x,p.y-25,8,65,"72513e");Disc(p.x+4,p.y+50,40,43,"203e37");Disc(p.x-6,p.y+60,32,34,"3a6846");Disc(p.x-10,p.y+70,22,20,"72914b");
            Rect(p.x+2,p.y-20,2,44,"a17b4b");
            for(int i=0;i<14;i++)
            {
                int x=p.x-28+(i*13)%60,y=p.y+28+(i*17)%58;
                Disc(x,y,5+i%4,3+i%3,i%3==0?"91a454":i%3==1?"4e7b47":"315d40");
            }
        }
        for(int i=0;i<9;i++){int x=210+(i*31)%110,y=306+(i*47)%90;Rect(x,y,3,3,"d1dca4");}
        Save("MenuExpedition");
    }
}
