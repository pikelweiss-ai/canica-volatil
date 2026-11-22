# Full de ruta · Canica Volàtil (definitiu a 2026-11-21)

> Decidit amb Ferran. Cada versió té un contingut tancat: **no s'hi afig res sense preguntar-li**.
> Detalls de cada disseny a `Docs/Dissenys/` (índex a `Docs/INDEX.md`).

## v1.4.0 · «Dungeons més variades»

[Paquets d'Alpha v1.4.0](https://github.com/pikelweiss-ai/canica-volatil/releases/tag/v1.4.0-alpha)

| Fase | Contingut | Disseny |
|------|-----------|---------|
| 1 | **Generació de dungeons**: sales amb cantons retallats (L, T, creu, cercle, pilars) mantenint la creu central; **anells** (25 % des del pis 3); sales llargues i grans; sales **Emboscada**, **Arena d'onades** i **Secreta**; modificadors **foscor**, **gel relliscós** i **flames** | `v1.4-bosses-enemics-sales.md` §4 i §9 |
| 2 | **Sorolls nous**: importar els 23 WAV i lligar-los als mobs (mínim 5 de diferents) | §6 |
| 3 | **6 enemics nous**: Mímic, Sanador, Escut-guerrer, Talp, Franctirador de cristall i Banshee (sprites i sorolls fets) | §1 |
| 4 | **Portes** amb estats (tancada → mitjana → oberta) segons la distància del jugador. Les claus queden pendents. | §5 i §8 |
| 5 | **Llançaflames sense combustible**: l'arma es queda a la hotbar i diu «Sense combustible!» | `v1.4-mercader.md` §8b |
| 6 | **Menú principal i selector nous** amb el fons del portal, la canica original, els botons amb Chakra Petch i tres portades il·lustrades. **Novetats** és el cinqué botó. | |

Fora de la v1.4.0: tot el que hi ha a sota.
**Blocs nous**: la v1.4.0 **no** té catàleg. Només, **si queda temps**, 2-3 variants de terra i 3-4 decoracions per tema (`v1.4-bosses-enemics-sales.md` §9 punt 4). **Tot l'ús dels blocs va a la v1.5** (decisió de Ferran).

## v1.4.1 · publicada (2026-11-21)
> Afegit sobre el pla: variacions de blocs per ambient (terra, parets, objectes i decoració), bosses +20 de vida amb cures, Reina Aranya amb teranyines, Rei de Gel amb subdits, Escut de mirall amb energia i detector d'encallament dels bosses. [Notes](../RELEASE_NOTES.md)
- Fet: els altres 6 enemics (Salamandra, Llimac àcid, Escorpí, Llop de neu, Bombardier, Torreta antiga) i els **4 microbosses** amb barra de vida.
- **Mercader** i màquina expenedora a la sala de tresor, amb la seua interfície i missatges al xat.
- **Nova pantalla PERSONALITZAR** (Canigues, Barrets, Esteles) estil Stumble Guys.
- **Almanac** fase 1 (llistes d'armes i d'enemics).


## v1.5.0 · publicada (2026-11-22) · [Traspàs](../TRASPAS_V151.md)
Release `v1.5.0-alpha` publicada a GitHub i marcada **Latest** (commit `6675b44`). La decisió D1 de la v1.6 queda resolta; no cal publicar res abans de continuar.
- **Els vuit bosses nous, tots de colp** (Cor de Cristall, Cavaller Corrupte, Cuc de Sorra, Fènix, Hidra del Llac, Rei de les Clavegueres, Cuiner Boig, Professor Infernal). Disseny a `Docs/Dissenys/v1.4-bosses-enemics-sales.md` §3.
- **Portes amb clau.** La porta de la sala del boss està tancada amb clau, i **la clau s'ha d'anar a buscar a una sala aleatòria** de la planta (sala de clau; pot estar vigilada). Cal sistema de claus (recollir, gastar, mostrar a la HUD), sincronització online i que el mapa marque on és la porta. Disseny de portes a `v1.4-bosses-enemics-sales.md` §5 i §8.
- Els deu pendents de [Docs/PENDENTS_APUNTATS.md](PENDENTS_APUNTATS.md) (abans «v1.4.3»): arreglos petits, microbosses des del pis 2 i bugs d'online.
- **Almanac de blocs i 4t mode «Creador» (pròximament).** De tot el creador de mapes, a la v1.5 només entra el **catàleg mínim de blocs (`BlockData`) amb l'Almanac de blocs** i una **quarta targeta «CREADOR» al selector, apagada i amb «PRÒXIMAMENT»**. Els objectes tenen hitbox pròpia i independent per bloc. L'editor (pintar, guardar, carregar, rotació, autotile, plantilles, kits i compartir mapes) passa a la **v1.6**. Disseny: [v1.5-almanac-blocs-i-mode-creador.md](Dissenys/v1.5-almanac-blocs-i-mode-creador.md).
- **Amulets** (16, 3 ranures a la hotbar, tria 1 de 3 en acabar cada pis, sincronització online) i **tenda de cosmètics** amb qualitats; ranura d'amulet i sala Botiga al mercader.
- **Almanac fase 2** (animacions, filtres, amulets i blocs).


## v1.6 · en desenvolupament · [Disseny](Dissenys/v1.6-creador-de-mapes.md) · [Traspàs](../TRASPAS_CODEX_V160.md)
Decisions D1–D10 resoltes per Ferran el 2026-11-22 amb les propostes per defecte. El mode Creador inclou catàleg, editor, magatzem, eines, marcadors de joc, kits i compartir per fitxer/codi; els mapes importats per codi es poden llançar també des del selector mòbil. Els bots `catalog160`, `mapdata160`, `editor160`, `tile160` i `share160` donen `NETBOT PASS`; Linux i Android també s'han recompilat i els manifests coincideixen. Els fitxers `.canicamap` comprimits permeten 64×64 ple, i `perf160` ha mesurat 219,9 FPS de mitjana a 1600×900 en este PC; queden pendents QA visual 4:3, combat manual i proves en dispositius físics. Consulta [TRASPAS_V161](../TRASPAS_V161.md).
- **Online de mapes (fase 8)**: es trasllada a la v1.7, tal com permet D2, perquè requereix integrar el mapa personalitzat al flux de sala, transferir-lo en trossos i carregar una partida cooperativa pròpia. No s'ha implementat ni provat.


## v1.6.1 · pendents apuntats per Ferran (2026-11-22, sense implementar)
- **Tabulador (Tab) solapat.** La tecla **Tab** fa dues coses alhora: **alternar l'aim** (`GameControls`, 8a acció, tecla primària `KeyCode.Tab`) i **mostrar els amulets d'equip de tots els jugadors** (disseny de la v1.4/v1.5). Es trepitgen. Arreglo previst: separar-les. Per exemple, deixar Tab només per a l'aim i posar els amulets d'equip en una altra tecla reassignable (o mantenir-la premuda), i avisar-ho a Controls i a l'ajuda de mòbil. Cal provar-ho també online, perquè els amulets de tots es veuen allí. Només apuntat; Ferran decidirà quina tecla.
- **El cofre es destrueix quan mates el boss.** Ferran ha vist que, en matar el boss, un cofre desapareix (es destrueix). Sense detall encara: cal saber quin cofre (el del boss, el de la sala o qualsevol de la planta), si passa en solitari i online, i amb quin boss. Només apuntat, sense investigar ni implementar. Pista per a qui el reproduïsca: mirar què fa la mort del boss amb els objectes de la sala (`EnemyBoss`, `DungeonGenerator`, sala del boss i portal) i amb les explosions o neteges d'objectes.
- **Un company es queda encallat dins de la paret i no pot fer res (online).** Un amic de Ferran (nom de jugador «elorinas») es va quedar **pillat a la paret**: no es podia moure ni atacar i, de fet, **tampoc li pegaven** els enemics que tenia al voltant (a la captura, tres Escut-guerrers). Captura a `memoria-claude`: `BUG - amic encallat a la paret amb escut-guerrers.png`. Sense reproduir ni investigar: no sabem la versió exacta, si era l'amfitrió o el client, ni com hi va arribar. Preguntar-ho abans d'arreglar-ho. Relacionat amb BUG-009 (enganxat a la paret) i BUG-022 (amic encallat després del boss), de les notes.
- **Detall del company encallat (captura del propi amic, 2026-11-22).** Passava a **Android** (es veuen els controls tàctils), al **pis 6**, a la sala **Arena, onada 3/3**, en una **sala online amb 2 jugadors** (UB78HK). El personatge està arran del mur, al límit entre la paret i el terra, i la zona fosca de l'esquerra és el buit. Captura: `memoria-claude/BUG - amic encallat a Android a l'Arena (pis 6).webp`. Continua sense investigar.
- **Android: millorar la jugabilitat dels joysticks.** Ferran: els joysticks són «un poc malos»: **en prémer-los, el jugador es mou i dispara automàticament** (el joystick dret apunta i ataca alhora, no es pot apuntar sense disparar). Per a la v1.6.1 (o una versió d'Android pròpia): revisar `MobileInput`/`MobileHUD` (joystick esquerre analògic i dret que apunta/ataca) i valorar zona morta, apuntar sense disparar (p. ex. disparar només passat un llindar d'arrossegament o amb un botó), mida i posició dels joysticks i opcions d'ajust. Només apuntat; pendent de provar en un telèfon real i de decidir què vol Ferran.
