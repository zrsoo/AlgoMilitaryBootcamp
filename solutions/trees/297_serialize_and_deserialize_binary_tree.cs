/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int x) { val = x; }
 * }
 */
public class Codec {

    // Encodes a tree to a single string.
    // Preorder DFS with explicit null sentinels: every node emits "val,",
    // every null emits "#,". The structure is implicit in the token order,
    // so no positional/array-index layout is needed (works for skewed trees
    // and negative values). O(n) time, O(n) space.
    public string serialize(TreeNode root) {
        var sb = new StringBuilder();
        Encode(root, sb);
        return sb.ToString();
    }

    private void Encode(TreeNode node, StringBuilder sb)
    {
        if(node == null)
        {
            sb.Append("#,");
            return;
        }

        sb.Append(node.val).Append(',');
        Encode(node.left, sb);
        Encode(node.right, sb);
    }

    // Decodes your encoded data to tree.
    // Consume tokens in the same preorder the encoder produced them, using a
    // shared cursor. A "#" token is a null subtree. O(n) time, O(n) space.
    public TreeNode deserialize(string data) {
        var tokens = data.Split(',');
        int i = 0;
        return Decode(tokens, ref i);
    }

    private TreeNode Decode(string[] tokens, ref int i)
    {
        string tok = tokens[i++];

        if(tok == "#")
            return null;

        var node = new TreeNode(int.Parse(tok));
        node.left = Decode(tokens, ref i);
        node.right = Decode(tokens, ref i);
        return node;
    }
}

// Your Codec object will be instantiated and called as such:
// Codec ser = new Codec();
// Codec deser = new Codec();
// TreeNode ans = deser.deserialize(ser.serialize(root));
