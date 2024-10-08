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
    /// Entidad Bug2
    /// </summary>
	class Bug2 : EnemyBase
	{
		private Sprite<AnimationType> animation;
		private BoxCollider boxCollider;
        private int sizeFrame = 32;
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

		public Bug2(Vector2 position) : base(EntityType.Bug2.ToString())
		{
			this.position = position;
			addComponent(new Bug2Behavior());
			boxCollider = addComponent<BoxCollider>();
			boxCollider.setHeight(27);
			boxCollider.setWidth(32);
			this.deathLimit = 0.5;
		}

		public override void onAddedToScene()
		{
			(scene as Level)?.SetMapCollition(this);
			this.position = new Vector2(this.position.X + 32 / 2, this.position.Y);

            // Cargamos las hojas de sprites
			var texture = scene.content.Load<Texture2D>(Content.Sprite.bug2);
			var subtextures = Subtexture.subtexturesFromAtlas(texture, sizeFrame, 27);
			animation = this.addComponent(new Sprite<AnimationType>(subtextures[0]));

			// Establecemos las animaciones
			//---------------------------------------------------------------------------------
			// Walk
			animation.addAnimation(AnimationType.Walk, new SpriteAnimation(new List<Subtexture>()
			{
				subtextures[3],
				subtextures[4],
				subtextures[5]
			}));

			// Dead
			animation.addAnimation(AnimationType.Death, new SpriteAnimation(new List<Subtexture>()
			{
				subtextures[6]
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
