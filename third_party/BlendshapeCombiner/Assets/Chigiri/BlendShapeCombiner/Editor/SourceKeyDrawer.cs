// BlendShape Combiner
//
// MIT License
//
// Copyright (c) 2020 TSUTSUMI Chigiri
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace enitimeago.NonDestructiveMMD.vendor.BlendShapeCombiner.Editor
{

    [CustomPropertyDrawer(typeof(SourceKey))]
    internal class SourceKeyDrawer : PropertyDrawer
    {

        void DrawNameSelector(Rect position, SerializedProperty property, BlendShapeCombiner root)
        {
            var label = "";
            var tooltip = "合成元となるシェイプキーの名前";
            var name = property.FindPropertyRelative("name");
            if (!root.useTextField)
            {
                var shapeKeys = root._shapeKeys;
                if (shapeKeys != null && 0 < shapeKeys.Length)
                {
                    var selected = -1;
                    for (var i = 0; i < shapeKeys.Length; i++)
                    {
                        if (shapeKeys[i] != name.stringValue) continue;
                        selected = i;
                        break;
                    }
                    if (name.stringValue == "") selected = 0;
                    if (0 <= selected)
                    {
                        selected = EditorGUI.Popup(position, label, selected, shapeKeys);
                        name.stringValue = shapeKeys[selected];
                        return;
                    }
                }
            }
            EditorGUI.PropertyField(position, name, new GUIContent(label, tooltip));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var root = property.serializedObject.targetObject as BlendShapeCombiner;
            var orgLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0;
            var rects = Helper.SplitRect(position, false, -1f, 40f, 16f, 40f, 16f);
            var r = 0;
            var style = new GUIStyle {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };
            var numStyle = EditorStyles.numberField;
            numStyle.margin = new RectOffset { };
            numStyle.padding = new RectOffset { };

            DrawNameSelector(rects[r++], property, root);

            var xSignBounds = property.FindPropertyRelative("xSignBounds");
            xSignBounds.intValue = EditorGUI.Popup(rects[r++], "", xSignBounds.intValue+1, new string[]{"L","LR","R"}) - 1;

            EditorGUI.LabelField(rects[r++], "", " x", style);

            var p = root.usePercentage ? 100.0 : 1.0;
            var scale = property.FindPropertyRelative("scale");
            scale.doubleValue = EditorGUI.DoubleField(rects[r++], scale.doubleValue * p, numStyle) / p;

            EditorGUI.LabelField(rects[r++], "", root.usePercentage ? "%" : "", style);

            EditorGUIUtility.labelWidth = orgLabelWidth;
        }

    }

}
