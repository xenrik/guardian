using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

public partial class DynamicLabel : Label
{
    [Export]
    public Variable[] Variables;

    [Export]
    public float UpdateSpeed = 1;

    private Dictionary<string, Expression> expressions = new();
    private List<Token> tokens = new();
    private double update = 0;

    public override void _Ready()
    {
        base._Ready();

        var variableNames = Variables.Select(variable => variable.Name).ToArray();
        foreach (var variable in Variables)
        {
            var expression = new Expression();
            var parseResult = expression.Parse(variable.Expression, variableNames);
            if (parseResult != Error.Ok)
            {
                Logger.Error($"Failed to parse expression: {variable.Expression}");
                continue;
            }

            expressions[variable.Name] = expression;
        }

        var text = Text;

        // Replace node references
        var nodeRefs = 0;
        var regex = new Regex(@"{#(\/?\/?[a-zA-Z0-9]+(\/[a-zA-Z0-9])?)(\.[\.a-zA-Z0-9\/]+)}");
        var matches = regex.Matches(text).Reverse();
        foreach (var match in matches)
        {
            Logger.Debug("Process match");
            foreach (Group group in match.Groups)
            {
                Logger.Debug("Group: " + group.Value);
            }

            string refExpr = $"GetNode('{match.Groups[1].Value}'){match.Groups[3].Value}";

            Logger.Debug($"Ref Expr: {refExpr}");
            var refId = "nodeRef_" + nodeRefs++;
            var expr = new Expression();
            var parseResult = expr.Parse(refExpr);
            if (parseResult != Error.Ok)
            {
                Logger.Error($"Failed to parse node reference expression: {refExpr}");
                continue;
            }

            expressions[refId] = expr;
            text = text.Substring(0, match.Index) +
                "{" + refId + "}" + text.Substring(match.Index + match.Length);
        }

        // Extract Tokens
        var current = 0;
        var next = text.IndexOf("{");
        while (next != -1)
        {
            tokens.Add(new Token(text.Substring(current, next - current), true));

            var tokenEnd = text.IndexOf("}", next);
            if (tokenEnd == -1)
            {
                Logger.Error($"Invalid expression: {Text} - unclosed brace");
                this.ProcessMode = ProcessModeEnum.Disabled;
                return;
            }

            tokens.Add(new Token(text.Substring(next + 1, tokenEnd - next - 1), false));
            current = tokenEnd + 1;
            next = text.IndexOf("{", current);
        }

        if (current < text.Length)
        {
            tokens.Add(new Token(text.Substring(current), true));
        }

        Logger.Debug($"Parsing Expression: {text}");
        Logger.Debug("Tokens:");
        foreach (Token token in tokens)
        {
            Logger.Debug($"\t'{token.Value}' (isPlainText: {token.IsPlainText})");
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // Update based on the speed
        update += delta;
        if (update < UpdateSpeed)
        {
            return;
        }
        update = 0;

        Logger.Debug("Updating!");

        // First build the values
        var values = new Dictionary<string, Variant>();
        foreach (var entry in expressions)
        {
            Godot.Collections.Array valuesArray = new(values.Values);
            var result = entry.Value.Execute(valuesArray, this);
            if (entry.Value.HasExecuteFailed())
            {
                Logger.Error($"Failed to execute expression: {entry.Key}");
                continue;
            }
            else
            {
                values[entry.Key] = result;
            }
        }

        // Now build the text
        string text = "";
        foreach (var token in tokens)
        {
            if (token.IsPlainText)
            {
                text += token.Value;
            }
            else
            {
                text += values[token.Value];
            }
        }

        Text = text;
    }

    public Node GetNode(string path)
    {
        Logger.Debug("Get node with path: " + path);

        Node currentNode;
        int pos = 0;
        while (pos < path.Length)
        {
            if (pos + 2 < path.Length && path.Substring(pos, 2) == "//")
            {

            }
        }

        if (path.StartsWith("//") || path.StartsWith("/"))
        {
            currentNode = GetTree().CurrentScene;
        }


        return null;
        /*
        Node currentNode;
        int pos = 0;
        int sep = 0;
        int offset = 0;
        while (path.Length > 0) {            
            if (path.StartsWith("//")) {
                offset = 2;
            } else if (path.StartsWith("/")) {
                offset = 1;
            } else {
                offset = 0;
            }

            sep = path.IndexOf("/", offset);            
            if (sep == -1) {
                part = path.Substring(offset);
            } else {
                part = path.Substring(offset, sep - offset);
            }

            if (currentNode == null) {
                if (offset > 0) {
                    currentNode = GetTree().CurrentScene;
                } else {
                    currentNode = this;
                }
            }
            
            // } || path.StartsWith("/")) {

        }
            currentNode = GetTree().CurrentScene;
        } else {
            currentNode = this;
        }

        
            path = path.Substring(2);
            var sep = path.IndexOf("/");
            if (sep != -1) {
                var root = path.Substring(0, sep);
                var rem = path.Substring(sep + 1);

                var node = GetTree().CurrentScene.FindChild(root);
            }
        } else if (path.StartsWith("/")) {
            return GetTree().CurrentScene.FindChild(path.Substring(1));
        } else {
            return FindChild(path);
        }
        */
    }

    private class Token
    {
        public readonly bool IsPlainText;
        public readonly string Value;

        public Token(string value, bool isPlainText)
        {
            Value = value;
            IsPlainText = isPlainText;
        }
    }
}
