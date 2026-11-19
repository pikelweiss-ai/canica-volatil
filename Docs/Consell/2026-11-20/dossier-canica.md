# Dossier de dades · Alpha v1.1.1 · 2026-11-20

Rareses (enum ChestRarity): 0 Comu · 4 PocComu · 1 Rar · 2 Epic · 3 Llegendari. Cooldown en segons. DPS teoric = damage*pellets/cooldown (sense explosio ni pierce).
Camera: si una arma no melee te range >= 9, la camera s'allunya per a veure tot l'abast (el 'amplificador de FOV').
Cooldown: PlayerAttack usa un sol nextAttackTime per a totes les armes (compartit).

## Armes
| Arma | Rar | Dany | Pellets | CD | DPS teoric | Abast | Vel. bala | V (graus) | Extres |
|---|---|---|---|---|---|---|---|---|---|
| AK47 | 4 | 1 | 1 | 0.12 | 8.3 | 7 | 18 | 4 |  |
| Ballesta | 1 | 3 | 1 | 0.6 | 5.0 | 9 | 20 | 0 | travessa 2;  |
| BallestaCristall | 2 | 2 | 3 | 0.75 | 8.0 | 9 | 20 | 9 | travessa 3; patro fix;  |
| Bumerang | 1 | 2 | 1 | 0.7 | 2.9 | 5.5 | 11 | 0 | torna;  |
| DiscCometa | 1 | 2 | 1 | 0.85 | 2.4 | 6 | 11 | 0 | travessa 1; torna;  |
| Escopeta | 1 | 1 | 7 | 0.85 | 8.2 | 4 | 15 | 22 | patro fix; abast aleatori 0.3;  |
| EspasaVidre | 0 | 3 | 1 | 0.55 | 5.5 | 1.7 | 12 | 0 | melee arc 100; colp alternat;  |
| Laser | 3 | 3 | 1 | 0.5 | 6.0 | 12 | 35 | 0 | travessa 5;  |
| LlancaTempesta | 2 | 3 | 1 | 0.85 | 3.5 | 2.5 | 12 | 0 | melee arc 55; dash 1.8; colp alternat;  |
| Llancaflames | 3 | 1 | 2 | 0.06 | 33.3 | 3.2 | 9 | 14 | abast aleatori 0.35;  |
| Llancagranades | 2 | 1 | 1 | 1 | 1.0 | 6 | 9 | 0 | explosio r1.6 dany 4;  |
| MartellOrbital | 2 | 7 | 1 | 1.1 | 6.4 | 2.2 | 12 | 0 | melee arc 160; radial;  |
| Minigun | 3 | 1 | 1 | 0.045 | 22.2 | 7.5 | 19 | 9 | arrancada 0.6s;  |
| Pedra | 0 | 1 | 1 | 0.5 | 2.0 | 6 | 12 | 0 |  |
| Pistola | 0 | 1 | 1 | 0.35 | 2.9 | 6 | 16 | 3 |  |
| ProjectorPolsos | 1 | 1 | 1 | 1.25 | 0.8 | 5 | 8 | 0 | explosio r1.25 dany 2;  |
| Revolver | 0 | 2 | 1 | 0.45 | 4.4 | 7.5 | 18 | 0 |  |
| SMG | 2 | 1 | 1 | 0.07 | 14.3 | 5 | 17 | 12 | abast aleatori 0.1;  |
| SabreCorall | 1 | 4 | 1 | 0.65 | 6.2 | 2 | 12 | 0 | melee arc 130; dash 0.85;  |
| Shuriken | 0 | 1 | 3 | 0.3 | 10.0 | 6 | 15 | 12 | travessa 1; patro fix;  |
| Sniper | 3 | 4 | 1 | 1.4 | 2.9 | 12 | 32 | 0 |  |
| Vareta | 2 | 2 | 1 | 0.35 | 5.7 | 9 | 9 | 0 | seguiment 6;  |
| VaretaEspores | 1 | 1 | 3 | 1.1 | 2.7 | 7 | 6 | 20 | seguiment 2; patro fix;  |

## Enemics i bosses (prefabs de Assets/Prefabs/Enemics)
| Enemic | Vida | Vel. | Dany | Vel. bala | Cooldown |
|---|---|---|---|---|---|
| Aranya | 3 | 3 | 1 |  | 2 |
| BoletVeri | 5 | 0 | 1 | 4 |  |
| Escarabat | 7 | 1.6 | 1 |  |  |
| Esporer | 3 | 1.2 |  | 4 |  |
| Esquelet | 4 | 2 |  | 7 |  |
| Golem | 12 | 1.3 | 1 |  | 3 |
| GolemGegant | 90 | 1.3 | 1 | 5 |  |
| Kamikaze | 2 | 2.6 |  |  |  |
| Llim | 5 | 2.2 | 1 |  |  |
| Mag | 4 | 2 |  | 5 | 3.2 |
| MagFosc | 65 | 2 | 1 | 5 |  |
| Malo | 4 | 2.6 | 1 |  |  |
| Ratpenat | 2 | 3.4 | 1 |  | 1.3 |
| ReiLlim | 70 | 1.6 | 1 | 5 |  |
| Tirador | 3 | 2.2 |  | 6 |  |

## Jugador
- Vida maxima: 5 cors.
- Hotbar 4 armes (1-4), 4 cures (5-8). Dash. Cooperatiu fins a 4 amb amfitrio autoritari.
