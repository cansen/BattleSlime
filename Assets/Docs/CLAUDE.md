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
- Interaction when player hits an object larger than itself (pushback implemented, final design TBD).

---

## Development Model

### Phase 1 — Core Prototype
1. Player movement + camera ✓
2. Soft body / jelly visuals ✓
3. Object collection + size growth ✓
4. Player collision + combat
   - **4a. System Setup** — collision detection architecture, layer/tag setup, trigger vs. physics collision decision, entry point for damage resolution
5. Shrinking ring

### Phase 2 — Multiplayer
6. Photon integration + session management
7. Sync player state (position, size, health)

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
- Check `read_console` after every script creation or modification for compile errors before proceeding.

## Unity Setup Requirements

- **Input System:** `InputSystem_Actions.inputactions` must have **Generate C# Class** enabled in the Inspector and Applied. Without this, all scripts using `InputSystem_Actions` will fail to compile.
- **Ground Layer:** Create a `Ground` layer in Unity and assign it to the arena plane. Set the same layer in `PlayerMovement → Ground Layer` on the Player. Required for jump to work at any size.

---

## Key Design Decisions

- Player characters are jelly-like, semi-transparent spheres with distinct colors. Every player entity must have `JellyEffect` and `PlayerStats` components.
- Objects spawned procedurally at game start via `ObjectSpawner`. No respawn.
- Collection rule: player can only collect objects smaller than `playerCurrentSize`. Larger objects push the player back.
- One large flat arena for prototype. Arena layout is content — no code work needed.
- Online multiplayer via Photon Engine, up to 8 players.
- Win condition: last player standing. Shrinking ring deals damage outside the safe zone.
- All tunable parameters exposed via Unity Inspector — never hardcode values.
