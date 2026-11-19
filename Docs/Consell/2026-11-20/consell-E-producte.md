# Consell de Canica · Grup E · Producte (retenció i plataforma) · Alpha v1.1.1 · 20/11/2026

Fonts llegides: dossier de dades, conclusions de Ferran, README, DISSENY (secció 11, Mode Creatiu), ALPHA_PLAN, ONLINE i els scripts `RunState.cs`, `CoinCounter.cs` (`CoinBank`), `CreativeMode.cs`, `Cosmetics/*`, `Dungeon/DungeonGenerator.cs`.

Fets del codi que condicionen tot el grup:
- `CoinBank` guarda un total de monedes a `PlayerPrefs` ("MonedesTotals"), però **només es mostra** a Personalitzar ("Monedes: N · X skins · Y barrets"). No es gasta en res.
- Els 25 cosmètics es trien lliurement amb fletxes (`CustomizeMenuUI`): **cap desbloqueig**.
- `RunState` és estàtic i només viu dins d'una run (pis, vida, monedes, armes, cures, llavor). **No es guarda cap rècord** (pis màxim, bosses vençuts, armes descobertes).
- El generador ja accepta `seed` i online usa `Seed + Floor * 7919`: la base per a reptes amb llavor existeix.
- Mode Creatiu: només al mapa lliure (`GameScene`), només l'amfitrió online, **no es guarda res**.
- Tenda, fitxes de col·lecció, mercader, santuari i relíquies estan **proposats a ALPHA_PLAN però no implementats**.

---

## 🧲 Hugo Navarro · Dev especialista en addicció i retenció (ètic)
> Un joc enganxa bé quan tornes perquè vols, no perquè et fa por perdre alguna cosa. Canica té el primer i, per sort, res del segon.

### Primera impressió
El combat i el cooperatiu sense IP són un bon motor de "una partida més" entre amics. El problema és el que passa quan la run acaba: tornes al menú amb un número de monedes que no serveix per a res. Cada sessió acaba "sense res guanyat", que és justament el que més mata la retenció D7.

### Els 4 bucles ara mateix
| Bucle | Nota | Què hi ha | Què falta |
|---|---|---|---|
| 30 s · combat | **7/10** | 23 armes amb rols diferents, dash, explosions, homing, retorn, onades, cofres per raresa | Feedback desigual entre armes (Ferran mateix demana sons i sprites per a Sniper, Escopeta, Projector…). Cooldown compartit fa que canviar d'arma no siga una decisió neta. **[cal provar]** |
| 5 min · sala/pis | **6/10** | Neteja de sala → cofre; sala d'elit (pis 3+), cambra de ressonància (pis 4+) amb arma garantida, boss per pis | Totes les sales són "combat" (RoomType: Start, Combat, Treasure, Boss). Cap decisió entre sales: ni mercader, ni santuari, ni camí alternatiu. |
| Run · sessió | **5/10** | Pisos amb dificultat creixent, 8 temes, bosses, botí compartit, reviure en netejar | No hi ha pantalla de resum ni rècord. Les monedes de la run no es gasten dins de la run. Si caus tots, tornes al pis 1 sense cap resultat que quede. |
| Setmana · meta | **2/10** | Total de monedes persistent (només visible), 25 cosmètics, online amb amics | Res a perseguir: cosmètics tots oberts, cap objectiu, cap col·lecció, cap rècord. L'únic motiu de tornar és "quedar amb Mario". Que és molt, però no escala. |

### Les 5 palanques de més impacte, ordenades per esforç
1. **S · Resum de run + rècord personal.** En morir o eixir: pis arribat, bosses vençuts, monedes, arma que més ha matat. Guardar `MillorPis` a `PlayerPrefs`. Dona sentit a cada sessió amb unes 50–100 línies.
2. **S · Gastar el total de monedes en cosmètics.** Bloquejar part dels 25 (no tots: deixar els actuals que ja usa cada jugador, com diu ALPHA_PLAN) i desbloquejar-los amb `CoinBank`. Preu fix i visible, sense atzar. Només amb monedes guanyades jugant.
3. **M · Col·lecció d'armes ("Bestiari d'armes").** Marcar cada arma quan la trobes per primera vegada; mostrar 14/23 amb silueta de les que falten i pista ("cambra de ressonància, pis 5"). Recompensa variable natural: els cofres ja tenen raresa.
4. **M · Repte de llavor setmanal.** Fer servir la `seed` que ja existeix: una llavor fixa per setmana (derivada de la data, sense servidor) i comparar pis entre amics. Bucle social de setmana gratis, sense comptes.
5. **L · Una sala de decisió per pis (mercader o santuari).** El primer lot d'ALPHA_PLAN. Dona ús a les monedes de la run i crea el "què trie?" que falta al bucle de 5 min. Cal sincronitzar-ho online, per això és L.

### Patrons foscos que rebutge (no per a aquest joc)
- ❌ **Caixes de botí de pagament o amb moneda "premium"**: Ferran no té monetització i juguen menors. Ni de broma.
- ❌ **FOMO agressiu**: botigues rotatives de 24 h, cosmètics que "desapareixen per sempre". El repte setmanal ha de ser opcional i sense premi exclusiu.
- ❌ **Ratxes diàries amb càstig** (perds la ratxa si no entres): genera obligació, no ganes.
- ❌ **Energia o temps d'espera per jugar**: contradiu tot el que fa bo el joc.
- ⚠️ **Notificacions push a Android**: no, en un joc per a amics.
- ✅ El que sí: recompenses variables **dins del joc** (cofres), metes visibles, joc social real.

### El que més em preocupa
1. Afegir tenda abans del resum de run: primer cal que cada partida "deixe alguna cosa".
2. Bloquejar cosmètics que la gent ja porta seria percebut com a llevar-los coses. Migració suau.
3. Tot és `PlayerPrefs` local: amb el cooperatiu, cada client es guarda les seues coses. Està bé per a amics, però que no es venga com a economia "segura". **[cal provar]**

### Nota del joc ara mateix: 5/10 · i amb les palanques 1–4: 7/10

---

## 🧱 Clara Pastor · Dev de jocs oberts / sandbox
> Teniu la llavor d'una plataforma, però una llavor no és un arbre. I Ferran és un sol jardiner.

### Primera impressió
El Mode Creatiu és més interessant del que pareix: ja pots generar qualsevol arma, enemic, boss i cofre, i online arriba als clients sense canviar el protocol (8). Amb les sales per codi de 6 lletres, açò ja és un "Garry's Mod de caniques" mínim. Però no es guarda res, i sense guardar no hi ha comunitat que cree.

### Obrir el joc: pros i contres
| Pros | Contres |
|---|---|
| El Creatiu i el catàleg `NetContent` (armes, enemics, cofres) ja existeixen i sincronitzen | Ferran és sol i està a 2n de SMX: cada mode nou és manteniment per a sempre |
| Les sales per codi permeten compartir moments sense servidor | `ntfy.sh` limita peticions per IP; no és un lloc on penjar mapes |
| Els amics joves (Pauet, Laia, Mario) creen reptes sols si els dones eines | El nucli encara té deutes: cooldown compartit, loot damunt del portal, cofres fantasma online |
| Un editor fa de banc de proves de balanç per a Ferran mateix | Un "joc obert" sense identitat dilueix la masmorra, que és el que el fa especial |

### Quin mode primer
Ni història, ni roleplay, ni supervivència. **Primer: "Arena creativa" guardable**, és a dir, el Creatiu que ja hi ha amb poder guardar la disposició d'enemics i cofres i tornar-la a carregar. Després, compartir-la per codi dins d'una sala. Raons: reutilitza el 90 % del que existeix, no necessita servidor, i és un mode on els amics es fan reptes entre ells ("aguanta 3 GolemGegant amb la Pedra").
La supervivència per onades seria la segona opció: és barata (ja hi ha onades i spawn d'enemics) però és contingut, no plataforma. Història i roleplay: descartats per a un dev sol.

### Full de ruta en 3 fases
| Fase | Què | Esforç | Condició per passar a la següent |
|---|---|---|---|
| 1 · Guardar | Desar/carregar la disposició del Creatiu a un fitxer JSON local (posició + nom de prefab del catàleg). 3 ranures. Permetre el Creatiu també als clients amb permís de l'amfitrió | **S–M** | Els bugs vells del nucli tancats (cooldown, portal, cofres fantasma) |
| 2 · Compartir | L'amfitrió carrega un mapa guardat a una sala de mapa lliure; els clients el reben amb els missatges `EnemySpawn`/`ChestSpawn` que ja hi ha. Mode "repte": començar onada quan tots estiguen LLEST | **M** | Mario i 2–3 amics han fet i jugat almenys uns quants mapes de veritat |
| 3 · Editar | Pintar tiles (parets, aigua, lava, rebotadors d'ALPHA_PLAN) al Creatiu, i exportar el mapa com a text curt per enviar-lo per WhatsApp | **L** | Que la fase 2 s'use; si no, parar ací sense remordiments |

### El que més em preocupa
1. Voler fer-ho tot alhora: PvP "Ruptura de nucli", tenda, sales noves i plataforma. Una cosa cada vegada.
2. Guardar per nom de prefab trenca mapes si es renomena un enemic. Cal un identificador estable (ALPHA_PLAN ja ho demana). **[cal provar]**
3. Compartir fitxers d'altres: només JSON de posicions, mai codi ni scripts.

### Veredicte: **encara no** com a plataforma. **Sí** a la fase 1 (guardar el Creatiu) quan el nucli estiga net.

### Nota del joc ara mateix: 6/10 · i amb la fase 1–2: 7,5/10
