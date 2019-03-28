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
        private bool newGame = false;
        private bool didFinal = true;

        public const int VALUESIZE = 45;
        public const int GENERALSIZE = 20;
        public const int SMALLSIZE = 15;
        public const int ROWHEIGHT = 70;
        public const int FINISHEDSIZE = 90;

        public JeoController()
        {

        }
        public bool StartGame()
        {
            game = BuildGame();

            mainView = BuildMain(RoundType.First);
            currentRound = RoundType.First;
            mainView.ShowDialog();

            if (!exit && !newGame)
            {
                mainView = BuildMain(RoundType.Second);
                currentRound = RoundType.Second;
                mainView.ShowDialog();
            }
            if (!exit && !newGame)
            {
                if (game.Cash > 0)
                {
                    currentQ = game.FinalQuestion;
                    mainView = BuildFinal();
                    currentRound = RoundType.Final;
                    mainView.ShowDialog();
                }
                else didFinal = false;
            }
            if (!exit && !newGame)
            {
                mainView = BuildFinished();
                mainView.ShowDialog();
            }
            return exit;
        }

        //build a JeoGame. if a category was used both before and after the clue value increase in 2001, there is a chance for
        //questions to be nulled. This cannot be fixed without rebuilding the database to include airdate of each question.
        //until that is done, re-build the game until those categories aren't included
        private JeoGame BuildGame()
        {
            JeoModel db = new JeoModel();

            JeoGame retGame;
            do
            {
                retGame = new JeoGame(db.RandomRound(RoundType.First, true), db.RandomRound(RoundType.Second, true), db.GetFinal());
            } while (CheckGameNull(retGame));

            db = null;

            return retGame;
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
            newGame.Click += new EventHandler(NewHandler);
            viewPanel.Controls.Add(newGame);

            Button exit = new Button();
            FormatButton(ref exit);
            exit.Click += new EventHandler(ExitHandler);
            exit.Text = "Exit Game";
            viewPanel.Controls.Add(exit);

            List<JeoCategory<JeoQuestion>> iterateRound = new List<JeoCategory<JeoQuestion>>();
            if (round == RoundType.First) iterateRound = game.FirstRound;
            if (round == RoundType.Second) iterateRound = game.SecondRound;
            foreach (JeoCategory<JeoQuestion> category in iterateRound)
            {

                //panel that holds a single category
                TableLayoutPanel cat = new TableLayoutPanel();
                cat.Dock = DockStyle.Fill;
                cat.RowCount = 6;
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
                cat.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));

                //category heading button
                Button catButton = new Button();
                catButton.Text = category.CatName;
                FormatButton(ref catButton);
                cat.Controls.Add(catButton);

                //handles different question values for each round
                int roundMultiplier = 0;
                if (round == RoundType.First) roundMultiplier = 200;
                if (round == RoundType.Second) roundMultiplier = 400;

                //try to add a question for each value. If one isn't found, add the category's daily double question
                JeoButton addButton = null;
                for (int i = 0; i < 5; i++)
                {
                    addButton = new JeoButton(category.Find(x => x.Value == (i * roundMultiplier) + roundMultiplier));
                    if (addButton.Question == null)
                    {
                        addButton = new JeoButton(category.Find(x => x.Type == QType.Double));
                    }

                    //format and register question button
                    addButton.Text = "$" + ((i * roundMultiplier) + roundMultiplier).ToString();
                    FormatButton(ref addButton);
                    addButton.Font = new Font("Arial", VALUESIZE, FontStyle.Bold);
                    addButton.Click += new EventHandler(QChosen);
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

            retForm.BackColor = Color.LightGray;
            retForm.FormBorderStyle = FormBorderStyle.None;
            retForm.WindowState = FormWindowState.Maximized;

            viewPanel = new TableLayoutPanel();
            viewPanel.Dock = DockStyle.Fill;
            viewPanel.RowCount = 5;
            viewPanel.ColumnCount = 2;
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, ROWHEIGHT));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, ROWHEIGHT));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, ROWHEIGHT));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, ROWHEIGHT));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

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
            sub.Click += new EventHandler(QSubmited);
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
            mainView.AcceptButton = sub;
            mainView.ActiveControl = input;

            return retForm;
        }
        private Form BuildFinished()
        {
            Form retForm = new Form();
            retForm.BackColor = Color.LightGray;
            retForm.FormBorderStyle = FormBorderStyle.None;
            retForm.WindowState = FormWindowState.Maximized;

            viewPanel = new TableLayoutPanel();
            viewPanel.Dock = DockStyle.Fill;
            viewPanel.RowCount = 2;
            viewPanel.ColumnCount = 2;
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            viewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            viewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            //final winnings heading button
            Button winnings = new Button();
            FormatButton(ref winnings);
            winnings.Font = new Font("Arial", FINISHEDSIZE, FontStyle.Bold);
            winnings.Text = "Your Final Winnings Were: $" + game.Cash;
            if (game.Cash > 0) winnings.Text += ". Congratulations!";
            viewPanel.Controls.Add(winnings);
            viewPanel.SetColumnSpan(winnings, 2);

            //new game button
            Button newGame = new Button();
            FormatButton(ref newGame);
            newGame.Font = new Font("Arial", FINISHEDSIZE, FontStyle.Bold);
            newGame.Text = "New Game";
            newGame.Click += new EventHandler(NewHandler);
            viewPanel.Controls.Add(newGame);

            //exit game button
            Button exit = new Button();
            FormatButton(ref exit);
            exit.Font = new Font("Arial", FINISHEDSIZE, FontStyle.Bold);
            exit.Click += new EventHandler(ExitHandler);
            exit.Text = "Exit Game";
            viewPanel.Controls.Add(exit);

            if (!didFinal)
            {
                winnings.Text = "Sorry! You do not have enough winnings to play Final Jeopardy";
            }

            retForm.Controls.Add(viewPanel);
            return retForm;
        }

        private bool CheckGameNull(JeoGame game)
        {
            bool checkNull = false;

            foreach(JeoCategory<JeoQuestion> category in game.FirstRound)
            {
                foreach(JeoQuestion question in category)
                {
                    if (question == null) checkNull = true;
                }
            }
            foreach (JeoCategory<JeoQuestion> category in game.SecondRound)
            {
                foreach (JeoQuestion question in category)
                {
                    if (question == null) checkNull = true;
                }
            }

            return checkNull;
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
        
        private void QChosen(object e, EventArgs args)
        {
            viewPanel.Hide();
            JeoButton btn = (JeoButton)e;
            btn.Text = "";
            btn.Click -= QChosen;
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
                sub.Click += new EventHandler(QSubmited);
                parent.Controls.Add(sub);
                view.AcceptButton = sub;

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
                view.ActiveControl = input;
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
                sub.Click += new EventHandler(QSubmited);
                parent.Controls.Add(sub);
                view.AcceptButton = sub;

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
                view.ActiveControl = bid;
                view.ShowDialog();
            }
        }
        private void QSubmited(object e, EventArgs args)
        {
            bool valid = true;
            if (currentQ.Type != QType.Standard)
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

                if (currentQ.Type != QType.Final) caller.Parent.Parent.Dispose();
                else viewPanel.Hide();

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
                yes.Click += new EventHandler(CorrectAns);
                yn.Controls.Add(yes);

                Button no = new Button();
                FormatButton(ref no);
                no.Text = "I Was Not Correct";
                no.Click += new EventHandler(IncorrectAns);
                yn.Controls.Add(no);

                parent.Controls.Add(yn);
                parent.SetColumnSpan(yn, 2);

                view.Controls.Add(parent);
                view.ShowDialog();
            }
        }
        private void ExitHandler(object e, EventArgs args)
        {
            Button btn = (Button)e;
            exit = true;
            btn.Parent.Parent.Dispose();
        }
        private void NewHandler(object e, EventArgs args)
        {
            Button btn = (Button)e;
            exit = false;
            newGame = true;
            btn.Parent.Parent.Dispose();
        }
        private void CorrectAns(object e, EventArgs args)
        {
            Control caller = (Control)e;

            if (currentQ.Type != QType.Final)
            {
                if (currentQ.Type == QType.Double)
                {
                    game.Cash += bidAmount;
                    cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                    mainView.Update();
                    caller.Parent.Parent.Parent.Dispose();
                }
                if (currentQ.Type == QType.Standard)
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
            else
            {
                game.Cash += bidAmount;
                mainView.Dispose();
                caller.Parent.Parent.Parent.Dispose();
            }
        }
        private void IncorrectAns(object e, EventArgs args)
        {
            Control caller = (Control)e;

            if (currentQ.Type == QType.Double)
            {
                game.Cash -= bidAmount;
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
            }
            if (currentQ.Type == QType.Standard)
            {
                game.Cash -= currentQ.Value;
                cashLabel.Text = "Your Current Winnings: $" + game.Cash;
                mainView.Update();
            }
            if (currentRound != RoundType.Final)
            {
                viewPanel.Show();
            }
            if (currentQ.Round == RoundType.Final)
            {
                game.Cash -= bidAmount;
                mainView.Dispose();
            }
            caller.Parent.Parent.Parent.Dispose();
        }

        private void FormatButton(ref Button btn)
        {
            btn.Dock = DockStyle.Fill;
            btn.ForeColor = Color.Yellow;
            btn.BackColor = Color.Blue;
            btn.UseMnemonic = false;
            btn.Font = new Font("Arial", GENERALSIZE, FontStyle.Bold);
        }
        private void FormatButton(ref JeoButton btn)
        {
            btn.Dock = DockStyle.Fill;
            btn.ForeColor = Color.Yellow;
            btn.BackColor = Color.Blue;
            btn.UseMnemonic = false;
            btn.Font = new Font("Arial", GENERALSIZE, FontStyle.Bold);
        }

    }
}
