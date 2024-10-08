﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tiled;
using Game.Entities;

namespace Game.Components
{
    /// <summary>
    /// Comportamiento de las controles del Player
    /// </summary>
    public class PlayerController : Component, IUpdatable
    {
        private Player playerEntity;
		private Vector2 initialPosition;
		private Vector2 velocity;
		public float moveSpeed = 150;
        public float gravity = 1000;
        public float jumpHeight = 16 * 5;
		private Vector2 moveDir;

		private float slideDir = 0;
		private float slideSpeed = 350;
		private float slideCurrentSpeed = 0;

		private float impulseAirDir = 0;
		private float impulseAirSpeed = 400;
		private float impulseAirCurrentSpeed = 0;

		private int jumpAirDir = 0;

		PlayerAnimations playerAnimation;
        TiledMapMover mover;
        BoxCollider boxCollider;
        TiledMapMover.CollisionState collisionState = new TiledMapMover.CollisionState();


		// Controls
        VirtualButton jumpInput;
        VirtualButton leftInput;
        VirtualButton rightInput;
        VirtualButton downInput;
		VirtualButton testInput;
		VirtualIntegerAxis xAxisInput;

		// State Movement
        public bool Sliding;
		public bool Jumping;
		public bool ImpulseAir;

		

		/// <summary>
		/// Init
		/// </summary>
		public override void onAddedToEntity()
        {
            this.playerEntity = (Player)this.entity;
            this.boxCollider = entity.getComponent<BoxCollider>();
            this.mover = entity.getComponent<TiledMapMover>();
            this.playerAnimation = entity.getComponent<PlayerAnimations>();
			this.initialPosition = playerEntity.position;
			this.moveDir = Vector2.Zero;

            setupInput();
        }

        public override void onRemovedFromEntity()
        {
            // deregister virtual input
            jumpInput.deregister();
            downInput.deregister();
            leftInput.deregister();
            rightInput.deregister();
            xAxisInput.deregister();
        }

        /// <summary>
        /// Configuramos los controles
        /// </summary>
        void setupInput()
        {
            // setup input for jumping. we will allow z on the keyboard or a on the gamepad
            jumpInput = new VirtualButton();
            jumpInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Z));
            jumpInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.A));

            downInput = new VirtualButton();
            downInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Down));
            downInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.DPadDown));

            leftInput = new VirtualButton();
            leftInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Left));
            leftInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.DPadLeft));

            rightInput = new VirtualButton();
            rightInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.Right));
            rightInput.nodes.Add(new Nez.VirtualButton.GamePadButton(0, Buttons.DPadRight));

			testInput = new VirtualButton();
			testInput.nodes.Add(new Nez.VirtualButton.KeyboardKey(Keys.C));

			// horizontal input from dpad, left stick or keyboard left/right
			xAxisInput = new VirtualIntegerAxis();
            xAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadDpadLeftRight());
            xAxisInput.nodes.Add(new Nez.VirtualAxis.GamePadLeftStickX());
            xAxisInput.nodes.Add(new Nez.VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));


        }

        void IUpdatable.update()
        {
			moveDir = new Vector2(xAxisInput.value, 0);

			// Onground
			//-----------------------------------------------------------------------
			if (collisionState.below)
			{
				jumpAirDir = 0;

				// Run
				if (moveDir.X != 0)
				{
					if (leftInput.isDown || rightInput.isDown)
						DoWalk();
				}
				else
				{
					// Idle
					DoIdle();
				}

				// Slide
				if (!Sliding && moveDir.X != 0 && downInput.isReleased)
				{
					DoSlide();
				}
				else
				{
					if (Sliding)
						DoSlide();
				}

				// Jump
				if (jumpInput.isPressed)
					DoJump();
			}


			// OnAir
			//-----------------------------------------------------------------------
			if (!collisionState.below)
			{
				Sliding = false;

				// WalkAir
				if (moveDir.X != 0)
				{
					if (leftInput.isDown || rightInput.isDown)
						DoWalkAir();
				}

				if (velocity.Y > 0)
					SetAnimation(AnimationType.Falling);

				if (collisionState.left || collisionState.right && jumpInput.isPressed)
				{
					DoJumpAir();
				}
			}

			// Aplicamos la gravedad
			this.velocity.Y += gravity * Time.deltaTime;

			// Move
			this.mover.move(this.velocity * Time.deltaTime, this.boxCollider, this.collisionState);

			// Golpe en la cabeza
			if (this.collisionState.above && this.velocity.Y < 0)
			{
				this.velocity.Y = 0;
				SoundManager.PlaySound(Content.Sound.land);
			}

            // Golpe sobre el piso
			if (this.collisionState.below)
				this.velocity.Y = 0;

		}

		#region Movimientos
		//------------------------------------------------------------------
        /// <summary>
        /// Parado
        /// </summary>
		private void DoIdle()
		{
			this.velocity.X = 0;
			SetAnimation(AnimationType.Idle);
		}

        /// <summary>
        /// Caminar
        /// </summary>
		private void DoWalk()
		{
			this.velocity.X = (moveDir.X) * moveSpeed;
			SetAnimation(AnimationType.Run);
		}

        /// <summary>
        /// Saltar
        /// </summary>
		private void DoJump()
		{
			this.velocity.Y = -Mathf.sqrt(2f * jumpHeight * gravity);
			SetAnimation(AnimationType.Jumping);
			if (Sliding)
				SoundManager.PlaySound(Content.Sound.slidejump);
			else
				SoundManager.PlaySound(Content.Sound.jump);
		}

        /// <summary>
        /// Caminar en el aire
        /// </summary>
		private void DoWalkAir()
		{
			this.velocity.X = (moveDir.X) * moveSpeed;
			SetAnimation(AnimationType.Jumping);
		}

        /// <summary>
        /// Deslizar
        /// </summary>
		private void DoSlide()
		{
			SetAnimation(AnimationType.Slide);
			
			if (!Sliding) // Slide off
			{
				this.slideDir = (moveDir.X);
				this.slideCurrentSpeed = this.slideSpeed;
				Sliding = true;
				SoundManager.PlaySound(Content.Sound.slide);
			}

			if (Sliding) // Slide on
			{
				this.slideCurrentSpeed -= Time.deltaTime * 500;
				this.velocity.X = (slideDir) * this.slideCurrentSpeed;

				if (this.slideCurrentSpeed <= 0 || this.collisionState.left || this.collisionState.right)
					Sliding = false;
			}
		}

        /// <summary>
        /// Salto en el aire (desde las paredes)
        /// </summary>
		private void DoJumpAir()
		{
			if (collisionState.left && jumpAirDir != -1 && jumpInput.isPressed)
			{
				DoJump();
				jumpAirDir = -1;
			}

			if (collisionState.right && jumpAirDir != 1 && jumpInput.isPressed)
			{
				DoJump();
				jumpAirDir = 1;
			}

		}

		//------------------------------------------------------------------
		#endregion

        /// <summary>
        /// Cambiamos la animacion del jugador
        /// </summary>
        /// <param name="typeAnimation"></param>
		private void SetAnimation(AnimationType typeAnimation)
		{
			if (this.moveDir.X < 0)
				playerAnimation.FlipX = true;

			if (this.moveDir.X > 0)
				playerAnimation.FlipX = false;
			
			if (playerAnimation.Animation != typeAnimation)
				playerAnimation.Animation = typeAnimation;
		}

        /// <summary>
        /// Reestablecemos la posicion inicial
        /// </summary>
		public void SetInitialPosition()
		{
			playerEntity.position = initialPosition;
			velocity.X = 0;
			velocity.Y = 0;
			playerAnimation.Animation = AnimationType.Idle;
			playerAnimation.FlipX = false;
		}

        /// <summary>
        /// Salto
        /// </summary>
		public void Jump()
		{
			playerAnimation.Animation = AnimationType.Jumping;
			this.velocity.Y = -Mathf.sqrt(2f * jumpHeight * gravity);
			SoundManager.PlaySound(Content.Sound.jump);
		}

        /// <summary>
        /// Impulso
        /// </summary>
		public void Impulse()
		{
			playerAnimation.Animation = AnimationType.Jumping;
			this.velocity.Y = -Mathf.sqrt(2f * jumpHeight * gravity);
		}
	}
}
