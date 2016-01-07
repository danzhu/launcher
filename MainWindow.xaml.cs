using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Engine engine;
        Session session;
        ListBoxItem noMatch = new ListBoxItem();

        public CompletionUnit SelectedCompletion
            => (completion.SelectedItem as CompletionItem)?.Unit;

        public MainWindow()
        {
            InitializeComponent();

            engine = Engine.Load("config.xml");

            session = new Session(engine);
            session.WidgetChanged += (sender, widget) => UpdateWidget(widget);
            session.CommandExecuted += (sender, lvl) => ClearInput();
            session.ExceptionThrown += (sender, e) => DisplayException(e);
            session.ScopeChanged += (sender, cmd) => ClearInput();

            noMatch.Content = "No completion";
        }
        
        public void UpdateText(TextBlock tb, string text = null)
        {
            if (text != null)
            {
                tb.Text = text;
                tb.Visibility = Visibility.Visible;
            }
            else
                tb.Visibility = Visibility.Collapsed;
        }

        public void UpdateCompletion()
        {
            engine.Analyze(session, input.Text);

            Level leaf = session.Leaf;
            Command cmd = leaf.Command;

            // Generate usage
            string usage = "";
            Level lvl = session.Root.Child; // ignore root (or else it looks weird)
            while (lvl != null)
            {
                usage += lvl.Command.Name + ' ';
                lvl = lvl.Child;
            }
            usage += cmd.Usage;
            UpdateText(usageTextBlock, usage);

            // Find parameter info
            string info = cmd.Parameters.ElementAtOrDefault(leaf.FirstUnmatchedParam)?.Info;
            UpdateText(argTextBlock, info != null ? info : cmd.Info);

            // Update completions
            completion.Items.Clear();
            foreach (CompletionItem c in session.Completion.Completions)
                completion.Items.Add(c);

            // TODO: no match display or not?
            if (completion.Items.Count == 0)
                completion.Items.Add(noMatch);

            // Auto select completion if auto complete
            if (session.Completion.AutoComplete)
                completion.SelectedIndex = 0;
        }

        public void AutoComplete(bool forced = false)
        {
            CompletionUnit u = SelectedCompletion;
            if (u == null)
            {
                if (forced && completion.Items[0] is CompletionItem)
                    u = ((CompletionItem)completion.Items[0]).Unit;
                else
                    return;
            }

            // Replace last word
            // TODO: replace current word (might be in the middle)
            input.SelectionStart = session.Input.Index;
            input.SelectionLength = input.Text.Length - session.Input.Index;
            input.SelectedText = u.Complete(session.Input.Text);
            input.SelectionStart = input.Text.Length;
        }

        public void ShiftSelection(int offset)
        {
            int res = completion.SelectedIndex + offset;
            if (res < 0)
                res = 0;
            else if (res >= completion.Items.Count)
                res = completion.Items.Count - 1;
            completion.SelectedIndex = res;
            ((ListBoxItem)completion.SelectedItem).BringIntoView();
        }

        public void ExecuteSelection()
        {
            Level lvl = SelectedCompletion is Command
                ? new Level((Command)SelectedCompletion) : session.Leaf;

            if (lvl == null)
                return;

            session.ExecuteCommand(engine, lvl);
        }

        public void DisplayException(Exception e)
        {
            UpdateText(usageTextBlock, e.GetType().Name);
            UpdateText(argTextBlock, e.Message);
            UpdateText(infoTextBlock);
        }

        public void ClearInput()
        {
            input.Clear();
        }

        public void UpdateWidget(Page e)
        {
            widget.Content = e;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            input.Focus();
            UpdateCompletion();
        }

        private void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCompletion();
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    ShiftSelection(1);
                    break;
                case Key.Up:
                    ShiftSelection(-1);
                    break;
                case Key.PageDown:
                    ShiftSelection(8);
                    break;
                case Key.PageUp:
                    ShiftSelection(-8);
                    break;
                case Key.Tab:
                    AutoComplete(true);
                    break;
                case Key.Space:
                    if (engine.AutoComplete)
                        AutoComplete();
                    return;
                case Key.Enter:
                    if (!engine.AutoLaunch)
                        AutoComplete();
                    ExecuteSelection();
                    break;
                case Key.Escape:
                    if (engine.Main != session.Scope)
                    {
                        session.Scope = engine.Main;
                        UpdateCompletion();
                    }
                    else
                        Close();
                    break;
                default:
                    return;
            }
            e.Handled = true;
        }

        private void completion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateText(infoTextBlock, SelectedCompletion?.Info);
        }
    }
}
