# Tower Info Panel Setup Guide

## How to Set Up the Tower Info Panel UI

### Step 1: Create the Info Panel

1. **In Unity Hierarchy**, right-click your **Canvas** → **UI → Panel**
2. Name it **"TowerInfoPanel"**
3. Position and size it where you want the tower info to appear (e.g., top-right corner)

### Step 2: Create Text Elements

For each piece of information, create a TextMeshPro text element:

1. **Right-click the TowerInfoPanel** → **UI → Text - TextMeshPro**
2. Name each text element:
   - **TowerNameText** (for tower name)
   - **DescriptionText** (for description)
   - **CostText** (for cost)
   - **RangeText** (for range)
   - **FireRateText** (for fire rate)
   - **DamageText** (for damage)
   - **ProjectileSpeedText** (for projectile speed)

3. **Position each text** where you want it in the panel
4. **Set the text content** (it will be updated automatically by the script)

### Step 3: Create Close Button (Optional)

1. **Right-click the TowerInfoPanel** → **UI → Button - TextMeshPro**
2. Name it **"CloseButton"**
3. Position it (e.g., top-right corner of the panel)
4. Set the button text to "X" or "Close"

### Step 4: Assign References to TowerInfoUI Script

1. **Select the GameObject** that has the **TowerInfoUI** component
2. In the Inspector, find the **Tower Info UI** component
3. **Drag and drop** each element into the corresponding field:
   - **Info Panel** → Drag the TowerInfoPanel GameObject
   - **Tower Name Text** → Drag the TowerNameText GameObject
   - **Description Text** → Drag the DescriptionText GameObject
   - **Cost Text** → Drag the CostText GameObject
   - **Range Text** → Drag the RangeText GameObject
   - **Fire Rate Text** → Drag the FireRateText GameObject
   - **Damage Text** → Drag the DamageText GameObject
   - **Projectile Speed Text** → Drag the ProjectileSpeedText GameObject
   - **Close Button** → Drag the CloseButton GameObject (if you created one)

### Step 5: Set Up Panel Toggle

1. In the **Tower Info UI** component, find **Panel Toggle** section
2. **Tower Selection Panel** → Drag the GameObject that contains your tower selection buttons
   - This is usually the GameObject with the **TowerSelectionUI** component
   - Or the parent GameObject that contains the button parent

### Quick Setup Example

**Hierarchy Structure:**
```
Canvas
├── TowerInfoPanel (Panel)
│   ├── TowerNameText (TextMeshPro)
│   ├── DescriptionText (TextMeshPro)
│   ├── CostText (TextMeshPro)
│   ├── RangeText (TextMeshPro)
│   ├── FireRateText (TextMeshPro)
│   ├── DamageText (TextMeshPro)
│   ├── ProjectileSpeedText (TextMeshPro)
│   └── CloseButton (Button)
└── TowerSelectionPanel (or your selection UI)
    └── ButtonParent
        └── (tower buttons)
```

### Notes

- The text elements will automatically update when you click on a tower
- If you don't assign a text field, that information won't be displayed (no error)
- The panel will automatically show/hide when clicking towers or clicking elsewhere
- Make sure the panel is initially **hidden** (unchecked in Inspector) or the script will hide it on Start
