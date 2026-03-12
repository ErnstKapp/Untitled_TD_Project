# Dialogue Cutscene Setup (Paris Intro)

A Ren'Py-style dialogue runs **before** loading Paris_Scene when the player clicks the Paris level button. The map stays visible in the background, slightly darkened.

---

## 1. Create the dialogue content (ScriptableObject)

1. In the **Project** window: **Right-click → Create → Tower Defense → Dialogue Data**.
2. Name it (e.g. `ParisIntroDialogue`).
3. Select the asset and in the **Inspector**:
   - **Dialogue Title**: optional (e.g. "Paris Intro").
   - **Lines**: add one element per line of dialogue. For each line:
     - **Speaker Name**: e.g. "Coach", "Referee".
     - **Text**: the line of dialogue.
     - **Portrait**: sprite for whoever is talking this line (leave empty to keep previous).
     - **Speaker On Left**: tick if this speaker is on the left, untick for right.

Example: 4 lines with two characters alternating (Coach left, Referee right).

---

## 2. Dialogue UI in the Overworld scene

Create a canvas/panel that will overlay the map when the cutscene plays.

1. **Canvas** (if you don’t have one): **UI → Canvas**. Set **Render Mode** to Screen Space - Overlay, **Canvas Scaler** to Scale With Screen Size (Reference Resolution e.g. 1920×1080).

2. **Cutscene panel** (child of Canvas):
   - Create **Empty** → name it `DialoguePanel`.
   - Add **Image** (or use a **Panel** from UI): this will dim the background.
     - Set **Color** to black with **Alpha** about **0.5–0.6** (map visible but darkened).
     - Set **Anchor** to stretch full screen (left 0, right 0, top 0, bottom 0), **Raycast Target** on so it blocks clicks behind.

3. **Portraits** (optional but recommended):
   - Under `DialoguePanel`, create two **Image** objects: `PortraitLeft`, `PortraitRight`.
   - Position them left and right (e.g. anchor to bottom-left and bottom-right, size ~200×200). Leave **Source Image** empty; the script will assign sprites per line.

4. **Dialogue text**:
   - Under `DialoguePanel`, create **TextMeshPro - Text** (or **UI - Text - TextMeshPro**). Name it `DialogueText`.
   - Place it at the bottom (e.g. a “dialogue box” area). Resize as needed.

5. **Speaker name** (optional):
   - Add another **TextMeshPro - Text** (e.g. above the dialogue text). Name it `SpeakerNameText`.

6. **Next button**:
   - Add a **Button** (e.g. “Next” or “Continue”) under `DialoguePanel`. When clicked it will advance to the next line; on the last line it will close the dialogue and load Paris.

7. **Click-anywhere** (optional):
   - Add a full-screen **Button** (no caption, transparent image) as a child of `DialoguePanel`, behind the text/portraits. In its **On Click ()** list, add the GameObject that has **DialogueUI** and choose **DialogueUI → AdvanceDialogue ()** so clicking anywhere advances.

8. **DialogueUI component**:
   - Select `DialoguePanel` (or a parent that holds all of the above).
   - **Add Component → Dialogue UI** (script).
   - Assign:
     - **Panel Root**: the GameObject that should be shown/hidden (e.g. `DialoguePanel` itself).
     - **Dim Overlay**: the full-screen black Image.
     - **Portrait Left** / **Portrait Right**: the two portrait Images.
     - **Dialogue Text**: the main dialogue TMP.
     - **Speaker Name Text**: the speaker name TMP (optional).
     - **Next Button**: the Next/Continue button (and/or wire a full-screen button’s On Click to **AdvanceDialogue ()**).

---

## 3. Dialogue Manager (persists across scenes)

1. Create an **Empty** GameObject in the **Overworld** scene, name it `DialogueManager`.
2. **Add Component → Dialogue Manager**.
3. In **Cutscenes Before Scenes**:
   - Size **1**.
   - **Scene Name**: `Paris_Scene` (exact name from Build Settings).
   - **Dialogue**: drag your `ParisIntroDialogue` asset here.
4. **Dialogue UI**: drag the GameObject that has the **DialogueUI** component (e.g. `DialoguePanel`).

The manager will **DontDestroyOnLoad** so it survives; the dialogue UI lives in the overworld and is shown there before the scene load.

---

## 4. Flow

1. Player clicks **Paris** level button (unlocked).
2. **SceneLoader.LoadScene("Paris_Scene")** is called.
3. **DialogueManager** sees a cutscene for `Paris_Scene` and runs it instead of loading immediately.
4. **DialogueUI** shows: dimmed map, portraits, text, Next (and optional click-anywhere).
5. Player advances through each line (Next or click).
6. After the last line, **DialogueManager** runs the completion callback → **SceneLoader** loads **Paris_Scene**.

No changes are needed on the Paris button; the cutscene is triggered automatically when loading `Paris_Scene`.

---

## 5. Adding more cutscenes

In **DialogueManager → Cutscenes Before Scenes**, add more entries: **Scene Name** = exact build scene name, **Dialogue** = your Dialogue Data asset.
