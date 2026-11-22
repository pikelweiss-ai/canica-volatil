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


## v1.5.0 · candidata local · [Traspàs](../TRASPAS_V151.md)
- **Els vuit bosses nous, tots de colp** (Cor de Cristall, Cavaller Corrupte, Cuc de Sorra, Fènix, Hidra del Llac, Rei de les Clavegueres, Cuiner Boig, Professor Infernal). Disseny a `Docs/Dissenys/v1.4-bosses-enemics-sales.md` §3.
- **Portes amb clau.** La porta de la sala del boss està tancada amb clau, i **la clau s'ha d'anar a buscar a una sala aleatòria** de la planta (sala de clau; pot estar vigilada). Cal sistema de claus (recollir, gastar, mostrar a la HUD), sincronització online i que el mapa marque on és la porta. Disseny de portes a `v1.4-bosses-enemics-sales.md` §5 i §8.
- Els deu pendents de [Docs/PENDENTS_APUNTATS.md](PENDENTS_APUNTATS.md) (abans «v1.4.3»): arreglos petits, microbosses des del pis 2 i bugs d'online.
- **Almanac de blocs i 4t mode «Creador» (pròximament).** De tot el creador de mapes, a la v1.5 només entra el **catàleg mínim de blocs (`BlockData`) amb l'Almanac de blocs** i una **quarta targeta «CREADOR» al selector, apagada i amb «PRÒXIMAMENT»**. Els objectes tenen hitbox pròpia i independent per bloc. L'editor (pintar, guardar, carregar, rotació, autotile, plantilles, kits i compartir mapes) passa a la **v1.6**. Disseny: [v1.5-almanac-blocs-i-mode-creador.md](Dissenys/v1.5-almanac-blocs-i-mode-creador.md).
- **Amulets** (16, 3 ranures a la hotbar, tria 1 de 3 en acabar cada pis, sincronització online) i **tenda de cosmètics** amb qualitats; ranura d'amulet i sala Botiga al mercader.
- **Almanac fase 2** (animacions, filtres, amulets i blocs).


## v1.6 · [Disseny](Dissenys/v1.6-creador-de-mapes.md) · [Traspàs](../TRASPAS_CODEX_V160.md) · [Missatge](../PROMPT_CODEX_V160.md)
> Dissenyat el 2026-11-22. Té **10 decisions obertes** (D1–D10) que s'han de preguntar a Ferran abans de començar; les més grans: publicar la v1.5 primer, jugar els mapes online (v1.6 o v1.7) i com es comparteixen.
- **Mode creador de mapes**: editor (pintar, guardar, carregar), rotació i flip de blocs, autotile de catifes i camins, plantilles de sala, plantes temàtiques, kits (Castell, Casa, Supermercat) i compartir mapes.
