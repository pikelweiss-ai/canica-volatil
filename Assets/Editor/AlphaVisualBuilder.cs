using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public static class AlphaVisualBuilder
{
    const string Dir = "Assets/Sprites/Pixel/Alpha";
    static readonly string[] Themes = { "Cova", "Bosc", "Desert", "Neu", "Volca", "Claveguera", "Cripta", "Cristall" };
    static readonly string[] Stone = { "535d70", "4f6548", "a58b65", "7793a5", "5d4649", "52645a", "5d586a", "67658a" };
    // Pixel art definit en codi: graella base i paletes editables, sense imatges externes.
    static readonly string[] Surface = {
        "mmmlmmmlmmmlmmmm","mmlmmmlmmmlmmmmm","mlmmmlmmmlmmmmmm","lmmmlmmmlmmmmmmm",
        "mmmlmmmlmmmmmmml","mmlmmmlmmmmmmmlm","mlmmmlmmmmmmmlmm","lmmmlmmmmmmmlmmm",
        "mmmlmmmmmmmlmmml","mmlmmmmmmmlmmmlm","mlmmmmmmmlmmmlmm","lmmmmmmmlmmmlmmm",
        "mmmmmmmlmmmlmmml","mmmmmlmmmlmmmlmm","mmmmlmmmlmmmlmmm","mmmlmmmlmmmlmmmm" };
    [MenuItem("Canica/Alpha/Preparar ambients i estil visual")]
    public static void BuildAssets()
    {
        AlphaArtworkBuilder.Build();
        Directory.CreateDirectory(Dir);
        Directory.CreateDirectory("Assets/Tiles/Alpha");
        for(int i=0;i<Themes.Length;i++)
        {
            Color tone=Hex(Stone[i]); int theme=i;
            Write(Themes[i]+"_TerraA",(x,y)=>Architecture(x,y,tone,false,0,theme));
            Write(Themes[i]+"_TerraB",(x,y)=>Architecture(x,y,tone,false,1,theme));
            Write(Themes[i]+"_Mur",(x,y)=>Architecture(x,y,tone,true,0,theme));
        }
        foreach(string name in new[] { "Lava", "Acid" })
        {
            bool lava = name == "Lava";
            for(int mask=0;mask<256;mask++)
                if(DamageTile.CanonicalMask(mask)==mask)
                    Write(name+"_"+mask, (x,y)=>Liquid(x,y,mask,lava));
        }
        var palette = new[] { "253042", "294632", "66503d", "344451", "503832", "303c39", "3c3d59", "293e3c" };
        for(int i=0;i<8;i++)
        {
            Color tint = Hex(palette[i]);
            Write("Fons_"+i,(x,y)=>tint * (((x+y*3)%16==0 || (y%8==0 && x%4!=0)) ? .65f : .38f));
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        // El fons es repeteix com a Sprite tiled, amb geometria rectangular completa.
        foreach(string file in Directory.GetFiles(Dir,"Fons_*.png"))
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            var settings = new TextureImporterSettings(); imp.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect; imp.SetTextureSettings(settings); imp.SaveAndReimport();
        }
        foreach(string name in new[] { "Lava", "Acid" })
        {
            var tile = AssetDatabase.LoadAssetAtPath<DamageTile>("Assets/Tiles/Masmorra/"+name+".asset");
            tile.connections = new Sprite[256];
            for(int mask=0;mask<256;mask++) if(DamageTile.CanonicalMask(mask)==mask) tile.connections[mask] = Sprite(name+"_"+mask);
            tile.sprite = tile.connections[255]; EditorUtility.SetDirty(tile);
        }
        var style = AssetDatabase.LoadAssetAtPath<AlphaVisualStyle>("Assets/Resources/AlphaVisualStyle.asset");
        if(style == null) { style = ScriptableObject.CreateInstance<AlphaVisualStyle>(); AssetDatabase.CreateAsset(style,"Assets/Resources/AlphaVisualStyle.asset"); }
        style.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        style.coinFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MinecraftStandard SDF.asset");
        style.grass = DefaultSprite("GrassTile"); style.brick = DefaultSprite("BrickTile"); style.wood = DefaultSprite("PlanksTile"); style.tree = DefaultSprite("TreeTile");
        style.player = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pixil-frame-0.png"); EditorUtility.SetDirty(style);
        style.logo=AlphaArtworkBuilder.Load("Logo");style.menuArtwork=AlphaArtworkBuilder.Load("MenuExpedition");
        var paths = AssetDatabase.FindAssets("t:DungeonTheme",new[] { "Assets/Dungeon/Temes" }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
        for(int i=0;i<paths.Length;i++)
        {
            var t = AssetDatabase.LoadAssetAtPath<DungeonTheme>(paths[i]); t.voidSprite = AlphaArtworkBuilder.Load("CavernBackdrop");
            string key=Themes.FirstOrDefault(k=>paths[i].Contains("_"+k+"."));
            if(key != null)
            {
                t.floorTiles=new TileBase[] { MakeTile(key+"_TerraA",false),MakeTile(key+"_TerraB",false) };
                t.wallTile=MakeTile(key+"_Mur",true);
            }
            if(paths[i].Contains("Bosc")) { t.floorTiles = new[] { AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/GrassTile.asset") }; t.refugeFloor = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/PlanksTile.asset"); }
            if(paths[i].Contains("Cripta")) { t.wallTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/BrickTile.asset"); t.refugeFloor = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/PlanksTile.asset"); }
            EditorUtility.SetDirty(t);
        }
        AssetDatabase.SaveAssets();
        Validate(); Debug.Log("ALPHA_VISUALS: assets preparats i connexions validades.");
    }
    static Tile MakeTile(string name,bool wall)
    {
        string path="Assets/Tiles/Alpha/"+name+".asset";
        var tile=AssetDatabase.LoadAssetAtPath<Tile>(path);
        if(tile==null) { tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,path); }
        tile.sprite=Sprite(name);tile.colliderType=wall ? Tile.ColliderType.Grid : Tile.ColliderType.None;EditorUtility.SetDirty(tile);return tile;
    }
    static Color Architecture(int x,int y,Color tone,bool wall,int variant,int theme)
    {
        if(wall)
        {
            Color face=Color.Lerp(tone,Hex(theme==0 ? "382b29" : "17212b"),.55f);
            if(y>=13) return y==15 ? Color.Lerp(tone,Hex("d6be8d"),.45f) : tone*.9f;
            int sx=(x+((y/6)%2)*8)%16;
            if(y==0 || y==6 || y==12 || sx==0) return face*.5f;
            if(y==5 || y==11) return face*1.2f;
            return sx==15 ? face*.7f : face;
        }
        int height=wall ? 4 : 8;
        int shifted=(x+((y/height)%2)*8)%16;
        bool seam=y%height==0 || shifted==0;
        if(seam) return tone*.48f;
        if(y%height==height-1) return tone*1.22f;
        if(shifted==15) return tone*.72f;
        int grain=(x*17+y*31+variant*53)%67;
        if(grain<4) return tone*(variant==0 ? .76f : 1.16f);
        // Musgo, vetes calentes o cristall: accents xicotets dins del material.
        if(theme==1 && (x+y*3)%29==0) return Hex("748357");
        if(theme==4 && wall && y==2 && x>9 && x<13) return Hex("a56542");
        if(theme==7 && x==5 && y>6 && y<11) return Hex("929ec3");
        return tone;
    }
    public static void Validate()
    {
        foreach(string name in new[] { "Lava", "Acid" })
        {
            var tile = AssetDatabase.LoadAssetAtPath<DamageTile>("Assets/Tiles/Masmorra/"+name+".asset");
            if(tile == null || tile.damage != 1 || tile.colliderType != Tile.ColliderType.None) throw new Exception("Perill alterat: "+name);
            int count = 0;
            for(int mask=0;mask<256;mask++) { if(tile.connections[DamageTile.CanonicalMask(mask)] == null) throw new Exception("Falta connexio: "+name+mask); if(DamageTile.CanonicalMask(mask)==mask) count++; }
            if(count != 47) throw new Exception("Les 47 connexions no coincideixen");
            ValidateTilemap(tile);
        }
        if(AlphaVisualStyle.Current == null || AlphaVisualStyle.Current.font == null) throw new Exception("Falta estil o font");
    }
    static void ValidateTilemap(DamageTile tile)
    {
        var grid = new GameObject("Prova de connexions",typeof(Grid));
        try
        {
            var go = new GameObject("Tilemap",typeof(Tilemap)); go.transform.SetParent(grid.transform,false);
            var map = go.GetComponent<Tilemap>();
            var neighbours = new[] { Vector3Int.up,Vector3Int.right,Vector3Int.down,Vector3Int.left,new Vector3Int(1,1,0),new Vector3Int(1,-1,0),new Vector3Int(-1,-1,0),new Vector3Int(-1,1,0) };
            for(int mask=0;mask<256;mask++)
            {
                if(DamageTile.CanonicalMask(mask)!=mask) continue;
                map.ClearAllTiles(); map.SetTile(Vector3Int.zero,tile);
                for(int i=0;i<8;i++) if((mask & (1<<i))!=0) map.SetTile(neighbours[i],tile);
                if(map.GetSprite(Vector3Int.zero)!=tile.connections[mask]) throw new Exception("Tilemap no connecta: "+tile.name+"/"+mask);
            }
            map.ClearAllTiles(); map.SetTile(Vector3Int.zero,tile); map.SetTile(Vector3Int.right,tile); map.SetTile(Vector3Int.right,null);
            if(map.GetSprite(Vector3Int.zero)!=tile.connections[0]) throw new Exception("La vora no s'actualitza en llevar un tile");
        }
        finally { UnityEngine.Object.DestroyImmediate(grid); }
    }
    static Sprite DefaultSprite(string name)
    {
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/"+name+".asset");
        return tile is RuleTile rule ? rule.m_DefaultSprite : tile is Tile plain ? plain.sprite : null;
    }
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Dir+"/"+name+".png");
    static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c); return c; }
    static Color Liquid(int x,int y,int mask,bool lava)
    {
        int edge = 99;
        if((mask&1)==0) edge=Math.Min(edge,15-y); if((mask&2)==0) edge=Math.Min(edge,15-x);
        if((mask&4)==0) edge=Math.Min(edge,y); if((mask&8)==0) edge=Math.Min(edge,x);
        if((mask&3)==3 && (mask&16)==0) edge=Math.Min(edge,(15-x)+(15-y));
        if((mask&6)==6 && (mask&32)==0) edge=Math.Min(edge,(15-x)+y);
        if((mask&12)==12 && (mask&64)==0) edge=Math.Min(edge,x+y);
        if((mask&9)==9 && (mask&128)==0) edge=Math.Min(edge,x+(15-y));
        if(edge==0) return Hex(lava ? "37272e" : "20372f");
        if(edge==1) return Hex(lava ? "ab442e" : "4e7339");
        if(edge==2) return Hex(lava ? "ffc34f" : "c4df66");
        return Hex(Surface[15-y][x]=='l' ? (lava ? "ffc34f" : "b6d96b") : (lava ? "df6135" : "70a34b"));
    }
    static void Write(string name,Func<int,int,Color> pixel)
    {
        var tex = new Texture2D(16,16,TextureFormat.RGBA32,false);
        for(int y=0;y<16;y++) for(int x=0;x<16;x++) { Color c=pixel(x,y); c.a=1; tex.SetPixel(x,y,c); }
        tex.Apply(); File.WriteAllBytes(Dir+"/"+name+".png",tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
    }
}
