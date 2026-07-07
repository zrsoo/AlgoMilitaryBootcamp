/*
// Definition for a Node.
public class Node {
    public int val;
    public IList<Node> neighbors;

    public Node() {
        val = 0;
        neighbors = new List<Node>();
    }

    public Node(int _val) {
        val = _val;
        neighbors = new List<Node>();
    }

    public Node(int _val, List<Node> _neighbors) {
        val = _val;
        neighbors = _neighbors;
    }
}
*/

public class Solution {
    public Node CloneGraph(Node node) {
        if (node == null) return null;

        var dict = new Dictionary<Node, Node>();
        return Clone(node, dict);
    }

    private Node Clone(Node node, Dictionary<Node, Node> dict)
    {
        if (dict.TryGetValue(node, out var existing))
            return existing;

        var copy = new Node(node.val);
        dict[node] = copy; // register BEFORE recursing — breaks cycles

        foreach (Node ne in node.neighbors)
            copy.neighbors.Add(Clone(ne, dict));

        return copy;
    }
}
