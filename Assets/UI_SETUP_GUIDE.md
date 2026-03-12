# UI Positioning Guide - Fixed Positions Relative to Background

If your UI panels and texts are moving around when the window size changes, follow these steps to fix them:

## Quick Fix: Use UIPositionHelper Script

1. **Select each UI element** (panels, texts, buttons) that should stay fixed
2. **Add the `UIPositionHelper` component** to each element
3. **Set the Anchor Position** (0-1 coordinates):
   - `0, 0` = Bottom-left corner
   - `0.5, 0.5` = Center
   - `1, 1` = Top-right corner
   - `0, 1` = Top-left corner
   - `1, 0` = Bottom-right corner
4. **Set Pixel Offset** to move the element from the anchor point
5. The element will now stay in a fixed position relative to the screen

## Manual Setup (Alternative)

### Step 1: Configure Canvas Scaler

1. **Select your Canvas** in the Hierarchy
2. In the Inspector, find the **Canvas Scaler** component
3. Set **UI Scale Mode** to **"Scale With Screen Size"**
4. Set **Reference Resolution** to your target resolution (e.g., 1920x1080)
5. Set **Match** to **0.5** (or adjust based on preference)
6. Set **Screen Match Mode** to **"Match Width Or Height"**

### Step 2: Fix UI Element Anchors

For each UI element (panels, texts, buttons):

1. **Select the UI element** in the Hierarchy
2. In the Inspector, look at the **Rect Transform** component
3. **Click the anchor preset** (the square icon in the top-left of Rect Transform)
4. Choose an anchor preset:
   - **Top-left** for elements in top-left
   - **Top-right** for elements in top-right
   - **Bottom-left** for elements in bottom-left
   - **Bottom-right** for elements in bottom-right
   - **Center** for centered elements
5. The element will now stay anchored to that position

### Step 3: Set Anchors Manually (Advanced)

If you need precise control:

1. **Select the UI element**
2. In the **Rect Transform**, set:
   - **Anchor Min**: Bottom-left anchor point (e.g., 0, 0 for bottom-left)
   - **Anchor Max**: Top-right anchor point (e.g., 0, 0 for bottom-left - same as Min for point anchor)
3. **Anchored Position**: Position offset from anchor in pixels
4. **Pivot**: Center point of the element (0.5, 0.5 for center)

## Common Anchor Presets

- **Bottom-Left**: Min (0, 0), Max (0, 0)
- **Bottom-Center**: Min (0.5, 0), Max (0.5, 0)
- **Bottom-Right**: Min (1, 0), Max (1, 0)
- **Center-Left**: Min (0, 0.5), Max (0, 0.5)
- **Center**: Min (0.5, 0.5), Max (0.5, 0.5)
- **Center-Right**: Min (1, 0.5), Max (1, 0.5)
- **Top-Left**: Min (0, 1), Max (0, 1)
- **Top-Center**: Min (0.5, 1), Max (0.5, 1)
- **Top-Right**: Min (1, 1), Max (1, 1)

## Tips

- **Use point anchors** (Min = Max) for elements that should stay a fixed size and position
- **Use stretch anchors** (Min ≠ Max) for elements that should resize with screen (like full-screen backgrounds)
- **Set Reference Resolution** in Canvas Scaler to match your background image resolution for best results
- **Test at different resolutions** to ensure UI stays in the right place

## Example: Tower Selection Panel

If your tower selection panel should be in the bottom-left:

1. Select the panel
2. Set Anchor Preset to **Bottom-Left**
3. Set **Anchored Position** to something like (100, 100) to offset from corner
4. The panel will now stay in the bottom-left regardless of window size
