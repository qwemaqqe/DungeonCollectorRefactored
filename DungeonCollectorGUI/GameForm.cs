using System.Drawing;
using System.Windows.Forms;

namespace DungeonCollectorGUI
{
    public class GameForm : Form
    {
        private GameEngine engine;
        private const int TileSize = 48;
        private Label lblLives, lblScore, lblMoves, lblLevel, lblMessage;
        private Panel mapPanel;
        private Button btnRestart, btnSettings;
        private System.Windows.Forms.Timer enemyTimer;
        private bool isTelegraphPhase = true;

        public GameForm()
        {
            engine = new GameEngine();
            SetupUI();
            SubscribeToEvents();
            SetupEnemyTimer();
            ShowMenuDialog();
        }

        private void SetupUI()
        {
            this.Text = "Dungeon Collector";
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;

            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 80;
            topPanel.BackColor = Color.FromArgb(40, 40, 40);

            lblLevel = MakeLabel("Level: 1", 10, 10, Color.White);
            lblLives = MakeLabel("Lives: 3", 150, 10, Color.LightCoral);
            lblScore = MakeLabel("Score: 0", 290, 10, Color.Gold);
            lblMoves = MakeLabel("Moves: 0", 430, 10, Color.LightBlue);
            lblMessage = MakeLabel("", 10, 45, Color.Yellow);
            lblMessage.Width = 600;

            topPanel.Controls.Add(lblLevel);
            topPanel.Controls.Add(lblLives);
            topPanel.Controls.Add(lblScore);
            topPanel.Controls.Add(lblMoves);
            topPanel.Controls.Add(lblMessage);

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 50;
            bottomPanel.BackColor = Color.FromArgb(40, 40, 40);

            btnRestart = new Button();
            btnRestart.Text = "Restart (R)";
            btnRestart.Location = new Point(10, 10);
            btnRestart.BackColor = Color.FromArgb(70, 70, 70);
            btnRestart.ForeColor = Color.White;
            btnRestart.FlatStyle = FlatStyle.Flat;
            btnRestart.Click += (s, e) => { engine.ResetCurrentLevel(); AdjustWindowSize(); mapPanel.Invalidate(); };

            btnSettings = new Button();
            btnSettings.Text = "Settings";
            btnSettings.Location = new Point(130, 10);
            btnSettings.BackColor = Color.FromArgb(70, 70, 70);
            btnSettings.ForeColor = Color.White;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Click += (s, e) => ShowSettingsDialog();

            bottomPanel.Controls.Add(btnRestart);
            bottomPanel.Controls.Add(btnSettings);

            mapPanel = new DoubleBufferedPanel();
            mapPanel.Dock = DockStyle.Fill;
            mapPanel.BackColor = Color.Black;
            mapPanel.Paint += MapPanel_Paint;

            this.Controls.Add(mapPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void SetupEnemyTimer()
        {
            enemyTimer = new System.Windows.Forms.Timer();
            enemyTimer.Interval = 500;
            enemyTimer.Tick += (s, e) =>
            {
                if (isTelegraphPhase)
                    engine.PrepareEnemyMoves();
                else
                    engine.ExecuteEnemyMoves();

                isTelegraphPhase = !isTelegraphPhase;
                mapPanel.Invalidate();
            };
            enemyTimer.Start();
        }

        private Label MakeLabel(string text, int x, int y, Color color)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.ForeColor = color;
            lbl.AutoSize = true;
            lbl.Font = new Font("Consolas", 12, FontStyle.Bold);
            return lbl;
        }

        private void SubscribeToEvents()
        {
            engine.OnStateChanged += () =>
            {
                AdjustWindowSize();
                UpdateHUD();
                mapPanel.Invalidate();
            };

            engine.OnMessage += (e) =>
            {
                lblMessage.Text = e.Data;
            };

            engine.OnGameOver += () =>
            {
                enemyTimer.Stop();
                UpdateHUD();
                mapPanel.Invalidate();
                DialogResult result = MessageBox.Show(
                    $"GAME OVER!\nScore: {engine.Player.Score}\nMoves: {engine.Player.Moves}\n\nPlay again?",
                    "Game Over",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    engine.StartNewGame();
                    AdjustWindowSize();
                    UpdateHUD();
                    mapPanel.Invalidate();
                    enemyTimer.Start();
                }
                else
                {
                    this.Close();
                }
            };

            engine.OnWin += () =>
            {
                enemyTimer.Stop();
                UpdateHUD();
                mapPanel.Invalidate();
                DialogResult result = MessageBox.Show(
                    $"YOU WIN!\nScore: {engine.Player.Score}\nMoves: {engine.Player.Moves}\n\nPlay again?",
                    "You Win!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    engine.StartNewGame();
                    AdjustWindowSize();
                    UpdateHUD();
                    mapPanel.Invalidate();
                    enemyTimer.Start();
                }
                else
                {
                    this.Close();
                }
            };
        }

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            if (engine.Map == null) return;

            Graphics g = e.Graphics;

            
            for (int y = 0; y < engine.Map.Height; y++)
            {
                for (int x = 0; x < engine.Map.Width; x++)
                {
                    MapElement element = engine.Map[x, y];
                    Rectangle rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    using (SolidBrush brush = new SolidBrush(element.GetDisplayColor()))
                        g.FillRectangle(brush, rect);

                    g.DrawRectangle(Pens.Black, rect);

                    string symbol = element.Symbol.ToString();
                    using (Font font = new Font("Consolas", 18, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        SizeF textSize = g.MeasureString(symbol, font);
                        float tx = rect.X + (TileSize - textSize.Width) / 2;
                        float ty = rect.Y + (TileSize - textSize.Height) / 2;
                        g.DrawString(symbol, font, textBrush, tx, ty);
                    }
                }
            }

            
            foreach (Enemy enemy in engine.Enemies)
            {
                if (enemy.HasTarget)
                {
                    Rectangle warnRect = new Rectangle(
                        enemy.NextX * TileSize,
                        enemy.NextY * TileSize,
                        TileSize, TileSize);

                    using (SolidBrush warnBrush = new SolidBrush(Color.FromArgb(120, 255, 0, 0)))
                        g.FillRectangle(warnBrush, warnRect);
                }
            }

            
            foreach (Enemy enemy in engine.Enemies)
            {
                Rectangle enemyRect = new Rectangle(
                    enemy.X * TileSize,
                    enemy.Y * TileSize,
                    TileSize, TileSize);

                using (SolidBrush brush = new SolidBrush(Color.DarkRed))
                    g.FillRectangle(brush, enemyRect);

                g.DrawRectangle(Pens.Black, enemyRect);

                using (Font font = new Font("Consolas", 18, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    SizeF textSize = g.MeasureString("M", font);
                    float tx = enemyRect.X + (TileSize - textSize.Width) / 2;
                    float ty = enemyRect.Y + (TileSize - textSize.Height) / 2;
                    g.DrawString("M", font, textBrush, tx, ty);
                }
            }

            
            Rectangle playerRect = new Rectangle(
                engine.Player.X * TileSize,
                engine.Player.Y * TileSize,
                TileSize, TileSize);

            using (SolidBrush brush = new SolidBrush(Color.DodgerBlue))
                g.FillRectangle(brush, playerRect);

            g.DrawRectangle(Pens.Black, playerRect);

            using (Font font = new Font("Consolas", 18, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                SizeF textSize = g.MeasureString("@", font);
                float tx = playerRect.X + (TileSize - textSize.Width) / 2;
                float ty = playerRect.Y + (TileSize - textSize.Height) / 2;
                g.DrawString("@", font, textBrush, tx, ty);
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (engine.Map == null) return;

            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: engine.MovePlayer(0, -1); break;
                case Keys.S: case Keys.Down: engine.MovePlayer(0, 1); break;
                case Keys.A: case Keys.Left: engine.MovePlayer(-1, 0); break;
                case Keys.D: case Keys.Right: engine.MovePlayer(1, 0); break;
                case Keys.R: engine.ResetCurrentLevel(); AdjustWindowSize(); mapPanel.Invalidate(); break;
            }
        }

        private void AdjustWindowSize()
        {
            if (engine.Map == null) return;
            int mapWidth = engine.Map.Width * TileSize;
            int mapHeight = engine.Map.Height * TileSize;
            this.ClientSize = new Size(mapWidth, mapHeight + 80 + 50);
        }

        private void UpdateHUD()
        {
            lblLives.Text = $"Lives: {engine.Player.Lives}";
            lblScore.Text = $"Score: {engine.Player.Score}";
            lblMoves.Text = $"Moves: {engine.Player.Moves}";
            lblLevel.Text = $"Level: {engine.CurrentLevel}/5  Difficulty: {engine.Difficulty}";
        }

        private void ShowMenuDialog()
        {
            DialogResult result = MessageBox.Show(
                "Welcome to Dungeon Collector!\nReady to start?",
                "Dungeon Collector",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                engine.StartNewGame();
                AdjustWindowSize();
                UpdateHUD();
                mapPanel.Invalidate();
            }
            else
            {
                this.Close();
            }
        }

        private void ShowSettingsDialog()
        {
            SettingsForm settingsForm = new SettingsForm(engine.Difficulty);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                engine.SetDifficulty(settingsForm.SelectedDifficulty);
                lblMessage.Text = $"Difficulty set to {engine.Difficulty}";
            }
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }
}