# Traspàs a Codex · v1.6.1 · Online que funcione a Batoi, SENSE servidor propi (2026-11-23)

> Escrit per Claude Code. **Només pla i investigació: no s'ha tocat cap codi.** Valencià informal; Ferran (17 anys) està aprenent: explica pas a pas.
> **Decisió de Ferran:** el joc ha de ser **peer-to-peer, sense cap servidor propi** i sense comptes. Res de crear servidors ni comptes.

## 1. Diagnòstic (ja comprovat; no ho repetisques)
- A Batoi (Linux Mint, `172.16.211.140`, v1.5) SALES ONLINE diu «No es pot arribar al servei de connexió (bore.pub)». A casa va bé.
- **No és un port.** Des de Batoi, 22 ports de sortida TCP passen tots (nota «L'online no va a l'institut de Batoi» a `memoria-claude\Canica Volàtil - Bugs pendents.md`). La IP de `bore.pub` (`159.223.110.159`) **no respon a res** des de Batoi, i **des de casa respon en ~103 ms** (comprovat el 23/11): **Batoi bloqueja eixa IP.**
- El joc **no és P2P pur**: usa `bore.pub` com a **pont** (tot el trànsit hi passa; per això ~300 ms) i `ntfy.sh` per a la llista de sales (`Assets/Scripts/Net/NetCloud.cs`: `TunnelHost` per defecte `bore.pub`, port de control 7835, `Directory` per defecte `https://ntfy.sh`). **Sense provar:** si `ntfy.sh` arriba des de Batoi.

## 2. Pla (en ordre): A) reserva de túnels, B) connexió directa amb UPnP
Protocol **24, sense pujar**: només es canvia lògica de client i s'afigen camps a l'anunci JSON (`Announce`), que els clients antics ignoren. No pujar el protocol; el tema del directori depén d'ell.

### A. Túnels de reserva i missatge clar (el canvi petit)
1. `TunnelHost` passa de ser un text a una **llista de candidats** configurable sense recompilar (PlayerPrefs o argument `-canicaTunnel a,b,c`). El client els prova **en ordre amb un timeout curt (~4 s)** i usa el primer que responga.
2. **Investiga quins servidors públics gratuïts són compatibles amb el protocol bore** (obert, JSON acabat en `\0` pel port 7835) **sense compte ni clau**. Ara només es coneix `bore.pub`. **No t'inventes cap servidor**; si no n'hi ha altres compatibles, documenta-ho i deixa la llista preparada amb només `bore.pub`. No cal escriure un client d'un altre protocol sense preguntar a Ferran.
3. **Missatge d'error clar** si res arriba: «No es pot arribar a cap servei. Prova XARXA LOCAL (LAN) o unir-te per IP.»
4. Si `ntfy.sh` també falla, que el missatge ho diga i proposi LAN/IP.

### B. Connexió directa amb UPnP/NAT-PMP (peer-to-peer de veres, l'amfitrió a casa)
1. En crear una sala d'Internet, l'amfitrió intenta **obrir el seu port TCP** al router per **UPnP (IGD)**: descobriment SSDP + `AddPortMapping` + `GetExternalIPAddress`, només amb `System.Net.Sockets`/`UnityWebRequest` (**sense paquets externs**). Lloguer limitat i renovat; **esborrar el mapeig en eixir**.
2. **Detecta CGNAT i IP privada** (`100.64.0.0/10`, `10/8`, `172.16/12`, `192.168/16`): si l'IP externa del router no és pública, **no ho intenta** i ho diu.
3. Si va bé, l'anunci porta `direct=true` i `ep` amb `IPpública:port`. El client prova **primer la connexió directa (timeout ~4 s)** i, si falla, el túnel de la reserva (A).
4. L'amfitrió a Batoi **no pot** fer-ho (no controla el router): el client de Batoi només fa una connexió d'**eixida** cap a un amfitrió a casa. A classe, LAN o IP directa ja funcionen.
5. Android: el `MulticastLock` i els permisos ja hi són (`AndroidLanSupport`).

## 3. Proves
- Ampliar `Tools/Net/test-online.ps1` amb: **túnel inabastable → es passa al següent candidat o es mostra el missatge**. Els **6 casos actuals han de continuar en PASS**.
- Prova unitària (bot) de la **classificació d'IP** (pública / privada / CGNAT).
- **UPnP no es pot provar amb bots**: cal un router real. **Diu clarament a Ferran què no s'ha provat.**
- **Validació per part de Ferran:** a Batoi, per a comprovar si una IP candidata arriba:
  `timeout 5 bash -c 'echo > /dev/tcp/<IP>/<PORT>' && echo OBERT || echo FALLA`
  i `curl -sS -m 8 https://ntfy.sh`. Després, una partida real: amfitrió a casa (UPnP) i client a Batoi.

## 4. Riscos que no s'han resolt
- UPnP falla amb **CGNAT** (molts operadors/mòbils) o si el router el té desactivat. És una limitació real, no un error.
- Pot ser que **no hi haja cap altre túnel gratuït compatible** i que `ntfy.sh` també estiga bloquejat a Batoi: aleshores a Batoi quedaria LAN/IP.
- **Traspàs de NAT amb STUN (UDP)** seria peer-to-peer pur, però exigeix passar de TCP a UDP i sovint falla en xarxes d'institut: **fora de l'abast** d'esta fase; només si Ferran ho demana.

## 5. Regles que no es poden saltar
- Treballa **només** a `C:\Users\Usuario\Documents\Unity\Canica Alpha`, branca `alpha/arsenal`. **Mai** «Canica 1».
- **Mai** `git reset --hard`, `clean` ni `stash`; `git add` fitxer a fitxer (mai `-A`); el soroll de reimportació d'Unity no es commiteja. Commits menuts, en valencià **sense accents**, amb la teua línia de coautoria.
- **No facis push a `main`, ni etiquetes, ni release** sense permís explícit de Ferran. El permís de la v1.6.0 **no val** per a la v1.6.1.
- **Mai crees comptes, servidors, claus ni tokens.** Cap secret al repo.
- Contingut generat als builders d'Editor, no als `.asset`. No crees `.meta` a mà.
- No digues que està provat si no ho has executat; diu què has comprovat i què no.
- En acabar: actualitza `memoria-claude\Canica Alpha.md` (`git pull --rebase`, canvia, `git add`, commit i push **només allí**).
