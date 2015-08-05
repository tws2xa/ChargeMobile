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

///
/// This class is from Professor Sherriff's Monogame Platform demo.
///
namespace Charge
{
    class Controls
    {
		// Keyboard controls
        private KeyboardState kb;
        private KeyboardState kbo;
        private GamePadState gp;
        private GamePadState gpo;

		//Touch controls
		Rectangle jumpControlRegion;
		Rectangle dragControlRegion;

		int screenMidpoint = GameplayVars.WinWidth / 2;

		const int UP_NUM = 0;
		const int DOWN_NUM = 1;
		const int LEFT_NUM = 2;
		const int RIGHT_NUM = 3;

		TouchCollection touchCollection;
		List<GestureSample> taps;

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
			InitializeKeyboardControls();
			InitializeTouchControls();
		}

		

		/// <summary>
		/// Resets all controls to clear for a new game
		/// Mainly useful in touch versions for clearing swipes, etc
		/// </summary>
		public void Reset()
        {
			taps.Clear();
			startedDrags = new int[] { 0, 0, 0, 0 };
			inRegionDrags = new int[] { 0, 0, 0, 0 };
			dragging = new bool[] { false, false, false, false };
			endDrags = new bool[] { false, false, false, false };

			prevFingersDown = countFingersDown();
			fingersDown = countFingersDown();
		}

        public void Update()
        {
			UpdateKeyboardControls();
			UpdateTouchControls();
        }

        public bool isPressed(Keys key, Buttons button)
        {
            return kb.IsKeyDown(key) || gp.IsButtonDown(button);
        }

        public bool onPress(Keys key, Buttons button)
        {
            return (kb.IsKeyDown(key) && kbo.IsKeyUp(key)) ||
                (gp.IsButtonDown(button) && gpo.IsButtonUp(button));
        }

        public bool onRelease(Keys key, Buttons button)
        {
            return (kb.IsKeyUp(key) && kbo.IsKeyDown(key)) ||
                (gp.IsButtonUp(button) && gpo.IsButtonDown(button));
        }

        public bool isHeld(Keys key, Buttons button)
        {
            return (kb.IsKeyDown(key) && kbo.IsKeyDown(key)) ||
                (gp.IsButtonDown(button) && gpo.IsButtonDown(button));
        }
		
        /// <summary>
        /// Checks if the jump control has been triggered
        /// </summary>
        public bool JumpTrigger()
        {
            return onPress(Keys.Space, Buttons.A) || (FingerDown() && !FingerDragAnyDirection());
        }

        /// <summary>
        /// Checks if the jump control has been released
        /// </summary>
        public bool JumpRelease()
        {
            return onRelease(Keys.Space, Buttons.A) || FingerUp();
        }

        /// <summary>
        /// Checks if the discharge control has been triggered
        /// </summary>
        public bool DischargeTrigger()
        {
            return isPressed(Keys.A, Buttons.X) || isPressed(Keys.Left, Buttons.X) || InRegionDragLeft();
        }

        /// <summary>
        /// Checks if the overcharge control has been triggered
        /// </summary>
        public bool OverchargeTrigger()
        {
            return isPressed(Keys.D, Buttons.B) || isPressed(Keys.D, Buttons.B) || InRegionDragRight();
        }

        /// <summary>
        /// Checks if the shoot control has been triggered
        /// </summary>
        public bool ShootTrigger()
        {
            return isPressed(Keys.S, Buttons.Y) || isPressed(Keys.S, Buttons.Y) || InRegionDragDown();
        }

        /// <summary>
        /// Checks if the pause control has been triggered
        /// </summary>
        public bool PauseTrigger()
        {
            return onPress(Keys.P, Buttons.Start) || InRegionDragUp();
        }

        /// <summary>
        /// Checks if the unpause control has been triggered
        /// </summary>
        public bool UnpauseTrigger()
        {
            return onPress(Keys.P, Buttons.Start) || Tap();
        }

        /// <summary>
        /// Checks if the menu up control has been triggered
        /// </summary>
        public bool MenuUpTrigger()
        {
            return onPress(Keys.Up, Buttons.LeftThumbstickUp) || BeginDragUp();
        }

        /// <summary>
        /// Checks if the menu down control has been triggered
        /// </summary>
        public bool MenuDownTrigger()
        {
            return onPress(Keys.Down, Buttons.LeftThumbstickDown) || BeginDragDown();
        }

        /// <summary>
        /// Checks if the menu select control has been triggered
        /// </summary>
        public bool MenuSelectTrigger()
        {
            return onPress(Keys.Space, Buttons.A) || onPress(Keys.Enter, Buttons.Start) || Tap();
        }

        /// <summary>
        /// Checks if the restart control has been triggered
        /// </summary>
        public bool RestartTrigger()
        {
            return onPress(Keys.Enter, Buttons.Start) || FinishDragLeft();
        }

        /// <summary>
        /// Checks if the menu increase control has been triggered
        /// </summary>
        public bool MenuIncreaseTrigger()
        {
            return onPress(Keys.Right, Buttons.LeftThumbstickRight) || BeginDragRight();
        }

        /// <summary>
        /// Checks if the menu decrease control has been triggered
        /// </summary>
        public bool MenuDecreaseTrigger()
        {
            return onPress(Keys.Left, Buttons.LeftThumbstickLeft) || BeginDragLeft();
        }

        /// <summary>
        /// Checks if the return to title screen control has been triggered
        /// </summary>
        /// <returns></returns>
        public bool TitleScreenTrigger()
        {
            return onPress(Keys.Back, Buttons.B) || FinishDragRight();
        }

        /// <summary>
        /// Checks if the tutorial skip control has been triggered
        /// </summary>
        /// <returns></returns>
        public bool TutorialSkipTrigger()
        {
            return onPress(Keys.Escape, Buttons.Start);
        }

        /// <summary>
        /// String explaining the control to start a new game.
        /// </summary>
        public string GetRestartString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Swipe Left";
			}

			return "Press [Enter]";
        }

        /// <summary>
        /// String explaining the control to return to title.
        /// </summary>
        public string GetReturnToTitleString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Swipe Right";
			}

			return "Press [Backspace]";
        }

        /// <summary>
        /// String explaining the control to unpause the game.
        /// </summary>
        public string GetUnpauseText()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Tap";
			}

			return "Press [P]";
        }

        public string GetJumpString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Tap the right side of the screen";
			}
			
            return "Press [Space]";
        }

        public string GetDischargeString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Swipe Left";
			}

			return "Press [A]";
        }

        public string GetShootString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Swipe Down";
			}

			return "Press [S]";
        }

        public string GetOverchargeString()
        {
			if (GameplayVars.isTouchMode)
			{
				return "Swipe Right";
			}

			return "Press [D]";
        }

		/// <summary>
		/// Checks if there was a tap somewhere within a particular region
		/// </summary>
		/// <param name="location">Region in which to check for a tap</param>
		public bool TapRegionCheck(Rectangle region)
		{
			foreach (GestureSample tap in taps)
			{
				if (region.Contains(tap.Position)) return true;
			}
			return false;
		}

		public bool ClickRegionCheck(Rectangle region)
		{
			MouseState mouseState = Mouse.GetState();

			if (mouseState.LeftButton != ButtonState.Pressed)
			{
				return false;
			}

			// Because of the VirtualResolution scaling the UI buttons to fit different screen sizes, we must apply the same scaling to the Mouse Position
			Vector3 scale = VirtualResolution.getTransformationMatrix().Scale;
			Rectangle scaledRegion = region;

			//Point mouseTransformation = VirtualResolution.GetTransformationForMouseClicks();

			scaledRegion.X = (int)Math.Round(scale.X * scaledRegion.X);
			scaledRegion.Y = (int)Math.Round(scale.Y * scaledRegion.Y);
			scaledRegion.Width = (int)Math.Round(scale.X * scaledRegion.Width);
			scaledRegion.Height = (int)Math.Round(scale.Y * scaledRegion.Height);

			// Because the current window size may not match the desired aspect ratio so the VirtualResolution will transform the game viewport to be centered in main window
			Point mouseTransformation = VirtualResolution.GetTransformationForMouseClicks();
			Point mousePosition = mouseState.Position;

			mousePosition.X -= mouseTransformation.X;
			mousePosition.Y -= mouseTransformation.Y;
			
			return scaledRegion.Contains(mousePosition);
		}

		private void InitializeKeyboardControls()
		{
			kb = Keyboard.GetState();
			kbo = Keyboard.GetState();
			gp = GamePad.GetState(PlayerIndex.One);
			gpo = GamePad.GetState(PlayerIndex.One);
		}

		private void InitializeTouchControls()
		{
			touchCollection = TouchPanel.GetState();
			taps = new List<GestureSample>();

			TouchPanel.EnabledGestures = GestureType.Tap | GestureType.HorizontalDrag | GestureType.VerticalDrag | GestureType.DragComplete;

			jumpControlRegion = new Rectangle(GameplayVars.WinWidth / 2, 0, GameplayVars.WinWidth / 2, GameplayVars.WinHeight);
			dragControlRegion = new Rectangle(0, 0, GameplayVars.WinWidth / 2, GameplayVars.WinHeight);

			registeredAnyDrag = false;
			registeredInRegionDrag = false;

			startedDrags = new int[] { 0, 0, 0, 0 };
			inRegionDrags = new int[] { 0, 0, 0, 0 };
			dragging = new bool[] { false, false, false, false };
			endDrags = new bool[] { false, false, false, false };

			prevFingersDown = 0;
			fingersDown = countFingersDown();
		}

		private void UpdateKeyboardControls()
		{
			kbo = kb;
			gpo = gp;
			kb = Keyboard.GetState();
			gp = GamePad.GetState(PlayerIndex.One);
		}

		private void UpdateTouchControls()
		{
			touchCollection = TouchPanel.GetState();
			prevFingersDown = fingersDown;
			fingersDown = countFingersDown();

			//Clear almost all counter variables (but not the start drags, those get cleared when a drag is complete)
			taps.Clear();
			for (int i = 0; i < startedDrags.Length; i++) { startedDrags[i] = 0; }
			for (int i = 0; i < startedDrags.Length; i++) { inRegionDrags[i] = 0; }
			for (int i = 0; i < endDrags.Length; i++) { endDrags[i] = false; }

			while (TouchPanel.IsGestureAvailable)
			{
				GestureSample gesture = TouchPanel.ReadGesture();
				if (gesture.GestureType == GestureType.Tap)
				{
					taps.Add(gesture);
				}

				if (gesture.GestureType == GestureType.HorizontalDrag)
				{
					if (gesture.Delta.X > gesture.Delta2.X)
					{
						if (!registeredAnyDrag) startedDrags[RIGHT_NUM]++;
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
						if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion))
						{
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
						if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion))
						{
							inRegionDrags[DOWN_NUM]++;
							registeredInRegionDrag = true;
						}
						dragging[DOWN_NUM] = true;
						registeredAnyDrag = true;
					}
					else if (gesture.Delta.Y < gesture.Delta2.Y)
					{
						if (!registeredAnyDrag) startedDrags[UP_NUM]++;
						if (!registeredInRegionDrag && DragInRegion(gesture, dragControlRegion))
						{
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

		private int countFingersDown()
		{
			int count = 0;
			foreach (TouchLocation touch in touchCollection)
			{
				if (jumpControlRegion.Contains(touch.Position)) count++;
			}
			return count;
		}

		/// <summary>
		/// Returns if the given gesture began or finished
		/// within the given region
		/// </summary>
		private Boolean DragInRegion(GestureSample gesture, Rectangle region)
		{
			Vector2 p1 = gesture.Position;
			Vector2 p2 = new Vector2(gesture.Position.X + gesture.Delta.X, gesture.Position.Y + gesture.Delta.Y);
			return (region.Contains(p1) || region.Contains(p2));
		}

		private bool Tap()
		{
			return (taps.Count > 0);
		}

		private bool FingerDown()
		{
			return (prevFingersDown < fingersDown);
		}

		private bool FingerUp()
		{
			return (prevFingersDown > fingersDown);
		}

		private bool FingerDragAnyDirection()
		{
			return (BeginDragDown() || BeginDragLeft() || BeginDragRight() || BeginDragUp());
		}

		private bool BeginDragLeft()
		{
			return startedDrags[LEFT_NUM] > 0;
		}

		private bool BeginDragRight()
		{
			return startedDrags[RIGHT_NUM] > 0;
		}

		private bool BeginDragUp()
		{
			return startedDrags[UP_NUM] > 0;
		}

		private bool BeginDragDown()
		{
			return startedDrags[DOWN_NUM] > 0;
		}

		private bool InRegionDragLeft()
		{
			return inRegionDrags[LEFT_NUM] > 0;
		}

		private bool InRegionDragRight()
		{
			return inRegionDrags[RIGHT_NUM] > 0;
		}

		private bool InRegionDragUp()
		{
			return inRegionDrags[UP_NUM] > 0;
		}

		private bool InRegionDragDown()
		{
			return inRegionDrags[DOWN_NUM] > 0;
		}

		private bool FinishDragUp()
		{
			return endDrags[UP_NUM];
		}

		private bool FinishDragDown()
		{
			return endDrags[DOWN_NUM];
		}

		private bool FinishDragLeft()
		{
			return endDrags[LEFT_NUM];
		}

		private bool FinishDragRight()
		{
			return endDrags[RIGHT_NUM];
		}

		private bool DraggingUp()
		{
			return dragging[UP_NUM];
		}

		private bool DraggingDown()
		{
			return dragging[DOWN_NUM];
		}

		private bool DraggingLeft()
		{
			return dragging[LEFT_NUM];
		}

		private bool DraggingRight()
		{
			return dragging[RIGHT_NUM];
		}
	}
}
