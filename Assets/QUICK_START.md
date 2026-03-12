# Quick Start Guide - Tower Defense Template

## 🚀 Getting Started (5-Minute Setup)

### Step 1: Open Unity and Your Scene
1. Open Unity Editor
2. Open `Assets/Scenes/SampleScene.unity` (or create a new 2D scene)

### Step 1.5: Add a Background (Optional)

**Method 1: Simple Background Sprite**
1. **Import your background image:**
   - Drag your background image into the Project window
   - Select it and set **Texture Type** to `Sprite (2D and UI)`
   - Click **Apply**

2. **Create the background GameObject:**
   - **GameObject → Create Empty** (or right-click Hierarchy → Create Empty)
   - Name it "Background"
   - Add component: **Sprite Renderer**
   - Assign your background sprite to the Sprite Renderer

3. **Position and scale the background:**
   - Select the Background GameObject
   - In **Transform** component:
     - **Position**: (0, 0, 0) - center of the scene
     - **Scale**: Adjust to fit your camera view (e.g., if camera size is 5, scale might be 10, 10, 1)
   - Or use **Sprite Renderer → Draw Mode → Tiled** for repeating backgrounds

4. **Set sorting order (so it renders behind everything):**
   - In **Sprite Renderer** component:
     - **Sorting Layer**: Default (or create a "Background" sorting layer)
     - **Order in Layer**: -10 (negative number = renders behind)

**Making Blocked Areas Visible (Render Above Background):**

If you have an empty GameObject on the "Blocked" layer that you want to see:

1. **Important: Blocked areas should NOT be inside Canvas!**
   - Canvas is only for UI elements (buttons, text, panels)
   - Blocked areas are game world objects
   - Create them as regular GameObjects in the main scene hierarchy (at the root level, not under Canvas)

2. **Add a SpriteRenderer component:**
   - Select your empty GameObject (should be at root level, not under Canvas)
   - **Add Component → Sprite Renderer**
   - Assign a sprite (you can use a simple square/circle sprite, or create one)

3. **Set it to render above background:**
   - In **Sprite Renderer** component:
     - **Sorting Layer**: Default (or same as background)
     - **Order in Layer**: 0 or higher (higher number = renders in front)
     - Background should have Order in Layer: -10
     - Blocked area should have Order in Layer: 0 or higher

4. **Visual example:**
   - Background: Order in Layer = -10 (renders behind)
   - Blocked area: Order in Layer = 0 (renders in front of background)
   - Enemies/Towers: Order in Layer = 1 or higher (renders in front of blocked areas)

**Hierarchy Structure:**
```
Scene Root
├── Canvas (UI only - buttons, text, panels)
├── Background (game world sprite)
├── BlockedArea1 (game world sprite - NOT in Canvas!)
├── BlockedArea2 (game world sprite)
├── GameManager
├── EnemyPath
└── TowerPlacement
```

**Note:** The "Blocked" layer (in Tags & Layers) is for physics/collision detection. The **Sorting Layer** and **Order in Layer** (in SpriteRenderer) control visual rendering order - these are different systems!

**Method 2: Using Camera Background Color**
- Select **Main Camera** in Hierarchy
- In **Camera** component, set **Background** color
- This is simpler but only gives a solid color, not an image

**Method 3: Create Sorting Layers (Recommended for complex scenes)**
1. **Edit → Project Settings → Tags and Layers**
2. Scroll to **Sorting Layers** section
3. Click **+** to add layers in order (bottom to top):
   - Background (renders first/behind)
   - Default (middle layer)
   - Foreground (renders last/in front)
4. Assign sprites to appropriate sorting layers

### Step 2: Use Quick Setup Tool (Easiest Way!)
1. In Unity menu: **Tools → Tower Defense → Quick Setup**
2. Click each button to create:
   - **Create Game Manager** - Sets up all managers
   - **Create Enemy Path** - Creates a basic path
   - **Create Tower Placement** - Sets up tower placement system

### Step 3: Import Your Own Sprites (Optional)

If you have custom sprite images (PNG, JPG, etc.):

1. **Create a Sprites folder** (optional but recommended):
   - In Project window, right-click `Assets/` → **Create → Folder**
   - Name it "Sprites"

2. **Import your images:**
   - Drag your image files into the `Assets/Sprites/` folder (or directly into `Assets/`)
   - Unity will automatically import them

3. **Configure sprite settings:**
   - Select your imported image in the Project window
   - In the **Inspector** window, you'll see import settings:
     - **Texture Type**: Change to `Sprite (2D and UI)`
     - **Sprite Mode**: 
       - `Single` = one sprite per image (most common)
       - `Multiple` = sprite sheet with multiple sprites
     - **Pixels Per Unit**: Usually 100 (adjust if your sprites are too big/small)
     - Click **Apply** button at the bottom

4. **Your sprite is now ready to use!** You can assign it to Sprite Renderer components.

**Supported formats:** PNG, JPG, TGA, BMP, GIF, PSD, TIFF

### Step 4: Create Your First Data Assets

#### Create Tower Data:
1. In Project window, right-click in `Assets/` folder
2. **Create → Tower Defense → Tower Data**
3. Name it "BasicTowerData"
4. Configure:
   - Cost: 50
   - Range: 3
   - Fire Rate: 1
   - Damage: 10
   - Projectile Speed: 5

#### Create Enemy Data:
1. Right-click → **Create → Tower Defense → Enemy Data**
2. Name it "BasicEnemyData"
3. Configure:
   - Max Health: 100
   - Move Speed: 2
   - Currency Reward: 10
   - Damage To Player: 1

### Step 4: Create Prefabs

#### Tower Prefab:

**Option A: Use Default Unity Shape**
1. **GameObject → 2D Object → Sprite → Square**
2. Name it "Tower"

**Option B: Use Your Own Sprite Image**
1. **Import your sprite image:**
   - Drag your image file (PNG, JPG, etc.) into the **Project** window (create an `Assets/Sprites/` folder first if you want)
   - Select the imported image in Project window
   - In the **Inspector**, set:
     - **Texture Type**: `Sprite (2D and UI)`
     - **Sprite Mode**: `Single` (or `Multiple` if it's a sprite sheet)
     - Click **Apply**
2. **Create GameObject with your sprite:**
   - **GameObject → Create Empty** (or right-click Hierarchy → Create Empty)
   - Name it "Tower"
   - Add component: **Sprite Renderer**
   - In Sprite Renderer component, click the circle next to "Sprite" field
   - Select your imported sprite from the picker
3. Continue with step 3 below...
3. Add components:
   - `Tower` script
   - `CircleCollider2D` (set as Trigger)
4. Create child GameObject "FirePoint" (empty, position where projectiles spawn)
5. In Tower script, assign:
   - Tower Data: Your BasicTowerData
   - Fire Point: The FirePoint child
6. **Create Prefab:**
   - In the **Hierarchy** window, find your "Tower" GameObject
   - **Click and drag** the "Tower" GameObject from the Hierarchy window
   - **Drop it** into the **Project** window (into a folder like `Assets/Prefabs/` if you have one, or just `Assets/`)
   - Unity will create a prefab file (blue cube icon) with the name "Tower"
   - The GameObject in Hierarchy will turn blue, indicating it's now a prefab instance
7. **Delete from scene:** Right-click the Tower in Hierarchy → Delete (or select and press Delete key)
   - The prefab remains in the Project window for later use

#### Enemy Prefab:

**Option A: Use Default Unity Shape**
1. **GameObject → 2D Object → Sprite → Circle**
2. Name it "Enemy"

**Option B: Use Your Own Sprite Image**
1. **Import your sprite image** (same process as Tower above)
2. **Create GameObject:**
   - **GameObject → Create Empty**
   - Name it "Enemy"
   - Add **Sprite Renderer** component
   - Assign your sprite to the Sprite Renderer
3. **Add Health Bar (Optional):**
   - Add Component → **EnemyHealthBar**
   - The health bar will be created automatically
   - Adjust **Offset** to position the bar above the enemy (default: 0, 1, 0)
   - Check **Show Health Text** to display numbers (e.g., "100/100")
4. Continue with step 3 below...
3. Add components:
   - `Enemy` script
   - `CircleCollider2D` (for collision)
4. In Enemy script, assign Enemy Data (will be set at runtime)
5. **Create Prefab:**
   - Drag the "Enemy" GameObject from **Hierarchy** to **Project** window
   - Creates a prefab file in your project
6. **Delete from scene:** Remove the Enemy GameObject from Hierarchy (the prefab stays in Project)

#### Projectile Prefab:

**Option A: Use Default Unity Shape**
1. **GameObject → 2D Object → Sprite → Circle**
2. Scale it down (Transform → Scale: 0.2, 0.2, 1)
3. Name it "Projectile"

**Option B: Use Your Own Sprite Image**
1. **Import your sprite image** (same process as above)
2. **Create GameObject:**
   - **GameObject → Create Empty**
   - Name it "Projectile"
   - Add **Sprite Renderer** component
   - Assign your sprite
   - Scale if needed (small projectiles work well at 0.2-0.5 scale)
3. Continue with step 3 below...
3. Add components:
   - `Projectile` script
   - `CircleCollider2D` (set as Trigger)
   - `Rigidbody2D` (set to Kinematic)
4. **Create Prefab:**
   - Drag the "Projectile" GameObject from **Hierarchy** to **Project** window
5. **Delete from scene:** Remove from Hierarchy (prefab remains in Project)

### Step 5: Link Prefabs to Data

1. Select your **BasicTowerData** asset
2. In Inspector, assign:
   - Tower Prefab: Your Tower prefab
   - Projectile Prefab: Your Projectile prefab
3. Select your **BasicEnemyData** asset
4. In Inspector, assign:
   - Enemy Prefab: Your Enemy prefab

### Step 6: Configure Wave Manager

1. **Select GameManager** in the Hierarchy window (left side)
   - The GameManager should have a child GameObject called "SpawnPoint"
   - If it doesn't exist, create it: Right-click GameManager → Create Empty → Name it "SpawnPoint"
   - Position SpawnPoint at the start of your enemy path (where enemies should first appear)

2. **Assign SpawnPoint to WaveManager:**
   - With **GameManager** still selected, look at the **Inspector** window (right side)
   - Find the **Wave Manager** component
   - You'll see a field called **"Spawn Point"** (it might say "None (Transform)")
   - **Drag and drop** the "SpawnPoint" GameObject from the Hierarchy window into this field
   - OR click the circle/target icon next to the field and search for "SpawnPoint"
   - The field should now show "SpawnPoint" instead of "None"

3. **Configure Waves:**
   - In the same WaveManager component, find the **Waves** list
   - Click the **+** button to add a new wave
   - Expand the new wave by clicking the arrow next to it
   - Click **+** in the **Enemies** list
   - Assign:
     - **Enemy Data**: Drag your BasicEnemyData ScriptableObject here
     - **Count**: Set to 5 (number of enemies to spawn)
   - Set **Time Between Spawns**: 1 (seconds between each enemy)
   - Set **Time Before Next Wave**: 5 (seconds to wait before next wave starts)

### Step 7: Setup Enemy Path

1. Select **EnemyPath** in Hierarchy
2. In Inspector, you'll see Waypoints list
3. Adjust waypoints to create your path (they're in local space)
4. Example path: (0,0) → (5,0) → (5,5) → (0,5)
5. You can see the path in Scene view (red line with spheres)

### Step 8: Setup UI (Basic)

1. **Create Canvas:**
   - **GameObject → UI → Canvas**
   - This creates a Canvas (for UI) and an EventSystem (for UI interactions)
   - The Canvas should fill your screen automatically

2. **Create TextMeshPro Text Elements:**

   **For Currency Text:**
   - Right-click on **Canvas** in Hierarchy → **UI → Text - TextMeshPro**
   - OR: **GameObject → UI → Text - TextMeshPro** (then drag it under Canvas)
   - Name it "CurrencyText"
   - In the **Rect Transform** component:
     - Click the anchor preset (top-left square) to anchor it to top-left
     - Set **Pos X**: 10, **Pos Y**: -10 (adjust for spacing from edge)
   - In **TextMeshPro - Text (UI)** component:
     - Set text to: "Currency: $100" (or whatever you want)
     - Adjust font size, color, etc. as desired

   **For Lives Text:**
   - Right-click on **Canvas** → **UI → Text - TextMeshPro**
   - Name it "LivesText"
   - Position it below CurrencyText:
     - Anchor to top-left
     - Set **Pos X**: 10, **Pos Y**: -40 (or adjust spacing)
   - Set text to: "Lives: 20"

   **For Wave Text:**
   - Right-click on **Canvas** → **UI → Text - TextMeshPro**
   - Name it "WaveText"
   - Position it at top-center:
     - Click anchor preset (top-center)
     - Set **Pos Y**: -10
   - Set text to: "Wave: 1/5"

   **Note:** The first time you create TextMeshPro, Unity may ask to import TMP Essentials. Click **Import TMP Essentials** if prompted.

3. **Create GameUI GameObject:**
   - Right-click in Hierarchy → **Create Empty**
   - Name it "GameUI"
   - Add Component → Search for "GameUI" → Add it

4. **Assign Text References:**
   - Select **GameUI** GameObject
   - In the **GameUI** component in Inspector:
     - Drag **CurrencyText** from Hierarchy to "Currency Text" field
     - Drag **LivesText** to "Lives Text" field
     - Drag **WaveText** to "Wave Text" field
     - Drag **EnemiesRemainingText** (if you created it) to "Enemies Remaining Text" field

### Step 9: Setup Tower Selection UI

1. Under Canvas, create **Panel** (name it "TowerSelectionPanel")
   - **Important:** Panels have a default white background that covers the screen!
   - To fix this: Select the Panel → In **Image** component → Set **Color** alpha to 0 (or click the color and set A to 0)
   - OR: Remove the Image component entirely if you don't need a background
   - **CRITICAL:** To allow clicking through the Panel to place towers:
     - Select the Panel
     - Add Component → **Canvas Group**
     - In Canvas Group component, **uncheck "Block Raycasts"**
     - This allows clicks to pass through the Panel to the game world
     - **Note:** Buttons and other interactive UI elements will still work because they have their own raycast blocking
2. Create as many **Button - TextMeshPro** children as you have tower types (e.g. "TowerButton1", "TowerButton2")
   - **Note:** TextMeshPro Button already has a TextMeshPro child with "Button" text - that's fine!
3. On each button:
   - Add `TowerButtonUI` component
   - In TowerButtonUI, assign **Tower Data** to the tower this button should place (e.g. BasicTowerData for the first button)
   - Add **Image** component (for tower icon) - if not already present
   - **Optional:** Drag the TextMeshPro child into **Name Text** and/or **Cost Text**; if left empty, the script shows name and cost in one label
4. Create empty GameObject "TowerSelectionUI"
5. Add `TowerSelectionUI` component
6. Assign **Button Parent** to your TowerSelectionPanel (the object that contains the tower buttons)

### Step 10: Configure Layers

1. **Edit → Project Settings → Tags and Layers**
2. Add layers:
   - Layer 8: "Enemy"
   - Layer 9: "Tower"
   - Layer 10: "Placement"
   - Layer 11: "Blocked"
3. Select your Enemy prefab, set Layer to "Enemy"
4. **Configure TowerPlacement:**
   - Select **TowerPlacement** GameObject in Hierarchy
   - In **Tower Placement** component:
     - **Placement Layer**: Can leave as "Nothing" (0) if you want to allow placement anywhere
     - **Blocked Layer**: Set to "Blocked" (Layer 11) - this prevents towers from being placed on blocked areas
     - **Important:** If you leave Blocked Layer as "Nothing" (0), towers can be placed anywhere
     - If you set it to "Blocked", you need to create GameObjects on the "Blocked" layer where towers shouldn't be placed

### Step 11: Test!

1. Press **Play**
2. Click a tower button in UI
3. Click on the scene to place tower
4. Enemies should spawn and follow the path
5. Towers should shoot at enemies!

---

## 📁 Project Navigation

### Key Folders:
- **Assets/Scripts/Managers/** - Core game systems
- **Assets/Scripts/Tower/** - Tower-related scripts
- **Assets/Scripts/Enemy/** - Enemy-related scripts
- **Assets/Scripts/Projectile/** - Projectile script
- **Assets/Scripts/UI/** - UI scripts

### Key Scripts to Understand:
1. **GameManager.cs** - Overall game state
2. **WaveManager.cs** - Controls enemy spawning
3. **Tower.cs** - How towers work
4. **Enemy.cs** - How enemies move and take damage
5. **TowerPlacement.cs** - How you place towers

### Workflow:
1. **Design Phase**: Create ScriptableObjects (TowerData, EnemyData)
2. **Art Phase**: Create prefabs with sprites
3. **Link Phase**: Connect prefabs to ScriptableObjects
4. **Setup Phase**: Configure managers and UI
5. **Test Phase**: Play and adjust!

---

## 🎯 Common Tasks

### Add a New Tower Type:
1. Create new TowerData ScriptableObject
2. Create new tower prefab (or duplicate existing)
3. Assign new TowerData to prefab
4. Create a new tower button under your tower UI panel (or duplicate an existing button), add TowerButtonUI if needed, and assign the new Tower Data to that button

### Tower Upgrades (Branching Tree):
1. **Create upgrade nodes:** Right-click in Project → **Create → Tower Defense → Tower Upgrade Node**. Each node defines: display name, cost, stat modifiers (flat add), optional **Sprite Override** (single sprite), optional **Animator Controller Override** (full Idle/Fire animations for the upgraded look – takes precedence over Sprite Override), optional projectile prefab override.
2. **Build the tree:** **Root Upgrade on Tower Data must always be the FIRST upgrade in the chain (level 2), not the last.** If you set Root Upgrade to level 3, the tower will jump from level 1 to level 3 in one click.
   - **One upgrade (level 1 → 2):** One node, assign as **Root Upgrade**, leave **Left Next** and **Right Next** empty.
   - **Two upgrades (level 1 → 2 → 3):** Create **level 2** node and **level 3** node. On Tower Data set **Root Upgrade = level 2 node**. On the **level 2** node set **Left Next** (or **Right Next**) = level 3 node. Leave level 3's Left/Right Next empty. First click buys level 2, second click buys level 3.
   - **Branches:** Set **Left Next** and **Right Next** on a node for two choices; use only one for a linear step. Leave both null for max tier.
3. **Link to tower:** On your **Tower Data** asset, assign **Root Upgrade** to the **first** upgrade node (the one the player buys first).
4. **UI:** In the Tower Info panel (TowerInfoUI), add upgrade buttons and assign them to TowerInfoUI:
   - **One choice (linear path):** Add a single **Button** and optional **Text** (label) inside the panel. Assign them to TowerInfoUI: **Upgrade Button**, **Upgrade Label**. When the tower has only one upgrade option, this button is shown and clicking it applies that upgrade. Leave Upgrade Left/Right empty.
   - **Two choices (branch):** Add **Upgrade Section** (e.g. empty GameObject), two **Buttons** (Upgrade Left / Upgrade Right), and optional **Text** for each. Assign: Upgrade Section, Upgrade Left Button, Upgrade Left Label, Upgrade Right Button, Upgrade Right Label. When the tower has two options, both buttons are shown; when it has one, the single **Upgrade Button** is used if assigned, otherwise the matching Left or Right button is shown.
   When a tower is selected, the panel shows available upgrades and cost; clicking applies the upgrade and refreshes stats and sprite.

### Animate a Tower (e.g. Opera Tower) – Idle + Fire
To play one animation while idle and a different animation when the tower fires. **Important:** Unity’s Animation window only enables **Add Property** when the **selected** GameObject has **both** an **Animator** (with a Controller assigned) and a **Sprite Renderer** on the same object. So we add both to the prefab root first (step 2), then create the clips (step 3).

1. **Sprite sheet ready:** Your tower sprite map (e.g. `opera-tower-sprite.png`) should be imported with **Sprite Mode: Multiple** and sliced in **Sprite Editor** so you have separate sprites (e.g. `opera-tower-sprite_0`, `_1`, …).

2. **Animator + Sprite Renderer on prefab (so Add Property works):**
   - In Project: right-click → **Create → Animator Controller**. Name it e.g. **OperaTower_Animator** (it can be empty for now).
   - Open your **opera_tower** prefab (double-click in Project). Select the **root** GameObject in the Hierarchy.
   - Ensure the root has **Sprite Renderer**: if it doesn’t, **Add Component → Sprite Renderer** (the Tower script adds one at runtime, but the Animation window needs it in the editor to show “Sprite Renderer → Sprite”). Assign a default sprite if you like (e.g. first frame).
   - Ensure the root has **Animator**: **Add Component → Animator** if needed. Assign **Controller** = **OperaTower_Animator**.
   - Keep the prefab root **selected** in the Hierarchy for the next step.

3. **Create the two Animation clips:**
   - In Project: right-click → **Create → Animation**. Name one **OperaTower_Idle** and one **OperaTower_Fire**.
   - Open **Window → Animation → Animation**. With the tower prefab root still selected, click the clip dropdown in the Animation window and choose **OperaTower_Idle** (or assign it if it’s not listed). **Add Property** should now be clickable.
   - Click **Add Property** → **Sprite Renderer** → **Sprite**. Add keyframes and assign the idle frame(s) (e.g. one sprite or a short loop).
   - In the Animation window dropdown, switch to **OperaTower_Fire**. Add property **Sprite Renderer → Sprite**, add keyframes with the firing sprites in order. Set **Sample** (e.g. 12) if needed.

4. **Wire up the Animator Controller:**
   - Double-click **OperaTower_Animator** to open the **Animator** window. Drag **OperaTower_Idle** and **OperaTower_Fire** into the Animator.
   - Set **Idle** as default: right-click Idle → **Set as Layer Default State**.
   - **Parameters** tab → **+** → **Trigger**, name it **Fire**.
   - **Idle → Fire:** Right-click Idle → **Make Transition** → click Fire. On the transition: **Conditions** add **Fire** (trigger).
   - **Fire → Idle:** Right-click Fire → **Make Transition** → click Idle. On the transition: enable **Has Exit Time** (Exit Time = 1), so when the fire clip ends it returns to Idle.

5. **Tower script:**
   - On the **opera_tower** prefab, **Tower** component: set **Tower Animator** to the **Animator component** on the same GameObject (drag the Animator component header from the Inspector, or drag the tower GameObject – not the .controller asset, or you’ll get a type mismatch). Set **Fire Trigger Name** to `Fire`.

6. **Test:** Enter Play mode, place the Opera tower; when it fires, the Fire trigger runs and the fire animation plays, then it returns to Idle.

### Add a New Enemy Type:
1. Create new EnemyData ScriptableObject
2. Create new enemy prefab (or duplicate existing)
3. In WaveManager, add enemy to a wave's enemy list

### Adjust Difficulty:
- **GameManager**: Change starting lives/currency
- **WaveManager**: Add more waves, more enemies per wave
- **TowerData**: Adjust cost, damage, range
- **EnemyData**: Adjust health, speed, rewards

---

## 🐛 Troubleshooting

**Animation window: "Add Property" is greyed out**
- The Animation window only enables Add Property when the **selected** GameObject has **both** an **Animator** (with a Controller assigned) and a **Sprite Renderer** on the **same** object. Fix: (1) Select your tower prefab root (double‑click prefab to open it). (2) Add **Sprite Renderer** if missing (Add Component → Sprite Renderer) – the Tower script adds one at runtime, but the editor needs it to show “Sprite Renderer → Sprite”. (3) Add **Animator** if missing and assign an Animator Controller. (4) In the Animation window, assign your clip – Add Property should become clickable.

**Animation window: "Add Property" turns white then grey again / unclickable**
- The Animation window only enables Add Property for whatever is **currently selected in the Hierarchy**. If you click the Scene view, another object, or the Project window, the tower prefab root gets deselected and Add Property goes grey again. Fix: click your **tower prefab root** in the Hierarchy again (the object that has Animator + Sprite Renderer). Keep that object selected while you work in the Animation window – don't click away. Then Add Property becomes clickable again.

**Upgrade not appearing on Tower Info UI**
- **Tower Data:** On the **Tower Data** asset used by the placed tower, set **Root Upgrade** to the upgrade node (for one upgrade level 1 → 2, that single node has **Left Next** and **Right Next** both empty). If Root Upgrade is empty, the tower has no upgrades and nothing will show.
- **One upgrade (level 1 → 2):** You only need **one** upgrade node. Assign it as **Root Upgrade** and leave its Left/Right Next empty. That node is shown as the only option; after you buy it, the tower is max tier.
- **Upgrade Section:** If your upgrade button lives inside a container (e.g. an empty GameObject), assign that container to TowerInfoUI **Upgrade Section**. The script turns this object on when the tower has upgrades; if it's not assigned, the container may stay off and the button stays hidden.
- **Assign at least one button:** On TowerInfoUI you must assign either **Upgrade Button** (for one choice) or **Upgrade Left Button** and/or **Upgrade Right Button** (for two choices). If the tower has upgrades but none of these are assigned, the Console will warn: "Tower has upgrades but no upgrade UI is assigned".
- **Check Console:** Enter Play, select a tower that should have upgrades. If you see the warning above, assign the missing references. If you see no warning but still no button, the tower likely has no **Root Upgrade** or the first node has no Left/Right Next.

**Enemies not spawning?**
- Check WaveManager has Spawn Point assigned
- Check waves are configured
- Check EnemyData has prefab assigned

**Towers not shooting?**
- Check Tower has TowerData assigned
- Check Fire Point is set
- Check enemies are on correct layer
- Check Tower's enemyLayer mask includes Enemy layer

**Can't place towers?**
- Check TowerPlacement layers are set
- Check you have enough currency
- Check TowerData has prefab assigned

**UI not updating?**
- Check GameUI has all text references assigned
- Check managers are in scene (GameManager GameObject)

---

## 📚 Next Steps

1. Add more tower types
2. Add more enemy types
3. Create better sprites/art
4. Add sound effects
5. Add particle effects
6. Add upgrade system
7. Add pause menu
8. Add game over/victory screens

## 🐛 Troubleshooting

### Canvas/UI Shows White/Milky Overlay

**Problem:** The game screen looks milky or has a white opaque layer covering everything.

**Solution:**
1. **Check for Panels with backgrounds:**
   - In Hierarchy, look for any **Panel** GameObjects under Canvas
   - Select the Panel
   - In Inspector, find the **Image** component
   - Set the **Color** alpha to 0 (click the color picker, set A slider to 0)
   - OR: Remove the Image component entirely if you don't need a background

2. **Check Canvas settings:**
   - Select the Canvas GameObject
   - Make sure **Render Mode** is set to "Screen Space - Overlay"
   - The Canvas itself shouldn't have a background color

3. **Check for other UI elements:**
   - Look for any Image components that might be covering the screen
   - Check their color/alpha values

**Quick Fix:** Select your Panel → Image component → Color → Set Alpha (A) to 0

### Can't Place Towers When Clicking

**Problem:** Can click tower button and see green preview, but clicking on the map doesn't place the tower.

**Solutions:**

1. **Check TowerPlacement layers:**
   - Select **TowerPlacement** GameObject
   - In **Tower Placement** component:
     - **Blocked Layer**: If set to "Everything" or wrong layer, change it to "Nothing" (0) to allow placement anywhere
     - OR set it to "Blocked" layer and make sure you're not clicking on blocked areas

2. **Check if clicking on UI:**
   - Make sure you're clicking on the game world, not on UI elements
   - The script prevents placement when clicking on UI

3. **Check currency:**
   - Make sure you have enough currency to buy the tower
   - Check the Console (Window → General → Console) for "Not enough currency!" messages

4. **Check camera:**
   - Make sure Main Camera is tagged as "MainCamera"
   - The script needs to find the camera to convert mouse position to world position

5. **View Debug Logs:**
   - **Open Console Window:** Window → General → Console (or press Ctrl+Shift+C / Cmd+Shift+C)
   - **Keep it open while playing:** The Console shows all Debug.Log messages in real-time
   - **When you click a tower button:** Look for `[TowerPlacement] StartPlacement called`
   - **When you click to place:** Look for `[TowerPlacement] Left mouse button clicked`
   - **Check the messages:** They'll tell you exactly why placement failed:
     - "Click detected on UI element" = clicking on UI instead of game world
     - "Position is blocked" = something is in the way
     - "Cannot afford tower" = not enough currency
     - "Main camera is null" = camera issue
   - **All logs are prefixed with `[TowerPlacement]`** so they're easy to find

**Quick Fix:** Set TowerPlacement → Blocked Layer to "Nothing" (0) to allow placement anywhere for testing.

### Blocked Areas Not Detected / Preview Not Turning Red

**Problem:** Blocked areas don't turn the preview red, and towers can still be placed on them.

**Solution - Check Blocked Area Setup:**

1. **Blocked area GameObject needs:**
   - **Layer**: Set to "Blocked" layer (Layer 11, or whatever layer you created)
   - **Collider2D component**: Must have a Collider2D (BoxCollider2D, CircleCollider2D, etc.)
   - **Collider2D must be enabled**: Check the checkbox in the Collider2D component
   - **Collider2D should NOT be a trigger**: Uncheck "Is Trigger" (unless you want it to be a trigger)

2. **TowerPlacement component needs:**
   - **Blocked Layer**: Must be set to "Blocked" layer (not "Nothing")
   - Check the layer mask value matches your Blocked layer number

3. **Debug steps:**
   - Open Console window
   - Hover over a blocked area
   - Look for logs showing colliders found
   - Check if the collider's layer matches the blocked layer mask

4. **Common issues:**
   - Blocked area has no Collider2D → Add one!
   - Collider2D is disabled → Enable it!
   - GameObject is on wrong layer → Set to "Blocked" layer
   - TowerPlacement Blocked Layer is set to "Nothing" → Set to "Blocked" layer
   - Collider2D is too small → Make sure it covers the area you want blocked

### How to View Debug Logs While Playing

1. **Open Console Window:**
   - Menu: **Window → General → Console**
   - Keyboard shortcut: **Ctrl+Shift+C** (Windows) or **Cmd+Shift+C** (Mac)

2. **Keep Console Open:**
   - Dock it to a tab or keep it as a separate window
   - It will update in real-time while the game is playing

3. **Filter Logs:**
   - Use the filter buttons at the top:
     - **Clear** = Remove all logs
     - **Collapse** = Group duplicate messages
     - **Clear on Play** = Auto-clear when starting play mode
   - Click on a log message to see more details

4. **Log Types:**
   - **White/Black** = Debug.Log (normal info)
   - **Yellow** = Debug.LogWarning (warnings)
   - **Red** = Debug.LogError (errors - these pause the game)

5. **What to Look For:**
   - When you click a tower button, you should see: `[TowerPlacement] StartPlacement called`
   - When you click to place, you should see: `[TowerPlacement] Left mouse button clicked`
   - Then check what happens next - the logs will tell you exactly where it fails!

Happy game making! 🎮
