# Traspàs a Claude Code · Alpha v1.2.0

## Punt de partida

- Worktree de l'Arsenal: `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace`, branca `alpha/arsenal`. El codi i les proves de la v1.2.0 estan a `ec9841d`; aquest traspàs forma part dels commits de documentació posteriors.
- El worktree `Builds\BugfixWorkspace`, branca `alpha/bugfixes`, conserva la v1.1.3 i canvis locals previs. Treballa des de l'Arsenal. No toques **Canica 1**.
- Llig `AGENTS.md` i després `CONSELL.md`, la referència principal del balanç, i `DISSENY.md`, `ONLINE.md` i `RELEASE_NOTES.md`. Les notes d'Obsidian són a `C:\Users\Usuario\memoria-claude`. Fes-hi `git pull --rebase` abans de llegir-les. Si les edites, fes commit i push allí. El GitHub del joc requereix el permís explícit de Ferran per fer-hi push o publicar.
- Unity 6000.0.53f1, URP 2D, Input Manager antic. TextMeshPro al món i TextMeshProUGUI al Canvas. Commits menuts en valencià sense accents, sense soroll de reimportació.

## Estat lliurat

- v1.1.2: BUG-014–020. v1.1.3: el portal blau d'arribada fa tornar tota la colla al pis anterior, amb sales, cofres i botí restaurats.
- v1.2.0, protocol 12: balanç de l'Arsenal, Disc amb tres rebots, 300 trets per Llançaflames i calor de Minigun per instància. Slow, verí i cadena elèctrica aplicats per l'amfitrió, canvis audiovisuals, bosses amb la nova progressió i retocs del botó + i les esteles. El Sniper conserva el zoom segons la proposta final de CONSELL.md.
- Builds de Windows, Linux i Android amb zero errors. Prova individual a Windows i `Tools/Net/test-online.ps1 -MobileClient` 6/6. Manifest de compatibilitat, SHA256 i signatura v2 i alineació de l'APK verificats.
- Paquets a `C:\Users\Usuario\Canica Alpha\Builds\StabilityWorkspace\Builds\Distribucio\Alpha-v1.2.0`. Hi ha ZIP Windows, TAR.GZ Linux, APK Android i SHA256SUMS.txt. La v1.2.0 està empaquetada en local. La pública continua sent la v1.1.1.

## Pas següent

Validar la sensació del balanç al Mode Creatiu amb Ferran, i comprovar l'execució en Android i Linux físics. La següent etapa del full de ruta és la progressió barata (resum i rècord, cosmètics amb monedes i repte setmanal amb llavor fixa). Després ve la v1.3 de varietat. Confirma l'etapa amb Ferran abans d'inventar números o avançar la publicació.

Per verificar canvis, executa `Tools/Build/build-alpha.ps1`, prova individualment i, si afecta l'online, passa `Tools/Net/test-online.ps1`. Si canvies el protocol, puja també la versió i actualitza `ONLINE.md`. Anota el resultat a «Canica Volàtil - Bugs pendents.md».
