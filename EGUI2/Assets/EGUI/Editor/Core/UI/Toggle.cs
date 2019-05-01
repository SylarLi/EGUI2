using UnityEngine;
using System.Collections.Generic;

namespace EGUI.UI
{
    [Persistence]
    public class Toggle : Selectable, IMouseClickHandler
    {
        [PersistentField]
        private static List<Toggle> AllToggles = new List<Toggle>();

        [PersistentField]
        private bool mIsOn = false;

        /// <summary>
        /// Is toggle on?
        /// </summary>
        public bool isOn
        {
            get
            {
                return mIsOn;
            }
            set
            {
                if (mIsOn != value)
                {
                    mIsOn = value;
                    onValueChange(mIsOn);
                    AllToggles.ForEach(i =>
                    {
                        if (mIsOn && i.isOn && group != -1 &&
                            i != this && i.group == group)
                        {
                            i.isOn = false;
                        }
                    });
                    FlushToggleState();
                }
            }
        }

        [PersistentField]
        private int mGroup = -1;

        /// <summary>
        /// Toggle group, -1 means no group.
        /// </summary>
        public int group { get { return mGroup; } set { mGroup = value; } }

        [PersistentField]
        private Graphic mToggleGraphic;

        public Graphic toggleGraphic { get { return mToggleGraphic; } set { if (mToggleGraphic != value) { mToggleGraphic = value; FlushToggleState(); } } }

        public delegate void OnValueChange(bool on);

        public OnValueChange onValueChange = on => { };

        public Toggle() : base()
        {
            if (!AllToggles.Contains(this))
            {
                AllToggles.Add(this);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            AllToggles.Remove(this);
        }

        public bool OnMouseClick(Event eventData)
        {
            isOn = !isOn;
            return true;
        }

        private void FlushToggleState()
        {
            if (toggleGraphic != null)
            {
                toggleGraphic.enabled = isOn;
            }
        }
    }
}
