# Grup H · Públics nous · Alpha v1.2.0 → v1.2.1

Fonts consultades: `context.md`, `dossier-canica.md`, `MOBILE_ALPHA.md`, `SettingsUI.cs`, `MobileHUD.cs`, `HotbarUI.cs`, `ItemPickup.cs` (`RarityColor`), `PlayerHealth.cs`, `PresentationSettings.cs`, `EnemyBullet.cs`, `DungeonUI.cs`, full `Fulls/Cofres.png`, i la mida real de l'APK a `Builds/Distribucio/Alpha-v1.2.0-release/` (63 154 791 bytes ≈ 63,2 MB).

---

## 🎨 Aina Bosch · Accessibilitat (deuteranòpica)
> Jo no jugue "pitjor": jugue amb la informació que el joc em dona. I ara mateix la raresa me la dona només en color.

### Primera impressió
El joc és llegible en forma (sales, portes, enemics amb silueta pròpia), però **tot el sistema de raresa és només color**: vora d'1 px a la icona (`ItemGraphics.Outlined`) i, als cofres, la mateixa caixa amb unes bandes de color diferent. A `Ajustos` hi ha PARTIDA, SO, PANTALLA i INTERFÍCIE (posició de vida/monedes): **cap opció de dificultat, de mida de text ni de daltonisme**. El gancho per a mi no falla per l'accessibilitat; falla perquè no entenc què val cada cosa que trobe.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| Nerf Sniper/Làser | 🤔 | Bé, però amb Làser a 12 d'abast i bala 35, el raig ha de ser visible més d'un frame | Mantín el traç ≥0,15 s |
| Espores sense homing | 👍 | Menys coses invisibles que passen | Que el verí tinga icona, no només tint verd |
| Comptes | 🤔 | Si existeixen, que guarden també els ajustos d'accessibilitat | Opcional, mai obligatori |

### Problemes concrets (gravetat)
1. **Alta · Raresa = color.** Gris (0.75,0.75,0.78), verd (0.32,0.78,0.36) i daurat (0.96,0.77,0.26) se'm barregen; blau (0.29,0.50,0.88) i morat (0.64,0.35,0.88) per a mi són quasi el mateix. Al full de cofres, Rar i Èpic només canvien el color de la banda.
2. **Mitjana · Bales enemigues** rosa-roja (1,0.3,0.4) sobre terres foscos/lava (Volcà) [cal provar].
3. **Mitjana · Text tàctil:** etiquetes de 19–21 punts a referència 1600×900, caselles de hotbar de 90 px. En un mòbil de 6" queda xicotet [cal provar].
4. **Mitjana · Armes sense descripció:** no hi ha cap text que explique "rebots 3", "calor", "cadena". Ho has de descobrir disparant.
5. **Baixa · Morts ràpides:** 5 cors i 1 s d'invulnerabilitat, sense cap mode assistit.

### Top 5 d'opcions per esforç (de menys a més)
1. **Lletra de raresa a la casella** (C/PC/R/E/L o 1–5 punts) a la hotbar i al terra. *S, ½ dia.*
2. **Forma als cofres:** comú sense res, rar 1 gemma, èpic 2, llegendari corona (ja té pany roig, falta més). *S, retocar 4 sprites.*
3. **Descripció d'1 línia per arma** en agafar-la ("Rebota 3 voltes", "S'escalfa si no pares"). *S–M.*
4. **Mida d'interfície** 100/125/150 % (un multiplicador al `CanvasScaler` dels HUD tàctil i hotbar). *M.*
5. **Mode assistit**: +2 cors i 1,5 s d'invulnerabilitat, marcat a la pantalla de resum (no compta rècords). *M.*

### El que més em preocupa
- Que l'economia de monedes→skins arribe abans que això: comprar coses que no distingisc no em motiva.
- Contrast de les bales a Volcà i Cristall **[cal provar]**.

### Nota ara: 5/10 · amb el top 5: 7/10

---

## 👩‍👦 Rosa i Martí · Família que juga junta
> Martí diu: "La boleta mola i el llançaflames més". Rosa diu: "Jo només vull saber on he d'anar sense morir-me al minut dos".

### Primera impressió
**Martí diu:** els bosses són guais, el Rei Llim fa riure. Però "totes les sales són iguals, entres, mates, ix". Al pis 3 ja ho ha vist tot excepte el tema. **Rosa diu:** el cooperatiu és **només online**: necessitem dos aparells (el PC i la tauleta) i fer CREAR SALA amb un codi. No hi ha pantalla partida. Quan caic, he d'esperar que Martí netege la sala per tornar (`Revive` amb vida plena), i si caiem tots dos, es perd tot: no hi ha cap manera de deixar-ho a mitges i continuar demà.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| Nerfs d'armes | 🤔 | No ho notem; el que notem és que les armes no diuen què fan | Text curt en agafar-les |
| Espasa de vidre a 4 | 👍 | Martí va a melee sempre (cos a cos, corrent) | I que trenque caixes (BUG-021!) |
| Més modes | 👍👍 | És el que ens falta | Vegeu baix |

### Les nostres propostes
- **Mode Família / Fàcil:** 8 cors, reviure el company en 10 s quedant-te al costat (no esperar a netejar), enemics −25 % vida. Rosa: "així no em sent el llast".
- **Masmorres:** **sales sorpresa** (una font que cura, una sala amb un enemic gegant i graciós, una sala de tresor amb trampa de llim). Martí vol "una sala on cau tot de monedes". Hui només hi ha Start/Combat/Treasure/Boss.
- **Bosses:** que facen coses graciosos i que avisen (el boss parpelleja abans de l'atac fort). Rosa no veu d'on li ve el mal [cal provar].
- **Mode Creatiu** al menú principal, no amagat al mapa lliure: Martí ve de Minecraft, crear enemics i bosses li encanta.
- **Guardar en eixir del pis** (continuar al pis 4). Amb nens, una partida que no es pot pausar no es juga.
- **Comptes:** **no** volem un compte per a un xiquet de 9 anys. Un perfil local amb nom i skin, sí. Les monedes per a skins: a Martí li encantaria comprar barrets.

### On ens hem perdut
- Els menús: "OBRIR (mantín)" i "TIRAR ARMA" a la tauleta; Martí tira l'arma sense voler [cal provar].
- Rosa no sap què és "Èpic" ni per què unes armes són millors.

### El que més ens preocupa
1. Morir massa ràpid i perdre-ho tot (5 cors, permadeath).
2. La Cripta amb esquelets i el Mag Fosc: per a 9 anys, prou bé, però sense música tètrica [cal provar].
3. Dependre d'Internet i un túnel públic (`bore.pub`, ~300 ms) per jugar al mateix menjador; la LAN ajuda, però si la Wi-Fi aïlla dispositius, no va.

### Nota ara: 5/10 · amb Mode Família, guardar per pis i sales sorpresa: 7,5/10

---

## 📱 Youssef El Amrani · Android de gama baixa
> Si va fluid al meu mòbil de 3 GB, el recomane al grup. Si s'escalfa o es talla l'online, l'esborre.

### Pas a pas
- **Instal·lar:** APK de **63,2 MB** (v1.2.0-release), Android 8+, signada amb **clau debug**: Android i Play Protect m'avisaran "aplicació no segura" [cal provar]. 63 MB amb dades em fa mal; Brawl en pesa més, però ell està a la Play Store. Objectiu raonable: <45 MB (només ARM64, comprimir textures, LZ4HC).
- **Primer minut:** horitzontal, safe area respectada (bé). No sé què fa cada arma: cap descripció.
- **Combat:** joystick dret = apuntar i **disparar sempre que el mous** (zona morta 16 %). Còmode amb dos polzes; **amb una mà no es pot jugar**. Canviar d'arma és tocar caselles de 90 px a dalt/baix [cal provar si arribe sense deixar el joystick]. Botons de 120×85 a referència 1600×900: correctes en 6,5", justos en 5,5".
- **Rendiment:** límit 30/60 FPS al mòbil (bé, 30 estalvia bateria). El Llançaflames (fins a 300 trets) i la Minigun (0,045 s) són les armes que més em poden fer baixar FPS [cal provar al mòbil real]. Sense opció de qualitat gràfica ni de partícules.
- **Online:** funciona amb dades, però ~300 ms pel túnel. **Si l'amfitrió ix o el sistema suspén l'app, es tanca la sala**: amb una notificació o un WhatsApp que obris, adeu partida si ets l'amfitrió. Les dades que consumeix no estan mesurades [cal provar].

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| Nerf Làser/Sniper | 👍 | Al mòbil apuntar fi és difícil; el Làser (travessa 5, 12 d'abast) es converteix en l'única arma útil | Nerf de dany, no de velocitat de bala |
| Espores sense homing | 🤔 | Al mòbil el homing ajudava molt | Compensar amb +1 pellet o arc més ample |
| Comptes | 👎 si és obligatori | No vull registrar-me amb dades | **Codi d'exportació** del perfil (text/QR) i prou |

### Les meues propostes
1. **Mode Ràfega 5 min**: 3 pisos curts i boss. Sessions curtes = el que jugue al bus.
2. **Opció "Gràfics baixos"**: menys partícules i `Fx.Burst`, sense ombres.
3. **Botó "Canvi d'arma"** al costat del joystick dret (següent arma), i opció de **disparar automàtic al més proper** per jugar amb una mà.
4. **Reconnexió** si l'app torna en <20 s, almenys per als clients.
5. Mostrar el **ping** a la sala.

### El que més em preocupa
- Que el mòbil s'escalfe en partides llargues al pis 6+ amb molts enemics (2 + pis + 0–1 per onada, fins a 3 onades) [cal provar].
- APK debug fora de la Play Store: als meus amics els fa por instal·lar-la.

### Nota ara: 5,5/10 · amb mode curt, gràfics baixos i APK <45 MB: 7,5/10
