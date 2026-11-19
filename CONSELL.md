# 🎲 Consell de Canica · Alpha v1.1.1 · 20/11/2026

Onze perfils han revisat el joc: un desenvolupador de balanç, jugadors casuals, un pro, gent que ve d'altres jocs del gènere, dos personatges de reacció i dos desenvolupadors de producte. Han partit de les **dades reals** del projecte (les 23 armes, els 15 enemics i el codi) i de les conclusions de Ferran.

> [!note] Límit
> Ningú ha jugat de veritat: analitzen números i codi. El que porta **[cal provar]** depén de la sensació a les mans, i ho ha de decidir Ferran (o Mario) jugant al **Mode Creatiu**.

Informes complets de cada perfil: [Docs/Consell/2026-11-20](Docs/Consell/2026-11-20/) · Skill: `consell-canica`.

---

## ⚡ En 30 segons

1. **Primer arreglar, després afegir.** Els 11 perfils coincideixen. El **cooldown compartit** fa que cap número de balanç es puga validar. Els **dos softlocks** (fills del boss fora de la sala, loot damunt del portal) maten partides bones. Les **monedes duplicades online** trenquen l'economia.
2. **La raresa no vol dir res ara mateix.** El Shuriken (comú) fa **10 DPS**, més que qualsevol arma rara. El Sniper (llegendari) fa **2,9**, igual que la Pistola. El Llançaflames fa **33 DPS** i fon un boss de 90 HP en 2,6 s.
3. **Les conclusions de Ferran estan ben encaminades**, sobretot les del Projector (cadena elèctrica), el Sniper i el Llançaflames. Tres tenen un **buff amagat** que cal acotar: Escopeta, Vareta d'espores i Martell.
4. **Les monedes no serveixen per a res** i la setmana no té cap objectiu (bucle setmanal: **2/10**). Si les monedes compren cosmètics i hi ha un resum de partida amb rècord, guanya molt per poc esforç.
5. **El joc obert: "encara no".** El primer pas raonable és una **arena creativa que es puga guardar**, partint del Mode Creatiu que ja existeix.

---

## ⚖️ Veredicte dels canvis de Ferran

| Arma | Canvi de Ferran | A favor | En contra / matís | **Proposta final** | Prio |
|---|---|---|---|---|---|
| 🔥 **Llançaflames** | Durabilitat, menys rang, sprite | Marta, Vortex, Ricard, Bru | Laia i Ricard: "acabarà guardat i no l'usarà ningú" | **300 trets** (18 s de foc, uns 600 de dany), rang 3,2 → **2,8**, barra **molt visible** a la hotbar. Si frustra: es recarrega en netejar sala **[cal provar]** | 🔴 |
| 🌀 **Minigun** | Sobreescalfament, con més obert | Tots | Dani: que la calor no es reinicie canviant d'arma | Calor +1 per tret, topall **60** (~2,7 s de foc), refreda **15/s**. Si arriba al topall, **2 s** bloquejada. Con 9° → **12°** | 🟠 |
| 🏹 **Ballesta** | Llevar el zoom de FOV | Marta, Vortex | Vortex: perd informació | Rang 9 → **8,5**: ja no activa el zoom, que salta amb abast ≥ 9 | 🟢 |
| 💜 **Ballesta de cristall** | Menys FOV, fletxes morades, alentir 0,5 s, sons | Tots (Laia: "rol de suport") | Ricard: en grup, frena quasi tot el temps | Rang **8,5**. Alentiment del **40 %** durant **0,5 s**, que no s'acumula | 🟠 |
| 🪃 **Bumerang / Disc de cometa** | Són la mateixa arma | Tots | — | El Bumerang es queda com està. El **Disc** passa a **rebotar a les parets** (3 rebots, sense retorn) | 🟠 |
| 💥 **Escopeta** | 2 de dany de prop, perdigons més lents, rojos i grossos, so | Bru, Iker | Marta, Vortex, Ricard: amb la proposta tal qual faria **16,5 DPS**, massa per a una rara | **6 perdigons**: **2** de dany a menys d'**1,5 u** i 1 més lluny (14,1 / 7,1 DPS). Velocitat 15 → **12** | 🟠 |
| 🗡️ **Espasa de vidre** | V més gran o rework | Pauet | Marta: no és inservible (5,5 DPS); el problema és l'abast | Abans de fer cap rework: rang 1,7 → **2,0** i arc 100° → **120°** **[cal provar]** | 🟢 |
| ⚡ **Projector de polsos** | Cadena elèctrica, bala 2, salt 1, més rang, sense àrea | **Unanimitat**. Bru i Noa: "el millor canvi" | Ara fa **0,8 DPS** i és la pitjor arma del joc | Bala **2**, salt **1**, **màxim 3 salts**, radi del salt **2,5 u**, sense repetir enemic. Rang **5,5**, CD 1,25 → **0,8** (2,5 DPS contra un sol enemic i ~6 contra grups) | 🔴 |
| 🔱 **Llança / Sable** | Canviar-ne una | Tots | La Llança (èpica, 3,5 DPS) fa menys que el Sable (raro, 6,2) | Es canvia la **Llança**: **estocada llarga en línia** (rang 3,5, arc 25°, travessa enemics, dash 1,8). El Sable es queda | 🟠 |
| 🔨 **Martell orbital** | Aim circular, dany 5 | Tots (aim) | Marta: amb 5 calen 3 colps per al Golem | Aim **circular** sí o sí. Dany **6**: el Golem continua caient en 2 colps | 🟠 |
| 🪨 **Pedra** | Encara més lenta | Marta, Vortex, Ricard | Pauet: no vol que la pitjor arma siga l'única que tens al principi | CD 0,5 → **0,65**. Que es note que és una arma de broma | 🟢 |
| 🔫 **SMG** | V més oberta, menys rang | Tots | — | V 12° → **15°**, rang 5 → **4,5** | 🟢 |
| 🎯 **Sniper** | Més dany, menys FOV, so de sniper | **Unanimitat** | Marta: el zoom és el que el fa sniper | Dany 4 → **12**, CD **1,5**: **8 DPS** i mata d'un tret qualsevol enemic normal. El zoom, mantindre'l | 🔴 |
| 🍄 **Vareta d'espores** | V més tancada, més ràpida, verda, verí de 3 en 3 s | Noa, Bru | Marta i Vortex: si el verí s'acumula, triplica el DPS | Cada bola fa **3 en total** (1 directe + 2 de verí en 2 s), amb **un màxim de 3 dosis** per enemic. V 20° → **14°**, velocitat 6 → **9** | 🟠 |

### ➕ Canvis que ixen de les dades (no els havia demanat ningú)

| Arma | Problema (dada) | Canvi | Qui |
|---|---|---|---|
| **Shuriken** (comú) | 10 DPS, més que qualsevol rara | CD 0,3 → **0,45** (6,7 DPS), o passar-lo a poc comú | Marta, Vortex |
| **Làser** (llegendari) | 6 DPS, 14,5 s per a matar el Gegant | Dany 3 → **4** (8 DPS) | Marta |
| **Pistola / Revòlver** | Rol quasi igual | Pistola **ràpida i precisa**, Revòlver **pesat** (més empenta, CD 0,55, dany 3) **[cal provar]** | Marta (rol); números de la síntesi |
| **Regla general** | Les armes de 2–3 DPS no acaben un boss (més de 30 s) | Mínim de **~4 DPS** per a les armes rares o millors | Marta |
| **Bosses** | Vida = (base + 45 × (pis − 1)) × (1 + 0,55 per cada jugador extra), i els atacs no canvien | **+25 HP per pis**, **un atac més cada 3 pisos** i bales del boss **+8 % per pis** (fins a +40 %). Dificultat per patró, no per vida | Vortex, Ricard |

**Tier list del Vortex (ara):** S = Llançaflames, Minigun · A = SMG, Shuriken, Ballesta de cristall, AK47, Escopeta · B = Martell, Sable, Làser, Vareta, Ballesta, Llançagranades · C = Espasa, Revòlver, Llança, Projector · D = Sniper, Bumerang, Vareta d'espores, Pistola, Disc, Pedra.

---

## 👾 Enemics, bosses i varietat

| Idea | D'on ve | Impacte | Esforç | Notes del consell |
|---|---|---|---|---|
| **Amulets** (escut, dany, cadència), en triar **1 de 3** en acabar cada pis | Ferran + Noa + Hugo | ⭐⭐⭐ | M | Escut d'1 colp cada 15 s · +1 de dany cada 3r impacte · −12 % de cooldown (mínim 0,08 s). Necessita el cooldown per arma arreglat |
| **Minibosses dins de sales** | Ferran + Vortex | ⭐⭐⭐ | S–M | Es pot partir de l'elit del pis 3 (ja existeix): més vida i un patró propi |
| **Enemic virus** que s'acobla i et dona un nerf temporal | Ferran | ⭐⭐ | M | Que es puga llevar amb el dash. Nerf de −20 % de velocitat o +20 % de cooldown durant 4 s **[cal provar]** |
| **Etapes temàtiques** (desert, fantasmal, bosc maligne) cada X pisos | Ferran + Laia | ⭐⭐⭐ | L | Una etapa = 3 pisos amb tema, enemics i boss propis |
| **Bosc maligne**: els arbres es tornen enemics en netejar la sala | Ferran + Bru | ⭐⭐ | M | Bru: "moment de clip". Avisar 1 s abans perquè no siga injust |
| **Bales enemigues més ràpides** (algunes) | Ferran | ⭐ | S | Proposta de la síntesi: Tirador 6 → 7,5, Mag 5 → 6; no tocar BoletVeri ni Esporer **[cal provar]** |
| **Estats elementals comuns** (foc, verí, electricitat, alentiment) | Noa + Marta | ⭐⭐⭐ | M | Un sol sistema `StatusEffect` per a les armes noves de Ferran. Sinergies: alentir + foc, verí + cadena |
| **Super carregada** (botó gran, es carrega fent dany) | Iker | ⭐⭐ | L | Ricard i Clara: massa per ara. Per a després de la v1.3 |
| **Caniques amb passiva** (vidre, acer, ull de gat) | Noa | ⭐⭐ | M | Aprofita les 25 skins. Opcional, i que no tape els cosmètics |

---

## 🧭 Com continuar el joc

**1. v1.1.2 · Estabilitat (abans que res).** Tots els perfils, fins i tot el Fan, posen això primer.
- Cooldown **per casella**: un `float[4]` a `PlayerAttack`, **no** al `WeaponData`, que és compartit. Només baixa a la mà; una arma que agafes del terra comença carregada. ⚠ Per a evitar l'exploit de soltar i recollir l'arma, cal guardar el cooldown dins de l'`ItemPickup`.
- Fills del boss **dins de la sala**: ara `Summon` busca la casella lliure més pròxima sense mirar els límits de la sala.
- Drops **apartats del portal** i portal de tornada.
- Monedes i cofres **autoritaris online**: l'amfitrió confirma i després els elimina per a tots.

**2. v1.2 · Arsenal amb identitat.** Fer la passada de balanç d'aquesta taula. Cada arma ha de tindre **sprite, color i so propis**: Pauet i Iker diuen que ara moltes "no tenen personalitat". La **durabilitat i la calor** han de ser estat de cada instància de l'arma (viatja amb `RunState` i amb l'`ItemPickup`), i això també s'ha de sincronitzar online.

**3. Donar sentit a cada partida (molt barat).** Hugo puntua el bucle setmanal amb un 2/10. Proposta:
- **resum de partida** i **rècord** de pis màxim (S);
- **monedes que compren cosmètics** a preus fixos (S);
- col·lecció d'armes descobertes (M);
- **repte setmanal amb una llavor fixa**, que el generador ja admet (M).

Cap patró fosc: res de pagar, ni ratxes que castiguen, ni notificacions.

**4. v1.3 · Varietat.** Amulets 1 de 3, minibosses, l'enemic virus i el primer **estat elemental** comú. Després, les **etapes temàtiques** començant pel **bosc maligne**.

**5. Joc obert: "encara no".** Clara: primer el nucli. Fase 1, guardar mapes del Creatiu en un JSON local (S–M). Fase 2, compartir-los per codi de sala (M). Fase 3, pintar tiles (L). Història i roleplay, descartats per a un desenvolupador sol. Laia demana que el Creatiu no siga només per a l'amfitrió.

**On no estan d'acord:**
- **Llançaflames que es trenca per sempre.** Marta, Vortex i Bru hi estan a favor; Laia i Ricard, en contra. La decisió és de Ferran; la proposta de compromís és que es recarregue en netejar sala.
- **La "super" de Brawl Stars.** Iker la vol; Ricard i Clara diuen que és massa aviat.
- **Ritme d'actualitzacions.** Bru vol novetats ja; Ricard diu que sense arreglar els bugs vells, "les xifres del balanç no serveixen".

---

## 🐛 Bugs i riscos (afegits a la nota de bugs com BUG-014–BUG-020)

| Gravetat | Bug | Pista al codi |
|---|---|---|
| 🔴 | Fills del boss fora de la sala → no pots avançar | `EnemyBoss.Summon` + `TileNav.NearestFree` sense límits de sala; `EnemySplitter` també |
| 🔴 | Loot damunt del portal post-boss → no es pot agafar | Apartar els drops de les caselles del portal i afegir portal de tornada |
| 🔴 | Moneda que agafa un i continua per a l'altre (online) | Pickup autoritari a l'amfitrió (hipòtesi, falta revisar `Net/`) |
| 🟠 | Cofre fantasma ja obert (online, el va veure Herrera) | Reenviar l'estat dels cofres als clients que entren tard |
| 🟠 | Cooldown compartit entre armes | Un sol `nextAttackTime` a `PlayerAttack.cs` |
| 🟠 | Les bales enemigues xoquen amb l'aigua | `EnemyBullet` explota amb qualsevol collider que no siga trigger |
| 🟢 | Barret d'astronauta per damunt d'altres jugadors | Sorting del barret = el del seu jugador + 1 |
| 🟢 | Xat darrere dels arbres | Decisió de disseny: millor en capa d'UI |

---

## 🧪 Pla de proves (Mode Creatiu)

Contra un **Golem (12 HP)** i un **GolemGegant (90 HP)**, amb cronòmetre:
1. **Cooldown:** Sniper → Pistola → Sniper; dos Snipers; soltar i recollir; passar de pis; morir online.
2. **Llançaflames:** trets fins a trencar-se (esperat: 300). Barra visible. Es manté en passar de pis, en morir i quan el recull un company.
3. **Minigun:** temps de foc fins al bloqueig (esperat: ~2,7 s), bloqueig de 2 s i refredament.
4. **TTK** de Sniper, Escopeta (de prop i de lluny), Martell, Projector i Vareta d'espores. Si alguna es desvia més d'un ±20 % del càlcul, és un bug.
5. **Projector:** 3 enemics en fila i un d'aïllat. Màxim 3 salts sense repetir enemic, i igual en tots els jugadors online.
6. **Càmera:** les dues Ballestes sense zoom; Sniper i Làser amb zoom, també al mòbil.

---

## 🗳️ Notes del consell

| Perfil | Ara | Amb els canvis |
|---|---|---|
| 📊 Marta · desenvolupadora balancejadora | 6 | 7,5 |
| 🧪 Dani · tester | 6 | 7 (8 si es tanquen els 🔴) |
| 🎒 Pauet · casual nou, 13 anys | 6 | 7,5 |
| 🫶 Laia · casual cooperativa, 17 anys | 6 | 7 (8 amb dos arreglos) |
| 🏆 Vortex · pro | 6 | 7,5 |
| ⭐ Iker · ve de Brawl Stars | 6,5 | 7,5 |
| 🗝️ Noa · ve de Soul Knight | 6 | 7,5 |
| 😒 Ricard "El Crític" (personatge inventat) | 6 | 7 (8 si arregla abans els bugs) |
| 🤩 Bru "El Fan" (personatge inventat) | 8 | 9,5 |
| 🔁 Hugo · retenció | 5 | 7 |
| 🧱 Clara · jocs oberts | 6 | 7,5 |
| **Mitjana** | **6,1** | **7,5** |

*Els perfils "El Crític" i "El Fan" són personatges inventats, inspirats en un estil de reacció. No representen ni citen cap creador real.*
