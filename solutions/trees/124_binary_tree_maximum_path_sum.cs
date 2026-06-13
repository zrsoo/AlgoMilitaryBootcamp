/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    private int best;

    public int MaxPathSum(TreeNode root) {
        best = int.MinValue;
        Gain(root);
        return best;
    }

    private int Gain(TreeNode node)
    {
        if(node == null) return 0;

        int l = Math.Max(Gain(node.left), 0);
        int r = Math.Max(Gain(node.right), 0);

        best = Math.Max(best, node.val + l + r);

        return node.val + Math.Max(l, r);
    }
}
