/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int x) { val = x; }
 * }
 */
public class Solution {
    public TreeNode LowestCommonAncestor(TreeNode root, TreeNode p, TreeNode q) {
        return LCA(root, p, q);
    }

    private TreeNode LCA(TreeNode node, TreeNode p, TreeNode q)
    {
        if(node == null || node == p || node == q)
            return node;

        var L = LCA(node.left, p, q);
        var R = LCA(node.right, p, q);

        if(L != null && R != null)
            return node;
        else if(L != null)
            return L;
        else if(R != null)
            return R;

        return null;
    }
}
