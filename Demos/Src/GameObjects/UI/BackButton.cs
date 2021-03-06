﻿using Microsoft.Xna.Framework;
using SE.Core;
using SE.Editor.UI;
using SE.UI;
using Button = SE.UI.Button;
using Vector2 = System.Numerics.Vector2;

namespace SEDemos.GameObjects.UI
{
    public class BackButton : UIObject
    {
        public override bool IsRootUIMenu => true;

        public override string RootUIName => "BackButton";

        public override int RootUIPriority => 5;

        public override bool DestroyOnLoad => false;

        private bool multithreadingToggle = true;
        private bool showControls = true;
        public BackButton() : base(Vector2.Zero)
        {
            if(Screen.IsFullHeadless)
                return;

            Button controls = EditorTheme.CreateButton(new Vector2(1760, 910), new Point(150, 70), 
                "Hide Controls", Transform, EditorTheme.ColorSet.Blue);

            Button back = EditorTheme.CreateButton(new Vector2(1760, 990), new Point(150, 70), 
                "Back to Menu", Transform, EditorTheme.ColorSet.Blue);

            Button disableMultithreading = EditorTheme.CreateButton(new Vector2(1450, 990), new Point(300, 70), 
                "Disable Multi-Threading", Transform, EditorTheme.ColorSet.Blue);

            back.Clicked += (sender, args) => SceneManager.SetCurrentScene("_DEMOS\\menu");
            disableMultithreading.Clicked += (sender, args) => {
                multithreadingToggle = !multithreadingToggle;
                disableMultithreading.TextString = multithreadingToggle ? "Disable Multi-threading" : "Enable Multi-threading";
            };
            controls.Clicked += (sender, args) => {
                showControls = !showControls;
                back.SetActive(showControls);
                disableMultithreading.SetActive(showControls);
                controls.TextString = showControls ? "Hide Controls" : "Show Controls";
            };
        }

        protected override void OnEnable(bool enableAllChildren = true)
        {
            base.OnEnable(enableAllChildren);
        }

        protected override void OnDisable(bool isRoot = false)
        {
            base.OnDisable(isRoot);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

    }
}
