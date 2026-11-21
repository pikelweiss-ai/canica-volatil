# Pendents apuntats per Ferran després de l'Alpha v1.4.2

> Apuntats jugant la v1.4.2 (21 de novembre de 2026). **Només estan apuntats: encara no s'ha arreglat res.** Es poden fer en una **v1.4.3** abans de la v1.5 o dins d'ella. Cada punt diu on mirar. Detall i conversa a `C:\Users\Usuario\memoria-claude\Canica Volàtil - Bugs pendents.md`.

| # | Què passa | On mirar | Arreglo previst |
|---|-----------|----------|-----------------|
| 1 | **El Talp va rar en online.** Sense detall. | `EnemyV140.cs` (tipus `Mole`, `LateUpdate`: colliders apagats també a les còpies dels clients; `aim`, ombra) | Reproduir amb dos processos (`test-online.ps1` o un bot host/join amb un Talp), comparar `VisualState`/`Aim` als dos costats i deixar els colliders de les còpies sempre actius (el mal el resol l'amfitrió). |
| 2 | **Torreta antiga: les seues bales no passen per damunt de l'aigua.** | `EnemyBullet.FixedUpdate`, `TileNav.IsBlocked(ignoreWater)` | Les bales passen per l'aigua normal (BUG-025) però es paren amb els `DamageTile` (àcid i lava). Saber en quin ambient passa i si la Torreta ha de ser una excepció. |
| 3 | **El mercader i la màquina no tenen els mateixos preus.** | `Shop.cs`, `Stock` | El mercader cobra `base × (1 + 0,1/pis) × 0,85–1,15`; la màquina, `base × 0,5 × 0,8–1,2`. Decidir si han de ser iguals o si la màquina ha de ser més barata (5–15 monedes). |
| 4 | **La «Sorpresa!» del mercader ensenya l'arma.** | `Shop.cs` (`icon = w.icon`), `ShopUI.Fill` | Amagar icona i nom (un «?» o una caixa tancada) fins que es compre. |
| 5 | **Xat del mercader:** ix amb l'etiqueta en cru (`<color=#F2C94C>Mercader:</color>`) i es repetix. Ferran vol només la bafarada damunt d'ell. | `Shop.Say` (`NetSession.LocalNotice`) | Llevar la línia de xat i deixar `ChatBubble`. |
| 6 | **Mímic a la botiga:** es veia com a enemic i no com a cofre. | `EnemyV140` (`awake`/`remoteState`, radi d'1,1 de despertar) | Revisar si la còpia del client mostra la criatura abans d'hora o si el desperta la proximitat del mercader o la taula. |
| 7 | **Text de la sala secreta** («Paret esquerdada a prop!…»): Ferran el llevaria, la paret ja és intuïtiva. | `DungeonVariety.cs` (`Pista sala secreta`) | Llevar el `TextMeshPro` i deixar la paret amb l'esquerda. |
| 8 | **Troncs d'escorça** (`Tronc_pi`, `Tronc_betula`, `Tronc_escorca`): ocupen la casella però la hitbox és més xicoteta, i es trenquen. | `DungeonScenery.cs` (`MakeScenery`) | Posar troncs tombats (`Tronc_caigut`); si en queden de drets, hitbox sencera i no trencables. |
| 9 | **Creatiu: «TREURE ENEMICS» no lleva els fills dels bosses** (ni trossos de llimacs ni ullets). | `CreativeMode.cs` (`ClearEnemies`, només la llista `spawned`) | Recórrer tots els `EnemyHealth` vius, com fa `DestroyAllChests`, i avisar els clients (`NetCoop.EnemyDied`). |
| 10 | **Microbosses poc vistos:** Ferran ha arribat al pis 4 i no n'ha vist cap. Proposa que isquen **des del pis 2**. | `DungeonGenerator.cs` (bloc `Theme.microbosses` a la primera onada): ara només des del pis 3, en una sala de combat normal (no la de progressió) i amb un 25 % per sala; `V141ContentBuilder.cs`: el tema **Neu** no en té cap | Des del pis 2; garantir un per pis (una sala de combat) en lloc de la sort; afegir microbosses a la Neu; opcionalment una sala d'elit. |

## Ordre suggerit

Primer els arreglos xicotets i segurs (3, 4, 5, 7, 9), després els visuals (8), el contingut (10) i, per últim, els que cal reproduir en online (1, 2, 6).
