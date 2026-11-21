# Missatge per a ChatGPT/Codex (copia i enganxa)

```
Hola! Soc Ferran. Treballem en el joc Unity "Canica Volàtil". Claude Code ha preparat tot el disseny i l'art de la v1.4.0 i ara vull que TU facis la versió, des d'on ho va deixar.

Parla'm en valencià, informal. Estic aprenent: explica'm pas a pas el que fas.

1) Treballa només al repositori: C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace (branca alpha/arsenal). MAI toques "Canica 1". No facis reset --hard, clean ni stash: el worktree té canvis meus (fonts i .meta que Unity reimporta) que no vull als commits.

2) Abans de res, llig en este ordre: AGENTS.md, Docs/INDEX.md, Docs/ROADMAP.md, TRASPAS_CODEX_V140.md i Docs/Dissenys/v1.4-bosses-enemics-sales.md. El traspàs té les 5 fases de la v1.4.0, els fitxers de codi que cal tocar, els criteris d'acceptació, com compilar i provar, i com publicar.

3) Fes les 5 fases EN ORDRE: (1) dungeons amb formes, anells i sales noves, (2) sorolls dels mobs, (3) 6 enemics nous, (4) portes amb estats, (5) Llançaflames sense combustible. Després de cada fase, compila i prova, i diga'm què has comprovat i què no.

4) NO facis res de la v1.4.1 ni de la v1.5 (microbosses, mercader, pantalla Personalitzar, almanac, amulets, catàleg de blocs). Estan al ROADMAP per a més endavant. Tot l'ús dels blocs nous va a la v1.5.

5) L'art (sprites) i els sorolls ja estan fets a la carpeta ArtNou_v1.4/ (dins del repositori, fora d'Assets). Copia'ls a Assets quan els necessites. Els sorolls no els he escoltat encara: si sonen malament, digues-m'ho.

6) Cada enemic nou ha de tindre una mecànica ÚNICA (taula del disseny): no els repetisques.

7) Si afig missatges online, puja NetProtocol.Version (ara 16) i actualitza ONLINE.md. Prova amb Tools/Net/test-online.ps1.

8) NO facis git push ni publiques la release a GitHub fins que jo t'ho diga explícitament. Quan estiga tot provat i jo done el permís, segueix la secció "Com publicar" del traspàs (release primer com a esborrany, pujant els fitxers un per un).

9) No guardes mai contrasenyes ni tokens. Les notes d'Obsidian són a C:\Users\Usuario\memoria-claude (fes pull abans de llegir i commit+push després d'editar).

Comença confirmant-me en 5 línies què has entés i per quina fase vas a començar.
```
