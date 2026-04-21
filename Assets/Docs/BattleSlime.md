## 0\. Description

Battle Slime is an online .io battle royale game (up to 8 players, Photon Engine) where the player's character grows by collecting various objects in the environment and competes with other players based on character size. Last player standing wins.

* The game is controlled with a controller or keyboard and mouse.  
* It is displayed and played from a 3rd person camera perspective.  
* The challenge and fun factor of the game comes from growing the character by collecting objects in the environment and competing with other players using the size advantage.
* All player characters are jelly-like semi-transparent entities with distinct colors, using Soft Body Physics.
* The arena is a single large flat plane. The playable area shrinks periodically — players outside take constant damage (battle royale ring).
* Collectible objects do not respawn for the prototype.

## 1. Controls

* The player's character can be controlled with both keyboard/mouse and gamepad.  
* The player controls the camera with the right analog stick.  
* The player moves in the relevant direction at **Player Movement Speed** using the analog stick.  
  * **Player Movement Speed** \= playerBaseMovementSpeed / (1 + (playerCurrentSize - 1) \* playerSizeMovementConstant)
    * Larger players move slower — size acts as a penalty divisor.  
  * After the player moves, they continue in the same vector at **playerSlideSpeed**, decelerating by **playSlideDamping**.  
* By pressing the X button, the player reaches **playerJumpHeight** height within **playerJumpHeightReachDuration**. The fall occurs under gravity.  
* Pressing L2 stops the jumping action within **instantStopDuration**.  
* When R2 is pressed, the character moves in the relevant direction at **Character Dash Speed**.  
  * **Character Dash Speed** \= **playerMovementSpeed** \* **playerDashMultiplier** 

## 2. Character

The character grows by the amount of objects it collects from the environment.

### Visual
* All player characters are represented as jelly-like, semi-transparent spheres with distinct colors per player.
* Characters use Soft Body Physics — the mesh deforms dynamically on movement, collision, and landing. Implemented via the `JellyEffect` component, which must be present on every player entity.
* **Known limitation:** The Rigidbody collider is a fixed shape. During a wobble, a second collision resolves against the original collider shape, not the deformed visual — causing a visual mismatch.
  * **Proposed fix:** Have `JellyEffect` update the `SphereCollider` radius in sync with the XZ scale deformation after each frame. The collider will follow the visual roughly, keeping physics and appearance consistent without the cost of a dynamic mesh collider. This is deferred post-prototype.
* Scale increases uniformly as the character grows (**playerCurrentSize** drives the transform scale).

* The character can grow up to **playerMaxSize** and cannot exceed this number.  
* The character grows by a specific **playerSizeIncrement** for each object collected.  
  * The growth amount each collected object contributes is **objectSizeValue**, defined per object.  
* When characters collide, the colliding characters interact in various ways based on the **playerMass** difference between their sizes and the player's instantaneous force value. These two values determine the character's momentum.  
  * The following scenarios occur as a result of comparing the mass and speed of interacting players. Based on the momentum difference:  
    * If it exceeds **playerDestructionThreshold**, the higher-value character destroys the lower-value character.  
    * The momentum of two interacting characters is calculated and compared with the following formula: Momentum= (M\*m) \* (V\*m(+m)). All characters lose the game if they fall below **playerInstantDeathSize** for any reason.  
    * The damage dealt between interacting characters is processed via the referenced Excel file: [BS Calc Sheet](https://docs.google.com/spreadsheets/d/1LdOaqqpMqNfyMIsS0osgUWegJjdbXir-9Gilp8yY0xs/edit?gid=0#gid=0)  
      * Whenever players collide with each other in any way, the damage taken is displayed numerically above the player's character for **playerDamageIndicatorDuration** and then disappears.  
      * When a larger character collides with a smaller character, the **playerCollisionForce** value calculated from the size/mass difference between the large and small character is applied to the smaller character.

## 2b. Game Modes

### Battle Royale
The primary game mode. A match has a finite, parametric length (`matchDuration`). A shrinking ring defines the safe playable area.

- After match start, a ring with radius `ringStartRadius` becomes the boundary.
- Every `ringShrinkInterval` seconds, the ring radius shrinks by `ringShrinkAmount`.
- The ring shrinks until it reaches `ringMinRadius`.
- Players outside the ring take `ringDamagePerSecond` damage per second.
- If ring damage in a single tick equals or exceeds `playerDestructionThreshold`, the player dies instantly.
- Last player standing wins.

| Parameter | Default Value | Notes |
|---|---|---|
| **matchDuration** | 120 | Total match length in seconds |
| **ringStartRadius** | 150.0 | Starting ring radius |
| **ringShrinkInterval** | 30 | Seconds between each shrink step |
| **ringShrinkAmount** | 50 | Radius reduction per shrink step — hits minimum at ~90s, final 30s at minimum radius |
| **ringMinRadius** | 15.0 | Minimum ring radius |
| **ringDamagePerSecond** | 5.0 | Damage per second while outside ring |
| **playerDestructionThreshold** | 50 | Shared with combat — instant death if damage ≥ this value |

> Additional game modes may be defined in future sections.

## 3. Environment

Players grow by collecting objects in three different sizes in a wide, flat area with various walls.

* All objects are defined with the **objectConsumable** property, meaning they can be destroyed.  
* All objects scale the player's 3D model by **objectSizeIncrement** at a rate of **playerSizePercentage**.  
* There is a shrinking ring in the area.  
  * When outside this ring, the player takes **playerDeathDurationHpDecrement** damage.  
  * After **circleStartTime**, a ring with a radius of **maxCircleRadius** becomes active.  
  * The ring shrinks over **circleShrinkTime** until it reaches a radius of **minCircleRadius**.

## 4. Objects

There are 3 different types of objects in different sizes (small, medium, large). The character grows when collecting these objects.

### Spawning
- Objects are spawned procedurally at game start by an `ObjectSpawner`.
- Spawn count, area bounds, and size value range are inspector-tunable.
- Objects are placed randomly within the arena bounds on the ground plane.

### Collection Rule
- Each object has a per-instance `objectSizeValue` — a magic number set in the Inspector.
- A player can only collect an object if `playerCurrentSize > objectSizeValue`.
- If `playerCurrentSize <= objectSizeValue`, the player is pushed back away from the object.

### On Collection
- Object is destroyed.
- Player's `playerCurrentSize` increases by the object's `objectSizeValue`.
- Visual scale maps non-linearly via cube root: `visualScale = playerCurrentSize ^ (1/3)`. This makes early growth feel gradual and large sizes require many more pickups. Object visual scale follows the same formula.

### On Collection Blocked
- A pushback force is applied to the player's Rigidbody in the direction away from the object.
- Pushback force magnitude is inspector-tunable.

| Parameter | Default Value | Notes |
|---|---|---|
| **objectSizeValue** (small) | 200 | Per object, inspector-tunable |
| **objectSizeValue** (medium) | 1000 | Per object, inspector-tunable |
| **objectSizeValue** (large) | 3000 | Per object, inspector-tunable |
| **objectSizeIncrement** | 0.1 | Scale added per collection |
| **objectConsumable** | true | All objects are consumable |

## 4b. Default Parameter Values

All values are exposed via Unity Inspector and subject to tuning.

### Movement
| Parameter | Default Value |
|---|---|
| playerBaseMovementSpeed | 5.0 |
| playerSizeMovementConstant | 0.05 |
| playerSlideSpeed | 3.0 |
| playSlideDamping | 4.0 |
| playerDashMultiplier | 2.5 |

### Jump
| Parameter | Default Value |
|---|---|
| playerJumpHeight | 2.0 |
| playerJumpHeightReachDuration | 0.4 |
| instantStopDuration | 0.1 |

### Character / Size
| Parameter | Default Value |
|---|---|
| playerMaxSize | 100000 |
| playerCurrentSize | 1000 |
| playerInstantDeathSize | 300 |
| playerSizePercentage | 1.0 |

### Combat
| Parameter | Default Value |
|---|---|
| playerDestructionThreshold | 50000 |
| playerDamageIndicatorDuration | 1.5 |
| playerCollisionForce | 10000 |

### Arena / Ring
| Parameter | Default Value |
|---|---|
| circleStartTime | 120.0 (2 min) |
| maxCircleRadius | 150.0 |
| circleShrinkTime | 180.0 (3 min) |
| minCircleRadius | 15.0 |
| playerDeathDurationHpDecrement | 5.0 per second |

## 4c. Combat System Setup

Before any combat logic is implemented, the following must be decided and configured:

- **Collision detection method** — physics collision (`OnCollisionEnter`) vs. trigger (`OnTriggerEnter`)
- **Layer/tag configuration** — which layers interact with which (Player vs. Player, Player vs. Object)
- **Entry point** — which script owns the collision event and routes to damage resolution
- **Damage application** — damage reduces `playerCurrentSize` directly and instantly
- **Elimination condition** — player is eliminated when `playerCurrentSize` falls below `playerInstantDeathSize`
- **PvE rule** — player larger than object → absorption (no formula). Player smaller than object → response TBD (currently pushback only).

---

## 5\. For future

* Characters that do not reach the desired size within a certain time limit lose (instant death or dot).  
* During a collision, the character with higher momentum deals damage first.  
* Interaction when encountering objects larger than oneself?? (hp loss?)

## 6. References

[https://store.steampowered.com/app/1050290/Blobio/](https://store.steampowered.com/app/1050290/Blobio/)  
[https://store.steampowered.com/app/848350/Katamari\_Damacy\_REROLL/](https://store.steampowered.com/app/848350/Katamari_Damacy_REROLL/)  
[https://store.steampowered.com/app/578080/PUBG\_BATTLEGROUNDS/](https://store.steampowered.com/app/578080/PUBG_BATTLEGROUNDS/)
