﻿using ImGuiNET;
using SE.Serialization;

namespace DeeZ.Editor.GUI.Properties
{
    public class PropertiesValue : GUIObject
    {
        private int index;
        private PropertiesComponentNode propertiesNode;
        private SerializedValue value;

        public string GetPopupID => "ItemContext" + GetHashCode() + index;

        public override void OnPaint()
        {
            if (!value.Override) {
                PushDisabledStyle();
            }

            float margin = 0.3f;
            float width = margin * GUI.GetContentRegionAvailable().X - 10.0f;

            GUI.AlignTextToFramePadding();
            GUI.SetNextItemWidth(width);
            ImGui.LabelText("##"+value.Name, value.Name);
            GUI.OpenPopupOnItemClick(GetPopupID, 0);

            GUI.PushMargin(margin);
            GUI.Indent();
            EditorGUIHelper.TryDisplayValue(index, value);
            GUI.PopMargin();
            if (!value.Override) {
                PopDisabledStyle();
            }
            CheckPopup();
        }

        private void CheckPopup()
        {
            if (GUI.BeginPopup(GetPopupID)) {
                GUI.Checkbox("Override", ref value.Override);
                GUI.EndPopup();
            }
        }

        private void PushDisabledStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, EditorGUI.Colors.TextDisabled);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, EditorGUI.Colors.FrameBgDisabled);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, EditorGUI.Colors.FrameBgDisabled);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, EditorGUI.Colors.FrameBgDisabled);
        }

        private void PopDisabledStyle()
        {
            ImGui.PopStyleColor(); 
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
        }

        public PropertiesValue(int index, PropertiesComponentNode propertiesNode, SerializedValue value)
        {
            this.index = index;
            this.propertiesNode = propertiesNode;
            this.value = value;
        }
    }
}
