using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using JeopardyCore;

namespace JeopardyCore
{
    class JeoController
    {
        private JeoModel db = new JeoModel();
        private TableLayoutPanel viewPanel; 
        private Random rando = new Random();
        private JeoGame game;
        private TextBox input;
        private TextBox bid;
        private int bidAmount;
        private int ddMinMax;
        private JeoQuestion currentQ;
        private Form mainView;
        private Button cashLabel;
        private RoundType currentRound;
        private bool exit = false;

        public const int VALUESIZE = 45;
        public const int GENERALSIZE = 20;
        public const int SMALLSIZE = 15;
        public const int ROWHEIGHT = 70;

        public JeoController()
        {
            game = new JeoGame(db.RandomRound(RoundType.First, true), db.RandomRound(RoundType.Second, true), db.GetFinal());

            mainView = BuildMain(RoundType.First);
            currentRound = RoundType.First;
            mainView.ShowDialog();

            if (!exit)
            {
                mainView = BuildMain(RoundType.Second);
                currentRound = RoundType.Second;
                mainView.ShowDialog();
            }
            if (!exit)
            {
                mainView = BuildFinal();
                currentQ = game.FinalQuestion;
                currentRound = RoundType.Final;
                mainView.ShowDialog();
            }
        }

        private Form BuildMain(RoundType round)
        {
            Form retForm = new Form();
            retForm.BackColor = Color.LightGray;

            viewPanel = new TableLayoutPanel();
            viewPanel.Dock = DockStyle.Fill;
            viewPanel.RowCount = 2;
            viewPanel.ColumnCount = 6;
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));

            cashLabel = new Button();
            FormatButton(ref cashLabel);
            cashLabel.Text = "Your Current Winnings: $" + game.Cash.ToString();
            viewPanel.Controls.Add(cashLabel);
            viewPanel.SetColumnSpan(cashLabel, 4);

            Button newGame = new Button();
            FormatButton(ref newGame);
            newGame.Text = "New Game";
            viewPanel.Controls.Add(newGame);

            Button exit = new Button();
            FormatButton(ref exit);
            exit.Click += new EventHandler(ExitClick);
            exit.Text = "Exit Game";
            viewPanel.Controls.Add(exit);

            List<JeoCategory<JeoQuestion>> iterateRound = new List<JeoCategory<JeoQuestion>>();
            if (round == RoundType.First) iterateRound = game.FirstRound;
            if (round == RoundType.Second) iterateRound = game.SecondRound;
            foreach (JeoCategory<JeoQuestion> category in iterateRound)
            {

                TableLayoutPanel cat = new TableLayoutPanel();
                cat.Dock = DockStyle.Fill;
                cat.RowCount = 6;
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));

                Button catLabel = new Button();
                catLabel.Text = category.CatName;
                FormatButton(ref catLabel);
                cat.Controls.Add(catLabel);

                int roundMultiplier = 0;
                if (round == RoundType.First) roundMultiplier = 200;
                if (round == RoundType.Second) roundMultiplier = 400;
                //try to add a question for each value. If one isn't found, add the category's daily double question
                JeoButton addButton;
                for (int i = 0; i < 5; i++)
                {
                    addButton = new JeoButton(category.Find(x => x.Value == (i * roundMultiplier) + roundMultiplier));
                    if (addButton.Question == null) addButton = new JeoButton(category.Find(x => x.Type == QType.Double));
                    addButton.Text = "$" + ((i * roundMultiplier) + roundMultiplier).ToString();
                    //testcode
                    if (addButton.Question.Type == QType.Double) addButton.Text = "DOUBLE";
                    FormatButton(ref addButton);
                    addButton.Font = new Font("Arial", VALUESIZE, FontStyle.Bold);
                    addButton.Click += new EventHandler(QClick);
                    cat.Controls.Add(addButton);
                    if (round == RoundType.First) game.FRQuestions.Add(addButton);
                    if (round == RoundType.Second) game.SRQuestions.Add(addButton);
                }
                viewPanel.Controls.Add(cat);
            }
            retForm.Controls.Add(viewPanel);
            retForm.FormBorderStyle = FormBorderStyle.None;
            retForm.WindowState = FormWindowState.Maximized;
            return retForm;
        }
        private Form BuildFinal()
        {
            Form retForm = new Form();
            if (game.Cash > 0)
                {
                    retForm.BackColor = Color.LightGray;
                    retForm.FormBorderStyle = FormBorderStyle.None;
                    retForm.WindowState = FormWindowState.Maximized;

                    viewPanel = new TableLayoutPanel();
                    viewPanel.Dock = DockStyle.Fill;
                    viewPanel.RowCount = 4;
                    viewPanel.ColumnCount = 2;
                    viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
                    viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                    viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                    viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                    viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

                    //create and add category heading button
                    Button cat = new Button();
                    FormatButton(ref cat);
                    cat.Text = game.FinalQuestion.Category;
                    viewPanel.Controls.Add(cat);
                    viewPanel.SetColumnSpan(cat, 2);

                    //create and add value display button
                    Button val = new Button();
                    FormatButton(ref val);
                    val.Text = "Final Jeopardy!";
                    viewPanel.Controls.Add(val);

                    //create and add clue display button. Set font to be smaller than default formatting
                    Button clu = new Button();
                    FormatButton(ref clu);
                    clu.Font = new Font("Arial", 15, FontStyle.Bold);
                    clu.Text = game.FinalQuestion.Clue;
                    viewPanel.Controls.Add(clu);

                    //create and add bid button
                    Button bidLabel = new Button();
                    FormatButton(ref bidLabel);
                    bidLabel.Font = new Font("Arial", 15, FontStyle.Bold);
                    bidLabel.Text = "Your Bid (Maximum $" + game.Cash + ")";
                    viewPanel.Controls.Add(bidLabel);

                    //create and add bid text field
                    bid = new TextBox();
                    bid.ForeColor = Color.Blue;
                    bid.BackColor = Color.Yellow;
                    bid.Font = new Font("Arial", 20, FontStyle.Bold);
                    bid.Multiline = true;
                    bid.Dock = DockStyle.Fill;
                    bid.TextAlign = HorizontalAlignment.Center;
                    viewPanel.Controls.Add(bid);

                    //create and add submit button
                    Button sub = new Button();
                    FormatButton(ref sub);
                    sub.Text = "Submit";
                    sub.Click += new EventHandler(QuestionSub);
                    viewPanel.Controls.Add(sub);

                    //create and add input text field
                    input = new TextBox();
                    input.ForeColor = Color.Blue;
                    input.BackColor = Color.Yellow;
                    input.Font = new Font("Arial", 20, FontStyle.Bold);
                    input.Multiline = true;
                    input.Dock = DockStyle.Fill;
                    input.TextAlign = HorizontalAlignment.Center;
                    viewPanel.Controls.Add(input);

                    retForm.Controls.Add(viewPanel);
                }
            return retForm;
        }

        private bool CheckFinished(List<JeoButton> questions)
        {
            bool retBool = true;
            foreach (JeoButton btn in questions)
            {
                if (btn.Answered == false)
                {
                    retBool = false;
                    break;
                }
            }
            return retBool;
        }
        
        private void QClick(object e, EventArgs args)
        {
            viewPanel.Hide();
            JeoButton btn = (JeoButton)e;
            btn.Text = "";
            btn.Click -= QClick;
            btn.Answered = true;
            currentQ = btn.Question;

            if (currentQ.Type == QType.Standard)
            {
                Form view = new Form();
                view.FormBorderStyle = FormBorderStyle.None;
                view.BackColor = Color.LightGray;
                view.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, (70 * 3));
                TableLayoutPanel parent = new TableLayoutPanel();
                parent.Dock = DockStyle.Fill;
                parent.RowCount = 3;
                parent.ColumnCount = 2;
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

                //create and add category heading button
                Button cat = new Button();
                FormatButton(ref cat);
                cat.Text = btn.Question.Category;
                parent.Controls.Add(cat);
                parent.SetColumnSpan(cat, 2);

                //create and add value display button
                Button val = new Button();
                FormatButton(ref val);
                val.Text = "$" + btn.Question.Value;
                parent.Controls.Add(val);

                //create and add clue display button. Set font to be smaller than default formatting
                Button clu = new Button();
                FormatButton(ref clu);
                clu.Font = new Font("Arial", 15, FontStyle.Bold);
                clu.Text = btn.Question.Clue;
                parent.Controls.Add(clu);

                //create and add submit button
                Button sub = new Button();
                FormatButton(ref sub);
                sub.Text = "Submit";
                sub.Click += new EventHandler(QuestionSub);
                parent.Controls.Add(sub);

                //create and add input text field
                input = new TextBox();
                input.ForeColor = Color.Blue;
                input.BackColor = Color.Yellow;
                input.Font = new Font("Arial", 20, FontStyle.Bold);
                input.Multiline = true;
                input.Dock = DockStyle.Fill;
                input.TextAlign = HorizontalAlignment.Center;
                parent.Controls.Add(input);

                view.Controls.Add(parent);
                view.ShowDialog();
            }
            else if (currentQ.Type == QType.Double)
            {
                Form view = new Form();
                view.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, (70 * 4));
                view.BackColor = Color.LightGray;
                view.FormBorderStyle = FormBorderStyle.None;
                TableLayoutPanel parent = new TableLayoutPanel();
                parent.Dock = DockStyle.Fill;
                parent.RowCount = 4;
                parent.ColumnCount = 2;
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

                //create and add category heading button
                Button cat = new Button();
                FormatButton(ref cat);
                cat.Text = btn.Question.Category;
                parent.Controls.Add(cat);
                parent.SetColumnSpan(cat, 2);

                //create and add value display button
                Button val = new Button();
                FormatButton(ref val);
                val.Text = "Daily Double!";
                parent.Controls.Add(val);

                //create and add clue display button. Set font to be smaller than default formatting
                Button clu = new Button();
                FormatButton(ref clu);
                clu.Font = new Font("Arial", 15, FontStyle.Bold);
                clu.Text = btn.Question.Clue;
                parent.Controls.Add(clu);

                //create and add bid button
                Button bidLabel = new Button();
                FormatButton(ref bidLabel);
                bidLabel.Font = new Font("Arial", 15, FontStyle.Bold);
                if (currentQ.Round == RoundType.First) ddMinMax = 1000;
                if (currentQ.Round == RoundType.Second) ddMinMax = 2000;
                bidLabel.Text = "Your Bid (Maximum $" + Math.Max(ddMinMax, game.Cash) + ")";
                parent.Controls.Add(bidLabel);

                //create and add bid text field
                bid = new TextBox();
                bid.ForeColor = Color.Blue;
                bid.BackColor = Color.Yellow;
                bid.Font = new Font("Arial", 20, FontStyle.Bold);
                bid.Multiline = true;
                bid.Dock = DockStyle.Fill;
                bid.TextAlign = HorizontalAlignment.Center;
                parent.Controls.Add(bid);

                //create and add submit button
                Button sub = new Button();
                FormatButton(ref sub);
                sub.Text = "Submit";
                sub.Click += new EventHandler(QuestionSub);
                parent.Controls.Add(sub);

                //create and add input text field
                input = new TextBox();
                input.ForeColor = Color.Blue;
                input.BackColor = Color.Yellow;
                input.Font = new Font("Arial", 20, FontStyle.Bold);
                input.Multiline = true;
                input.Dock = DockStyle.Fill;
                input.TextAlign = HorizontalAlignment.Center;
                parent.Controls.Add(input);

                view.Controls.Add(parent);
                view.ShowDialog();
            }
        }
        private void QuestionSub(object e, EventArgs args)
        {
            bool valid = true;
            if (currentQ.Type == QType.Double)
            {
                int bidTest;
                bid.Text = bid.Text.TrimStart('$');
                if (int.TryParse(bid.Text, out bidTest)) if(bidTest <= Math.Max(ddMinMax, game.Cash)) bidAmount = bidTest;
                else valid = false;
            }
            if (input.Text == "") valid = false;
            if (valid)
            {
                Control caller = (Control)e;
                string answer = input.Text;
                caller.Parent.Parent.Dispose();

                Form view = new Form();
                view.Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, (75 * 3));
                view.BackColor = Color.LightGray;
                view.FormBorderStyle = FormBorderStyle.None;
                TableLayoutPanel parent = new TableLayoutPanel();
                parent.Dock = DockStyle.Fill;
                parent.RowCount = 3;
                parent.ColumnCount = 2;
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
                parent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
                parent.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));

                Button answerLabel = new Button();
                FormatButton(ref answerLabel);
                answerLabel.Text = "Correct Answer";
                answerLabel.Font = new Font("Arial", SMALLSIZE, FontStyle.Bold);
                parent.Controls.Add(answerLabel);

                Button answerText = new Button();
                FormatButton(ref answerText);
                answerLabel.Font = new Font("Arial", SMALLSIZE, FontStyle.Bold);
                answerText.Text = currentQ.Answer;
                parent.Controls.Add(answerText);

                Button inputLabel = new Button();
                FormatButton(ref inputLabel);
                answerLabel.Font = new Font("Arial", SMALLSIZE, FontStyle.Bold);
                inputLabel.Text = "Your Answer";
                parent.Controls.Add(inputLabel);

                Button inputText = new Button();
                FormatButton(ref inputText);
                answerLabel.Font = new Font("Arial", SMALLSIZE, FontStyle.Bold);
                inputText.Text = answer;
                parent.Controls.Add(inputText);

                TableLayoutPanel yn = new TableLayoutPanel();
                yn.Dock = DockStyle.Fill;
                yn.ColumnCount = 2;
                yn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                yn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

                Button yes = new Button();
                FormatButton(ref yes);
                yes.Text = "I Was Correct";
                yes.Click += new EventHandler(CorrectClick);
                yn.Controls.Add(yes);

                Button no = new Button();
                FormatButton(ref no);
                no.Text = "I Was Not Correct";
                no.Click += new EventHandler(IncorrectClick);
                yn.Controls.Add(no);

                parent.Controls.Add(yn);
                parent.SetColumnSpan(yn, 2);

                view.Controls.Add(parent);
                view.ShowDialog();
            }
        }
        private void ExitClick(object e, EventArgs args)
        {
            Button btn = (Button)e;
            btn.Parent.Parent.Dispose();
        }
        private void CorrectClick(object e, EventArgs args)
        {
            Control caller = (Control)e;
            if (currentQ.Type != QType.Standard)
            {
                game.Cash += (bidAmount * 2);
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
                caller.Parent.Parent.Parent.Dispose();
            }
            else
            {
                game.Cash += currentQ.Value;
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
                caller.Parent.Parent.Parent.Dispose();
            }

            bool isFinished = false;
            if (currentRound == RoundType.First) isFinished = CheckFinished(game.FRQuestions);
            if (currentRound == RoundType.Second) isFinished = CheckFinished(game.SRQuestions);

            if (isFinished) mainView.Dispose(); 
            else viewPanel.Show();

        }
        private void IncorrectClick(object e, EventArgs args)
        {
            Control caller = (Control)e;

            if (currentQ.Type != QType.Standard)
            {
                game.Cash -= bidAmount;
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
            }
            else
            {
                game.Cash -= currentQ.Value;
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
            }
                if (currentRound != RoundType.Final)
            {
                caller.Parent.Parent.Parent.Dispose();
                viewPanel.Show();
            }
        }

        private void FormatButton(ref Button btn)
        {
            btn.Dock = DockStyle.Fill;
            btn.ForeColor = Color.Yellow;
            btn.BackColor = Color.Blue;
            btn.Font = new Font("Arial", GENERALSIZE, FontStyle.Bold);
        }
        private void FormatButton(ref JeoButton btn)
        {
            btn.Dock = DockStyle.Fill;
            btn.ForeColor = Color.Yellow;
            btn.BackColor = Color.Blue;
            btn.Font = new Font("Arial", GENERALSIZE, FontStyle.Bold);
        }

    }
}
