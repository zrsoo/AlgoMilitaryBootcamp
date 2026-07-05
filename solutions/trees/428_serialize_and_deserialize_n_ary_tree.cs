using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;
using Microsoft.VisualBasic;

// LC 428 — Serialize and Deserialize N-ary Tree
// Definition the judge provides:
public class Node {
    public int val;
    public IList<Node> children;

    public Node() {
        children = new List<Node>();
    }

    public Node(int _val) {
        val = _val;
        children = new List<Node>();
    }

    public Node(int _val, IList<Node> _children) {
        val = _val;
        children = _children;
    }
}

public class Codec {
    // Encodes a tree to a single string.
    public string serialize(Node root) {
        if(root == null) return "";

        var sb = new StringBuilder();
        sb.Append(root.val).Append(',').Append(root.children.Count).Append(',');

        for(int i = 0; i < root.children.Count; ++i)
            sb.Append(serialize(root.children[i]));

        return sb.ToString();
    }

    // Decodes your encoded data to tree.
    public Node deserialize(string data) {
        if(data == "") return null;

        var tokens = data.Split(',');

        int i = 0;
        return des(tokens, ref i);
    }

    private Node des(string[] tokens, ref int i)
    {
        var val = int.Parse(tokens[i++]);
        var count = int.Parse(tokens[i++]);

        var node = new Node(val, new List<Node>());

        for(int j = 0; j < count; ++j)
            node.children.Add(des(tokens, ref i));

        return node;
    }
}
