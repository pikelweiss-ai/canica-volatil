# Grup G · Sistemes · v1.2.0 → v1.2.1 · 20/11/2026

Fonts llegides: `context.md`, `dossier-canica.md`, `ONLINE.md`, `CoinCounter.cs` (on viu `CoinBank`), `CoinDrop.cs`, `EnemyHealth.cs`, `EnemyBoss.cs`, `Chest.cs`, `Breakable.cs`, `RunStats.cs`, `Cosmetics/` + `Resources/Cosmetics.asset`, `Dungeon/DungeonGenerator.cs`, `Net/NetCloud.cs`, `NetSession.cs`, `NetCoop.cs`, `NetAvatars.cs`, `NetLink.cs`.

---

## 🪙 Sara Llinares · Dissenyadora d'economia
> Unes monedes que no es poden gastar no són una economia: són un comptador de pols. I el pitjor és que ja en tens centenars al banc.

### Primera impressió
L'entrada de monedes està ben repartida i escala amb el pis, però l'eixida és zero: 63 cosmètics (28 skins, 17 barrets, 18 esteles) i tots gratis. Això explica part del "no té gancho": acabar una partida no et deixa res a la butxaca que puga canviar el que fas en la pròxima.

### Quantes monedes entren (solo, mitjanes calculades del codi)
- Enemic normal: 1–2 (Golem 1–3; Kamikaze i Ratpenat 1) → ~1,45 de mitjana. Boss: 12 fixes (`EnemyBoss.OnDied`). Cofre: rang+1 (comú 1 … llegendari 4) → ~1,7 per cofre. Caixes/barrils: 40 % d'1 moneda.
- Enemics per pis = sales de combat × onades × (2 + pis + 0–1). Sales de combat ≈ sales − 3 (inici, tresor, boss).

| Pis | Sales combat | Onades | Enemics/onada | Enemics | Monedes del pis | Acumulat |
|---|---|---|---|---|---|---|
| 1 | ~5,5 | 1,5 | 3,5 | ~29 | ~67 | ~67 |
| 2 | ~6,5 | 1,5 | 4,5 | ~44 | ~91 | ~158 |
| 3 | ~7,5 | 2,5 | 5,5 | ~103 | ~180 | ~340 |
| 4 | ~8,5 | 2,5 | 6,5 | ~138 | ~230 | ~570 |
| 5 | ~8,5 | 2,5 | 7,5 | ~159 | ~265 | ~835 |

(No compte les caixes: són ~+5 per pis.) El salt del pis 2 al 3 és enorme (+134 % d'enemics) per la 3a onada fixa: també ho note com a ritme, no sols com a economia.

**Per hora:** no hi ha dades de temps reals. Estimació meua: ~15–20 monedes/minut → **~900–1.200/h** **[cal provar]**. El resum de partida ja guarda temps i monedes: que Ferran apunte 5 partides i ho tanquem.

**Cooperatiu:** el nombre d'enemics no depén dels jugadors (`2 + pis + 0–1`, línia 771) i la moneda és "del primer que l'agafa". En 4 jugadors, cadascú cobra ~1/4. Això castiga jugar amb amics. A més, `ONLINE.md` diu "cadascú rep el seu botí" i el codi diu el contrari: cal triar i documentar-ho.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís |
|---|---|---|---|
| Que les monedes servisquen per a skins | 👍 | És l'eixida que falta, sense tocar el balanç | Botiga de baix |
| "Cal un compte perquè tinga sentit" | 🤔 | No cal: per a cosmètics basta un perfil local. El compte només evita perdre-ho en canviar d'aparell | Primer perfil local + exportar (Jordi) |

### Les meues propostes
- **Botiga de cosmètics (preus explicables per raresa):**

| Raresa | Quants | Preu | Temps aprox. (a ~1.000/h) |
|---|---|---|---|
| Gratis d'inici | 8 skins, 5 barrets, 3 esteles | 0 | — |
| Comú | ~20 | 150 | 1 partida fins al pis 2–3 |
| Rar | ~15 | 400 | ~25 min |
| Èpic | ~10 | 900 | ~1 h |
| Llegendari (Arc de Sant Martí, 3–4 skins) | ~5 | 2.000 | ~2 h |

Total ≈ 28.000 monedes ≈ **25–30 h** per a tindre-ho tot. Per a un roguelike està bé; si les proves donen menys de 600/h, baixeu tots els preus un 30 %.
- **Una moneda persistent, amb una decisió:** a la v1.3, una sala-botiga dins de la masmorra (nou tipus de sala; ara només n'hi ha 4) on gastes monedes de la partida en una cura o en tornar a tirar un cofre. El que **no** gastes va al banc. Així cada moneda té dos usos i hi ha una decisió de veritat, i dona varietat a les masmorres.
- **No castigar la mort:** tot el que agafes va al banc encara que mores (com ja passa). Res de perdre el 50 %.
- **Cooperatiu:** monedes instanciades (cada jugador veu i agafa les seues) o, si és car, botí ×0,6 per jugador i cadascú el seu. Mai menys que jugant sol.
- **Les monedes que ja hi ha al banc:** respecteu-les. És un regal per als primers jugadors i no trenca res.

### El que més em preocupa
1. Farming: comprovar que les monedes del lobby i del mapa lliure **no** sumen al banc ni reapareixen **[cal provar]** (`CoinBank.Add` només exclou l'escena del Creatiu).
2. Que la botiga es convertisca en grind: si un cosmètic que vols costa més de 2 h, molta gent ho deixa.
3. Cap diner real, mai. Ni "packs", ni anuncis per monedes.

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7,5/10

---

## 🌐 Jordi Mas · Enginyer de xarxa
> Primera regla: no prometes comptes que no pots mantindre. Segona: si depens d'un servei gratuït, tin un pla B el dia que caiga.

### Primera impressió
L'online actual és més sòlid del que esperava per a un desenvolupador sol: estrella amb amfitrió autoritari, TCP amb `NoDelay`, posicions a 20/s (enemics 15/s), contrasenya amb SHA-256, i la llista només accepta adreces `bore.pub:port`. El problema és la dependència: **dos serveis gratuïts sense compte ni garantia** (`bore.pub` i `ntfy.sh`) i cap pla B, i un suavitzat exponencial (`Lerp` amb `Exp(-15·dt)`) que amb 300 ms i fluctuacions fa que tot vaja a salts.

### Comptes sense servidor: què es pot fer i què no
| Opció | Què dona | Contres / trampes | Cost | Esforç |
|---|---|---|---|---|
| **A. Perfil local + codi d'exportació** | JSON (monedes, cosmètics, rècords) a `persistentDataPath` en lloc de `PlayerPrefs`, amb còpia de seguretat i un codi Base64 + checksum (o QR) per a passar-lo a un altre aparell | Manual. Es pot editar (per a cosmètics, igual). A Android, desinstal·lar l'APK esborra les dades | 0 € | **S** |
| **B. Unity Gaming Services: Authentication + Cloud Save** | Inici de sessió (anònim lligat a l'aparell, o usuari+contrasenya per a canviar d'aparell) i dades al núvol de Unity. No has de guardar contrasenyes tu | Dependència de Unity i dels seus preus. Cloud Save escriu des del client: es pot fer trampa amb les monedes (per evitar-ho, Cloud Code = **L**). Té pla gratuït, però **no sé els límits exactes actuals** ni si demana targeta: cal mirar-ho al panell. Ferran té 17 anys: millor que el compte de l'organització siga de Víctor. Cal una política de privacitat. Linux **[cal provar]** | 0 € dins del pla gratuït | **M** |
| **C. Comptes de plataforma** | Steam Cloud (PC) o Google Play Games (Android) | Cada un només en la seua plataforma: trenca el joc creuat. Steam Direct costa uns 100 $ i Google Play uns 25 $ (una sola vegada) | Diners | M–L |
| **D. Servidor propi** | Control total | Guardar contrasenyes, seguretat, RGPD amb menors, servidor que paga algú i que cau a les 3 de la matinada | VPS o crèdits que caduquen | **L** |
| ❌ Usar `ntfy.sh` com a "base de dades" | — | Els temes són públics: qualsevol llig i escriu | — | No |

**Sense servidor propi es pot:** perfil persistent, còpia, passar-lo d'aparell (A), i fins i tot un compte real si delegues en Unity (B). **No es pot:** economia a prova de trampes, rànquings fiables ni compres. Per a cosmètics gratis no ho necessites.

**Recomanació:** v1.2.1 → A (és la base: B serà "pujar este mateix JSON"). Beta → B, només si la gent demana jugar al PC i al mòbil amb el mateix perfil.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís |
|---|---|---|---|
| Comptes perquè les monedes tinguen sentit | 🤔 | El sentit el dona la botiga; el compte només dona portabilitat | A ara, B a la Beta |
| "Sense servidor no es pot" | 👍/🤔 | Té raó per a un compte segur; no la té per a un perfil que no es perd | Taula de dalt |

### Rework de la connexió (per prioritat)
1. **Mesurar (S):** ara el ping és "temps fins al túnel + el de l'amfitrió (aproximat)". Un ping real client↔amfitrió dins de la partida i al log. Sense això no sabem si els 300 ms són reals **[cal provar entre dues cases]**.
2. **Pla B dels serveis (S):** els paràmetres `-canicaTunnel` i `-canicaDirectory` ja existeixen. Que el joc provi una llista (bore.pub → un segon túnel → IP directa) i ho mostre. `bore` és de codi obert: es pot muntar al teu servidor el dia que en tingues un.
3. **Interpolació amb búfer (M):** guardar instantànies amb marca de temps i dibuixar ~100 ms en el passat en lloc del `Lerp` exponencial. És el que més millora la sensació amb latència alta.
4. **Reconnexió del client (M):** si un client cau, que puga tornar en 30 s amb el mateix lloc, vida i armes. Ara torna al navegador.
5. **Connexió directa amb UPnP quan es puga (M):** saltar el túnel si el router ho permet. No funciona amb CGNAT; el túnel continua sent la reserva.
6. **Migració d'amfitrió (L): no ara.** Amb amfitrió autoritari i port de túnel lligat al seu PC és molta feina. Més barat: guardar l'estat del pis (ja es fa per al portal de tornada) i que la colla puga reprendre la partida.
7. **Unity Relay (L):** servidors europeus i UDP, però el netcode és `System.Net.Sockets` propi i caldria passar a Unity Transport. Límits gratuïts exactes: no els sé.

### El que més em preocupa
1. El dia que `bore.pub` o `ntfy.sh` canvien les normes, l'online mor sencer. `ntfy.sh` limita per IP: una classe sencera darrere d'una IP pot topar-hi **[cal provar]**.
2. L'amfitrió accepta el dany que li envien els clients: en sales públiques, un client modificat pot fer trampes. Amb amics no importa; en públiques, sí.
3. Que es promoguen "comptes" i després no es puguen mantindre.

### Nota del joc ara mateix: 6,5/10 · i amb els canvis: 7,5/10
