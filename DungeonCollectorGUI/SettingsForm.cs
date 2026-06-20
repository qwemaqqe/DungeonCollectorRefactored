public class SettingsForm : Form
{
    public Difficulty SelectedDifficulty { get; private set; }

    public SettingsForm(Difficulty current)
    {
        
        SetupUI(current);
    }

    private void SetupUI(Difficulty current)
    {
        this.Text = "Settings";
        this.Size = new Size(300, 200);
        this.BackColor = Color.FromArgb(40, 40, 40);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;

        Label lbl = new Label();
        lbl.Text = "Select Difficulty:";
        lbl.ForeColor = Color.White;
        lbl.Location = new Point(20, 20);
        lbl.AutoSize = true;
        lbl.Font = new Font("Consolas", 11);

        ComboBox combo = new ComboBox();
        combo.Items.AddRange(new object[] { "Easy", "Normal", "Hard" });
        combo.SelectedItem = current.ToString();
        combo.Location = new Point(20, 50);
        combo.Width = 240;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;

        Button btnOk = new Button();
        btnOk.Text = "Save";
        btnOk.Location = new Point(20, 100);
        btnOk.BackColor = Color.FromArgb(70, 70, 70);
        btnOk.ForeColor = Color.White;
        btnOk.FlatStyle = FlatStyle.Flat;
        btnOk.Click += (s, e) =>
        {
            SelectedDifficulty = (Difficulty)Enum.Parse(typeof(Difficulty), combo.SelectedItem.ToString());
            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        this.Controls.Add(lbl);
        this.Controls.Add(combo);
        this.Controls.Add(btnOk);
    }
}