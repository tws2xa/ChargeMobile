using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input.Touch;

namespace Charge
{
    class Controls
    {
        Rectangle jumpControlRegion;
        Rectangle dragControlRegion;

        int screenMidpoint = GameplayVars.WinWidth / 2;

        const int UP_NUM = 0;
        const int DOWN_NUM = 1;
        const int LEFT_NUM = 2;
        const int RIGHT_NUM = 3;

        TouchCollection touchCollection;
        
        int tapNum;
        
        int prevFingersDown;
        int fingersDown;

        int[] startedDrags;
        int[] inRegionDrags;
        bool[] dragging; 
        bool[] endDrags;


        bool registeredAnyDrag;
        bool registeredInRegionDrag;
        
        public Controls()
        {
            this.touchCollection = TouchPanel.GetState();
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.HorizontalDrag | GestureType.VerticalDrag | GestureType.DragComplete;

            jumpControlRegion = new Rectangle(GameplayVars.WinWidth / 2, 0, GameplayVars.WinWidth / 2, GameplayVars.WinHeight);
            dragControlRegion = new Rectangle(0, 0, GameplayVars.WinWidth / 2, GameplayVars.WinHeight);

            registeredAnyDrag = false;
            registeredInRegionDrag = false;

            tapNum = 0;
            startedDrags = new int[] { 0, 0, 0, 0 };
            inRegionDrags = new int[] { 0, 0, 0, 0 };
            dragging = new bool[] { false, false, false, false };
            endDrags = new bool[] { false, false, false, false };

            prevFingersDown = 0;
            fingersDown = countFingersDown();
        }

        public int countFingersDown()
        {
            int count = 0;
            foreach (TouchLocation touch in touchCollection)
            {
                if (jumpControlRegion.Contains(touch.Position)) count++;
            }
            return count;
        }

        /// <summary>
        /// Resets all control options (to prevent carryover controls from menu taps, swipes, etc)
        /// </summary>
        public void Reset()
        {
            tapNum = 0;
            startedDrags = new int[] { 0, 0, 0, 0 };
            inRegionDrags = new int[] { 0, 0, 0, 0 };
            dragging = new bool[] { false, false, false, false };
            endDrags = new bool[] { false, false, false, false };

            prevFingersDown = countFingersDown();
            fingersDown = countFingersDown();
        }

        public void Update()
        {
            this.touchCollection = TouchPanel.GetState();
            prevFingersDown = fingersDown;
            fingersDown = countFingersDown();

            tapNum = 0;


            //Clear almost all counter variables (but not the start drags, those get cleared when a drag is complete)
            tapNum = 0;
            for (int i = 0; i < startedDrags.Length; i++) { startedDrags[i] = 0; }
            for (int i = 0; i < startedDrags.Length; i++) { inRegionDrags[i] = 0; }
            for (int i = 0; i < endDrags.Length; i++) { endDrags[i] = false; }

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                if (gesture.GestureType == GestureType.Tap)
                {
                    tapNum++;
                }

                if (gesture.GestureType == GestureType.HorizontalDrag)
                {
                    if (gesture.Delta.X > gesture.Delta2.X)
                    {
                        if(!registeredAnyDrag) startedDrags[RIGHT_NUM]++;
                        if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion))
                        {
                            inRegionDrags[RIGHT_NUM]++;
                            registeredInRegionDrag = true;
                        }
                        dragging[RIGHT_NUM] = true;
                        registeredAnyDrag = true;
                    }
                    else if (gesture.Delta.X < gesture.Delta2.X)
                    {
                        if (!registeredAnyDrag) startedDrags[LEFT_NUM]++;
                        if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion)) {
                            inRegionDrags[LEFT_NUM]++;
                            registeredInRegionDrag = true;
                        }
                        dragging[LEFT_NUM] = true;
                        registeredAnyDrag = true;
                    }
                }
                if (gesture.GestureType == GestureType.VerticalDrag)
                {
                    if (gesture.Delta.Y > gesture.Delta2.Y)
                    {
                        if (!registeredAnyDrag) startedDrags[DOWN_NUM]++;
                        if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion)) {
                            inRegionDrags[DOWN_NUM]++;
                            registeredInRegionDrag = true;
                        }
                        dragging[DOWN_NUM] = true;
                        registeredAnyDrag = true;
                    }
                    else if (gesture.Delta.Y < gesture.Delta2.Y)
                    {
                        if (!registeredAnyDrag) startedDrags[UP_NUM]++;
                        if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion)) {
                            inRegionDrags[UP_NUM]++;
                            registeredInRegionDrag = true;
                        }
                        dragging[UP_NUM] = true;
                        registeredAnyDrag = true;
                    }
                }

                if (gesture.GestureType == GestureType.DragComplete)
                {
                    for (int i = 0; i < dragging.Length; i++)
                    {
                        if (dragging[i]) endDrags[i] = true;
                        dragging[i] = false;
                    }
                    registeredInRegionDrag = false;
                    registeredAnyDrag = false;
                }
            }
        }

        /// <summary>
        /// Returns if the given gesture began or finished
        /// within the given region
        /// </summary>
        public Boolean DragInRegion(GestureSample gesture, Rectangle region)
        {
            Vector2 p1 = gesture.Position;
            Vector2 p2 = new Vector2(gesture.Position.X + gesture.Delta.X, gesture.Position.Y + gesture.Delta.Y);
            return (region.Contains(p1) || region.Contains(p2));
        }

        #region Control Check Methods
        public bool Tap()
        {
            return (tapNum > 0);
        }

        public bool FingerDown()
        {
            return (prevFingersDown < fingersDown);
        }

        public bool FingerUp()
        {
            return (prevFingersDown > fingersDown);
        }

        public bool FingerDragAnyDirection()
        {
            return (BeginDragDown() || BeginDragLeft() || BeginDragRight() || BeginDragUp());
        }

        public bool BeginDragLeft()
        {
            return startedDrags[LEFT_NUM] > 0;
        }

        public bool BeginDragRight()
        {
            return startedDrags[RIGHT_NUM] > 0;
        }

        public bool BeginDragUp()
        {
            return startedDrags[UP_NUM] > 0;
        }

        public bool BeginDragDown()
        {
            return startedDrags[DOWN_NUM] > 0;
        }

        public bool InRegionDragLeft()
        {
            return inRegionDrags[LEFT_NUM] > 0;
        }

        public bool InRegionDragRight()
        {
            return inRegionDrags[RIGHT_NUM] > 0;
        }

        public bool InRegionDragUp()
        {
            return inRegionDrags[UP_NUM] > 0;
        }

        public bool InRegionDragDown()
        {
            return inRegionDrags[DOWN_NUM] > 0;
        }

        public bool FinishDragUp()
        {
            return endDrags[UP_NUM];
        }

        public bool FinishDragDown()
        {
            return endDrags[DOWN_NUM];
        }

        public bool FinishDragLeft()
        {
            return endDrags[LEFT_NUM];
        }

        public bool FinishDragRight()
        {
            return endDrags[RIGHT_NUM];
        }

        public bool DraggingUp()
        {
            return dragging[UP_NUM];
        }

        public bool DraggingDown()
        {
            return dragging[DOWN_NUM];
        }

        public bool DraggingLeft()
        {
            return dragging[LEFT_NUM];
        }

        public bool DraggingRight()
        {
            return dragging[RIGHT_NUM];
        }
        #endregion

        #region Game Triggers
        /// <summary>
        /// Checks if the jump control has been triggered
        /// </summary>
        public bool JumpTrigger()
        {
            return (FingerDown() && !FingerDragAnyDirection());
        }

        /// <summary>
        /// Checks if the jump control has been released
        /// </summary>
        public bool JumpRelease()
        {
            return FingerUp();
        }

        /// <summary>
        /// Checks if the discharge control has been triggered
        /// </summary>
        public bool DischargeTrigger()
        {
            return InRegionDragLeft();
        }

        /// <summary>
        /// Checks if the overcharge control has been triggered
        /// </summary>
        public bool OverchargeTrigger()
        {
            return InRegionDragRight();
        }

        /// <summary>
        /// Checks if the shoot control has been triggered
        /// </summary>
        public bool ShootTrigger()
        {
            return InRegionDragDown();
        }

        /// <summary>
        /// Checks if the pause control has been triggered
        /// </summary>
        public bool PauseTrigger()
        {
            //return false;
            return InRegionDragUp();
        }

        /// <summary>
        /// Checks if the unpause control has been triggered
        /// </summary>
        public bool UnpauseTrigger()
        {
            return Tap();
        }

        /// <summary>
        /// Checks if the menu up control has been triggered
        /// </summary>
        public bool MenuUpTrigger()
        {
            return BeginDragUp();
        }

        /// <summary>
        /// Checks if the menu down control has been triggered
        /// </summary>
        public bool MenuDownTrigger()
        {
            return BeginDragDown();
        }

        /// <summary>
        /// Checks if the menu increase control has been triggered
        /// </summary>
        public bool MenuIncreaseTrigger()
        {
            return BeginDragRight();
        }

        /// <summary>
        /// Checks if the menu decrease control has been triggered
        /// </summary>
        public bool MenuDecreaseTrigger()
        {
            return BeginDragLeft();
        }

        /// <summary>
        /// Checks if the menu select control has been triggered
        /// </summary>
        public bool MenuSelectTrigger()
        {
            return Tap();
        }

        /// <summary>
        /// Checks if the return to title screen control has been triggered
        /// </summary>
        /// <returns></returns>
        public bool TitleScreenTrigger()
        {
            return FinishDragRight();
        }

        /// <summary>
        /// Checks if the restart control has been triggered
        /// </summary>
        public bool RestartTrigger()
        {
            return FinishDragLeft();
        }

        /// <summary>
        /// String explaining the control to start a new game.
        /// </summary>
        public string GetRestartString()
        {
            return "Swipe Left";
        }

        /// <summary>
        /// String explaining the control to return to title.
        /// </summary>
        public string GetReturnToTitleString()
        {
            return "Swipe Right";
        }

        /// <summary>
        /// String explaining the control to unpause the game.
        /// </summary>
        public string GetUnpauseText()
        {
            return "Tap";
        }

        public string GetJumpString()
        {
            return "Tap the right side of the screen";
        }

        public string GetDischargeString()
        {
            return "Swipe Left";
        }

        public string GetShootString()
        {
            return "Swipe Down";
        }

        public string GetOverchargeString()
        {
            return "Swipe Right";
        }
        #endregion
    }
}
