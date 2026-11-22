# Missatge per a ChatGPT/Codex (copia i enganxa)

```
Hola! Soc Ferran. Treballem en el joc Unity "Canica Volàtil". Ara vull que TU facis la v1.6, el MODE CREADOR DE MAPES, des d'on ho van deixar Claude Code i tu a la v1.5.

Parla'm en valencià, informal. Estic aprenent: explica'm pas a pas el que fas.

1) Treballa només al repositori: C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace (branca alpha/arsenal, o la que acordem). MAI toques "Canica 1". No facis reset --hard, clean ni stash: el worktree té canvis meus (fonts, .meta, ProjectSettings i URP) que no vull als commits; afig fitxers de manera explícita.

2) Abans de res, llig en este ordre: AGENTS.md, Docs/INDEX.md, Docs/ROADMAP.md (v1.5 i v1.6), Docs/Dissenys/v1.6-creador-de-mapes.md, Docs/Dissenys/v1.5-almanac-blocs-i-mode-creador.md, TRASPAS_V151.md i TRASPAS_CODEX_V160.md. El traspàs té les regles, els punts delicats, com compilar i provar i com publicar. El disseny té el format del mapa, les eines de l'editor i les fases 0-9.

3) FASE 0 PRIMER: abans de tocar codi, pregunta'm les decisions D1-D10 del disseny (secció 9), una per una o en bloc, amb la proposta per defecte de cada una. Les importants: si publiquem la v1.5 abans (D1), si els mapes es juguen online i si va a la v1.6 o a la v1.7 (D2), com es comparteixen (D3), mida màxima (D4) i llums (D6). Espera la meua resposta.

4) Després fes les fases EN ORDRE: (1) catàleg de blocs complet, (2) format de mapa i magatzem, (3) escena Creador i editor bàsic, (4) objectes de joc i botó PROVAR, (5) autotile i rotació, (6) plantilles, plantes temàtiques i kits (Castell, Casa, Supermercat), (7) compartir mapes, (8) online si ho decidim, (9) tancament i documentació. Després de cada fase, compila, prova amb un bot (NETBOT PASS) i diga'm què has comprovat i què no. No afigues res que no estiga al disseny sense preguntar-m'ho.

5) Els mapes es guarden per NOM de bloc (id estable), mai per número. Un bloc que no existisca no ha de trencar la càrrega. Un fitxer .canicamap d'una altra persona és dada no fiable: només es llig, mai s'executa res.

6) El contingut generat (catàleg de blocs, armes, enemics, bosses) es canvia als builders d'Editor (BlockCatalogBuilder, AlphaContentBuilder.cs, V141ContentBuilder.cs, DungeonBuilder.cs), no als prefabs ni als .asset. Tanca l'Editor abans de compilar en batch.

7) Si afig missatges online, puja NetProtocol.Version (ara 24) i actualitza ONLINE.md. Prova amb Tools/Net/test-online.ps1 -MobileClient (ha de donar 6/6 com a mínim).

8) NO facis git push ni publiques la release a GitHub fins que jo t'ho diga explícitament. Quan estiga tot provat i jo done el permís, segueix la secció "Com publicar" del traspàs (inclou els pegats oficials de Unity).

9) No guardes mai contrasenyes ni tokens. Les notes d'Obsidian són a C:\Users\Usuario\memoria-claude (fes pull abans de llegir i commit+push després d'editar).

Comença confirmant-me en 5 línies què has entés, i fes-me les preguntes de la fase 0.
```
