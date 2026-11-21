# Missatge per a ChatGPT/Codex (copia i enganxa)

```
Hola! Soc Ferran. Treballem en el joc Unity "Canica Volàtil". La v1.4.2 ja està publicada i ara vull que TU facis la v1.5, des d'on ho va deixar Claude Code.

Parla'm en valencià, informal. Estic aprenent: explica'm pas a pas el que fas.

1) Treballa només al repositori: C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace (branca alpha/arsenal). MAI toques "Canica 1". No facis reset --hard, clean ni stash: el worktree té canvis meus (fonts, .meta, ProjectSettings i URP) que no vull als commits; afig fitxers de manera explícita.

2) Abans de res, llig en este ordre: AGENTS.md, Docs/INDEX.md, Docs/ROADMAP.md (secció v1.5), Docs/PENDENTS_APUNTATS.md, TRASPAS_CODEX_V150.md i Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md. El traspàs té les fases de la v1.5, els criteris d'acceptació, com compilar i provar, i com publicar.

3) Fes les fases EN ORDRE: (0) els deu pendents apuntats, (1) els vuit bosses nous tots de colp, (2) portes amb clau (la clau s'ha d'anar a buscar a una sala aleatòria), (3) amulets, (4) tenda de cosmètics amb qualitats, (5) Almanac fase 2 + Almanac de blocs + un 4t mode «Creador» al selector que diu «Pròximament» i no fa res, (6) tancament i documentació. Després de cada fase, compila, prova i diga'm què has comprovat i què no.

4) De l'editor de mapes NO facis res més que l'Almanac de blocs i la targeta «Creador» apagada. Guardar o carregar mapes, pintar i plantilles són per a la v1.6. Els objectes del catàleg tenen hitbox pròpia i independent per bloc.

5) Falta l'sprite del Rei de les Clavegueres (només hi ha 7 bosses a ArtNou_v1.4/Bosses/Sprites). Pregunta'm si el dibuixe jo o si en fas un provisional.

6) El contingut generat (armes, enemics, bosses) es canvia als builders d'Editor (AlphaContentBuilder.cs, V141ContentBuilder.cs, DungeonBuilder.cs), no als prefabs ni als .asset. Tanca l'Editor abans de compilar en batch.

7) Si afig missatges online, puja NetProtocol.Version (ara 19) i actualitza ONLINE.md. Prova amb Tools/Net/test-online.ps1 -MobileClient (ha de donar 6/6) i amb els bots (NETBOT PASS).

8) NO facis git push ni publiques la release a GitHub fins que jo t'ho diga explícitament. Quan estiga tot provat i jo done el permís, segueix la secció "Com publicar" del traspàs (inclou els pegats oficials de Unity).

9) No guardes mai contrasenyes ni tokens. Les notes d'Obsidian són a C:\Users\Usuario\memoria-claude (fes pull abans de llegir i commit+push després d'editar).

Comença confirmant-me en 5 línies què has entés i per quina fase vas a començar.
```
