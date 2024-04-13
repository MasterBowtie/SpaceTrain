
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace apedaile{
  public class MainMenuView : GameStateView {

    private enum MenuState {
      NewGame,
      HighScores,
      Settings,
      About,
      Quit,
    }

    private SpriteFont mainFont;
    private SpriteFont titleFont;

    private Texture2D background;
    private Rectangle backRect;
    private Song music;
    private bool canPlayMusic = true;

    private MenuState currentSelection = MenuState.NewGame;
    private GameStateEnum nextState = GameStateEnum.MainMenu;
    private bool waitforKeyRelease = true;

    public override void setupInput(KeyboardInput keyboard) 
    {
      keyboard.registerCommand(Keys.Up, waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp), GameStateEnum.MainMenu, Actions.up);
      keyboard.registerCommand(Keys.Down, waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown), GameStateEnum.MainMenu, Actions.down);
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem), GameStateEnum.MainMenu, Actions.select);
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
      // background = contentManager.Load<Texture2D>("Images/background");
      // backRect = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
    }

    public override GameStateEnum processInput(GameTime gameTime)
    {
      if (nextState != GameStateEnum.MainMenu) {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.MainMenu;
        return nextState;
      }
      return GameStateEnum.MainMenu;
    }
    
    public override void update(GameTime gameTime)
    {
      if (MediaPlayer.State == MediaState.Stopped) {
        // MediaPlayer.Play(music);
      }
    }

    public override void render(GameTime gameTime)
    {
      Vector2 biggest = mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
      spriteBatch.Begin();

      // spriteBatch.Draw(background, backRect, Color.White);

      drawMenuItem(titleFont, "Game Title", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth/2 - titleFont.MeasureString("Game Title").X/2, titleFont.MeasureString("Game Title").X, false);

      float bottom = drawMenuItem(mainFont, "New Game", graphics.PreferredBackBufferHeight * .4f , x, biggest.X + buffer, currentSelection == MenuState.NewGame);

      bottom = drawMenuItem(mainFont, "High Scores", bottom, x, biggest.X + buffer, currentSelection == MenuState.HighScores);
      
      bottom = drawMenuItem(mainFont, "Settings", bottom, x, biggest.X + buffer, currentSelection == MenuState.Settings);
      
      bottom = drawMenuItem(mainFont, "About", bottom, x, biggest.X + buffer, currentSelection == MenuState.About);

      bottom = drawMenuItem(mainFont, "Quit", bottom, x, biggest.X + buffer, currentSelection == MenuState.Quit);

      spriteBatch.End();
    }

    private float drawMenuItem(SpriteFont font, string text, float y, float x, float xSize, bool selected) {
      Vector2 stringSize = font.MeasureString(text);
      
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth/2 - stringSize.X/2, y), selected? Color.Yellow : Color.White);

      return y + stringSize.Y;
    }

    public void moveUp(GameTime gameTime, float value) {
      if (currentSelection != MenuState.NewGame) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != MenuState.Quit) {
        currentSelection = currentSelection + 1;
      }
    }

    public void selectItem(GameTime gameTime, float value) {
      switch (currentSelection) {
        case MenuState.NewGame: {
          nextState = GameStateEnum.Tutorial;
          break;
        }
        case MenuState.HighScores: {
          nextState = GameStateEnum.HighScores;
          break;
        }
        case MenuState.Settings: {
          nextState = GameStateEnum.Settings;
          break;
        }
        case MenuState.About: {
          nextState = GameStateEnum.About;
          break;
        }
        case MenuState.Quit: {
          nextState = GameStateEnum.Exit;
          break;
        }
      }
    }

    public void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    public void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }
  }
}