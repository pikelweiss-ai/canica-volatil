using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Ampliacio incremental: preserva els identificadors i les opcions anteriors.
public static class AlphaContentBuilder
{
    static Sprite S(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pixel/"+name+".png");
    public static void Build()
    {
        var db=AssetDatabase.LoadAssetAtPath<CosmeticsDatabase>("Assets/Resources/Cosmetics.asset");
        var skins=db.skins.ToList();
        foreach(string name in new[] { "Ciclope","Gat","Gota" })if(!skins.Any(s=>s.sprite==S("Skins/"+name)))skins.Add(new CosmeticsDatabase.Hat { name=name=="Ciclope"?"Cíclop":name,sprite=S("Skins/"+name) });
        // Skins retocades per Ferran (20/11/2026). La skin triada es guarda per posicio,
        // aixi que Flor i Irisada ocupen el lloc de Corall i Obsidiana i les noves van al final.
        foreach(var swap in new[] { ("Corall","Flor"),("Obsidiana","Irisada") })
            for(int i=0;i<skins.Count;i++)if(skins[i].name==swap.Item1){var h=skins[i];h.name=swap.Item2;h.sprite=S("Skins/"+swap.Item2);skins[i]=h;}
        foreach(string name in new[] { "Llop","Ciclotep","Postal" })if(!skins.Any(s=>s.name==name))skins.Add(new CosmeticsDatabase.Hat { name=name,sprite=S("Skins/"+name) });
        db.skins=skins.ToArray();var hats=db.hats.ToList();
        foreach(var h in hats)if(h.sprite!=null && h.sprite.name=="Astronauta"){h.fullHelmet=true;h.name="Casc d'astronauta";}
        foreach(var entry in new[] { ("Bruixa","Bruixa"),("Explorador","Explorador"),("Enginyer","Enginyer"),("Capita","Capità"),("Mag","Mag"),("Cuiner","Cuiner"),("Gat","Orelles de gat"),("Cowboy","Cowboy"),("Antenes","Antenes") })
            if(!hats.Any(h=>h.sprite==S("Barrets/"+entry.Item1)))hats.Add(new CosmeticsDatabase.Hat {name=entry.Item2,sprite=S("Barrets/"+entry.Item1)});
        db.hats=hats.ToArray();
        foreach(var t in db.trails)
        {
            if(t.name=="Foc")t.shape=TrailShape.Sparks;else if(t.name=="Elèctric")t.shape=TrailShape.Zigzag;else if(t.name=="Toxic")t.shape=TrailShape.Rings;else if(t.name=="Blanc")t.shape=TrailShape.Dots;
        }
        var trails=db.trails.ToList();
        void Trail(string name,TrailShape shape,Color color){if(!trails.Any(t=>t.name==name))trails.Add(new CosmeticsDatabase.Tint {name=name,shape=shape,color=color});}
        Trail("Pols d'estrelles",TrailShape.Stars,new Color(1,.8f,.3f));Trail("Bombolles",TrailShape.Rings,new Color(.4f,.9f,.8f));
        Trail("Petjades",TrailShape.Footsteps,new Color(.9f,.85f,.7f));Trail("Punts de pluja",TrailShape.Dots,new Color(.4f,.7f,1));
        Trail("Espurnes",TrailShape.Sparks,new Color(1,.55f,.2f));Trail("Ziga-zaga",TrailShape.Zigzag,new Color(.7f,.5f,1));
        // v1.3.1: esteles noves
        Trail("Cors",TrailShape.Hearts,new Color(1,.4f,.6f));Trail("Flamarada",TrailShape.Flames,new Color(1,.5f,.1f));
        Trail("Floc de neu",TrailShape.Snow,new Color(.8f,.95f,1));Trail("Diamants",TrailShape.Diamonds,new Color(.4f,.95f,1));
        Trail("Fulles",TrailShape.Leaves,new Color(.4f,.85f,.35f));Trail("Arc de Sant Martí",TrailShape.Stars,new Color(1,1,1,.99f));
        db.trails=trails.ToArray();EditorUtility.SetDirty(db);

        var disc=Weapon("DiscCometa","Disc de cometa","Bumerang",ChestRarity.Rar);
        disc.damage=2;disc.cooldown=.85f;disc.returns=false;disc.wallBounces=3;disc.pierce=1;disc.spinSpeed=800;disc.projectileSprite=disc.icon;disc.range=9;disc.bounceDamage=1;disc.zoomRange=0;disc.bounceRangeBonus=.15f;EditorUtility.SetDirty(disc);
        var lance=Weapon("LlancaTempesta","Llança de tempesta","EspasaVidre",ChestRarity.Epic);
        lance.melee=true;lance.meleeArc=25;lance.meleeDashDistance=1.2f;lance.cooldown=.85f;lance.damage=3;lance.range=3f;lance.meleeMaxTargets=3;lance.meleeLineWidth=.7f;EditorUtility.SetDirty(lance);
        var spores=Weapon("VaretaEspores","Vareta d'espores","Vareta",ChestRarity.Rar);
        spores.damage=1;spores.pellets=3;spores.fixedSpread=true;spores.spreadAngle=10;spores.cooldown=1f;spores.homing=0;spores.projectileSpeed=11;spores.projectileEffect=ProjectileEffect.Poison;spores.range=7;spores.projectileColor=new Color(.6f,1,.4f);spores.tintProjectileSprite=true;EditorUtility.SetDirty(spores);
        var pulse = Weapon("ProjectorPolsos", "Projector de polsos", "Llancagranades", ChestRarity.Rar);
        pulse.damage = 3; pulse.cooldown = 0.8f; pulse.range = 6.5f; pulse.projectileSpeed = 8f;
        pulse.explosionRadius = 0f; pulse.explosionDamage = 0; pulse.projectileEffect = ProjectileEffect.Chain;
        pulse.sound = WeaponSound.Electricitat; pulse.projectileSprite = null; pulse.projectileColor = new Color(.3f, .72f, 1f);
        EditorUtility.SetDirty(pulse);

        var crystal = Weapon("BallestaCristall", "Ballesta de cristall", "Ballesta", ChestRarity.Epic);
        crystal.damage = 2; crystal.cooldown = .6f; crystal.range = 8.5f; crystal.projectileEffect = ProjectileEffect.Slow; crystal.pierce = 3;
        crystal.pellets = 3; crystal.spreadAngle = 9f; crystal.fixedSpread = true;
        crystal.sound = WeaponSound.Cristall; crystal.projectileSprite = S("Armes/Fletxa"); crystal.tintProjectileSprite = true; crystal.projectileColor = new Color(.66f, .48f, 1f);
        EditorUtility.SetDirty(crystal);

        // ---- Alpha v1.3.1: sis armes noves amb comportament propi ----
        var turretStaff = Weapon("BaculTorreta", "Bàcul de torreta", "Vareta", ChestRarity.Rar);
        turretStaff.special = SpecialKind.Turret; turretStaff.specialValue = 9f; turretStaff.damage = 1; turretStaff.cooldown = 10f;
        turretStaff.range = 7f; turretStaff.homing = 0f; turretStaff.zoomRange = 0f; turretStaff.spreadAngle = 0f; EditorUtility.SetDirty(turretStaff);
        var minelauncher = Weapon("Llancamines", "Llançamines", "Llancagranades", ChestRarity.Rar);
        minelauncher.special = SpecialKind.Mine; minelauncher.specialValue = 1.5f; minelauncher.damage = 4; minelauncher.cooldown = 0.8f;
        minelauncher.explosionRadius = 0f; minelauncher.zoomRange = 0f; minelauncher.spreadAngle = 0f; EditorUtility.SetDirty(minelauncher);
        var lightBow = Weapon("ArcLlum", "Arc de llum", "Ballesta", ChestRarity.Epic);
        lightBow.special = SpecialKind.Charge; lightBow.specialValue = 1.2f; lightBow.damage = 2; lightBow.cooldown = 0.3f;
        lightBow.pierce = 3; lightBow.projectileSpeed = 26f; lightBow.range = 10f; lightBow.zoomRange = 0f; lightBow.pellets = 1; lightBow.spreadAngle = 0f;
        lightBow.projectileSprite = S("Armes/Fletxa"); lightBow.tintProjectileSprite = true; lightBow.projectileColor = new Color(1f, .92f, .55f);
        lightBow.sound = WeaponSound.Cristall; EditorUtility.SetDirty(lightBow);
        var mirrorShield = Weapon("EscutMirall", "Escut de mirall", "EspasaVidre", ChestRarity.Epic);
        mirrorShield.special = SpecialKind.Shield; mirrorShield.specialValue = 120f; mirrorShield.melee = true; mirrorShield.meleeArc = 90f;
        mirrorShield.range = 1.8f; mirrorShield.damage = 3; mirrorShield.cooldown = 0.7f; mirrorShield.knockbackForce = 6f;
        mirrorShield.meleeDashDistance = 0f; mirrorShield.alternateSwing = false; mirrorShield.projectileColor = new Color(.7f, .9f, 1f);
        EditorUtility.SetDirty(mirrorShield);
        var healWand = Weapon("VaretaCurativa", "Vareta curativa", "Vareta", ChestRarity.Rar);
        healWand.special = SpecialKind.HealBolt; healWand.specialValue = 7f; healWand.damage = 1; healWand.cooldown = 1.1f; healWand.homing = 0f;
        healWand.range = 7f; healWand.projectileColor = new Color(.45f, 1f, .5f); healWand.zoomRange = 0f; healWand.spreadAngle = 0f; EditorUtility.SetDirty(healWand);
        var grapple = Weapon("Ganxo", "Ganxo d'abordatge", "Pistola", ChestRarity.Comu);
        grapple.special = SpecialKind.Grapple; grapple.specialValue = 1.1f; grapple.damage = 2; grapple.cooldown = 1.6f; grapple.range = 6f;
        grapple.zoomRange = 0f; grapple.spreadAngle = 0f; EditorUtility.SetDirty(grapple);

        foreach(string path in Directory.GetFiles("Assets/Prefabs/Cofres","*.prefab"))
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try {var chest=root.GetComponent<Chest>();if(chest!=null){var pool=(chest.lootTable??new WeaponData[0]).ToList();foreach(var w in new[] {disc,lance,spores,turretStaff,minelauncher,lightBow,mirrorShield,healWand,grapple})if(w.rarity==chest.rarity && !pool.Contains(w))pool.Add(w);chest.lootTable=pool.ToArray();PrefabUtility.SaveAsPrefabAsset(root,path);}}
            finally {PrefabUtility.UnloadPrefabContents(root);}
        }
        var beetle=Enemy("Escarabat","Malo");var bh=beetle.GetComponent<EnemyHealth>();bh.maxHealth=7;bh.knockbackResist=.7f;
        beetle.GetComponent<EnemyPathfindingAI>().moveSpeed=1.6f;SaveEnemy(beetle,"Escarabat");
        var sporer=Enemy("Esporer","Tirador");var shoot=sporer.GetComponent<EnemyShooter>();shoot.pattern=EnemyShooter.Pattern.Ring8;shoot.fireCooldown=2.8f;shoot.telegraphTime=.65f;shoot.bulletSpeed=4;shoot.useColor=true;shoot.bulletColor=new Color(.6f,1,.4f);sporer.GetComponent<EnemyPathfindingAI>().moveSpeed=1.2f;SaveEnemy(sporer,"Esporer");
        // ---- Alpha v1.3.1: tres enemics nous i dos bosses ----
        var gargoyle=Enemy("Gargola","BoletVeri");var gargoyleShot=gargoyle.GetComponent<EnemyShooter>();
        gargoyleShot.pattern=EnemyShooter.Pattern.Spread3;gargoyleShot.fireCooldown=2.4f;gargoyleShot.telegraphTime=.5f;gargoyleShot.bulletSpeed=5.5f;
        gargoyleShot.useColor=true;gargoyleShot.bulletColor=new Color(.75f,.8f,.9f);gargoyle.GetComponent<EnemyHealth>().maxHealth=8;SaveEnemy(gargoyle,"Gargola");
        var crab=Enemy("Cranc","Malo");crab.GetComponent<EnemyHealth>().maxHealth=9;crab.GetComponent<EnemyHealth>().knockbackResist=.8f;
        crab.GetComponent<EnemyPathfindingAI>().moveSpeed=1.5f;SaveEnemy(crab,"Cranc");
        var frostGhost=Enemy("FantasmaGel","Mag");frostGhost.GetComponent<EnemyHealth>().maxHealth=5;SaveEnemy(frostGhost,"FantasmaGel");
        GameObject NewBoss(string id,string title,int hp,float speed,EnemyBoss.Attack[] attacks,string minion,Color bullets,Color dust)
        {
            var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemics/MagFosc.prefab");root.name=id;
            var sr=root.GetComponent<SpriteRenderer>();sr.sprite=S("Enemics/"+id);sr.color=Color.white;
            root.GetComponent<EnemyHealth>().maxHealth=hp;var pathing=root.GetComponent<EnemyPathfindingAI>();if(pathing!=null)pathing.moveSpeed=speed;
            var boss=root.GetComponent<EnemyBoss>();boss.bossName=title;boss.attacks=attacks;boss.minionPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/"+minion+".prefab");
            boss.useBulletColor=true;boss.bulletColor=bullets;boss.dustColor=dust;SaveEnemy(root,id);
            return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/"+id+".prefab");
        }
        var spiderQueen=NewBoss("ReinaAranya","Reina Aranya",80,1.8f,new[]{EnemyBoss.Attack.Summon,EnemyBoss.Attack.Fan,EnemyBoss.Attack.Jump,EnemyBoss.Attack.Charge},"Aranya",new Color(.75f,.4f,1f),new Color(.5f,.3f,.7f));
        var iceKing=NewBoss("ReiGel","Rei de Gel",85,1.5f,new[]{EnemyBoss.Attack.Ring,EnemyBoss.Attack.Spiral,EnemyBoss.Attack.Charge,EnemyBoss.Attack.Fan},"FantasmaGel",new Color(.65f,.9f,1f),new Color(.7f,.85f,1f));
        var bush=Prop("ArbustBaies","Arbust de baies",false);
        var mushroom=Prop("BoletLluminós","Bolet lluminós",true);
        foreach(string path in Directory.GetFiles("Assets/Dungeon/Temes","*.asset"))
        {
            var theme=AssetDatabase.LoadAssetAtPath<DungeonTheme>(path);if(theme==null)continue;
            var list=(theme.enemies??new GameObject[0]).ToList();
            if(path.Contains("Bosc")||path.Contains("Claveguera")) {var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/Esporer.prefab");if(!list.Contains(p))list.Add(p);theme.decorPrefabs=new[] {bush,mushroom};}
            if(path.Contains("Desert")||path.Contains("Cripta")||path.Contains("Volca")){var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/Escarabat.prefab");if(!list.Contains(p))list.Add(p);}
            // v1.3.1: enemics nous per tema i bosses nous (cinc bosses per a huit temes)
            void AddEnemy(string id){var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemics/"+id+".prefab");if(p!=null&&!list.Contains(p))list.Add(p);}
            if(path.Contains("Desert")||path.Contains("Cripta")||path.Contains("Volca"))AddEnemy("Gargola");
            if(path.Contains("Claveguera")||path.Contains("Bosc")||path.Contains("Desert"))AddEnemy("Cranc");
            if(path.Contains("Neu")||path.Contains("Cristall")||path.Contains("Cripta"))AddEnemy("FantasmaGel");
            if(path.Contains("Bosc")||path.Contains("Cripta"))theme.boss=spiderQueen;
            if(path.Contains("Neu"))theme.boss=iceKing;
            theme.enemies=list.ToArray();EditorUtility.SetDirty(theme);
        }
        AssetDatabase.SaveAssets();Validate();Debug.Log("ALPHA_CONTENT: cares, 4 barrets, 6 esteles, 5 armes, 2 enemics i 2 objectes preparats.");
    }
    static WeaponData Weapon(string id,string name,string template,ChestRarity rarity)
    {
        string path="Assets/Weapons/"+id+".asset";var w=AssetDatabase.LoadAssetAtPath<WeaponData>(path);
        if(w==null){var source=AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/"+template+".asset");if(source==null)throw new Exception("Falta arma base "+template);w=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(w,path);}
        w.weaponName=name;w.rarity=rarity;w.icon=S("Armes/"+id);return w;
    }
    static GameObject Enemy(string id,string template)
    {
        var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemics/"+template+".prefab");root.name=id;var sprite=root.GetComponent<SpriteRenderer>();sprite.sprite=S("Enemics/"+id);sprite.color=Color.white;return root;
    }
    static void SaveEnemy(GameObject root,string id){try{PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Enemics/"+id+".prefab");}finally{PrefabUtility.UnloadPrefabContents(root);}}
    static GameObject Prop(string sprite,string name,bool mushroom)
    {
        var root=new GameObject(name,typeof(SpriteRenderer),typeof(BoxCollider2D),typeof(Breakable));
        root.GetComponent<SpriteRenderer>().sprite=S("Masmorra/"+sprite);root.GetComponent<SpriteRenderer>().sortingLayerName="Player";
        root.GetComponent<BoxCollider2D>().size=new Vector2(.65f,.55f);root.GetComponent<BoxCollider2D>().offset=new Vector2(0,-.1f);
        var b=root.GetComponent<Breakable>();b.health=mushroom?2:1;b.coinChance=.15f;b.healChance=mushroom?.3f:.15f;b.debrisColor=mushroom?new Color(.5f,1,.8f):new Color(.3f,.65f,.3f);
        string path="Assets/Prefabs/Masmorra/"+sprite+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);return prefab;
    }
    public static void Validate()
    {
        var db=AssetDatabase.LoadAssetAtPath<CosmeticsDatabase>("Assets/Resources/Cosmetics.asset");
        if(db.skins.Any(s=>s.sprite!=null && (s.sprite.rect.width!=16||s.sprite.rect.height!=16)))throw new Exception("Skin fora de 16x16");
        if(db.skins.Skip(1).Any(s=>s.sprite==null))throw new Exception("Hi ha una skin sense sprite");
        if(!db.hats.Any(h=>h.fullHelmet && h.sprite!=null))throw new Exception("Falta casc complet");
        if(db.trails.Select(t=>t.shape).Distinct().Count()<7)throw new Exception("Falten formes d'estela");
        var faces=new HashSet<string>();
        foreach(var skin in db.skins)
        {
            if(skin.sprite==null)continue;var sprite=skin.sprite;string mask="";
            for(int y=4;y<12;y++)for(int x=3;x<13;x++){var c=sprite.texture.GetPixel((int)sprite.rect.x+x,(int)sprite.rect.y+y);mask+=c.r<.16f&&c.g<.16f&&c.b<.2f?"#":".";}
            faces.Add(mask);
        }
        if(faces.Count<10)throw new Exception("Les cares encara s'assemblen massa");
        var helmet=db.hats.First(h=>h.fullHelmet).sprite;
        if(helmet.texture.GetPixel((int)helmet.rect.x+7,(int)helmet.rect.y+7).a>.1f)throw new Exception("El casc tapa la cara");
        var ak = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/AK47.asset");
        if (ak == null || ak.rarity != ChestRarity.PocComu) throw new Exception("L'AK ha de tindre qualitat verda");
        var first=AssetDatabase.LoadAssetAtPath<DungeonTheme>("Assets/Dungeon/Temes/Tema_1_Cova.asset");
        if(first.enemies.Any(e=>e.name=="Esporer"||e.name=="Escarabat"))throw new Exception("La dificultat inicial no s'ha de canviar");
        foreach(string id in new[] {"DiscCometa","LlancaTempesta","VaretaEspores","ProjectorPolsos","BallestaCristall","BaculTorreta","Llancamines","ArcLlum","EscutMirall","VaretaCurativa","Ganxo"})if(AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/"+id+".asset")==null)throw new Exception("Falta arma "+id);
    }
}
