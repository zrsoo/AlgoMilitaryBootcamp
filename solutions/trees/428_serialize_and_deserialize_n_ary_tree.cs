using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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

        Encode(root, sb);

        Console.WriteLine(sb.ToString());

        return sb.ToString();
    }

    private void Encode(Node node, StringBuilder sb)
    {
        sb.Append(node.val).Append(',');
        sb.Append(node.children.Count).Append(',');

        foreach(var c in node.children)
            Encode(c, sb);
    }

    // Decodes your encoded data to tree.
    public Node deserialize(string data) {
        if(string.IsNullOrWhiteSpace(data)) return null;

        string[] nodes = data.Split(',');
        int i = 0;

        return Decode(nodes, ref i);
    }

    private Node Decode(string[] nodes, ref int i)
    {
        Node node = new Node(int.Parse(nodes[i++]));
        int childrenCount = int.Parse(nodes[i++]);

        for(int j = 0; j < childrenCount; ++j)
            node.children.Add(Decode(nodes, ref i));

        return node;
    }
}
