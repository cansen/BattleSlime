# BattleSlime — Claude Code Instructions

## Project
3D .io game (Unity 6 URP). Players grow their character by collecting objects in the environment and compete with other players based on character size.
Signature mechanics: **Growth** (collecting objects scales the character up) and **Combat** (momentum-based collisions between players determine damage and destruction). Larger players move slower — speed = baseSpeed / (1 + (size - 1) * constant).

**Docs:** `Assets/Docs/` (GDD in BattleSlime.md)
**Repo:** `https://github.com/cansen/BattleSlime.git`
**User style:** Direct and brief — no fluff. Tests things independently and returns with specific issues. Match this tone.
**Platforms:** TBD
**Controls:** Keyboard/Mouse — 3rd person camera (controller support in Phase 3)

---

## Documentation Rule

Any change made during prototyping — mechanic, parameter, system, visual, or architectural — must be reflected in **both** `CLAUDE.md` and `BattleSlime.md` before moving on. If a change conflicts with existing content in either document, flag it to the user immediately before proceeding.

All future and performance related concerns must be noted in the **Future Concerns** section. Do not scatter them across other sections.

---

## Future Concerns

### Pending Design Work
- **Full numeric rescale required** — starting sizes, object sizes, movement constants, and combat formula params (DmgC, DmgM) were designed at different scales and do not produce meaningful results together. Needs a unified pass once core mechanics are validated.
- **objectSizeValue must be coupled to actual object scale** — currently set independently in the Inspector. Needs a parametric link so visual size and gameplay size value stay in sync. Deferred, revisit after numeric rescale.

### Performance
- **`JellyEffect` runs every frame** — trivial at 8 players, monitor if AI bots are added.
- **SphereCollider sync** — updating collider mid-simulation triggers physics recalculations. Benchmark when player count scales.
- **Profiler checkpoint:** Run after Photon integration. Network-syncing transform and scale changes is the most likely performance bottleneck.

### Known Limitations
- **Collider vs. visual mismatch during wobble** — second collision mid-wobble resolves against original collider shape. Fix already implemented: `SphereCollider` radius synced with XZ scale in `JellyEffect`.
- **Non-sphere shapes break collider sync** — `SyncColliderRadius()` only works for `SphereCollider`. Player shape must remain a sphere for the prototype.

### Deferred Features
- Visual mesh can be swapped to a more stylized shape post-prototype without changing the collider.
- Object respawn system.
- **Size/mass coupling** — design principle confirmed (size proportional to mass), but `Rigidbody.mass` is not yet set dynamically. Needs a future pass, likely alongside the numeric rescale.
- **Mini sphere networking** — mini spheres currently spawn via `Instantiate()`, not `Runner.Spawn()`. In multiplayer, only the authority client sees them. Needs `CollectibleObjectPrefab` registered in Fusion's NetworkProjectConfig and `Runner.Spawn()` restored to sync across clients.

---

## Development Model

### Phase 1 — Core Prototype
1. Player movement + camera ✓
2. Soft body / jelly visuals ✓
3. Object collection + size growth ✓
4. Player collision + combat ✓
5. Shrinking ring — **Battle Royale** game mode
   - Finite match length (`matchDuration` = 120s)
   - Ring starts at `ringStartRadius`, shrinks by `ringShrinkAmount` every `ringShrinkInterval` seconds
   - Ring shrinks until it reaches `ringMinRadius`
   - Players outside the ring take `ringDamagePerSecond` damage per second
   - If ring damage in a tick meets or exceeds `playerDestructionThreshold` (shared with combat), player dies instantly
   - Game mode tagged **Battle Royale** — additional game modes may be added later

### Phase 2 — Multiplayer ✓
6. Photon integration + session management ✓
7. Sync player state (position, size, health) ✓

### Phase 3 — Polish
8. UI (health, size indicator, player count)
9. Playtesting + parameter tuning
10. Controller support

---

## Architecture Principles

- **No singletons.** Use dependency injection and events instead.
- All `GameLogic` and `Architecture` code must be **pure C# — no Unity dependencies.**
- Only the rendering/view layer (`Rendering/`, `UI/`) may depend on Unity APIs.

---

## TDD Approach

- **EditMode tests** — pure C# logic. Fast, no scene required.
- **PlayMode tests** — only when Unity runtime is needed (physics, scene transitions).
- Test naming: `MethodName_StateUnderTest_ExpectedBehavior`

---

## Code Guidelines

Based on Clean Code (Robert C. Martin).

### Naming
- Full words only — no abbreviations, no Hungarian notation.
- No underscore prefix on private fields. Use `this` when disambiguation is needed.
- Classes: **PascalCase** noun. Methods: **PascalCase** verb first. Variables: **camelCase**. Constants: **UPPER_CASE**.

### Structure
- Always explicit access modifiers.
- Curly braces on a **new line**, always — including single-line bodies.
- 4 spaces per indent. Max **3 indent levels**, max **15 lines**, max **3 parameters** per method.
- Single Responsibility — one class, one job.
- No `var` except for anonymous types.
- No explanatory comments — refactor to make code self-explanatory.
- No empty `Start()` or `Update()` methods.

### Unity-Specific
- Cache `GetComponent`, `transform`, `Camera.main` — never call in `Update`.
- Use `CompareTag("X")` not `go.tag == "X"`.
- Avoid `FindObject*` — use `OnEnable`/`OnDisable` registration instead.
- Avoid `foreach` on hot paths — use `for`.
- Prefer `sqrMagnitude` over `magnitude` for comparisons.
- Use object pooling for frequently instantiated/destroyed objects.

---

## Folder Structure

All project assets live under `Assets/Project/`. Each file type has its own subfolder: `Scripts/`, `Prefabs/`, `Materials/`, `Textures/`, `Audio/`, `Animations/`, `Shaders/`, `Scenes/`. Third-party packages (Photon, etc.) stay outside `Assets/Project/`. All filenames: **PascalCase**, no spaces, no abbreviations.

---

## Git Conventions

Format: `<type>(<scope>): <subject>` — present tense, max 70 chars.

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `asset`, `scene`, `shader`, `anim`, `prefab`, `config`

Rules: small single-purpose commits, never commit generated files (Library/, Temp/), never add `Co-Authored-By`.

---

## Tools

- **UnityMCP** — active via HTTP at `http://127.0.0.1:8080/mcp`. Use for scene/object/script operations.
- **CoPlay MCP** — use in Phase 3 for `generate_or_edit_images`, `generate_sfx`, `generate_music`.
- After every script creation or modification, use MCP to check for compile errors and notify the user before proceeding. Do not assume compilation succeeded.

## Unity Setup Requirements

- **Input System:** `InputSystem_Actions.inputactions` must have **Generate C# Class** enabled in the Inspector and Applied. Without this, all scripts using `InputSystem_Actions` will fail to compile.
- **Ground Layer:** Create a `Ground` layer in Unity and assign it to the arena plane. Set the same layer in `PlayerMovement → Ground Layer` on the Player. Required for jump to work at any size.

---

## Key Design Decisions

- Player characters are jelly-like, semi-transparent spheres with distinct colors. Every player entity must have `JellyEffect` and `PlayerStats` components.
- Objects spawned procedurally at game start via `ObjectSpawner`. No respawn.
- Collection rule:
  - Player size > collectible size → player eats it, grows by `objectSizeValue`, no damage to player.
  - Player size ≤ collectible size, collectible stationary → combat formula kicks in, collectible velocity = `stationaryVelocity` (default 0.1). Only player takes damage. Collectible stays in scene. Knockback applied proportional to `max(collectibleMomentum - playerMomentum, 0) * knockbackForce`.
  - Player size ≤ collectible size, collectible moving → full PvP formula with actual Rigidbody velocity. Only player takes damage. Collectible stays in scene. Same momentum-proportional knockback.
- Whenever damage is dealt (PvP, PvC, or ring), `TakeDamage` spawns up to `maxMiniSpheresPerHit` (default 10) mini spheres via `Instantiate()`. Each carries `damage / numSpheres` size value, despawns after 10s, has `canDamagePlayer = false`, and is launched outward with `miniSphereScatterForce`. A `miniSphereGraceDuration` (default 1.5s) prevents any player from collecting them immediately after spawn.
- **Design principle:** Size is proportional to mass. Not yet wired in Unity physics — expect a future tuning pass.
- One large flat arena for prototype. Arena layout is content — no code work needed.
- Online multiplayer via Photon Engine, up to 8 players.
- Win condition: last player standing. Shrinking ring deals damage outside the safe zone.
- All tunable parameters exposed via Unity Inspector — never hardcode values.
