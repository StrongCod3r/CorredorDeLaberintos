﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using Nez.Textures;
using Game.Components;
using Game.Entities.Base;
using Game.Scenes;

namespace Game.Entities
{
    /// <summary>
    /// Entidad Bug1
    /// </summary>
	class Bug1 : EnemyBase
	{
		private Sprite<AnimationType> animation;
		private int sizeFrame = 32;
		private BoxCollider boxCollider;
		public int Width { get => (int)boxCollider.width; }
		public int Height { get => (int)boxCollider.height; }

		public AnimationType Animation
		{
			get => this.animation.currentAnimation;
			set => this.animation.play(value);
		}
		public bool FlipX { get => this.animation.flipX; set => this.animation.flipX = value; }
		private double deathCounter;
		private double deathLimit;


		public Bug1(Vector2 position) : base(EntityType.Bug1.ToString())
		{
			this.position = position;
			addComponent(new Bug1Behavior());
			boxCollider = addComponent<BoxCollider>();
			boxCollider.setHeight(21);
			boxCollider.setWidth(32);
			this.deathLimit = 0.5;
		}

		public override void onAddedToScene()
		{
			(scene as Level)?.SetMapCollition(this);
			this.position = new Vector2(this.position.X + sizeFrame / 2, this.position.Y);

            // Cargamos las hojas de sprites
			var texture = scene.content.Load<Texture2D>(Content.Sprite.bug1);
			var subtextures = Subtexture.subtexturesFromAtlas(texture, sizeFrame, 21);
			animation = this.addComponent(new Sprite<AnimationType>(subtextures[0]));

			// Establecemos las animaciones
			//---------------------------------------------------------------------------------
			// Walk
			animation.addAnimation(AnimationType.Walk, new SpriteAnimation(new List<Subtexture>()
			{
				subtextures[1],
				subtextures[2],
				subtextures[3]
			}));

			// Dead
			animation.addAnimation(AnimationType.Death, new SpriteAnimation(new List<Subtexture>()
			{
				subtextures[0]
			}));


			this.Animation = AnimationType.Walk;
		}

		public override void update()
		{
			base.update();

			if (!Alive)
			{ 
                // Incrementamos el temporizador
				deathCounter += Time.deltaTime;
				if (deathCounter > deathLimit)
					this.destroy();
			}
		}

		public override void Kill()
		{
			this.Animation = AnimationType.Death;
			this.Alive = false;
			SoundManager.PlaySound(Content.Sound.squish);
		}

	}
}
