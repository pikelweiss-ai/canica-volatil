# Missatge per a Claude — Continuar l’online de Canica Volàtil

Claude, continua l’online des de la branca `alpha/bugfixes`, al worktree `C:\Users\Usuario\Canica Alpha\Builds\BugfixWorkspace`. El punt de partida és `f74c1e4`. Llig [AGENTS.md](AGENTS.md) i [ONLINE.md](ONLINE.md). La compilació Windows més recent és `Builds/AlphaReview/Windows-cares-classiques/CanicaAlphaReview.exe`.

Ferran vol provar si dos amics poden jugar cadascú des de casa amb el PC d’un d’ells com a amfitrió, sense haver de contractar ni desplegar un servidor separat.

## Què hi ha fet

- L’amfitrió és qui crea la sala i porta la masmorra. El relay, quan s’usa, publica i resol el codi curt i transporta els missatges.
- **La IP directa ja està implementada.** El camp d’unió accepta `IP:port` i entra per TCP directament. Consulta `JoinByCode` i el listener a `Assets/Scripts/Net/NetSession.cs`, el parser a `NetProtocol.cs` i el formulari a `RoomBrowserUI.cs`.
- La regressió Windows de protocol 8 ha passat 4/4 al mateix PC, amb LAN i relay local. Els logs són a `Builds/SkinsReworkQA/Online/result.txt`. La masmorra per IP directa ja va passar en loopback a `Builds/TrailsBossTests/DirectAndRelay`.
- Encara falta una partida entre dos dispositius físics i una prova entre dos xarxes diferents. Tampoc hi ha cap relay públic verificat. El protocol de l’Alpha publicada és 5 i la branca de proves usa 8. Els participants de cada prova han d’usar la mateixa revisió.

## Per on continuar

1. Exercita la connexió directa amb dos equips. Crea una sala **Xarxa local** i entra al camp de codi amb la IP i el port TCP real de l’amfitrió (7777 per defecte, amb alternatives fins a 7787). Comprova entrada al lobby, llest, inici de masmorra, sincronització bàsica i què passa quan tanca l’amfitrió.
2. Repetix des d’una altra xarxa si teniu dos equips disponibles. Comprova que l’adreça i el port de l’amfitrió són accessibles. Si la xarxa requerix reenviament de port o l’operadora usa CGNAT, anota la condició durant la prova i coordina amb Ferran qualsevol canvi del router o tallafocs.
3. Si la connexió funciona però el flux confon, fes visible l’adreça i el port de la sala i aclarix al menú quan cal usar IP directa o codi curt. Reutilitza la connexió existent i torna a passar `Tools/Net/test-online.ps1`. Si la prova entre cases queda pendent, indica exactament què has pogut comprovar.
4. Deixa el relay públic com a via separada. Si Ferran vol que un codi curt funcione entre cases amb la implementació actual, caldrà un relay accessible que parle el mateix protocol.

Anota la compilació, si els equips eren físics o bots, la via de connexió i el resultat. Mantín les proves de LAN i relay local durant qualsevol canvi.
