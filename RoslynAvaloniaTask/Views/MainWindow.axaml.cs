using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TextMateSharp.Grammars;

namespace RoslynAvaloniaTask.Views;

public partial class MainWindow : Window
{
    private readonly TextEditor _textEditor;

    private TextBox _treeTextBox;
    //private RegistryOptions _registryOptions;
    private TextMateSharp.Grammars.RegistryOptions _registryOptions;
    private CompletionWindow _completionWindow;
    private OverloadInsightWindow _insightWindow;
    /* Line Colorizer */
    //private LineColorizer _lineColorizer = new LineColorizer();
    private int _currentLineNumber;
    public MainWindow()
    {
        InitializeComponent();
        _treeTextBox = this.FindControl<TextBox>("treeTextBox");
        _textEditor = this.FindControl<TextEditor>("Editor");
        _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
        _textEditor.Background = Brushes.Transparent;
        _textEditor.ShowLineNumbers = true;
        _registryOptions = new TextMateSharp.Grammars.RegistryOptions(ThemeName.DarkPlus);
        _textEditor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        
        var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
        
        /* ADDING FUNCTIONALITIES TO TEXT EDITOR */
        _textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
        _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        _textEditor.TextArea.RightClickMovesCaret = false;
        
        /* TRYING SOMETHING WITH LINE COLORIZING */
        int offset = _textEditor.CaretOffset;
        DocumentLine currentLine = _textEditor.Document.GetLineByOffset(offset);
    }
    
    private void Caret_PositionChanged(object sender, EventArgs e)
    {
        /* WORKING LINE SELECTING */
        int offset = _textEditor.CaretOffset;
        DocumentLine currentLine = _textEditor.Document.GetLineByOffset(offset);
        _textEditor.Select(currentLine.Offset, currentLine.Length);
        LineColorizer lineColorizer = new LineColorizer(currentLine.LineNumber);
        //lineColorizer.HighlightLine(currentLine);
        /* SEE IF IT WORKS - NO */
        _textEditor.TextArea.TextView.LineTransformers.Add(new LineColorizer(currentLine.Offset));
        _textEditor.TextArea.TextView.HighlightedLine = currentLine.LineNumber; 
    }
    
     private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Traverse();
    }
    
    private void Traverse()
    {
        int level = 0;
        /* initializing stack for DFS on a syntax tree */
        Stack<(SyntaxNodeOrToken tokenOrNode, int level)> myStack =
            new Stack<(SyntaxNodeOrToken tokenOrNode, int level)>();
        HashSet<SyntaxNodeOrToken> visitedSet = new HashSet<SyntaxNodeOrToken>();
        //Console.WriteLine(CodeText);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(_textEditor.Text);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        
        myStack.Push((root, 0));
        visitedSet.Add(root);
        
        _treeTextBox.Text = "";

        while (myStack.Count != 0)
        {
            var current = myStack.Pop();
            string currString = "";
            for (int i = 0; i < current.level; i++)
                currString += "|\t";
            
            // Check if it's a node or token
            if (current.tokenOrNode.IsNode)
            {
                // If it's a node, you can cast it to SyntaxNode and further investigate
                SyntaxNode node = current.tokenOrNode.AsNode();
                currString += $"Node: [{node.Kind()}]\n";
                _treeTextBox.Text += currString + "\n";
            }
            else
            {
                //If it's a token, you can cast it to SyntaxToken and further investigate
                SyntaxToken token = current.tokenOrNode.AsToken();
                currString += $"Token: [{token.Kind()}]\n";
                _treeTextBox.Text += currString + "\n";
            }
            
            foreach (var tokenOrNode in current.tokenOrNode.ChildNodesAndTokens())
            {
                if (visitedSet.Add(tokenOrNode))
                    myStack.Push((tokenOrNode, current.level + 1));
            }
        }
        
        string rootString = root.ToString();
        var rootKind = root.Kind();
        var rootKindString = rootKind.ToString();
    }
    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Update current line highlight when caret index changes
    }
    
    
    /* CODE COMPLETION - NOT WORKING */
    private void textEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
    {
        if (e.Text.Length > 0 && _completionWindow != null)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                _completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        _insightWindow?.Hide();
        // Do not set e.Handled=true.
        // We still want to insert the character that was typed.
    }
    
    private void textEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
    {
        if (e.Text == ".")
        {

            _completionWindow = new CompletionWindow(_textEditor.TextArea);
            _completionWindow.Closed += (o, args) => _completionWindow = null;

            var data = _completionWindow.CompletionList.CompletionData;
            data.Add(new MyCompletionData("Item1"));
            data.Add(new MyCompletionData("Item2"));
            data.Add(new MyCompletionData("Item3"));
            data.Add(new MyCompletionData("Item4"));
            data.Add(new MyCompletionData("Item5"));
            data.Add(new MyCompletionData("Item6"));
            data.Add(new MyCompletionData("Item7"));
            data.Add(new MyCompletionData("Item8"));
            data.Add(new MyCompletionData("Item9"));
            data.Add(new MyCompletionData("Item10"));
            data.Add(new MyCompletionData("Item11"));
            data.Add(new MyCompletionData("Item12"));
            data.Add(new MyCompletionData("Item13"));


            _completionWindow.Show();
        }
        else if (e.Text == "(")
        {
            _insightWindow = new OverloadInsightWindow(_textEditor.TextArea);
            _insightWindow.Closed += (o, args) => _insightWindow = null;

            _insightWindow.Provider = new MyOverloadProvider(new[]
            {
                ("Method1(int, string)", "Method1 description"),
                ("Method2(int)", "Method2 description"),
                ("Method3(string)", "Method3 description"),
            });

            _insightWindow.Show();
        }

        int offset = _textEditor.CaretOffset;
        DocumentLine currentLine = _textEditor.Document.GetLineByOffset(offset);

       
        //_lineColorizer.HighlightLine(currentLine)
    }
    
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            Text = text;
        }

        public IImage Image => null;

        public string Text { get; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description => "Description for " + Text;

        public double Priority { get; } = 0;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
    
    private class MyOverloadProvider : IOverloadProvider
    {
        private readonly IList<(string header, string content)> _items;
        private int _selectedIndex;

        public MyOverloadProvider(IList<(string header, string content)> items)
        {
            _items = items;
            SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(CurrentHeader));
                OnPropertyChanged(nameof(CurrentContent));
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        public int Count => _items.Count;
        public string CurrentIndexText => $"{SelectedIndex + 1} of {Count}";
        public object CurrentHeader => _items[SelectedIndex].header;
        public object CurrentContent => _items[SelectedIndex].content;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /* CODE COMPLETION - NOT WORKING */
    
    public class LineColorizer : DocumentColorizingTransformer
    {
        public void HighlightLine(DocumentLine line)
        {
            ColorizeLine(line);
        }
        
        int lineNumber;

        public LineColorizer(int lineNumber)
        {
            this.lineNumber = lineNumber;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber) {
                ChangeLinePart(line.Offset, line.Length, ApplyChanges);
            }
        }

        void ApplyChanges(VisualLineElement element)
        {
            // This is where you do anything with the line
            element.TextRunProperties.SetForegroundBrush(Brushes.Red);
        }
    }
    
}