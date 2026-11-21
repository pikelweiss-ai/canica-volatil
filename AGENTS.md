# AGENTS.md — Canica Alpha (per a Codex)

Joc 2D top-down en píxel art, combat estil Brawl Stars, amb mode masmorra procedural.
Unity 6000.0.53f1, URP 2D, C#. Guia completa: `C:\Users\Usuario\memoria-claude\Canica Alpha - Guia per a Codex.md`.
Disseny: `DISSENY.md`. **Traspàs actual (v1.4): `TRASPAS_CODEX_V140.md`; índex de documents i d'art: `Docs/INDEX.md`.**

## Regles
- Parla en valencià, informal. L'usuari (Ferran) està aprenent: explica pas a pas les mecàniques noves.
- Treballa només en aquesta carpeta. **Mai toques el projecte "Canica 1".**
- Text del món = `TextMeshPro`; UI de Canvas = `TextMeshProUGUI`. Mira el codi abans d'assumir-ho.
- Input antic (`Input.GetAxisRaw`, `Input.GetKeyDown`), igual que la resta de scripts.
- No edites `.unity`/prefabs a mà si Unity té canvis pendents. No crees `.meta` a mà.
- Contingut generat (masmorra, enemics, temes, cofres): canvia l'script d'Editor (`Assets/Editor/DungeonBuilder.cs`, `ChestPrefabCreator.cs`) i regenera amb el menú **Canica**, no l'asset.
- Píxel art: blocs 16×16, armes 32 px, 16 PPU; dins de carpetes `Pixel/` s'importa sol. Sprites amb `Tools/PixelArt/*.ps1`.
- Commits xicotets, missatge en valencià sense accents. `git status` net abans de començar.

## On està cada cosa
- `Assets/Scripts` (joc), `Assets/Scripts/Dungeon` (masmorra), `Assets/Scripts/Cosmetics`
- `Assets/Weapons` (WeaponData), `Assets/Dungeon/Temes` (DungeonTheme), `Assets/Prefabs/Enemics`
- Escenes: `MainMenu`, `GameScene`, `Lobby`, `Dungeon`
