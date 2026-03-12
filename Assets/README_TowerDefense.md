# 2D Tower Defense Game Template

A complete template for creating a 2D tower defense game in Unity.

## Project Structure

```
Assets/
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs          # Main game state management
│   │   ├── CurrencyManager.cs      # Currency system
│   │   └── WaveManager.cs          # Wave spawning system
│   ├── Tower/
│   │   ├── Tower.cs                # Tower behavior
│   │   ├── TowerPlacement.cs       # Tower placement system
│   │   └── TowerData.cs            # Tower ScriptableObject
│   ├── Enemy/
│   │   ├── Enemy.cs                # Enemy behavior
│   │   ├── EnemyPath.cs            # Path system for enemies
│   │   └── EnemyData.cs            # Enemy ScriptableObject
│   ├── Projectile/
│   │   └── Projectile.cs           # Projectile behavior
│   └── UI/
│       ├── GameUI.cs               # Main game UI
│       ├── TowerSelectionUI.cs     # Tower selection buttons
│       └── TowerButtonUI.cs        # Individual tower button
```

## Setup Instructions

### 1. Create Game Manager GameObject

1. Create an empty GameObject named "GameManager"
2. Add the following components:
   - `GameManager`
   - `CurrencyManager`
   - `WaveManager`
3. Configure GameManager:
   - Set Starting Lives (e.g., 20)
   - Set Starting Currency (e.g., 100)

### 2. Create Enemy Path

1. Create an empty GameObject named "EnemyPath"
2. Add the `EnemyPath` component
3. In the Inspector, add waypoints to define the path enemies will follow
4. Waypoints are in local space relative to the EnemyPath transform

### 3. Setup Wave Manager

1. On the GameManager GameObject, configure WaveManager:
   - Assign the Spawn Point (create an empty GameObject at the start of your path)
   - Create Wave ScriptableObjects or configure waves in the Inspector
   - Each wave contains:
     - List of enemies to spawn
     - Time between spawns
     - Time before next wave

### 4. Create Tower Data Assets

1. Right-click in Project window → Create → Tower Defense → Tower Data
2. Configure each tower:
   - Name, description, icon
   - Cost, range, fire rate, damage
   - Tower prefab (create this next)
   - Projectile prefab (create this next)

### 5. Create Tower Prefab

1. Create a GameObject with:
   - SpriteRenderer (for visual)
   - Tower script
   - CircleCollider2D (for range detection)
2. Create a child GameObject for the turret head (optional, for rotation)
3. Create a child GameObject for the fire point (where projectiles spawn)
4. Assign the TowerData ScriptableObject
5. Save as prefab

### 6. Create Enemy Data Assets

1. Right-click in Project window → Create → Tower Defense → Enemy Data
2. Configure each enemy:
   - Name, sprite
   - Max health, move speed
   - Currency reward
   - Damage to player

### 7. Create Enemy Prefab

1. Create a GameObject with:
   - SpriteRenderer
   - Enemy script
   - Collider2D (for projectile detection)
2. Assign the EnemyData ScriptableObject
3. Save as prefab

### 8. Create Projectile Prefab

1. Create a GameObject with:
   - SpriteRenderer (small sprite)
   - Projectile script
   - Collider2D (set as trigger)
   - Rigidbody2D (set to Kinematic)
2. Save as prefab

### 9. Setup Tower Placement

1. Create an empty GameObject named "TowerPlacement"
2. Add the `TowerPlacement` component
3. Configure layers:
   - Placement Layer: Where towers can be placed
   - Blocked Layer: Where towers cannot be placed

### 10. Setup UI

1. Create a Canvas (UI → Canvas)
2. Create UI elements:
   - Currency text (TextMeshPro)
   - Lives text (TextMeshPro)
   - Wave text (TextMeshPro)
   - Enemies remaining text (TextMeshPro)
3. Add `GameUI` component to a GameObject and assign all text references

4. Create Tower Selection Panel:
   - Create a Panel for tower buttons
   - Create a button prefab with:
     - Image (for tower icon)
     - TextMeshPro (for name and cost)
     - Button component
     - `TowerButtonUI` component
   - Add `TowerSelectionUI` component and assign:
     - Button parent (the panel)
     - Button prefab
     - Available towers array (drag your TowerData assets)

### 11. Layer Setup

1. Go to Edit → Project Settings → Tags and Layers
2. Create layers:
   - "Enemy" (for enemies)
   - "Tower" (for towers)
   - "Placement" (for valid placement areas)
   - "Blocked" (for blocked placement areas)

### 12. Physics2D Setup

1. Ensure enemies are on the "Enemy" layer
2. Ensure towers can detect enemies (set enemyLayer in Tower script)

## Features

- **Tower System**: Place towers, automatic targeting, projectile firing
- **Enemy System**: Path following, health, currency rewards
- **Wave System**: Configurable waves with multiple enemy types
- **Currency System**: Earn currency by defeating enemies, spend to place towers
- **UI System**: Display currency, lives, wave info, tower selection
- **Game State**: Lives system, game over, victory conditions

## Customization

- Create multiple TowerData ScriptableObjects for different tower types
- Create multiple EnemyData ScriptableObjects for different enemy types
- Adjust wave configurations in WaveManager
- Customize UI layout and styling
- Add special effects, sounds, and animations

## Tips

- Use the EnemyPath gizmos in Scene view to visualize and edit paths
- Tower range indicators are hidden by default (can be shown on selection)
- Projectiles automatically track their target
- Enemies automatically follow the path defined by EnemyPath waypoints
