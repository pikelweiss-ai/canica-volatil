# Traspàs a Codex · v1.6.1 · Online que funcione a l'institut de Batoi (2026-11-23)

> Escrit per Claude Code a petició de Ferran. **Només hi ha investigació i pla: no s'ha tocat cap codi.** Valencià informal; Ferran (17 anys) està aprenent: explica pas a pas.

## 1. El problema
Des del PC de classe de Batoi (Linux Mint, `172.16.211.140`, joc v1.5), **SALES ONLINE** diu «No es pot arribar al servei de connexió (bore.pub)». A casa, amb amics, l'online va perfectament.

Ferran pensava que era un **port** bloquejat pel tallafocs de Batoi. **No ho és.** Fets (nota «L'online no va a l'institut de Batoi» a `memoria-claude\Canica Volàtil - Bugs pendents.md`):
- Des de Batoi, **22 ports de sortida TCP** provats contra `portquiz.net` (22, 53, 80, 123, 443, 465, 587, 993, 3478, 5222, 7780, 7835, 8000, 8080, 8443, 8888, 9000, 25565, 27015, 30000, 40000, 50000): **tots passen**.
- Des de Batoi, **`bore.pub` (IPv4 `159.223.110.159`) no respon a res** (ports 22, 80, 443, 7835, ni ping). No hi ha IPv6.
- **Des de casa (23/11, comprovat): `bore.pub:7835` respon en ~103 ms.** O siga, `bore.pub` **està viu**: **Batoi bloqueja eixa IP**, no un port.
- **Conclusió: obrir més ports no arregla res.** Cal una **IP diferent** que Batoi no bloquege.
- **Sense provar:** si `ntfy.sh` (el directori de sales, HTTPS) arriba des de Batoi. El HTTPS general funciona, però la IP de `ntfy.sh` podria estar bloquejada igual. **Comprova-ho primer.**

## 2. Com està fet ara (codi llegit, sense tocar)
- `Assets/Scripts/Net/NetCloud.cs`: `TunnelHost` per defecte `"bore.pub"` (argument `-canicaTunnel`), `TunnelControlPort = 7835` (constant), `Directory` per defecte `https://ntfy.sh` (`-canicaDirectory`). `InUse` és cert quan **no** hi ha servidor propi (`NetSession.ServerAddress` buit).
- `Assets/Scripts/Net/RelayServer.cs`: el **servidor de sales propi** és el mateix joc sense finestra: `CanicaVolatil.x86_64 -batchmode -nographics -canicaServer <port>` (7780 per defecte). Fa de relay i guarda la llista de sales públiques; **amb ell no calen `bore.pub` ni `ntfy.sh`**.
- `Assets/Scripts/Net/NetSession.cs`: `ServerOverride` i la clau `Net_Server` a PlayerPrefs. UI: camp «SERVIDOR DE SALES» a `RoomBrowserUI.cs`.
- Guia: `memoria-claude\Canica Volàtil - Online.md`, `ONLINE.md` al repo.

## 3. Què cal fer (en ordre)
1. **Decidir l'allotjament amb Ferran** (només ell pot crear comptes/pagar; **tu no en crees mai**). Opcions: una VPS xicoteta (Oracle Cloud Always Free, Hetzner…), o AWS EC2 de Serveis en Xarxa (compte: la IP canvia cada sessió d'AWS Academy, no és estable). Cal una **IP pública fixa** i un **port**. Recomanat: **443** (el més difícil de bloquejar); alternativa **8443/7780**.
2. **Provar la IP candidata des de Batoi** abans de gastar res, en un terminal d'un PC de classe:
   `timeout 5 bash -c 'echo > /dev/tcp/<IP>/<PORT>' && echo OBERT || echo FALLA`
   Provar també `curl -sS -m 8 https://ntfy.sh` per saber si el directori arriba.
3. **Posar el servidor en marxa** a la màquina (Linux): copiar el paquet Linux de la release `v1.6.0-alpha`, servei `systemd` amb reinici automàtic, port obert al tallafocs del proveïdor, i `-canicaServer <port>`. Documentar-ho a `ONLINE.md`.
4. **Client: provar automàticament el servidor propi si `bore.pub` falla.** Ara, si el túnel no arriba, simplement falla. Que el joc, en no poder connectar al túnel (timeout curt), **passe a una llista de servidors per defecte** (el propi de Ferran) i només mostre un error si **cap** funciona. Que la llista es puga canviar sense recompilar (PlayerPrefs / argument), i que accepte **diversos ports** del mateix servidor (p. ex. 443, 8443, 7780) provats en ordre.
5. **Missatge d'error clar** quan res arriba: «No es pot arribar a cap servidor. Prova la pestanya XARXA LOCAL (LAN) o unir-te per IP.» (Ferran ja ho tenia apuntat.)
6. **Proves:** ampliar `Tools/Net/test-online.ps1` amb un cas «túnel bloquejat → servidor propi» (p. ex. `-canicaTunnel 127.0.0.1` o una IP morta) i comprovar que es passa al servidor propi. Els 6 casos actuals han de continuar en PASS.
7. Si **només canvies lògica de client** (fallback, missatges), **no cal pujar el protocol** (continua 24). Si toques missatges de xarxa, puja'l i diu-ho a Ferran. No mesclis això amb els canvis de protocol 25 (mapes en online, Creatiu per a tots, pausa), que són una altra fase.

## 4. Riscos que no s'han resolt
- Un **relay en TCP cru al 443** (no és TLS) podria ser bloquejat per un tallafocs amb inspecció de paquets. Caldria **provar-ho de veritat des de Batoi**; si falla, plantejar-ne una variant (p. ex. WebSocket/TLS) o un altre port.
- `ntfy.sh` pot estar bloquejat a Batoi igual que `bore.pub`: amb servidor propi no cal, però cal comprovar-ho.
- No s'ha provat res a Batoi des d'esta sessió; tot el de Batoi ve de la nota de Ferran/Claude Code de Linux.

## 5. Regles que no es poden saltar
- Treballa **només** a `C:\Users\Usuario\Documents\Unity\Canica Alpha`, branca `alpha/arsenal`. **Mai** «Canica 1».
- **Mai** `git reset --hard`, `clean` ni `stash`; `git add` fitxer a fitxer (mai `-A` al repo del joc); soroll de reimportació d'Unity no es commiteja. Commits menuts, en valencià **sense accents**, amb la teua línia de coautoria.
- **No facis push a `main`, ni etiquetes, ni release** sense permís explícit de Ferran. El permís de la v1.6.0 **no val** per a la v1.6.1.
- **Mai crees comptes, ni demanes ni guardes contrasenyes, claus o tokens.** La IP/domini del servidor propi es pot posar a la configuració, però **cap secret** al repo.
- Contingut generat als builders d'Editor, no als `.asset`. No crees `.meta` a mà.
- No digues que està provat si no ho has executat; diu què has comprovat i què no.
- En acabar: actualitza `memoria-claude\Canica Alpha.md` (`git pull --rebase`, canvia, `git add`, commit i push **només allí**).
