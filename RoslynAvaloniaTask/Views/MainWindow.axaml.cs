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
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
    /* FIELDS CONNECTED WITH SYNTAX TREE TEXT BOX */
    private Dictionary<int, SyntaxNodeOrToken> linesToNodes = new Dictionary<int, SyntaxNodeOrToken>();
    private SyntaxTreeCaretEvent _treeCaretEvent;
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
        //_textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged; /* FUNCTIONALITY DISABLED FOR NOW */
        _textEditor.TextArea.RightClickMovesCaret = false;
        
        /* ADDING FUNCTIONALITIES TO SYNTAX TREE TEXT BOX */
        _treeTextBox.IsReadOnly = true;
        _treeTextBox.PointerMoved += TreeTextBoxOnPointerMoved; 
        _treeCaretEvent += OnTreeCaretEvent;
        
        /* TRYING SOMETHING WITH LINE COLORIZING */
        int offset = _textEditor.CaretOffset;
        DocumentLine currentLine = _textEditor.Document.GetLineByOffset(offset);
        
    }

    private void OnTreeCaretEvent(object sender, SyntaxTreeCaretEventArgs e) /* CARET EVENT HANDLER */
    {
        SyntaxNodeOrToken nodeOrToken = e.NodeOrToken;
        //int lineNumberInFile = e.LineNumber;

        SyntaxNode? node = null;
        SyntaxToken? token = null;
        if (nodeOrToken.IsNode)
            node = nodeOrToken.AsNode();
        if (nodeOrToken.IsToken)
            token = nodeOrToken.AsToken();

        SourceText? sourceText = null;
        string? text = null;
        if (node != null)
        {
            sourceText = node.GetText();
            text = sourceText.ToString();
        }
        if (token != null)
        {
            text = token.ToString();
        }
        
        /* WE HAVE TOKEN OR NODE TEXT  - NOW TIME TO HIGHLIGHT IT IN THE CODE EDITOR */
        FileLinePositionSpan span = nodeOrToken.SyntaxTree.GetLineSpan(nodeOrToken.Span);
                
        int startLine = span.StartLinePosition.Line;
        int endLine = span.EndLinePosition.Line;
        
        /* CHANGING THE SELECTING APPEREANCE */
        _textEditor.TextArea.SelectionBrush = new SolidColorBrush(Colors.Transparent, 1.00);
        //_textEditor.TextArea.SelectionForeground = new SolidColorBrush(Colors.Aqua, 1.00);
        _textEditor.TextArea.SelectionBorder = new Pen(Brushes.Red);
        
        /* Let's check if this is something longer or just "one-liner" */
        if (startLine != endLine)
        {
            /* we have to select multiple lines */
            int startOffset = _textEditor.Document.GetOffset(startLine + 1, 0);
            DocumentLine endDocumentLine = _textEditor.Document.GetLineByNumber(endLine + 1);
            int endOffset = _textEditor.Document.GetOffset(endLine + 1, 0) + endDocumentLine.Length;
            _textEditor.Select(startOffset, endDocumentLine.EndOffset - startOffset);
        }
        
        Console.WriteLine(text);
    }
    
    private void Caret_PositionChanged(object sender, EventArgs e)
    {
        /* WORKING LINE SELECTING */
        int offset = _textEditor.CaretOffset;
        DocumentLine currentLine = _textEditor.Document.GetLineByOffset(offset);
        //_textEditor.TextArea.ClearSelection();
        /* SEE IF IT WORKS */
        
        _textEditor.Select(currentLine.Offset, currentLine.Length);
        //LineColorizer lineColorizer = new LineColorizer(currentLine.LineNumber);
        //lineColorizer.HighlightLine(currentLine);
        /* SEE IF IT WORKS - NO */
        // _textEditor.TextArea.TextView.LineTransformers.Add(new LineColorizer(currentLine.Offset));
        // _textEditor.TextArea.TextView.HighlightedLine = currentLine.LineNumber; 
    }
    
    private void TreeTextBoxOnPointerMoved(object? sender, PointerEventArgs e)
    {
        int caretIndex = _treeTextBox.CaretIndex;
        if (_treeTextBox.Text != null)
        {
            try
            {
                string textBeforeCaret = _treeTextBox.Text.Substring(0, caretIndex);
                int lineNumber = textBeforeCaret.Split('\n').Length;
                lineNumber--;
                
                /* GETTING CORESPONDING NODE AND LINE NUMBER IN SOURCE CODE */
                SyntaxNodeOrToken nodeOrToken = linesToNodes[lineNumber];
                FileLinePositionSpan span = nodeOrToken.SyntaxTree.GetLineSpan(nodeOrToken.Span);
                
                int lineNumberInFile = span.StartLinePosition.Line;
                
                if (_currentLineNumber != lineNumber)
                {
                    /* GENERATING EVENT */
                    SyntaxTreeCaretEventArgs args = new SyntaxTreeCaretEventArgs(nodeOrToken, lineNumberInFile);
                    _treeCaretEvent?.Invoke(this, args);
                    
                    /* DEBUGGING */
                    _currentLineNumber = lineNumber;
                    Console.WriteLine($"Line number in file: {lineNumberInFile + 1}\n");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
            }
        }
    }
    
     private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Traverse();
        Console.WriteLine("Traversing tree...\n");
    }
    
    private void Traverse()
    {
        _treeTextBox.Clear();
        linesToNodes = new Dictionary<int, SyntaxNodeOrToken>(); // setting new instance 
        
        int line = 0;
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
        //linesToNodes.Add(line++, root); // not needed
        
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
                _treeTextBox.Text += currString;
            }
            else
            {
                //If it's a token, you can cast it to SyntaxToken and further investigate
                SyntaxToken token = current.tokenOrNode.AsToken();
                currString += $"Token: [{token.Kind()}]\n";
                _treeTextBox.Text += currString;
            }
            linesToNodes.Add(line++, current.tokenOrNode);
            
            foreach (var tokenOrNode in current.tokenOrNode.ChildNodesAndTokens())
            {
                if (visitedSet.Add(tokenOrNode))
                    myStack.Push((tokenOrNode, current.level + 1));
            }
        }
    }
    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        // Update current line highlight when caret index changes
    }
    
    public class SyntaxTreeCaretEventArgs : EventArgs
    {
        // Property to hold custom data
        public SyntaxNodeOrToken NodeOrToken { get; }
        public int LineNumber { get; }
        
        // Constructor to initialize custom data
        public SyntaxTreeCaretEventArgs(SyntaxNodeOrToken nodeOrToken, int lineNumber)
        {
            NodeOrToken = nodeOrToken;
            LineNumber = lineNumber;
        }
    }
    
    public delegate void SyntaxTreeCaretEvent(object sender, SyntaxTreeCaretEventArgs e);
    
    /* -------------------------------------------------------------------------------------------------------------- */
    /* ------------------------------------------- NOT RELEVANT ----------------------------------------------------- */
    /* -------------------------------------------------------------------------------------------------------------- */
    
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
}