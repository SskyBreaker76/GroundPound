using SkySoft.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SkySoft.Events.Graph
{
    public enum EventGraphInventoryOperation
    {
        AddItem,
        RemoveItem,
        EditMoney
    }

    public class EditInventory : CompositeNode
    {
        public EventGraphInventoryOperation Operation;
        public override Color NodeTint => Color.yellow;
        public override string DecorativeName => $"{(Operation == EventGraphInventoryOperation.AddItem ? "Add Item/s" : (Operation == EventGraphInventoryOperation.RemoveItem ? "Remove Item/s" : "Edit Money"))}";
        public override string Description => $"{(Operation == EventGraphInventoryOperation.AddItem || Operation == EventGraphInventoryOperation.RemoveItem ? (TargetItem ? TargetItem.Name : "<b>NO ITEM</b>") : "Coppers")}{((Amount > 0 && Operation != EventGraphInventoryOperation.RemoveItem) ? $" += {Amount}" : $" -= {(Amount * (Operation == EventGraphInventoryOperation.RemoveItem ? 1 : -1))}")}";

        public Item TargetItem;
        public int Amount;
        public bool RemoveStack = false;

        public override void Run(Action OnDone)
        {
            if (DebugMode)
                UnityEngine.Debug.Log($"Run: {name}");

            switch (Operation)
            {
                case EventGraphInventoryOperation.AddItem:
                    Inventory.Inventory.LocalInventory.AddItem(TargetItem, Amount); 
                    break;
                case EventGraphInventoryOperation.RemoveItem:
                    Inventory.Inventory.LocalInventory.RemoveItem(TargetItem, Amount);
                    break;
                case EventGraphInventoryOperation.EditMoney:
                    Inventory.Inventory.LocalInventory.AddMoney(Amount);
                    break;
            }

            ConnectionDict[0].Target.Run(OnDone);
        }

        public override void DrawInspector()
        {
#if UNITY_EDITOR
            Operation = (EventGraphInventoryOperation)EditorGUILayout.EnumPopup("Operation", Operation);
            if (Operation != EventGraphInventoryOperation.EditMoney)
            {
                TargetItem = (Item)EditorGUILayout.ObjectField("Item", TargetItem, typeof(Item), false);
                if (Operation == EventGraphInventoryOperation.RemoveItem)
                {
                    RemoveStack = EditorGUILayout.Toggle("Remove All", RemoveStack);
                }
                else
                {
                    RemoveStack = false;
                }

                if (RemoveStack)
                {
                    Amount = SkyEngine.GeneralStackSize;
                }
                else
                {
                    Amount = EditorGUILayout.IntSlider(Amount, 0, SkyEngine.GeneralStackSize);
                }
            }
            else
            {
                TargetItem = null;
                RemoveStack = false;
                Amount = EditorGUILayout.IntField("Amount", Amount);
            }
#endif
        }
    }
}