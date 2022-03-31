using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Snake
{
	public static class Constants
	{
		public const int ScreenWidth = 800;
		public const int ScreenHeight = 800;
		public const int TileWidth = ScreenWidth / 20;
		public const int TileHeight = ScreenHeight / 20;

	}

	public class SnakePart
	{
		private Vector2 _position;

		public SnakePart() { }
		public SnakePart(int x, int y)
		{
			_position.X = x;
			_position.Y = y;
		}

		public SnakePart(Vector2 position)
		{
			_position = position;
		}

		// Getters & Setters
		public Vector2 GetPosition()
		{
			return _position;
		}

		public void SetPosition(Vector2 position)
		{
			_position = position;
		}
	}

	public class Snake
	{
		private enum Direction
		{
			Up = 0,
			Down,
			Left,
			Right
		}
		
		
		private Texture2D _bodyPartTexture;
		private List<SnakePart> _snakeParts;
		private Direction _movingDirection;
		public bool _isAlive;

		public Snake(GraphicsDevice graphics, int startX, int startY, int width, int height)
		{
			_snakeParts = new List<SnakePart>();
			_movingDirection = Direction.Down;
			_isAlive = true;

			SnakePart startingBodyPart = new SnakePart(startX, startY);
			_snakeParts.Add(startingBodyPart);

			_bodyPartTexture = new Texture2D(graphics, width, height);
			Color[] pixelData = new Color[width * height];
			for(int i = 0; i < pixelData.Length; i++)
			{
				pixelData[i] = Color.White;
			}
			_bodyPartTexture.SetData(pixelData);
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			for(int i = 0; i < _snakeParts.Count; i++)
			{
				spriteBatch.Draw(_bodyPartTexture, _snakeParts[i].GetPosition(), Color.Red);
			}
		}

		public void Update(GameTime gameTime)
		{
			HandleInput();
			HandleMovement();

			SelfCollisions();
			ScreenBorderCollisions();
		}

		public void Eat()
		{
			SnakePart newPart = new SnakePart(_snakeParts[0].GetPosition());
			_snakeParts.Add(newPart);
		}

		private void HandleInput()
		{
			if(Keyboard.GetState().IsKeyDown(Keys.W))
			{
				_movingDirection = Direction.Up;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.S))
			{
				_movingDirection = Direction.Down;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.A))
			{
				_movingDirection = Direction.Left;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.D))
			{
				_movingDirection = Direction.Right;
			}
		}

		private void HandleMovement()
		{
			Vector2 tempPos = new Vector2();

			switch (_movingDirection)
			{
				case Direction.Up:
					tempPos = _snakeParts[0].GetPosition();
					tempPos.Y -= Constants.TileHeight;
					break;

				case Direction.Down:
					tempPos = _snakeParts[0].GetPosition();
					tempPos.Y += Constants.TileHeight;
					break;

				case Direction.Left:
					tempPos = _snakeParts[0].GetPosition();
					tempPos.X -= Constants.TileWidth;
					break;

				case Direction.Right:
					tempPos = _snakeParts[0].GetPosition();
					tempPos.X += Constants.TileWidth;
					break;
			}

			if (_snakeParts.Count > 1)
			{
				SnakePart tempPart;
				tempPart = _snakeParts[_snakeParts.Count - 1];
				_snakeParts.RemoveAt(_snakeParts.Count - 1);
				tempPart.SetPosition(tempPos);
				_snakeParts.Insert(0, tempPart);
			}
			else
			{
				_snakeParts[0].SetPosition(tempPos);
			}
		}

		private void SelfCollisions()
		{
			for(int i = 0; i < _snakeParts.Count - 1; i++)
			{
				if(i == 0)	continue;

				if(_snakeParts[i].GetPosition() == _snakeParts[0].GetPosition())
				{
					_isAlive = false;
				}
			}
		}

		private void ScreenBorderCollisions()
		{
			if(_snakeParts[0].GetPosition().X >= Constants.ScreenWidth || _snakeParts[0].GetPosition().X < 0 
				|| _snakeParts[0].GetPosition().Y < 0 || _snakeParts[0].GetPosition().Y >= Constants.ScreenHeight)
			{
				_isAlive = false;
			}
		}

		public void Reset()
		{
			_snakeParts.Clear();

			SnakePart initialPart = new SnakePart(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
			_snakeParts.Add(initialPart);
			_movingDirection = Direction.Down;
			_isAlive = true;
		}

		// Getters & Setters
		public Vector2 GetPosition()
		{
			return _snakeParts[0].GetPosition();
		}
	}

	public class Fruit
	{
		private Texture2D _fruitTexture;
		private Vector2 _position;
		private int _width;
		private int _height;

		public Fruit(GraphicsDevice graphics, int w, int h)
		{
			SetRandomPosition();
			_width = w;
			_height = h;

			_fruitTexture = new Texture2D(graphics, w, h);
			Color[] pixelData = new Color[w * h];
			for (int i = 0; i < pixelData.Length; i++)
			{
				pixelData[i] = Color.White;
			}
			_fruitTexture.SetData(pixelData);
		}

		public void Draw(SpriteBatch batch)
		{
			batch.Draw(_fruitTexture, _position, Color.Green);
		}

		public void Eaten()
		{
			SetRandomPosition();
		}

		public void Reset()
		{
			SetRandomPosition();
		}

		private void SetRandomPosition()
		{
			Random rnd = new Random();

			_position.Y = rnd.Next(0, Constants.ScreenHeight / Constants.TileHeight) * Constants.TileHeight;
			_position.X = rnd.Next(0, Constants.ScreenWidth / Constants.TileWidth) * Constants.TileWidth;
		}

		// Getters and Setters
		public Vector2 GetPosition()
		{
			return _position;
		}
	}

	public class Game1 : Game
	{ 
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Snake _snake;
		private Fruit _fruit;

		private int _score;
		private SpriteFont _font;
		private Vector2 _scorePosition;

		private bool _menuWaiting;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			this.IsFixedTimeStep = true;//false;
			this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 10d); //60);
		}

		protected override void Initialize()
		{
			// Window initialising
			_graphics.PreferredBackBufferWidth = Constants.ScreenWidth;
			_graphics.PreferredBackBufferHeight = Constants.ScreenHeight;
			_graphics.ApplyChanges();

			_snake = new Snake(_graphics.GraphicsDevice, Constants.ScreenWidth / 2, Constants.ScreenHeight / 2, Constants.TileWidth, Constants.TileHeight);
			_fruit = new Fruit(_graphics.GraphicsDevice, Constants.TileWidth, Constants.TileHeight);

			_score = 0;
			_font = Content.Load<SpriteFont>("Arial");
			_scorePosition = new Vector2(0, 0);

			_menuWaiting = true;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();


			if(!_menuWaiting)
			{
				_snake.Update(gameTime);

				if(_snake.GetPosition() == _fruit.GetPosition())
				{
					_score++;
					_fruit.Eaten();
					_snake.Eat();
				}

				if(!_snake._isAlive)
				{
					ResetGame();
				}
			}
			else
			{
				if(Keyboard.GetState().IsKeyDown(Keys.Enter))
				{
					_menuWaiting = false;
				}
			}


			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			
			_spriteBatch.Begin();
			_fruit.Draw(_spriteBatch);
			_snake.Draw(_spriteBatch);

			if(_menuWaiting)
			{
				_spriteBatch.DrawString(_font, "Press Enter To Start", new Vector2(0, 0), Color.White);
			}
			else
			{
				string scoreText = "Score: " + _score;
				_spriteBatch.DrawString(_font, scoreText, _scorePosition, Color.White);
			}

			_spriteBatch.End();

			base.Draw(gameTime);
		}

		private void ResetGame()
		{
			_score = 0;
			_snake.Reset();
			_fruit.Reset();
			_menuWaiting = true;
		}
	}
}
